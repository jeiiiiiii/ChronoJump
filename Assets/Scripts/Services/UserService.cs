using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UserService
{
    private readonly IFirebaseService _firebaseService;

    public UserService(IFirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    public void SaveUserData(FirebaseUser user, bool isTeacher, Action<bool, string> callback)
    {
        SaveUserData(user, isTeacher, null, callback);
    }

    public void SaveUserData(FirebaseUser user, bool isTeacher, string code, Action<bool, string> callback)
    {
        DocumentReference docRef = _firebaseService.DB
            .Collection("userAccounts")
            .Document(user.UserId);

        var userData = new Dictionary<string, object>
        {
            { "displayName", user.DisplayName },
            { "email", user.Email },
            { "role", isTeacher ? "teacher" : "student" },
            { "createdAt", FieldValue.ServerTimestamp }
        };

        Debug.Log($"Saving user data for {user.DisplayName}...");

        docRef.SetAsync(userData).ContinueWithOnMainThread(setTask =>
        {
            if (setTask.IsCanceled || setTask.IsFaulted)
            {
                Debug.LogError("Failed to save user data: " + setTask.Exception);
                callback(false, "Failed to save user data. Please try again.");
                return;
            }

            Debug.Log($"User data for {user.DisplayName} saved successfully.");

            if (isTeacher)
            {
                HandleTeacherRegistration(user, callback);
            }
            else
            {
                HandleStudentRegistration(user, code, callback);
            }
        });
    }


    private void HandleTeacherRegistration(FirebaseUser user, Action<bool, string> callback)
    {   
        var teacherData = new Dictionary<string, object>
        {
            { "teachFirstName", ExtractFirstName(user.DisplayName) },
            { "teachLastName", ExtractLastName(user.DisplayName) },
            { "title", "Teacher" },    
            { "userId", user.UserId },
            { "dateUpdated", FieldValue.ServerTimestamp },
        };

        _firebaseService.DB.Collection("teachers").AddAsync(teacherData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Failed to create teacher profile: " + task.Exception);
                callback(false, "Failed to create teacher profile. Please try again.");
                return;
            }

            Debug.Log($"Teacher profile created successfully for {user.DisplayName}");
            callback(true, "Teacher registration successful! You can now create classes.");
        });
    }


    private void HandleStudentRegistration(FirebaseUser user, string classCode, Action<bool, string> callback)
    {
        _firebaseService.DB.Collection("classes")
            .WhereEqualTo("classCode", classCode)
            .Limit(1)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted || task.Result == null || task.Result.Count == 0)
                {
                    Debug.LogError("Class not found or failed to fetch teacher ID.");
                    callback(false, "Invalid class code.");
                    return;
                }

                QuerySnapshot snapshot = task.Result;
                DocumentSnapshot classDoc = snapshot.Documents.FirstOrDefault();

                string teacherId = "";
                if (classDoc != null && classDoc.TryGetValue("teacherId", out string fetchedTeacherId))
                {
                    teacherId = fetchedTeacherId;
                }

                // Generate a new studId for students & studentProgress
                DocumentReference studentRef = _firebaseService.DB.Collection("students").Document();
                string studId = studentRef.Id;

                // Student profile data
                var studentData = new Dictionary<string, object>
                {
                    { "classCode", classCode },
                    { "studName", user.DisplayName },
                    { "studProfilePic", "" },
                    { "teachId", teacherId },
                    { "userId", user.UserId },
                    { "dateUpdated", FieldValue.ServerTimestamp }
                };

                // Leaderboard data (auto-ID)
                var leaderboardData = new Dictionary<string, object>
                {
                    { "classCode", classCode },
                    { "dateUpdated", FieldValue.ServerTimestamp },
                    { "displayName", user.DisplayName },
                    { "overallScore", "0" },
                    { "studId", studId }
                };

                // Progress data (same studId)
                var progressData = new Dictionary<string, object>
                {
                    { "currentStory", null },
                    { "dateUpdated", FieldValue.ServerTimestamp },
                    { "overallScore", "0" },
                    { "successRate", "0%" }
                };

                // Save student profile
                studentRef.SetAsync(studentData).ContinueWithOnMainThread(studentTask =>
                {
                    if (studentTask.IsCanceled || studentTask.IsFaulted)
                    {
                        Debug.LogError("Failed to create student profile: " + studentTask.Exception);
                        callback(false, "Failed to create student profile. Please try again.");
                        return;
                    }

                    // Save leaderboard (auto-ID)
                    _firebaseService.DB.Collection("studentLeaderboards").AddAsync(leaderboardData);

                    // Save progress (studId)
                    DocumentReference progressRef = _firebaseService.DB.Collection("studentProgress").Document(studId);
                    progressRef.SetAsync(progressData);

                    Debug.Log($"✅ Student profile created (studId: {studId}), leaderboard entry, and progress initialized.");
                    callback(true, "Student registration successful!");
                });
            });
    }



    public void GetUserData(Action<UserAccountModel> callback)
    {
        if (_firebaseService.CurrentUser == null)
        {
            Debug.LogError("No current user signed in.");
            callback(null);
            return;
        }

        DocumentReference docRef = _firebaseService.DB.Collection("userAccounts")
            .Document(_firebaseService.CurrentUser.UserId);
            
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Failed to get user data: " + task.Exception);
                callback(null);
                return;
            }

            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                UserAccountModel userData = MapDocumentToUserAccount(snapshot);
                callback(userData);
            }
            else
            {
                Debug.LogWarning("User document does not exist.");
                callback(null);
            }
        });
    }

    private UserAccountModel MapDocumentToUserAccount(DocumentSnapshot snapshot)
    {
        return new UserAccountModel
        {
            userId = snapshot.Id,
            displayName = snapshot.GetValue<string>("displayName"),
            email = snapshot.GetValue<string>("email"),
            role = snapshot.GetValue<string>("role")
        };
    }

    private string ExtractFirstName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName)) return "";
        
        string[] nameParts = fullName.Split(' ');
        return nameParts[0];
    }

    private string ExtractLastName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName)) return "";
        
        string[] nameParts = fullName.Split(' ');
        return nameParts.Length > 1 ? nameParts[nameParts.Length - 1] : "";
    }
}