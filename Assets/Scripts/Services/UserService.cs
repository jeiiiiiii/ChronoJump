using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using System;
using UnityEngine;

public class UserService
{
    private readonly IFirebaseService _firebaseService;

    public UserService(IFirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    public void SaveUserData(FirebaseUser user, bool isTeacher, Action<bool, string> callback)
    {
        DocumentReference docRef = _firebaseService.DB.Collection("userAccounts").Document(user.UserId);
        
        var userData = new
        {
            displayName = user.DisplayName,
            email = user.Email,
            role = isTeacher ? "teacher" : "student"
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
            callback(true, "Registration successful");
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
}