using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ClassService
{
    private readonly IFirebaseService _firebaseService;
    private readonly TeacherService _teacherService;

    public ClassService(IFirebaseService firebaseService, TeacherService teacherService)
    {
        _firebaseService = firebaseService;
        _teacherService = teacherService;
    }

    public void CreateClass(string className, string classLevel, Action<bool, string> callback)
    {
        if (_firebaseService.CurrentUser == null)
        {
            Debug.LogError("No current user signed in.");
            callback(false, "No user signed in.");
            return;
        }

        _teacherService.GetTeacherData(_firebaseService.CurrentUser.UserId, teacherData =>
        {
            if (teacherData == null)
            {
                Debug.LogError("No teacher data found for current user.");
                callback(false, "No teacher data found.");
                return;
            }

            CreateClassWithTeacherData(className, classLevel, teacherData.teachId, callback);
        });
    }

    private void CreateClassWithTeacherData(string className, string classLevel, string teacherId, Action<bool, string> callback)
    {
        GenerateUniqueClassCode().ContinueWithOnMainThread(codeTask =>
        {
            if (codeTask.IsCanceled || codeTask.IsFaulted)
            {
                Debug.LogError("Failed to generate class code: " + codeTask.Exception);
                callback(false, "Failed to generate class code. Please try again.");
                return;
            }

            string classCode = codeTask.Result;
            SaveNewClass(className, classLevel, classCode, teacherId, callback);
        });
    }

    private void SaveNewClass(string className, string classLevel, string classCode, string teacherId, Action<bool, string> callback)
    {
        var newClass = new
        {
            className = className,
            classLevel = classLevel,
            classCode = classCode,
            teachId = teacherId,
            dateCreated = Timestamp.GetCurrentTimestamp()
        };

        _firebaseService.DB.Collection("classes").AddAsync(newClass)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("Failed to create class: " + task.Exception);
                    callback(false, "Failed to create class. Please try again.");
                    return;
                }

                Debug.Log($"Class {className} created successfully with code: {classCode}");
                callback(true, classCode);
            });
    }

    public void DeleteClass(string classCode, Action<bool, string> callback)
    {
        // First, get the class document to delete
        GetClassByCode(classCode).ContinueWithOnMainThread(getClassTask =>
        {
            if (getClassTask.IsCanceled || getClassTask.IsFaulted || getClassTask.Result == null)
            {
                Debug.LogError("Failed to find class: " + getClassTask.Exception);
                callback(false, "Class not found.");
                return;
            }

            string classDocumentId = getClassTask.Result.Id;
            
            // Delete all students and their complete data first
            DeleteCompleteStudentData(classCode).ContinueWithOnMainThread(deleteStudentsTask =>
            {
                if (deleteStudentsTask.IsCanceled || deleteStudentsTask.IsFaulted)
                {
                    Debug.LogError("Failed to delete student data: " + deleteStudentsTask.Exception);
                    callback(false, "Failed to delete student data. Please try again.");
                    return;
                }

                // Now delete the class document
                _firebaseService.DB.Collection("classes").Document(classDocumentId).DeleteAsync()
                    .ContinueWithOnMainThread(deleteClassTask =>
                    {
                        if (deleteClassTask.IsCanceled || deleteClassTask.IsFaulted)
                        {
                            Debug.LogError("Failed to delete class: " + deleteClassTask.Exception);
                            callback(false, "Failed to delete class. Please try again.");
                            return;
                        }

                        Debug.Log($"Class {classCode} and all student data deleted successfully.");
                        callback(true, "Class and all student data deleted successfully.");
                    });
            });
        });
    }

    public void EditClassName(string classCode, string newClassName, Action<bool, string> callback)
    {
        GetClassByCode(classCode).ContinueWithOnMainThread(getClassTask =>
        {
            if (getClassTask.IsCanceled || getClassTask.IsFaulted || getClassTask.Result == null)
            {
                Debug.LogError("Failed to find class: " + getClassTask.Exception);
                callback(false, "Class not found.");
                return;
            }

            string classDocumentId = getClassTask.Result.Id;
            
            // Update the class name
            var updateData = new Dictionary<string, object>
            {
                { "className", newClassName },
                { "dateModified", Timestamp.GetCurrentTimestamp() }
            };

            _firebaseService.DB.Collection("classes").Document(classDocumentId).UpdateAsync(updateData)
                .ContinueWithOnMainThread(updateTask =>
                {
                    if (updateTask.IsCanceled || updateTask.IsFaulted)
                    {
                        Debug.LogError("Failed to update class name: " + updateTask.Exception);
                        callback(false, "Failed to update class name. Please try again.");
                        return;
                    }

                    Debug.Log($"Class name updated successfully to: {newClassName}");
                    callback(true, "Class name updated successfully.");
                });
        });
    }

    private async Task<DocumentSnapshot> GetClassByCode(string classCode)
    {
        QuerySnapshot snapshot = await _firebaseService.DB.Collection("classes")
            .WhereEqualTo("classCode", classCode)
            .Limit(1)
            .GetSnapshotAsync();

        return snapshot.Count > 0 ? snapshot.Documents.FirstOrDefault() : null;
    }

    private async Task DeleteCompleteStudentData(string classCode)
    {
        try
        {
            // Step 1: Get all students in this class
            QuerySnapshot studentsSnapshot = await _firebaseService.DB.Collection("students")
                .WhereEqualTo("classCode", classCode)
                .GetSnapshotAsync();

            Debug.Log($"Found {studentsSnapshot.Count} students to delete in class {classCode}");

            var deleteTasks = new List<Task>();

            // Step 2: For each student, delete all their associated data
            foreach (var studentDoc in studentsSnapshot.Documents)
            {
                var studentData = studentDoc.ToDictionary();
                string userId = studentData.ContainsKey("userId") ? studentData["userId"].ToString() : null;
                string studId = studentData.ContainsKey("studId") ? studentData["studId"].ToString() : null;

                Debug.Log($"Processing student deletion - UserId: {userId}, StudId: {studId}");

                if (!string.IsNullOrEmpty(userId))
                {
                    // Delete from userAccounts collection using userId as document ID
                    deleteTasks.Add(DeleteUserAccount(userId));
                    
                    // Delete from Firebase Auth using userId
                    deleteTasks.Add(DeleteFromFirebaseAuth(userId));
                }

                if (!string.IsNullOrEmpty(studId))
                {
                    // Delete from studentProgress collection using studId as document ID
                    deleteTasks.Add(DeleteStudentProgress(studId));
                    
                    // Delete from studentLeaderboards collection using studId as document ID
                    deleteTasks.Add(DeleteStudentLeaderboards(studId));
                }

                // Delete the student document itself
                deleteTasks.Add(studentDoc.Reference.DeleteAsync());
            }

            // Wait for all deletion tasks to complete
            await Task.WhenAll(deleteTasks);
            Debug.Log($"Successfully deleted all data for {studentsSnapshot.Count} students from class {classCode}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error deleting complete student data: {ex.Message}");
            throw;
        }
    }

    private async Task DeleteUserAccount(string userId)
    {
        try
        {
            await _firebaseService.DB.Collection("userAccounts").Document(userId).DeleteAsync();
            Debug.Log($"Deleted userAccount for userId: {userId}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to delete userAccount for userId {userId}: {ex.Message}");
            // Don't throw - we want to continue with other deletions
        }
    }

    private async Task DeleteFromFirebaseAuth(string userId)
    {
        try
        {
            var function = Firebase.Functions.FirebaseFunctions.DefaultInstance
                .GetHttpsCallable("deleteUser");

            var result = await function.CallAsync(new Dictionary<string, object>
            {
                { "userId", userId }
            });

            Debug.Log($"Successfully requested deletion of user {userId}. Result: {result?.Data}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to delete from Firebase Auth for userId {userId}: {ex.Message}\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Debug.LogError($"Inner exception: {ex.InnerException.Message}");
            }
        }
    }




    private async Task DeleteStudentProgress(string studId)
    {
        try
        {
            await _firebaseService.DB.Collection("studentProgress").Document(studId).DeleteAsync();
            Debug.Log($"Deleted studentProgress for studId: {studId}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to delete studentProgress for studId {studId}: {ex.Message}");
            // Don't throw - we want to continue with other deletions
        }
    }

    private async Task DeleteStudentLeaderboards(string studId)
    {
        try
        {
            await _firebaseService.DB.Collection("studentLeaderboards").Document(studId).DeleteAsync();
            Debug.Log($"Deleted studentLeaderboards for studId: {studId}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to delete studentLeaderboards for studId {studId}: {ex.Message}");
            // Don't throw - we want to continue with other deletions
        }
    }

    public async Task<string> GenerateUniqueClassCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new System.Random();
        string classCode;
        bool exists = true;

        do
        {
            classCode = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            exists = await DoesClassCodeExist(classCode);
        }
        while (exists);

        return classCode;
    }

    private async Task<bool> DoesClassCodeExist(string classCode)
    {
        QuerySnapshot snapshot = await _firebaseService.DB.Collection("classes")
            .WhereEqualTo("classCode", classCode)
            .Limit(1)
            .GetSnapshotAsync();

        return snapshot.Count > 0;
    }
}