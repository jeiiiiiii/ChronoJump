using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

    // Initialize Firebase services (Auth and Firestore) asynchronously on the main thread
    // using ContinueWithOnMainThread to ensure thread safety with Unity API calls. 
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

    // Sign in user with email and password, handling errors and invoking callback with success status and message
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
                Debug.Log("SignIn failed.");
                callback(false, "Your email or password is incorrect.");
                return;
            }

            var authResult = task.Result;
            FirebaseUser user = authResult.User;
            callback(true, "Login successful");
        });
    }

    // Register a new user with email, password, and display name, saving additional user data to Firestore
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

                Debug.Log("SignUp failed.");
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

    // Retrieve user data from Firestore based on the currently signed-in user
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
                userData.userId = snapshot.Id;
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

    // Retrieve teacher data including associated class codes from Firestore based on userId
    public async void GetTeacherData(string userId, Action<TeacherModel> callback)
    {
        try
        {
            Query teachQuery = DB.Collection("teachers").WhereEqualTo("userId", userId).Limit(1);
            QuerySnapshot teachQuerySnapshot = await teachQuery.GetSnapshotAsync();

            if (teachQuerySnapshot.Count == 0)
            {
                Debug.LogWarning("No teacher data found for userId: " + userId);
                callback(null);
                return;
            }



            var teacherDoc = teachQuerySnapshot.Documents.FirstOrDefault();
            if (teacherDoc == null)
            {
                Debug.LogError("Teacher document is null after query.");
                callback(null);
                return;
            }

            // list down here the content of teacherDoc for debugging
            Debug.Log($"Teacher Document ID: {teacherDoc.Id}");
            foreach (var field in teacherDoc.ToDictionary())
            {
                Debug.Log($"Field: {field.Key}, Value: {field.Value}");
            }

            Dictionary<string, List<string>> classCodes = new Dictionary<string, List<string>>();
            Query classQuery = DB.Collection("classes").WhereEqualTo("teachId", teacherDoc.Id);
            QuerySnapshot querySnapshot = await classQuery.GetSnapshotAsync();

            foreach (DocumentSnapshot document in querySnapshot.Documents)
            {
                string classCode = document.GetValue<string>("classCode");
                string className = document.GetValue<string>("className");
                string classLevel = document.GetValue<string>("classLevel");
                classCodes[classCode] = new List<string> { classLevel, className };
            }

            TeacherModel teacherData = new TeacherModel
            {
                userId = teacherDoc.ContainsField("userId") ? teacherDoc.GetValue<string>("userId") : "",
                teachId = teacherDoc.Id,
                teachFirstName = teacherDoc.ContainsField("teachFirstName") ? teacherDoc.GetValue<string>("teachFirstName") : "",
                teachLastName = teacherDoc.ContainsField("teachLastName") ? teacherDoc.GetValue<string>("teachLastName") : "",
                title = teacherDoc.ContainsField("title") ? teacherDoc.GetValue<string>("title") : "",
                teachProfileIcon = teacherDoc.ContainsField("teachProfilePic") ? teacherDoc.GetValue<string>("teachProfilePic") : "",
                classCode = classCodes
            };

            callback(teacherData);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error getting teacher data: {ex.Message}");
            callback(null);
        }
    }

    public void CreateClass(string className, string classLevel, Action<bool, string> callback)
    {
        if (CurrentUser == null)
        {
            Debug.LogError("No current user signed in.");
            callback(false, "No user signed in.");
            return;
        }

        GetTeacherData(CurrentUser.UserId, teacherData =>
        {
            if (teacherData == null)
            {
                Debug.LogError("No teacher data found for current user.");
                callback(false, "No teacher data found.");
                return;
            }

            GenerateUniqueClassCode().ContinueWithOnMainThread(codeTask =>
            {
                if (codeTask.IsCanceled || codeTask.IsFaulted)
                {
                    Debug.LogError("Failed to generate class code: " + codeTask.Exception);
                    callback(false, "Failed to generate class code. Please try again.");
                    return;
                }

                string classCode = codeTask.Result;

                var newClass = new
                {
                    className = className,
                    classLevel = classLevel,
                    classCode = classCode,
                    teachId = teacherData.teachId,
                    dateCreated = Timestamp.GetCurrentTimestamp()
                };

                DB.Collection("classes").AddAsync(newClass).ContinueWithOnMainThread(task =>
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
            });
        });
    }

    public async Task<string> GenerateUniqueClassCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new System.Random();
        string classCode;
        bool exists = true;

        // Try until a unique code is found
        do
        {
            classCode = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            // Check Firestore for existing classCode
            QuerySnapshot snapshot = await DB.Collection("classes")
                .WhereEqualTo("classCode", classCode)
                .Limit(1)
                .GetSnapshotAsync();

            exists = snapshot.Count > 0;
        }
        while (exists);

        return classCode;
    }

    public void GetStudentsInClass(string classCode, Action<List<StudentModel>> callback)
    {
        Debug.Log($"Fetching students in class: {classCode}");
        DB.Collection("students")
          .WhereEqualTo("classCode", classCode)
          .GetSnapshotAsync()
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsCanceled || task.IsFaulted)
              {
                  Debug.LogError("Failed to get students: " + task.Exception);
                  callback(null);
                  return;
              }

              QuerySnapshot snapshot = task.Result;
              List<StudentModel> students = new List<StudentModel>();

              foreach (DocumentSnapshot document in snapshot.Documents)
              {
                  StudentModel student = new StudentModel
                  {
                      studId = document.Id,
                      teachId = document.ContainsField("teachId") ? document.GetValue<string>("teachId") : "",
                      userId = document.ContainsField("userId") ? document.GetValue<string>("userId") : "",
                      studName = document.ContainsField("studName") ? document.GetValue<string>("studName") : "",
                      studProfilePic = document.ContainsField("studProfilePic") ? document.GetValue<string>("studProfilePic") : "",
                      classCode = document.ContainsField("classCode") ? document.GetValue<string>("classCode") : "",
                  };
                  Debug.Log($"Found student: {student.studName} with ID: {student.studId}");
                  students.Add(student);
              }

              callback(students);
          });
    }
}



