using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    public FirebaseFirestore db { get; private set; }
    private bool isFirebaseReady = false;

    private void Awake()
    {
        // Singleton pattern to persist across scenes
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
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Debug.Log("Firebase is ready.");
                db = FirebaseFirestore.DefaultInstance;
                isFirebaseReady = true;

                // ✅ Log Firebase config details for verification
                FirebaseApp app = FirebaseApp.DefaultInstance;
                #if UNITY_EDITOR
                                Debug.Log("✅ Firebase Project ID: " + app.Options.ProjectId);
                #endif
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    public async void AddTestData()
    {
        if (!isFirebaseReady) return;

        DocumentReference docRef = db.Collection("users").Document("testUser");
        Dictionary<string, object> user = new Dictionary<string, object>
        {
            { "name", "Chrono" },
            { "score", 100 },
            { "isTeacher", false }
        };

        await docRef.SetAsync(user).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
                Debug.Log("Document successfully written!");
            else
                Debug.LogError("Error writing document: " + task.Exception);
        });
    }

    public async void ReadTestData()
    {
        if (!isFirebaseReady) return;

        DocumentReference docRef = db.Collection("users").Document("testUser");
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            Debug.Log($"Document data: {snapshot.ToDictionary()["name"]}");
        }
        else
        {
            Debug.Log("No such document!");
        }
    }
    
    private void Start()
    {
        // Optional: wait a second to make sure Firebase is initialized
        Invoke(nameof(TestConnection), 1.5f);
    }

    private void TestConnection()
    {
        AddTestData();
    }

}

