using Firebase.Auth;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    // Core Firebase service
    private IFirebaseService _firebaseService;

    // Services (make them public so other managers can use them directly)
    public AuthService AuthService { get; private set; }
    public UserService UserService { get; private set; }
    public TeacherService TeacherService { get; private set; }
    public ClassService ClassService { get; private set; }
    public StudentService StudentService { get; private set; }

    // Public Firebase references
    public FirebaseAuth Auth => _firebaseService?.Auth;
    public FirebaseFirestore DB => _firebaseService?.DB;
    public FirebaseUser CurrentUser => _firebaseService?.CurrentUser;
    public UserAccountModel CurrentUserData => _firebaseService?.CurrentUserData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeServices();
    }

    private async void InitializeServices()
    {
        _firebaseService = new FirebaseService();
        
        bool initialized = await _firebaseService.InitializeAsync();
        if (!initialized)
        {
            Debug.LogError("Failed to initialize Firebase services");
            return;
        }

        // Initialize once here
        AuthService = new AuthService(_firebaseService);
        UserService = new UserService(_firebaseService);
        TeacherService = new TeacherService(_firebaseService);
        ClassService = new ClassService(_firebaseService, TeacherService);
        StudentService = new StudentService(_firebaseService);

        Debug.Log("✅ All Firebase services initialized successfully!");
    }

    // Optional: you can keep these wrappers for backward compatibility
    public void SignIn(string email, string password, Action<bool, string> callback)
    {
        AuthService?.SignIn(email, password, callback);
    }

    public void SignUp(string email, string password, string displayName, bool isTeacher, Action<bool, string> callback)
    {
        AuthService?.SignUp(email, password, displayName, isTeacher, callback);
    }

    public void GetUserData(Action<UserAccountModel> callback)
    {
        UserService?.GetUserData(callback);
    }

    public void GetTeacherData(string userId, Action<TeacherModel> callback)
    {
        TeacherService?.GetTeacherData(userId, callback);
    }

    public void CreateClass(string className, string classLevel, Action<bool, string> callback)
    {
        ClassService?.CreateClass(className, classLevel, callback);
    }

    public async Task<string> GenerateUniqueClassCode()
    {
        return await ClassService?.GenerateUniqueClassCode();
    }

    public void GetStudentsInClass(string classCode, Action<List<StudentModel>> callback)
    {
        StudentService?.GetStudentsInClass(classCode, callback);
    }
}
