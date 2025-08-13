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
            Debug.Log($"User signed in successfully: {user.Email}");
            callback(true, "Login successful");
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
        docRef.GetSnapshotAsync().ContinueWith(task =>
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
                userData.createdAt = snapshot.GetValue<DateTime>("createdAt");
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



