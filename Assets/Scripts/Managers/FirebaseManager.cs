using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using System;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    public FirebaseAuth Auth { get; private set; }
    public FirebaseFirestore DB { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Auth = FirebaseAuth.DefaultInstance;
                DB = FirebaseFirestore.DefaultInstance;

                Debug.Log("✅ Firebase initialized successfully!");
            }
            else
            {
                Debug.LogError($"❌ Could not resolve Firebase dependencies: {task.Result}");
            }
        });
    }

    public FirebaseUser CurrentUser => Auth.CurrentUser;
    public UserAccountModel CurrentUserData { get; private set; }

    public void SignIn(string email, string password, Action<bool, string> callback)
    {
        Auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignIn was canceled.");
                callback(false, "SignIn was canceled.");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError("SignIn failed: " + task.Exception);
                callback(false, "Your email or password is incorrect.");
                return;
            }

            var authResult = task.Result;
            FirebaseUser user = authResult.User;
            callback(true, "Login successful");
        });
    }

    public void SignUp(string email, string password, string displayName, bool isTeacher, Action<bool, string> callback)
    {
        Auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignUp was canceled.");
                callback(false, "SignUp was canceled.");
                return;
            }

            if (task.IsFaulted)
            {
                string errorMessage = "Unknown error eccurred. Please try again.";

                foreach (var inner in task.Exception.Flatten().InnerExceptions)
                {
                    if (inner is FirebaseException firebaseEx)
                    {
                        var errorCode = (AuthError)firebaseEx.ErrorCode;
                        switch (errorCode)
                        {
                            case AuthError.EmailAlreadyInUse:
                                errorMessage = "This email is already in use.";
                                break;
                            case AuthError.InvalidEmail:
                                errorMessage = "The email address is badly formatted.";
                                break;
                            case AuthError.WeakPassword:
                                errorMessage = "The password is too weak.";
                                break;
                            default:
                                errorMessage = firebaseEx.Message;
                                break;
                        }
                    }
                }

                Debug.LogError("SignUp failed: " + task.Exception);
                callback(false, errorMessage);
                return;
            }

            var authResult = task.Result;
            FirebaseUser user = authResult.User;

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
                Debug.Log($"User {displayName} registered successfully with email: {email}");

                DocumentReference docRef = DB.Collection("userAccounts").Document(user.UserId);
                var userData = new
                {
                    displayName = user.DisplayName,
                    email = user.Email,
                    role = isTeacher ? "teacher" : "student"
                };
                Debug.Log($"Saving user data for {displayName}...");

                docRef.SetAsync(userData).ContinueWithOnMainThread(setTask =>
                {
                    if (setTask.IsCanceled || setTask.IsFaulted)
                    {
                        Debug.LogError("Failed to save user data: " + setTask.Exception);
                        callback(false, "Failed to save user data. Please try again.");
                        return;
                    }

                    Debug.Log($"User data for {displayName} saved successfully.");
                    callback(true, "Registration successful");
                });
            });
        });
    }

    public void GetUserData(Action<UserAccountModel> callback)
    {
        if (CurrentUser == null)
        {
            Debug.LogError("No current user signed in.");
            callback(null);
            return;
        }

        
        DocumentReference docRef = DB.Collection("userAccounts").Document(CurrentUser.UserId);
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
                UserAccountModel userData = new UserAccountModel();
                userData.userID = snapshot.Id;
                userData.displayName = snapshot.GetValue<string>("displayName");
                userData.email = snapshot.GetValue<string>("email");
                userData.role = snapshot.GetValue<string>("role");
                callback(userData);
            }
            else
            {
                Debug.LogWarning("User document does not exist.");
                callback(null);
            }
        });
    }
}



