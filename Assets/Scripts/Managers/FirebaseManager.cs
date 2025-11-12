using Firebase.Auth;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

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

    public void SignIn(string email, string password, Action<bool, string> callback)
    {
        AuthService?.SignIn(email, password, callback);
    }

    // Original SignUp method without code parameter
    public void SignUp(string email, string password, string displayName, bool isTeacher, Action<bool, string> callback)
    {
        AuthService?.SignUp(email, password, displayName, isTeacher, callback);
    }

    // New SignUp method with code parameter
    public void SignUp(string email, string password, string displayName, bool isTeacher, string code, Action<bool, string> callback)
    {
        AuthService?.SignUp(email, password, displayName, isTeacher, code, callback);
    }

    // Code validation methods
    public void ValidateTeacherCode(string code, Action<bool> callback)
    {
        if (string.IsNullOrEmpty(code))
        {
            callback?.Invoke(false);
            return;
        }

        // Check if the code exists in the teacherCodes collection
        DB.Collection("teacherCodes").Document(code).GetSnapshotAsync().ContinueWith(task =>
        {
            UnityDispatcher.RunOnMainThread(() =>
            {
                if (task.IsCompletedSuccessfully && task.Result.Exists)
                {
                    callback?.Invoke(true);
                }
                else
                {
                    callback?.Invoke(false);
                }
            });
        });
    }

    public void ValidateClassCode(string classCode, Action<bool> callback)
    {
        if (string.IsNullOrEmpty(classCode))
        {
            callback?.Invoke(false);
            return;
        }

        DB.Collection("classes")
        .WhereEqualTo("classCode", classCode)
        .Limit(1)
        .GetSnapshotAsync()
        .ContinueWith((Task<QuerySnapshot> task) =>
        {
            UnityDispatcher.RunOnMainThread(() =>
            {
                if (task.IsCompletedSuccessfully && task.Result != null && task.Result.Count > 0)
                {
                    var doc = task.Result.Documents.FirstOrDefault(); // ✅ safe access
                    if (doc != null)
                    {
                        var classData = doc.ToDictionary();
                        bool isActive = classData.ContainsKey("isActive") ? (bool)classData["isActive"] : true;
                        callback?.Invoke(isActive);
                        return;
                    }
                }

                callback?.Invoke(false);
            });
        });
    }

    public void GetClassDetails(string classCode, Action<ClassDetailsModel> callback)
    {
        if (string.IsNullOrEmpty(classCode))
        {
            callback?.Invoke(null);
            return;
        }

        DB.Collection("classes")
            .WhereEqualTo("classCode", classCode)
            .Limit(1)
            .GetSnapshotAsync()
            .ContinueWith((Task<QuerySnapshot> task) =>
            {
                UnityDispatcher.RunOnMainThread(() =>
                {
                    if (task.IsCompletedSuccessfully && task.Result != null && task.Result.Count > 0)
                    {
                        var doc = task.Result.Documents.FirstOrDefault();
                        if (doc != null)
                        {
                            var classData = doc.ToDictionary();
                            string teacherId = classData.ContainsKey("teachId") ? classData["teachId"].ToString() : "";

                            // If we have a teacherId, fetch the teacher's name
                            if (!string.IsNullOrEmpty(teacherId))
                            {
                                // Use TeacherService to get teacher data
                                TeacherService.GetTeacherData(teacherId, (teacherModel) =>
                                {
                                    string teacherName = "Unknown Teacher";

                                    if (teacherModel != null)
                                    {
                                        // Use the teacher model properties directly
                                        teacherName = $"{teacherModel.teachFirstName} {teacherModel.teachLastName}".Trim();

                                        if (string.IsNullOrEmpty(teacherName))
                                            teacherName = "Unknown Teacher";
                                    }
                                    else
                                    {
                                        Debug.LogError($"❌ Teacher data not found for teacherId: {teacherId}");
                                    }

                                    var classDetails = new ClassDetailsModel
                                    {
                                        classCode = classCode,
                                        className = classData.ContainsKey("className") ? classData["className"].ToString() : "Unknown Class",
                                        classLevel = classData.ContainsKey("classLevel") ? classData["classLevel"].ToString() : "Unknown Level",
                                        teacherName = teacherName,
                                        isActive = classData.ContainsKey("isActive") ? (bool)classData["isActive"] : true
                                    };

                                    callback?.Invoke(classDetails);
                                });
                            }
                            else
                            {
                                // No teacherId found
                                Debug.LogError($"❌ No teachId found in class document for class: {classCode}");

                                var classDetails = new ClassDetailsModel
                                {
                                    classCode = classCode,
                                    className = classData.ContainsKey("className") ? classData["className"].ToString() : "Unknown Class",
                                    classLevel = classData.ContainsKey("classLevel") ? classData["classLevel"].ToString() : "Unknown Level",
                                    teacherName = "Unknown Teacher",
                                    isActive = classData.ContainsKey("isActive") ? (bool)classData["isActive"] : true
                                };

                                callback?.Invoke(classDetails);
                            }
                            return;
                        }
                    }

                    Debug.LogError($"❌ Class document not found for class code: {classCode}");
                    callback?.Invoke(null);
                });
            });
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

    public void GetStudentLeaderboard(string classCode, Action<List<LeaderboardStudentModel>> callback)
    {
        StudentService?.GetStudentLeaderboard(classCode, callback);
    }

    public void MarkStudentAsRemoved(string userId, string classCode, Action<bool> callback)
    {
        ClassService?.MarkStudentAsRemoved(userId, classCode, callback);
    }

    public IFirebaseService GetFirebaseService()
    {
        return _firebaseService;
    }

    // Add these methods to your existing FirebaseManager class

public void GetAllChapters(Action<Dictionary<string, Dictionary<string, object>>> callback)
{
    DB.Collection("chapters").GetSnapshotAsync().ContinueWith(task =>
    {
        UnityDispatcher.RunOnMainThread(() =>
        {
            if (task.IsCompletedSuccessfully && task.Result != null)
            {
                var chapters = new Dictionary<string, Dictionary<string, object>>();
                foreach (var document in task.Result.Documents)
                {
                    chapters[document.Id] = document.ToDictionary();
                }
                callback?.Invoke(chapters);
            }
            else
            {
                Debug.LogError("Failed to load chapters: " + task.Exception);
                callback?.Invoke(null);
            }
        });
    });
}

public void GetStoriesByChapter(string chapterId, Action<Dictionary<string, Dictionary<string, object>>> callback)
{
    DocumentReference chapterRef = DB.Collection("chapters").Document(chapterId);
    
    DB.Collection("stories")
        .WhereEqualTo("chapter", chapterRef)
        .GetSnapshotAsync().ContinueWith(task =>
        {
            UnityDispatcher.RunOnMainThread(() =>
            {
                if (task.IsCompletedSuccessfully && task.Result != null)
                {
                    var stories = new Dictionary<string, Dictionary<string, object>>();
                    foreach (var document in task.Result.Documents)
                    {
                        stories[document.Id] = document.ToDictionary();
                    }
                    callback?.Invoke(stories);
                }
                else
                {
                    Debug.LogError($"Failed to load stories for chapter {chapterId}: " + task.Exception);
                    callback?.Invoke(null);
                }
            });
        });
}

    public void GetQuizAttemptsByStudentAndStory(string studentStudId, string storyId, Action<Dictionary<string, Dictionary<string, object>>> callback)
    {
        Debug.Log($"🔍 Querying quiz attempts for student studId: {studentStudId}, story: {storyId}");

        // First, we need to get the chapter ID for this story to build the correct quizId
        DB.Collection("stories").Document(storyId).GetSnapshotAsync().ContinueWith(storyTask =>
        {
            UnityDispatcher.RunOnMainThread(() =>
            {
                if (storyTask.IsCompletedSuccessfully && storyTask.Result.Exists)
                {
                    var storyData = storyTask.Result.ToDictionary();
                    DocumentReference chapterRef = storyData["chapter"] as DocumentReference;
                    string chapterId = chapterRef?.Id;

                    if (!string.IsNullOrEmpty(chapterId))
                    {
                        // Build the correct quizId format: "CH001_ST002"
                        string correctQuizId = $"{chapterId}_{storyId}";
                        Debug.Log($"🎯 Built correct quizId: {correctQuizId}");

                        // Query the subcollection: quizAttempts/{studId}/attempts/
                        DB.Collection("quizAttempts")
                            .Document(studentStudId)
                            .Collection("attempts")
                            .WhereEqualTo("quizId", correctQuizId)
                            .GetSnapshotAsync().ContinueWith(attemptsTask =>
                            {
                                UnityDispatcher.RunOnMainThread(() =>
                                {
                                    if (attemptsTask.IsCompletedSuccessfully && attemptsTask.Result != null)
                                    {
                                        var attempts = new Dictionary<string, Dictionary<string, object>>();
                                        foreach (var document in attemptsTask.Result.Documents)
                                        {
                                            var data = document.ToDictionary();
                                            attempts[document.Id] = data;

                                            Debug.Log($"✅ Found quiz attempt: {document.Id}");
                                            Debug.Log($"   - quizId: {data.GetValueOrDefault("quizId", "N/A")}");
                                            Debug.Log($"   - attemptNumber: {data.GetValueOrDefault("attemptNumber", "N/A")}");
                                            Debug.Log($"   - score: {data.GetValueOrDefault("score", "N/A")}");
                                            Debug.Log($"   - isPassed: {data.GetValueOrDefault("isPassed", "N/A")}");
                                        }
                                        Debug.Log($"📊 Found {attempts.Count} quiz attempts for studId {studentStudId} and quizId {correctQuizId}");
                                        callback?.Invoke(attempts);
                                    }
                                    else if (attemptsTask.IsFaulted)
                                    {
                                        Debug.LogError($"❌ Failed to load quiz attempts: {attemptsTask.Exception}");
                                        callback?.Invoke(new Dictionary<string, Dictionary<string, object>>());
                                    }
                                    else
                                    {
                                        Debug.Log($"❌ No quiz attempts found in subcollection for studId {studentStudId} and quizId {correctQuizId}");
                                        callback?.Invoke(new Dictionary<string, Dictionary<string, object>>());
                                    }
                                });
                            });
                    }
                    else
                    {
                        Debug.LogError($"❌ Could not get chapter ID for story {storyId}");
                        callback?.Invoke(new Dictionary<string, Dictionary<string, object>>());
                    }
                }
                else
                {
                    Debug.LogError($"❌ Could not load story document {storyId}");
                    callback?.Invoke(new Dictionary<string, Dictionary<string, object>>());
                }
            });
        });
    }

    // Add these methods to FirebaseManager.cs

public void GetPublishedStoriesByClass(string classCode, Action<Dictionary<string, Dictionary<string, object>>> callback)
{
    Debug.Log($"🔍 Querying published stories for class: {classCode}");
    
    DB.Collection("createdStories")
        .WhereEqualTo("isPublished", true)
        .WhereArrayContains("assignedClasses", classCode)
        .GetSnapshotAsync().ContinueWith(task =>
        {
            UnityDispatcher.RunOnMainThread(() =>
            {
                if (task.IsCompletedSuccessfully && task.Result != null)
                {
                    var stories = new Dictionary<string, Dictionary<string, object>>();
                    foreach (var document in task.Result.Documents)
                    {
                        stories[document.Id] = document.ToDictionary();
                        Debug.Log($"✅ Found published story: {document.Id} - {document.GetValue<string>("title")}");
                    }
                    Debug.Log($"📊 Returning {stories.Count} published stories for class {classCode}");
                    callback?.Invoke(stories);
                }
                else
                {
                    Debug.LogError($"❌ Failed to load published stories: {task.Exception}");
                    callback?.Invoke(null);
                }
            });
        });
}

    public void GetPublishedStoryQuizAttempts(string storyId, string studentStudId, Action<Dictionary<string, Dictionary<string, object>>> callback)
    {
        Debug.Log($"🔍 Fetching quiz attempts for published storyId: {storyId}, student: {studentStudId}");

        DB.Collection("createdStories")
            .Document(storyId)
            .Collection("quizAttempts")
            .Document(studentStudId)
            .Collection("attempts")
            .GetSnapshotAsync()
            .ContinueWith(task =>
            {
                UnityDispatcher.RunOnMainThread(() =>
                {
                    var attempts = new Dictionary<string, Dictionary<string, object>>();

                    if (task.IsCompletedSuccessfully && task.Result != null)
                    {
                        foreach (var doc in task.Result.Documents)
                        {
                            var data = doc.ToDictionary();
                            attempts[doc.Id] = data;

                            Debug.Log($"✅ Found quiz attempt for story {storyId}: {doc.Id}");
                            Debug.Log($"   - quizId: {data.GetValueOrDefault("quizId", "N/A")}");
                            Debug.Log($"   - attemptNumber: {data.GetValueOrDefault("attemptNumber", "N/A")}");
                            Debug.Log($"   - score: {data.GetValueOrDefault("score", "N/A")}");
                            Debug.Log($"   - isPassed: {data.GetValueOrDefault("isPassed", "N/A")}");
                        }
                        Debug.Log($"📊 Total {attempts.Count} quiz attempts for story {storyId}");
                    }
                    else if (task.IsFaulted)
                    {
                        Debug.LogError($"❌ Error fetching quiz attempts for story {storyId}: {task.Exception}");
                    }
                    else
                    {
                        Debug.Log($"ℹ️ No quiz attempts found for story {storyId}");
                    }

                    callback?.Invoke(attempts);
                });
            });
    }

    public void UpdateStudentName(string userId, string newName, System.Action<bool> callback)
{
    if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(newName))
    {
        Debug.LogError("❌ UpdateStudentName: Invalid parameters");
        UnityDispatcher.RunOnMainThread(() => callback?.Invoke(false));
        return;
    }

    Debug.Log($"💾 Updating student name for userId: {userId} to: {newName}");

    // First, get the student document by querying the students collection with userId
    DB.Collection("students")
        .WhereEqualTo("userId", userId)
        .GetSnapshotAsync()
        .ContinueWith(task =>
        {
            UnityDispatcher.RunOnMainThread(() =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError($"❌ Failed to find student: {task.Exception}");
                    callback?.Invoke(false);
                    return;
                }

                var snapshot = task.Result;
                var documents = snapshot.Documents.ToList(); // Convert to List for indexing

                if (documents.Count == 0)
                {
                    Debug.LogError("❌ No student document found for userId");
                    callback?.Invoke(false);
                    return;
                }

                // Get the student document and its data
                var studentDoc = documents[0];
                string studentDocId = studentDoc.Id;
                var studentData = studentDoc.ToDictionary();
                string classCode = studentData.ContainsKey("classCode") ? studentData["classCode"].ToString() : null;

                Debug.Log($"✅ Found student document: {studentDocId}, classCode: {classCode}");

                // Create a batch write to update all collections atomically
                var batch = DB.StartBatch();

                // 1. Update studName in students collection
                var studentRef = DB.Collection("students").Document(studentDocId);
                batch.Update(studentRef, new Dictionary<string, object>
                {
                    { "studName", newName },
                    { "dateUpdated", FieldValue.ServerTimestamp }
                });

                // 2. Update displayName in userAccounts collection
                var userRef = DB.Collection("userAccounts").Document(userId);
                batch.Update(userRef, new Dictionary<string, object>
                {
                    { "displayName", newName }
                });

                // 3. Update displayName in studentLeaderboards collection (if exists)
                // Note: studentLeaderboards uses studId (student doc ID) as the document ID
                if (!string.IsNullOrEmpty(classCode))
                {
                    // Use the student document ID directly since that's what studentLeaderboards uses
                    var leaderboardRef = DB.Collection("studentLeaderboards").Document(studentDocId);
                    
                    // Check if the leaderboard document exists for this class
                    leaderboardRef.GetSnapshotAsync().ContinueWith(leaderboardTask =>
                    {
                        UnityDispatcher.RunOnMainThread(() =>
                        {
                            if (leaderboardTask.IsFaulted || leaderboardTask.IsCanceled)
                            {
                                Debug.LogWarning($"⚠️ Could not check leaderboard: {leaderboardTask.Exception}");
                                // Continue anyway - don't fail the whole operation
                                CommitBatch(batch, callback);
                                return;
                            }

                            var leaderboardDoc = leaderboardTask.Result;
                            
                            if (leaderboardDoc.Exists)
                            {
                                var leaderboardData = leaderboardDoc.ToDictionary();
                                string leaderboardClassCode = leaderboardData.ContainsKey("classCode") 
                                    ? leaderboardData["classCode"].ToString() 
                                    : null;
                                
                                // Verify this leaderboard entry is for the correct class
                                if (leaderboardClassCode == classCode)
                                {
                                    batch.Update(leaderboardRef, new Dictionary<string, object>
                                    {
                                        { "displayName", newName },
                                        { "dateUpdated", FieldValue.ServerTimestamp }
                                    });

                                    Debug.Log($"✅ Added leaderboard update to batch for studId: {studentDocId}");
                                }
                                else
                                {
                                    Debug.LogWarning($"⚠️ Leaderboard class mismatch. Expected: {classCode}, Found: {leaderboardClassCode}");
                                }
                            }
                            else
                            {
                                Debug.Log($"ℹ️ No leaderboard entry found for studId: {studentDocId} in class {classCode}");
                            }

                            // Commit the batch with all updates
                            CommitBatch(batch, callback);
                        });
                    });
                }
                else
                {
                    // No class code, just commit what we have
                    Debug.LogWarning("⚠️ No classCode found, skipping leaderboard update");
                    CommitBatch(batch, callback);
                }
            });
        });
}

    // Helper method to commit the batch
    private void CommitBatch(WriteBatch batch, System.Action<bool> callback)
    {
        batch.CommitAsync().ContinueWith(commitTask =>
        {
            UnityDispatcher.RunOnMainThread(() =>
            {
                if (commitTask.IsFaulted || commitTask.IsCanceled)
                {
                    Debug.LogError($"❌ Failed to update student name: {commitTask.Exception}");
                    callback?.Invoke(false);
                }
                else
                {
                    Debug.Log($"✅ Student name updated successfully in all collections");
                    callback?.Invoke(true);
                }
            });
        });
    }

    public void GetStudentByUserId(string userId, System.Action<StudentModel> callback)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("❌ GetStudentByUserId: Invalid userId");
            UnityDispatcher.RunOnMainThread(() => callback?.Invoke(null));
            return;
        }

        DB.Collection("students")
            .WhereEqualTo("userId", userId)
            .GetSnapshotAsync()
            .ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError($"❌ Failed to get student: {task.Exception}");
                    UnityDispatcher.RunOnMainThread(() => callback?.Invoke(null));
                    return;
                }

                var snapshot = task.Result;
                var documents = snapshot.Documents.ToList();

                if (documents.Count == 0)
                {
                    Debug.LogWarning($"⚠️ No student found for userId: {userId}");
                    UnityDispatcher.RunOnMainThread(() => callback?.Invoke(null));
                    return;
                }

                var studentDoc = documents[0];
                var studentData = studentDoc.ConvertTo<StudentModel>();
                studentData.studId = studentDoc.Id;

                Debug.Log($"✅ Found student: {studentData.studName}");
                UnityDispatcher.RunOnMainThread(() => callback?.Invoke(studentData));
            });
    }

}
