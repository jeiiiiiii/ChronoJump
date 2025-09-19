using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using System;
using UnityEngine;

public class AuthService
{
    private readonly IFirebaseService _firebaseService;

    public AuthService(IFirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    public void SignIn(string email, string password, Action<bool, string> callback)
{
    _firebaseService.Auth.SignInWithEmailAndPasswordAsync(email, password)
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignIn was canceled.");
                callback(false, "SignIn was canceled.");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.Log("SignIn failed.");
                callback(false, "Your email or password is incorrect.");
                return;
            }

            var authResult = task.Result;
            FirebaseUser user = authResult.User;

            // 🔎 Now check Firestore for isRemoved flag
            var db = Firebase.Firestore.FirebaseFirestore.DefaultInstance;
            db.Collection("userAccounts").Document(user.UserId).GetSnapshotAsync().ContinueWithOnMainThread(docTask =>
            {
                if (docTask.IsCanceled || docTask.IsFaulted)
                {
                    callback(false, "Error verifying account.");
                    return;
                }

                var snapshot = docTask.Result;
                if (!snapshot.Exists)
                {
                    callback(false, "Account does not exist in records.");
                    return;
                }

                // ✅ Check if isRemoved is true
                if (snapshot.ContainsField("isRemoved") && snapshot.GetValue<bool>("isRemoved"))
                {
                    callback(false, "This account has been removed.");
                    
                    // Optionally, also sign them out immediately
                    _firebaseService.Auth.SignOut();
                }
                else
                {
                    callback(true, "Login successful");
                }
            });
        });
}


    // Original SignUp method for backward compatibility
    public void SignUp(string email, string password, string displayName, bool isTeacher, Action<bool, string> callback)
    {
        SignUp(email, password, displayName, isTeacher, null, callback);
    }

    // New SignUp method with code parameter
    public void SignUp(string email, string password, string displayName, bool isTeacher, string code, Action<bool, string> callback)
    {
        _firebaseService.Auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("SignUp was canceled.");
                    callback(false, "SignUp was canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    string errorMessage = GetAuthErrorMessage(task.Exception);
                    Debug.Log("SignUp failed.");
                    callback(false, errorMessage);
                    return;
                }

                var authResult = task.Result;
                FirebaseUser user = authResult.User;

                UpdateUserProfile(user, displayName, isTeacher, code, callback);
            });
    }

    // Updated to accept code parameter
    private void UpdateUserProfile(FirebaseUser user, string displayName, bool isTeacher, string code, Action<bool, string> callback)
    {
        UserProfile profile = new UserProfile
        {
            DisplayName = displayName,
        };

        user.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(updateTask =>
        {
            if (updateTask.IsCanceled || updateTask.IsFaulted)
            {
                Debug.LogError("Failed to update user profile: " + updateTask.Exception);
                callback(false, "Failed to update user profile. Please try again.");
                return;
            }

            Debug.Log($"User {displayName} registered successfully with email: {user.Email}");
            
            var userService = new UserService(_firebaseService);
            userService.SaveUserData(user, isTeacher, code, callback);
        });
    }

    private string GetAuthErrorMessage(Exception exception)
    {
        string errorMessage = "Unknown error occurred. Please try again.";

        // Handle AggregateException (from async operations)
        if (exception is AggregateException aggregateEx)
        {
            foreach (var inner in aggregateEx.Flatten().InnerExceptions)
            {
                if (inner is FirebaseException firebaseEx)
                {
                    errorMessage = GetFirebaseErrorMessage(firebaseEx);
                    break;
                }
            }
        }
        // Handle direct FirebaseException
        else if (exception is FirebaseException firebaseEx)
        {
            errorMessage = GetFirebaseErrorMessage(firebaseEx);
        }
        // Handle other exceptions
        else if (exception.InnerException is FirebaseException innerFirebaseEx)
        {
            errorMessage = GetFirebaseErrorMessage(innerFirebaseEx);
        }

        return errorMessage;
    }

    private string GetFirebaseErrorMessage(FirebaseException firebaseEx)
    {
        var errorCode = (AuthError)firebaseEx.ErrorCode;
        switch (errorCode)
        {
            case AuthError.EmailAlreadyInUse:
                return "This email is already in use.";
            case AuthError.InvalidEmail:
                return "The email address is badly formatted.";
            case AuthError.WeakPassword:
                return "The password is too weak.";
            default:
                return firebaseEx.Message;
        }
    }
}