using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseService : IFirebaseService
{
    public FirebaseAuth Auth { get; private set; }
    public FirebaseFirestore DB { get; private set; }
    public FirebaseUser CurrentUser => Auth?.CurrentUser;
    public UserAccountModel CurrentUserData { get; private set; }

    public async Task<bool> InitializeAsync()
    {
        try
        {
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            
            if (dependencyStatus == DependencyStatus.Available)
            {
                Auth = FirebaseAuth.DefaultInstance;
                DB = FirebaseFirestore.DefaultInstance;
                
                Debug.Log("✅ Firebase initialized successfully!");
                return true;
            }
            else
            {
                Debug.LogError($"❌ Could not resolve Firebase dependencies: {dependencyStatus}");
                return false;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Firebase initialization failed: {ex.Message}");
            return false;
        }
    }

    public void SetCurrentUserData(UserAccountModel userData)
    {
        CurrentUserData = userData;
    }
}