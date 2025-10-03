using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Firebase.Firestore;
using System.Linq;
using System;

[System.Serializable]
public class PublishedStory
{
    public string storyId;
    public string storyTitle;
    public string classCode;
    public string className;
    public string publishDate;
    
    public PublishedStory(StoryData story, string classCode, string className)
    {
        this.storyId = story.storyId;
        this.storyTitle = story.storyTitle;
        this.classCode = classCode;
        this.className = className;
        this.publishDate = System.DateTime.Now.ToString("MMM dd, yyyy");
    }
}

public class StoryManager : MonoBehaviour
{
    // Main list of stories
    public List<StoryData> allStories = new List<StoryData>();
    public List<PublishedStory> publishedStories = new List<PublishedStory>();

    public List<StoryData> stories => allStories;
    public int currentStoryIndex = -1;

    // ✅ ADD THIS EVENT at the top of StoryManager class
    public System.Action OnStoriesLoaded;

    // Firebase integration
    public bool UseFirestore { get; private set; } = false;
    public bool IsFirebaseReady => FirebaseManager.Instance != null &&
                                  FirebaseManager.Instance.CurrentUser != null;

    // Teacher-specific storage
    private string _currentTeachId;
    private TeacherModel _currentTeacher;

    // Firebase Firestore instance
    private FirebaseFirestore _firestore;

    public StoryData currentStory
    {
        get => GetCurrentStory();
        set
        {
            if (value == null)
            {
                currentStoryIndex = -1;
                return;
            }

            int idx = allStories.IndexOf(value);
            if (idx >= 0)
            {
                currentStoryIndex = idx;
            }
            else
            {
                allStories.Add(value);
                currentStoryIndex = allStories.Count - 1;
            }
        }
    }

    private static StoryManager _instance;
    public static StoryManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to find existing instance in scene
                _instance = FindFirstObjectByType<StoryManager>();

                if (_instance == null)
                {
                    // Create new instance
                    GameObject go = new GameObject("StoryManager");
                    _instance = go.AddComponent<StoryManager>();
                    DontDestroyOnLoad(go);

                    // Initialize
                    _instance.InitializeFirebaseIntegration();
                    _instance.LoadStories();
                    Debug.Log("✅ StoryManager created and initialized");
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeFirebaseIntegration();
            // ❌ REMOVED: LoadStories() from here - wait for explicit teacher context
            Debug.Log("✅ StoryManager initialized - waiting for explicit teacher context to load stories");
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"⚠️ Destroying duplicate StoryManager. Existing: {_instance.gameObject.name}, New: {gameObject.name}");
            Destroy(gameObject);
        }
    }


    private void LoadTeacherData()
    {
        if (FirebaseManager.Instance.CurrentUser == null) return;

        FirebaseManager.Instance.GetTeacherData(FirebaseManager.Instance.CurrentUser.UserId, teacher =>
        {
            if (teacher != null)
            {
                _currentTeacher = teacher;
                _currentTeachId = teacher.teachId;
                Debug.Log($"✅ Teacher data loaded: {teacher.teachFirstName} {teacher.teachLastName} (ID: {teacher.teachId})");

                // Update TeacherPrefs
                TeacherPrefs.SetString("CurrentTeachId", _currentTeachId);
                TeacherPrefs.Save();

                // ✅ Load stories AFTER teacher data is available
                LoadStories();
            }
            else
            {
                Debug.LogWarning("⚠️ No teacher data found for current user");
                // Still load stories with default teacher ID
                LoadStories();
            }
        });
    }


    // UPDATED: Initialize Firebase integration without CreatorModeService
    private void InitializeFirebaseIntegration()
    {
        if (FirebaseManager.Instance != null)
        {
            _firestore = FirebaseFirestore.DefaultInstance;
            UseFirestore = true;
            Debug.Log("✅ StoryManager: Firestore mode enabled");

            // Load teacher data if user is logged in
            if (FirebaseManager.Instance.CurrentUser != null)
            {
                LoadTeacherData();
            }
        }
        else
        {
            Debug.Log("ℹ️ StoryManager: Local JSON mode (Firebase not available)");
        }
    }

    private static void AutoCreate()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("StoryManager");
            _instance = go.AddComponent<StoryManager>();
            DontDestroyOnLoad(go);
            Debug.Log("✅ StoryManager auto-created.");
        }
    }


    public string GetCurrentTeacherId()
    {
        // Priority 1: Use loaded teacher data
        if (!string.IsNullOrEmpty(_currentTeachId))
        {
            return _currentTeachId;
        }

        // Priority 2: Use TeacherPrefs
        if (TeacherPrefs.HasKey("CurrentTeachId"))
        {
            return TeacherPrefs.GetString("CurrentTeachId");
        }

        // Priority 3: Fallback to Firebase user ID
        if (FirebaseManager.Instance?.CurrentUserData?.role == "teacher" &&
            FirebaseManager.Instance.CurrentUser != null)
        {
            return FirebaseManager.Instance.CurrentUser.UserId;
        }

        return "default";
    }

    // Get teacher-specific base directory
    private string GetTeacherBaseDirectory()
    {
        string teachId = GetCurrentTeacherId();
        string safeTeachId = string.IsNullOrEmpty(teachId) ? "default" : teachId.Replace("/", "_").Replace("\\", "_");
        return Path.Combine(Application.persistentDataPath, safeTeachId);
    }

    public bool IsCurrentUserTeacher()
    {
        return FirebaseManager.Instance?.CurrentUserData?.role == "teacher" ||
               !string.IsNullOrEmpty(_currentTeachId);
    }

    public TeacherModel GetCurrentTeacher()
    {
        return _currentTeacher;
    }

    // Add this method to StoryManager.cs
    public void ClearStoriesForNewTeacher()
    {
        allStories.Clear();
        publishedStories.Clear();
        currentStoryIndex = -1;
        currentStory = null;

        Debug.Log($"🧹 Cleared stories for teacher switch");
    }


    public void SetCurrentTeacher(string teachId)
    {
        if (!string.IsNullOrEmpty(teachId))
        {
            // Clear existing stories before switching teachers
            ClearStoriesForNewTeacher();

            _currentTeachId = teachId;
            TeacherPrefs.SetString("CurrentTeachId", teachId);
            TeacherPrefs.Save();
            Debug.Log($"✅ Current teacher set to: {teachId}");

            // ✅ Only load stories if Firebase is ready and we have a valid teacher
            if (UseFirestore && IsFirebaseReady)
            {
                LoadStories();
            }
            else
            {
                Debug.Log("ℹ️ StoryManager: Loading local stories for teacher");
                LoadStoriesLocal();
                LoadPublishedStories();
            }
        }
    }



    public void ClearCurrentTeacher()
    {
        _currentTeachId = null;
        _currentTeacher = null;
        TeacherPrefs.DeleteKey("CurrentTeachId");
        TeacherPrefs.Save();
        allStories.Clear();
        currentStoryIndex = -1;
        Debug.Log("✅ Teacher data cleared");
    }

    // --- EXISTING METHODS ---
    public StoryData GetCurrentStory()
    {
        if (currentStoryIndex >= 0 && currentStoryIndex < allStories.Count)
            return allStories[currentStoryIndex];
        return null;
    }

    public bool SetCurrentStoryByIndex(int index)
    {
        if (index >= 0 && index < allStories.Count)
        {
            currentStoryIndex = index;
            Debug.Log($"📖 Current story set by index: {allStories[index].storyTitle}");
            return true;
        }
        Debug.LogWarning($"SetCurrentStoryByIndex failed: index {index} out of range (count={allStories.Count})");
        return false;
    }

    public void SetCurrentStory(StoryData story)
    {
        if (story == null)
        {
            Debug.LogError("⚠ Tried to set a null story as current!");
            currentStoryIndex = -1;
            return;
        }

        int idx = allStories.IndexOf(story);
        if (idx >= 0)
        {
            currentStoryIndex = idx;
        }
        else
        {
            allStories.Add(story);
            currentStoryIndex = allStories.Count - 1;
        }

        Debug.Log($"📖 Current story set to: {story.storyTitle}");
    }

    public StoryData CreateNewStory(string title = null)
    {
        var s = new StoryData();

        if (!string.IsNullOrEmpty(title))
            s.storyTitle = title;
        else
            s.storyTitle = "";

        allStories.Add(s);
        currentStoryIndex = allStories.Count - 1;

        SaveStories(); // ✅ Save immediately

        Debug.Log($"✅ Created new story: '{(string.IsNullOrEmpty(s.storyTitle) ? "<empty>" : s.storyTitle)}' (Index: {currentStoryIndex})");
        return s;
    }

    public void DebugCurrentState()
    {
        Debug.Log("🔍 === StoryManager Debug Info ===");
        Debug.Log($"🔍 Total Stories: {allStories.Count}");
        Debug.Log($"🔍 Current Story Index: {currentStoryIndex}");
        Debug.Log($"🔍 Current Story: {(currentStory != null ? currentStory.storyTitle : "NULL")}");
        Debug.Log($"🔍 Current Teacher ID: {GetCurrentTeacherId()}");
        Debug.Log($"🔍 Using Firestore: {UseFirestore}");
        Debug.Log($"🔍 Teacher Directory: {GetTeacherBaseDirectory()}");

        for (int i = 0; i < allStories.Count; i++)
        {
            Debug.Log($"🔍 Story {i}: {allStories[i].storyTitle}");
        }
        Debug.Log("🔍 === End Debug Info ===");
    }

    public bool EnsureCurrentStoryExists()
    {
        if (currentStory == null && allStories.Count > 0)
        {
            SetCurrentStoryByIndex(0);
            Debug.Log($"⚠️ No current story set, auto-selected: {currentStory.storyTitle}");
            return true;
        }
        return currentStory != null;
    }

    // === INTEGRATED FIRESTORE METHODS ===

    // UPDATED: Save story to Firestore
    public async Task<bool> SaveStoryToFirestore(StoryData story)
    {
        try
        {
            if (!IsFirebaseReady)
            {
                Debug.LogError("❌ No user logged in, cannot save to Firestore");
                return false;
            }

            // Get teacher ID
            string teachId = GetCurrentTeacherId();
            if (string.IsNullOrEmpty(teachId))
            {
                Debug.LogError("❌ No teacher ID found, cannot save story");
                return false;
            }

            // Use the story's built-in index
            int storyIndex = story.storyIndex;
            if (storyIndex < 0)
            {
                Debug.LogWarning($"⚠️ Story '{story.storyTitle}' has invalid index: {storyIndex}. Using list position.");
                storyIndex = allStories.IndexOf(story);
                if (storyIndex == -1)
                {
                    storyIndex = allStories.Count;
                }
            }

            // Map to Firestore model
            var firestoreStory = MapToFirestoreStory(story, teachId, storyIndex);

            // Save main story document
            var storyRef = _firestore
                .Collection("createdStories")
                .Document(story.storyId);

            await storyRef.SetAsync(firestoreStory);

            // Save dialogues and questions subcollections
            await SaveDialoguesToFirestore(story.storyId, story.dialogues);
            await SaveQuestionsToFirestore(story.storyId, story.quizQuestions);

            Debug.Log($"✅ Story '{story.storyTitle}' saved to Firestore successfully with index: {storyIndex}");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to save story to Firestore: {ex.Message}");
            return false;
        }
    }

    // Update DeleteStoryFromFirestore to validate ownership
    public async Task<bool> DeleteStoryFromFirestore(string storyId)
    {
        try
        {
            if (string.IsNullOrEmpty(storyId))
            {
                Debug.LogError("Cannot delete story: storyId is null or empty");
                return false;
            }

            if (!IsFirebaseReady)
            {
                Debug.LogError("❌ No user logged in, cannot delete from Firestore");
                return false;
            }

            string currentTeachId = GetCurrentTeacherId();

            // ✅ FIXED: Verify the story belongs to current teacher before deletion
            DocumentReference storyDocRef = _firestore
                .Collection("createdStories")
                .Document(storyId);

            var storySnapshot = await storyDocRef.GetSnapshotAsync();
            if (!storySnapshot.Exists)
            {
                Debug.LogError($"❌ Story {storyId} does not exist in Firestore");
                return false;
            }

            var firestoreStory = storySnapshot.ConvertTo<StoryDataFirestore>();

            // ✅ CRITICAL: Check if the story belongs to the current teacher
            if (firestoreStory.teachId != currentTeachId)
            {
                Debug.LogError($"❌ Permission denied: Story {storyId} belongs to teacher {firestoreStory.teachId}, not current teacher {currentTeachId}");
                return false;
            }

            // Proceed with deletion
            await storyDocRef.DeleteAsync();
            Debug.Log($"✅ Story {storyId} deleted from Firestore by teacher {currentTeachId}");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to delete story from Firestore: {ex.Message}");
            return false;
        }
    }


    // === SUBCOLLECTION METHODS ===

    private async Task SaveDialoguesToFirestore(string storyId, List<DialogueLine> dialogues)
    {
        if (dialogues == null) return;

        var dialoguesRef = _firestore
            .Collection("createdStories")
            .Document(storyId)
            .Collection("dialogues");

        // Clear existing dialogues
        var existingDialogues = await dialoguesRef.GetSnapshotAsync();
        var existingDocs = existingDialogues.Documents.ToList();
        foreach (var doc in existingDocs)
        {
            await doc.Reference.DeleteAsync();
        }

        // Save new dialogues
        for (int i = 0; i < dialogues.Count; i++)
        {
            var dialogueFirestore = MapToFirestoreDialogue(dialogues[i], i);
            await dialoguesRef.AddAsync(dialogueFirestore);
        }
    }

    private async Task SaveQuestionsToFirestore(string storyId, List<Question> questions)
    {
        if (questions == null) return;

        var questionsRef = _firestore
            .Collection("createdStories")
            .Document(storyId)
            .Collection("questions");

        // Clear existing questions
        var existingQuestions = await questionsRef.GetSnapshotAsync();
        var existingDocs = existingQuestions.Documents.ToList();
        foreach (var doc in existingDocs)
        {
            await doc.Reference.DeleteAsync();
        }

        // Save new questions
        for (int i = 0; i < questions.Count; i++)
        {
            var questionFirestore = MapToFirestoreQuestion(questions[i], i);
            await questionsRef.AddAsync(questionFirestore);
        }
    }

    private async Task<List<DialogueLine>> LoadDialoguesFromFirestore(string storyId)
    {
        var dialogues = new List<DialogueLine>();

        try
        {
            var dialoguesSnapshot = await _firestore
                .Collection("createdStories")
                .Document(storyId)
                .Collection("dialogues")
                .OrderBy("orderIndex")
                .GetSnapshotAsync();

            var dialogueDocs = dialoguesSnapshot.Documents.ToList();
            foreach (var dialogueDoc in dialogueDocs)
            {
                var dialogueFirestore = dialogueDoc.ConvertTo<DialogueLineFirestore>();
                dialogues.Add(MapToUnityDialogue(dialogueFirestore));
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to load dialogues: {ex.Message}");
        }

        return dialogues;
    }

    private async Task<List<Question>> LoadQuestionsFromFirestore(string storyId)
    {
        var questions = new List<Question>();

        try
        {
            var questionsSnapshot = await _firestore
                .Collection("createdStories")
                .Document(storyId)
                .Collection("questions")
                .GetSnapshotAsync();

            var questionDocs = questionsSnapshot.Documents.ToList();
            foreach (var questionDoc in questionDocs)
            {
                var questionFirestore = questionDoc.ConvertTo<QuestionFirestore>();
                questions.Add(MapToUnityQuestion(questionFirestore));
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to load questions: {ex.Message}");
        }

        return questions;
    }

    // === MAPPING METHODS (moved from CreatorModeService) ===

    private StoryDataFirestore MapToFirestoreStory(StoryData unityStory, string teachId, int storyIndex)
    {
        return new StoryDataFirestore
        {
            storyId = unityStory.storyId,
            title = unityStory.storyTitle,
            description = unityStory.storyDescription,
            backgroundUrl = unityStory.backgroundPath,
            character1Url = unityStory.character1Path,
            character2Url = unityStory.character2Path,
            teachId = teachId,
            createdAt = string.IsNullOrEmpty(unityStory.createdAt) ?
                Timestamp.GetCurrentTimestamp() :
                ConvertToTimestamp(unityStory.createdAt),
            updatedAt = Timestamp.GetCurrentTimestamp(),
            isPublished = unityStory.assignedClasses?.Count > 0,
            assignedClasses = unityStory.assignedClasses ?? new List<string>(),
            storyIndex = storyIndex
        };
    }

    private StoryData MapToUnityStory(StoryDataFirestore firestoreStory,
                                List<DialogueLine> dialogues,
                                List<Question> questions)
    {
        return new StoryData
        {
            storyId = firestoreStory.storyId,
            storyTitle = firestoreStory.title,
            storyDescription = firestoreStory.description,
            backgroundPath = firestoreStory.backgroundUrl,
            character1Path = firestoreStory.character1Url,
            character2Path = firestoreStory.character2Url,
            assignedClasses = firestoreStory.assignedClasses ?? new List<string>(),
            dialogues = dialogues,
            quizQuestions = questions,

            // ✅ FIXED: Use the actual storyIndex from Firestore, not default 0
            storyIndex = firestoreStory.storyIndex
        };
    }


    private DialogueLineFirestore MapToFirestoreDialogue(DialogueLine unityDialogue, int orderIndex)
    {
        return new DialogueLineFirestore
        {
            characterName = unityDialogue.characterName,
            dialogueText = unityDialogue.dialogueText,
            orderIndex = orderIndex
        };
    }

    private DialogueLine MapToUnityDialogue(DialogueLineFirestore firestoreDialogue)
    {
        return new DialogueLine(
            firestoreDialogue.characterName,
            firestoreDialogue.dialogueText
        );
    }

    private QuestionFirestore MapToFirestoreQuestion(Question unityQuestion, int orderIndex)
    {
        return new QuestionFirestore
        {
            questionText = unityQuestion.questionText,
            choices = new List<string>(unityQuestion.choices),
            correctAnswerIndex = unityQuestion.correctAnswerIndex,
            orderIndex = orderIndex
        };
    }

    private Question MapToUnityQuestion(QuestionFirestore firestoreQuestion)
    {
        return new Question(
            firestoreQuestion.questionText,
            firestoreQuestion.choices.ToArray(),
            firestoreQuestion.correctAnswerIndex
        );
    }

    // === UTILITY METHODS ===

    private Timestamp ConvertToTimestamp(string dateString)
    {
        if (DateTime.TryParse(dateString, out DateTime date))
        {
            return Timestamp.FromDateTime(date);
        }
        return Timestamp.GetCurrentTimestamp();
    }

    // --- UPDATED SAVE & LOAD STORIES (TEACHER-SPECIFIC) ---
    public async void SaveStories()
    {
        // Always save locally for backup
        SaveStoriesLocal();

        Debug.Log("💾 Stories saved locally - Firestore save requires explicit 'Save & Publish'");
    }

    private bool _isLoading = false;

    public async void LoadStories()
    {
        // Prevent double loading
        if (_isLoading)
        {
            Debug.Log("⚠️ LoadStories already in progress, skipping...");
            return;
        }

        _isLoading = true;

        try
        {
            // Try Firestore first if available
            if (UseFirestore)
            {
                bool success = await LoadStoriesFromFirestoreAsync();
                if (success)
                {
                    Debug.Log("✅ Stories loaded from Firestore");
                    LoadPublishedStories();
                    OnStoriesLoaded?.Invoke();
                    _isLoading = false;
                    return;
                }
            }

            // Fallback to local loading
            LoadStoriesLocal();
            LoadPublishedStories();
            OnStoriesLoaded?.Invoke();
        }
        finally
        {
            _isLoading = false;
        }
    }

    // In LoadStoriesFromFirestoreAsync method, ensure the query includes teachId filter
    private async Task<bool> LoadStoriesFromFirestoreAsync()
    {
        try
        {
            if (!IsFirebaseReady)
            {
                Debug.LogError("❌ No user logged in, cannot load from Firestore");
                return false;
            }

            // Get current teacher ID
            string currentTeachId = GetCurrentTeacherId();
            if (string.IsNullOrEmpty(currentTeachId))
            {
                Debug.LogError("❌ No teacher ID found, cannot load stories");
                return false;
            }

            Debug.Log($"🔍 Loading stories for teacher: {currentTeachId}");

            // ✅ FIXED: Query stories ONLY for the current teacher
            var storiesQuery = _firestore
                .Collection("createdStories")
                .WhereEqualTo("teachId", currentTeachId); // This is the key filter

            var snapshot = await storiesQuery.GetSnapshotAsync();

            var loadedStories = new List<StoryData>();
            var documents = snapshot.Documents.ToList();

            Debug.Log($"📚 Found {documents.Count} stories in Firestore for teacher {currentTeachId}");

            foreach (var storyDoc in documents)
            {
                var firestoreStory = storyDoc.ConvertTo<StoryDataFirestore>();

                // ✅ DOUBLE CHECK: Ensure this story belongs to the current teacher
                if (firestoreStory.teachId != currentTeachId)
                {
                    Debug.LogWarning($"⚠️ Skipping story {firestoreStory.storyId} - belongs to teacher {firestoreStory.teachId}, not current teacher {currentTeachId}");
                    continue;
                }

                // Load dialogues and questions
                var dialogues = await LoadDialoguesFromFirestore(storyDoc.Id);
                var questions = await LoadQuestionsFromFirestore(storyDoc.Id);

                // Map back to Unity model
                var unityStory = MapToUnityStory(firestoreStory, dialogues, questions);
                loadedStories.Add(unityStory);
            }

            // Update stories list
            allStories = loadedStories;
            Debug.Log($"✅ Loaded {loadedStories.Count} stories from Firestore for teacher: {currentTeachId}");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to load stories from Firestore: {ex.Message}");
            return false;
        }
    }


    // Keep the public void method for external calls
    public async void LoadStoriesFromFirestore()
    {
        if (!IsFirebaseReady) return;

        try
        {
            bool success = await LoadStoriesFromFirestoreAsync();
            if (success)
            {
                Debug.Log("✅ Stories loaded from Firestore");
            }
            else
            {
                Debug.LogError("❌ Failed to load stories from Firestore");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to load stories from Firestore: {ex.Message}");
        }
    }


    // Teacher-specific local storage methods with proper directory structure
    private string GetTeacherStoriesFilePath()
    {
        string baseDir = GetTeacherBaseDirectory();
        string teacherStoriesDir = Path.Combine(baseDir, "TeacherStories");

        if (!Directory.Exists(teacherStoriesDir))
        {
            Directory.CreateDirectory(teacherStoriesDir);
        }

        return Path.Combine(teacherStoriesDir, "stories.json");
    }

    private string GetTeacherPublishedStoriesFilePath()
    {
        string baseDir = GetTeacherBaseDirectory();
        string teacherStoriesDir = Path.Combine(baseDir, "TeacherStories");

        if (!Directory.Exists(teacherStoriesDir))
        {
            Directory.CreateDirectory(teacherStoriesDir);
        }

        return Path.Combine(teacherStoriesDir, "published_stories.json");
    }

    private void SaveStoriesLocal()
    {
        string filePath = GetTeacherStoriesFilePath();
        string json = JsonUtility.ToJson(new StoryListWrapper { stories = allStories }, true);
        File.WriteAllText(filePath, json);
        Debug.Log($"✅ Stories saved locally for teacher: {GetCurrentTeacherId()}");
        Debug.Log($"📁 File: {filePath}");
    }

    // In LoadStoriesLocal method, add teacher validation
    private void LoadStoriesLocal()
    {
        string filePath = GetTeacherStoriesFilePath();
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            StoryListWrapper wrapper = JsonUtility.FromJson<StoryListWrapper>(json);

            // ✅ FIXED: Always create new list to ensure clean state
            allStories = wrapper.stories ?? new List<StoryData>();

            Debug.Log($"📚 Loaded {allStories.Count} local stories for teacher: {GetCurrentTeacherId()}");
            Debug.Log($"📁 File: {filePath}");
        }
        else
        {
            allStories = new List<StoryData>();
            Debug.Log($"❌ No saved stories found for teacher: {GetCurrentTeacherId()}, starting fresh.");
            Debug.Log($"📁 Expected file: {filePath}");
        }
    }

    

    [System.Serializable]
    private class StoryListWrapper
    {
        public List<StoryData> stories;
    }

    // --- UPDATED BACKGROUND HANDLING (COMPATIBLE WITH RELATIVE PATHS) ---
    public string SaveBackground(Texture2D tex, string storyId = null)
    {
        // Let ImageStorage handle the saving with relative paths
        if (tex == null) return string.Empty;

        // Ensure we have a current story
        var story = GetCurrentStory();
        if (story == null)
        {
            Debug.LogError("❌ No current story to save background to");
            return string.Empty;
        }

        // Use ImageStorage to save with relative paths
        ImageStorage.UploadedTexture = tex;
        ImageStorage.SaveCurrentImageToStory();

        return story.backgroundPath; // This will now be a relative path
    }


    public Texture2D LoadBackground(string relativePath)
    {
        // Use ImageStorage to load from relative paths
        return ImageStorage.LoadImage(relativePath);
    }


    // Update the DebugFileStructure method to show both absolute and relative paths:
    [ContextMenu("Debug File Structure")]
    public void DebugFileStructure()
    {
        string baseDir = Application.persistentDataPath;
        Debug.Log($"📁 FILE STRUCTURE FOR: {GetCurrentTeacherId()}");
        Debug.Log($"📁 Persistent Data Path: {baseDir}");

        if (Directory.Exists(baseDir))
        {
            string[] directories = Directory.GetDirectories(baseDir);
            foreach (string dir in directories)
            {
                Debug.Log($"   📂 {Path.GetFileName(dir)}/");
                string[] files = Directory.GetFiles(dir);
                foreach (string file in files)
                {
                    Debug.Log($"      📄 {Path.GetFileName(file)}");
                }

                // Also show subdirectories for story images
                string[] subDirs = Directory.GetDirectories(dir);
                foreach (string subDir in subDirs)
                {
                    Debug.Log($"      📂 {Path.GetFileName(subDir)}/");
                    string[] subFiles = Directory.GetFiles(subDir);
                    foreach (string subFile in subFiles)
                    {
                        Debug.Log($"         📄 {Path.GetFileName(subFile)}");
                    }
                }
            }
        }
        else
        {
            Debug.Log("   📁 Directory does not exist yet");
        }

        // Also debug the stories and their relative paths
        Debug.Log("🔍 STORY PATHS DEBUG:");
        for (int i = 0; i < allStories.Count; i++)
        {
            var story = allStories[i];
            if (story != null)
            {
                Debug.Log($"Story {i}: '{story.storyTitle}'");
                Debug.Log($"  Background: {story.backgroundPath} (Exists: {ImageStorage.ImageExists(story.backgroundPath)})");
                Debug.Log($"  Char1: {story.character1Path} (Exists: {ImageStorage.ImageExists(story.character1Path)})");
                Debug.Log($"  Char2: {story.character2Path} (Exists: {ImageStorage.ImageExists(story.character2Path)})");
            }
        }
    }

    // Add this method to StoryManager.cs
    // Add these methods to your StoryManager.cs
    [ContextMenu("Migrate to Relative Paths")]
    public void MigrateToRelativePaths()
    {
        Debug.Log("🚀 Starting automatic path migration...");
        var result = PathMigrationTool.MigrateAllStoriesToRelativePaths();

        Debug.Log($"🎉 Migration Results:");
        Debug.Log($"   Stories Processed: {result.storiesProcessed}");
        Debug.Log($"   Paths Migrated: {result.pathsMigrated}");
        Debug.Log($"   Errors: {result.errors}");

        foreach (string migratedFile in result.migratedFiles)
        {
            Debug.Log($"   📄 {migratedFile}");
        }

        // Validate the migration
        PathMigrationTool.ValidateMigration();
    }


    [ContextMenu("Validate Path Migration")]
    public void ValidatePathMigration()
    {
        PathMigrationTool.ValidateMigration();
    }


    [ContextMenu("Check if Migration Needed")]
    public void CheckMigrationNeeded()
    {
        bool needed = PathMigrationTool.CheckIfMigrationNeeded();
        if (needed)
        {
            Debug.Log("🔄 Migration is needed - some paths are still absolute");
        }
        else
        {
            Debug.Log("✅ All paths are already relative - no migration needed");
        }
    }



    // --- PUBLISHED STORIES (UPDATED FOR TEACHER-SPECIFIC STORAGE) ---
    public bool PublishStory(StoryData story, string classCode, string className)
    {
        // Prevent duplicate by storyId and classCode
        var existing = publishedStories.Find(p => p.storyId == story.storyId && p.classCode == classCode);
        if (existing != null)
        {
            Debug.Log($"Story '{story.storyTitle}' is already published to class {className}");
            return false;
        }

        // Prevent duplicate by title and classCode (optional)
        var existingTitle = publishedStories.Find(p => p.storyTitle == story.storyTitle && p.classCode == classCode);
        if (existingTitle != null)
        {
            Debug.Log($"A story with the title '{story.storyTitle}' is already published to class {className}");
            return false;
        }

        var publishedStory = new PublishedStory(story, classCode, className);
        publishedStories.Add(publishedStory);
        SavePublishedStories();

        Debug.Log($"Published story '{story.storyTitle}' to class {className}");
        return true;
    }

    public void DeletePublishedStory(string storyId, string classCode)
    {
        publishedStories.RemoveAll(p => p.storyId == storyId && p.classCode == classCode);
        SavePublishedStories();
        Debug.Log($"Deleted published story with ID: {storyId}");
    }

    public List<PublishedStory> GetPublishedStoriesForClass(string classCode)
    {
        return publishedStories.FindAll(p => p.classCode == classCode);
    }

    private void SavePublishedStories()
    {
        string filePath = GetTeacherPublishedStoriesFilePath();
        string json = JsonUtility.ToJson(new PublishedStoryWrapper { publishedStories = publishedStories }, true);
        File.WriteAllText(filePath, json);
        Debug.Log($"Published stories saved for teacher: {GetCurrentTeacherId()}");
        Debug.Log($"📁 File: {filePath}");
    }

    private void LoadPublishedStories()
    {
        string filePath = GetTeacherPublishedStoriesFilePath();
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            PublishedStoryWrapper wrapper = JsonUtility.FromJson<PublishedStoryWrapper>(json);
            publishedStories = wrapper.publishedStories ?? new List<PublishedStory>();
            Debug.Log($"Loaded {publishedStories.Count} published stories for teacher: {GetCurrentTeacherId()}");
            Debug.Log($"📁 File: {filePath}");
        }
        else
        {
            publishedStories = new List<PublishedStory>();
            Debug.Log($"No published stories found for teacher: {GetCurrentTeacherId()}");
            Debug.Log($"📁 Expected file: {filePath}");
        }
    }

    [System.Serializable]
    private class PublishedStoryWrapper
    {
        public List<PublishedStory> publishedStories;
    }

    // UPDATED: Explicit Firestore operations
    public async void SaveCurrentStoryToFirestore()
    {
        if (!IsFirebaseReady || currentStory == null) return;

        try
        {
            bool success = await SaveStoryToFirestore(currentStory);
            if (success)
            {
                Debug.Log($"✅ Story '{currentStory.storyTitle}' saved to Firestore");
            }
            else
            {
                Debug.LogError($"❌ Failed to save story to Firestore");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to save story to Firestore: {ex.Message}");
        }
    }

    [ContextMenu("Debug Local Stories File")]
    public void DebugLocalStoriesFile()
    {
        string filePath = GetTeacherStoriesFilePath();
        Debug.Log($"🔍 Checking local stories file: {filePath}");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            Debug.Log($"📄 Local stories file content: {json}");
        }
        else
        {
            Debug.Log("❌ Local stories file does not exist");
        }
    }
}
