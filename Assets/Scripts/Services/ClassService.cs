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

    #region Helpers: Build update data

    private Dictionary<string, object> GetRemovedData() => new()
    {
        { "isRemoved", true },
        { "dateRemoved", Timestamp.GetCurrentTimestamp() }
    };

    private Dictionary<string, object> GetRestoreData() => new()
    {
        { "isRemoved", false },
        { "dateRestored", Timestamp.GetCurrentTimestamp() }
    };

    #endregion

    #region Class Management

    public void CreateClass(string className, string classLevel, Action<bool, string> callback)
    {
        if (_firebaseService.CurrentUser == null)
        {
            callback(false, "No user signed in.");
            return;
        }

        _teacherService.GetTeacherData(_firebaseService.CurrentUser.UserId, teacherData =>
        {
            if (teacherData == null)
            {
                callback(false, "No teacher data found.");
                return;
            }

            CreateClassWithTeacherData(className, classLevel, teacherData.teachId, callback);
        });
    }

    private void CreateClassWithTeacherData(string className, string classLevel, string teacherId, Action<bool, string> callback)
    {
        _ = GenerateUniqueClassCode().ContinueWithOnMainThread(async codeTask =>
        {
            if (codeTask.IsFaulted || codeTask.IsCanceled)
            {
                callback(false, "Failed to generate class code.");
                return;
            }

            string classCode = codeTask.Result;
            await SaveNewClass(className, classLevel, classCode, teacherId, callback);
        });
    }

    private async Task SaveNewClass(string className, string classLevel, string classCode, string teacherId, Action<bool, string> callback)
    {
        try
        {
            var newClass = new
            {
                className,
                classLevel,
                classCode,
                teachId = teacherId,
                dateCreated = Timestamp.GetCurrentTimestamp()
            };

            await _firebaseService.DB.Collection("classes").AddAsync(newClass);
            Debug.Log($"[ClassService] Created class {className} with code {classCode}");
            callback(true, classCode);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ClassService] Failed to create class: {ex.Message}");
            callback(false, "Failed to create class. Please try again.");
        }
    }

    public async void DeleteClass(string classCode, Action<bool, string> callback)
    {
        try
        {
            var classDoc = await GetClassByCode(classCode);
            if (classDoc == null)
            {
                callback(false, "Class not found.");
                return;
            }

            await MarkCompleteStudentDataAsRemoved(classCode);

            await classDoc.Reference.DeleteAsync();
            Debug.Log($"[ClassService] Class {classCode} deleted and all student data marked as removed.");
            callback(true, "Class deleted and all student data marked as removed.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ClassService] DeleteClass error: {ex.Message}");
            callback(false, "Failed to delete class.");
        }
    }

    public async void EditClassName(string classCode, string newClassName, Action<bool, string> callback)
    {
        try
        {
            var classDoc = await GetClassByCode(classCode);
            if (classDoc == null)
            {
                callback(false, "Class not found.");
                return;
            }

            var updateData = new Dictionary<string, object>
            {
                { "className", newClassName },
                { "dateModified", Timestamp.GetCurrentTimestamp() }
            };

            await classDoc.Reference.UpdateAsync(updateData);
            Debug.Log($"[ClassService] Class name updated to: {newClassName}");
            callback(true, "Class name updated successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ClassService] EditClassName error: {ex.Message}");
            callback(false, "Failed to update class name.");
        }
    }

    private async Task<DocumentSnapshot> GetClassByCode(string classCode)
    {
        var snapshot = await _firebaseService.DB.Collection("classes")
            .WhereEqualTo("classCode", classCode)
            .Limit(1)
            .GetSnapshotAsync();

        return snapshot.Count > 0 ? snapshot.Documents.First() : null;
    }

    #endregion

    #region Student Soft Delete / Restore

    private async Task MarkCompleteStudentDataAsRemoved(string classCode)
    {
        var studentsSnapshot = await _firebaseService.DB.Collection("students")
            .WhereEqualTo("classCode", classCode)
            .WhereEqualTo("isRemoved", false)
            .GetSnapshotAsync();

        var tasks = studentsSnapshot.Documents.Select(studentDoc =>
            MarkStudentAndRelatedDataAsRemoved(studentDoc));

        await Task.WhenAll(tasks);
    }

    public async Task MarkStudentAndRelatedDataAsRemoved(DocumentSnapshot studentDoc)
    {
        var data = studentDoc.ToDictionary();
        string userId = data.ContainsKey("userId") ? data["userId"]?.ToString() : null;
        string studId = studentDoc.Id; // use the Firestore document ID as studId

        Debug.Log($"[ClassService] Starting removal process for student - userId: {userId}, studId: {studId}");

        var tasks = new List<Task>();
        
        // Create separate data for each update to avoid race conditions
        tasks.Add(studentDoc.Reference.UpdateAsync(GetRemovedData()));

        if (!string.IsNullOrEmpty(userId))
        {
            Debug.Log($"[ClassService] Adding user account removal task for userId: {userId}");
            tasks.Add(MarkUserAccountAsRemoved(userId));
        }

        if (!string.IsNullOrEmpty(studId))
        {
            Debug.Log($"[ClassService] Adding progress and leaderboard removal tasks for studId: {studId}");
            tasks.Add(MarkStudentProgressAsRemoved(studId));
            tasks.Add(MarkStudentLeaderboardAsRemoved(studId));
        }

        Debug.Log($"[ClassService] Executing {tasks.Count} removal tasks");
        await Task.WhenAll(tasks);
        Debug.Log($"[ClassService] Completed all removal tasks for student");
    }

    private async Task MarkUserAccountAsRemoved(string userId)
    {
        try
        {
            Debug.Log($"[ClassService] Marking user account {userId} as removed");
            await _firebaseService.DB.Collection("userAccounts").Document(userId).UpdateAsync(GetRemovedData());
            Debug.Log($"[ClassService] Successfully marked user account {userId} as removed");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ClassService] Failed to mark user account {userId} as removed: {ex.Message}");
            Debug.LogError($"[ClassService] Stack trace: {ex.StackTrace}");
        }
    }

   private async Task MarkStudentProgressAsRemoved(string studId)
{
    try
    {
        Debug.Log($"[ClassService] Marking student progress {studId} as removed");
        var progressDoc = await _firebaseService.DB.Collection("studentProgress").Document(studId).GetSnapshotAsync();

        if (progressDoc.Exists)
        {
            await progressDoc.Reference.UpdateAsync(GetRemovedData());
            Debug.Log($"[ClassService] Successfully marked student progress {studId} as removed");
        }
        else
        {
            Debug.LogWarning($"[ClassService] No progress document found for {studId}, skipping");
        }
    }
    catch (Exception ex)
    {
        Debug.LogError($"[ClassService] Failed to mark progress for {studId}: {ex.Message}");
    }
}

private async Task MarkStudentLeaderboardAsRemoved(string studId)
{
    try
    {
        Debug.Log($"[ClassService] Marking leaderboard {studId} as removed");
        var leaderboardDoc = await _firebaseService.DB.Collection("studentLeaderboards").Document(studId).GetSnapshotAsync();

        if (leaderboardDoc.Exists)
        {
            await leaderboardDoc.Reference.UpdateAsync(GetRemovedData());
            Debug.Log($"[ClassService] Successfully marked leaderboard {studId} as removed");
        }
        else
        {
            Debug.LogWarning($"[ClassService] No leaderboard document found for {studId}, skipping");
        }
    }
    catch (Exception ex)
    {
        Debug.LogError($"[ClassService] Failed to mark leaderboard for {studId}: {ex.Message}");
    }
}



    public async void MarkStudentAsRemoved(string userId, string classCode, Action<bool> callback)
    {
        try
        {
            Debug.Log($"[ClassService] Starting MarkStudentAsRemoved for userId: {userId}, classCode: {classCode}");
            
            var query = await _firebaseService.DB.Collection("students")
                .WhereEqualTo("userId", userId)
                .WhereEqualTo("classCode", classCode)
                .Limit(1)
                .GetSnapshotAsync();

            if (query.Count == 0)
            {
                Debug.LogWarning($"[ClassService] No student found with userId: {userId} and classCode: {classCode}");
                callback(false);
                return;
            }

            Debug.Log($"[ClassService] Found student document, proceeding with removal");
            await MarkStudentAndRelatedDataAsRemoved(query.Documents.First());
            Debug.Log($"[ClassService] Student {userId} removed from class {classCode}");
            callback(true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ClassService] MarkStudentAsRemoved error: {ex.Message}");
            Debug.LogError($"[ClassService] Stack trace: {ex.StackTrace}");
            callback(false);
        }
    }

    public async void RestoreStudent(string studId, Action<bool, string> callback)
    {
        try
        {
            var studentDoc = await _firebaseService.DB.Collection("students").Document(studId).GetSnapshotAsync();
            if (!studentDoc.Exists)
            {
                callback(false, "Student not found.");
                return;
            }

            var data = studentDoc.ToDictionary();
            if (!data.TryGetValue("isRemoved", out var removedObj) || !(bool)removedObj)
            {
                callback(false, "Student is not marked as removed.");
                return;
            }

            await RestoreStudentData(studId);
            Debug.Log($"[ClassService] Student {studId} restored");
            callback(true, "Student restored successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ClassService] RestoreStudent error: {ex.Message}");
            callback(false, "Failed to restore student.");
        }
    }

    private async Task RestoreStudentData(string studId)
    {
        var tasks = new List<Task>();

        var studentDoc = await _firebaseService.DB.Collection("students").Document(studId).GetSnapshotAsync();
        if (studentDoc.Exists)
        {
            var data = studentDoc.ToDictionary();
            string userId = data.ContainsKey("userId") ? data["userId"]?.ToString() : null;

            // Create separate restore data for each operation
            tasks.Add(studentDoc.Reference.UpdateAsync(GetRestoreData()));

            if (!string.IsNullOrEmpty(userId))
                tasks.Add(_firebaseService.DB.Collection("userAccounts").Document(userId).UpdateAsync(GetRestoreData()));

            tasks.Add(_firebaseService.DB.Collection("studentProgress").Document(studId).UpdateAsync(GetRestoreData()));

            var leaderboardQuery = await _firebaseService.DB.Collection("studentLeaderboards")
                .WhereEqualTo("studId", studId)
                .GetSnapshotAsync();

            // Create separate restore data for each leaderboard document
            foreach (var doc in leaderboardQuery.Documents)
            {
                tasks.Add(doc.Reference.UpdateAsync(GetRestoreData()));
            }

            await Task.WhenAll(tasks);
        }
    }

    #endregion

    #region Utility

    public async Task<string> GenerateUniqueClassCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new System.Random();

        string classCode;
        do
        {
            classCode = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        while (await DoesClassCodeExist(classCode));

        return classCode;
    }

    private async Task<bool> DoesClassCodeExist(string classCode)
    {
        var snapshot = await _firebaseService.DB.Collection("classes")
            .WhereEqualTo("classCode", classCode)
            .Limit(1)
            .GetSnapshotAsync();

        return snapshot.Count > 0;
    }

    public async Task<List<DocumentSnapshot>> GetActiveStudentsForClass(string classCode)
    {
        var snapshot = await _firebaseService.DB.Collection("students")
            .WhereEqualTo("classCode", classCode)
            .WhereEqualTo("isRemoved", false)
            .GetSnapshotAsync();

        return snapshot.Documents.ToList();
    }

    #endregion
}