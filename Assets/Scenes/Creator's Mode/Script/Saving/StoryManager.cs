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

    // Add parameterless constructor for Firestore
    public PublishedStory() { }

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

    // Firebase integration
    public bool UseFirestore { get; private set; } = false;

    // Add this property to StoryManager.cs for easier access
    public bool IsFirebaseReady => FirebaseManager.Instance != null && 
                              FirebaseManager.Instance.CurrentUser != null &&
                              _firestore != null;

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
            // ✅ IMMEDIATE RETURN NULL FOR STUDENTS
            if (FirebaseManager.Instance?.CurrentUserData?.role == "student")
            {
                return null;
            }

            if (_instance == null)
            {
                // Try to find existing instance in scene
                _instance = FindFirstObjectByType<StoryManager>();

                if (_instance == null)
                {
                    // Create new instance ONLY for teachers
                    GameObject go = new GameObject("StoryManager");
                    _instance = go.AddComponent<StoryManager>();
                    DontDestroyOnLoad(go);

                    // Initialize
                    _instance.InitializeFirebaseIntegration();
                    _instance.LoadStories();
                    Debug.Log("✅ StoryManager created and initialized for teacher");
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

            // ✅ CHECK IF USER IS TEACHER BEFORE INITIALIZING
            if (FirebaseManager.Instance?.CurrentUserData?.role == "teacher")
            {
                InitializeFirebaseIntegration();
                Debug.Log("✅ StoryManager initialized for teacher");
            }
            else
            {
                Debug.Log("ℹ️ StoryManager: Skipping initialization for student user");
                // Don't initialize Firebase integration for students
            }
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

        // ✅ EXTRA SAFETY CHECK: Only load teacher data for actual teachers
        if (FirebaseManager.Instance.CurrentUserData?.role != "teacher")
        {
            Debug.Log("ℹ️ LoadTeacherData: Skipping - user is not a teacher");
            return;
        }

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

            // ✅ CHECK USER ROLE BEFORE LOADING TEACHER DATA
            if (FirebaseManager.Instance.CurrentUser != null)
            {
                // Only load teacher data if the current user is actually a teacher
                if (FirebaseManager.Instance.CurrentUserData?.role == "teacher")
                {
                    LoadTeacherData();
                }
                else
                {
                    Debug.Log("ℹ️ StoryManager: Skipping teacher data load for student user");
                    // Students don't need to load stories, so we're done here
                }
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

    public StoryData CreateNewStory(string title = null, int? forceSlotIndex = null)
    {
        int targetIndex;

        // If a specific slot was requested (from StorySelector), use it
        if (forceSlotIndex.HasValue)
        {
            targetIndex = forceSlotIndex.Value;
        }
        else
        {
            // Find the first null slot
            targetIndex = -1;
            for (int i = 0; i < allStories.Count && i < 6; i++)
            {
                if (allStories[i] == null)
                {
                    targetIndex = i;
                    break;
                }
            }

            // If no null slots, check if we can add more
            if (targetIndex == -1 && allStories.Count < 6)
            {
                targetIndex = allStories.Count;
            }
        }

        if (targetIndex < 0 || targetIndex >= 6)
        {
            Debug.LogError("Maximum 6 stories reached or invalid slot!");
            return null;
        }

        var s = new StoryData();
        s.storyIndex = targetIndex;

        if (!string.IsNullOrEmpty(title))
            s.storyTitle = title;
        else
            s.storyTitle = "";

        // Ensure list is large enough
        while (allStories.Count <= targetIndex)
        {
            allStories.Add(null);
        }

        // Place story in its slot
        allStories[targetIndex] = s;
        currentStoryIndex = targetIndex;

        SaveStories();

        Debug.Log($"Created new story in slot {targetIndex}: '{(string.IsNullOrEmpty(s.storyTitle) ? "<empty>" : s.storyTitle)}'");
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

            if (firestoreStory.teachId != currentTeachId)
            {
                Debug.LogError($"❌ Permission denied: Story {storyId} belongs to teacher {firestoreStory.teachId}");
                return false;
            }

            // ✅ DELETE DIALOGUES SUBCOLLECTION
            var dialoguesRef = storyDocRef.Collection("dialogues");
            var dialoguesSnapshot = await dialoguesRef.GetSnapshotAsync();
            foreach (var dialogueDoc in dialoguesSnapshot.Documents)
            {
                await dialogueDoc.Reference.DeleteAsync();
            }
            Debug.Log($"🗑️ Deleted {dialoguesSnapshot.Count} dialogues for story {storyId}");

            // ✅ DELETE QUESTIONS SUBCOLLECTION
            var questionsRef = storyDocRef.Collection("questions");
            var questionsSnapshot = await questionsRef.GetSnapshotAsync();
            foreach (var questionDoc in questionsSnapshot.Documents)
            {
                await questionDoc.Reference.DeleteAsync();
            }
            Debug.Log($"🗑️ Deleted {questionsSnapshot.Count} questions for story {storyId}");

            // Now delete the main document
            await storyDocRef.DeleteAsync();
            Debug.Log($"✅ Story {storyId} and all subcollections deleted from Firestore");
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
    // Helper function to convert Timestamp to display string
    string ConvertTimestampToString(Timestamp timestamp)
    {
        if (timestamp != null)
        {
            try
            {
                DateTime date = timestamp.ToDateTime();
                // Store in a format that DateTime.TryParse can handle
                return date.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to convert timestamp: {ex.Message}");
            }
        }
        return "";
    }

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
        storyIndex = firestoreStory.storyIndex,

        // ✅ FIXED: Proper timestamp conversion
        createdAt = ConvertTimestampToString(firestoreStory.createdAt),
        updatedAt = ConvertTimestampToString(firestoreStory.updatedAt)
    };
}




    private DialogueLineFirestore MapToFirestoreDialogue(DialogueLine unityDialogue, int orderIndex)
    {
        return new DialogueLineFirestore(
            unityDialogue.characterName,
            unityDialogue.dialogueText,
            unityDialogue.selectedVoiceId, // Make sure this is passed
            orderIndex
        );
    }


    private DialogueLine MapToUnityDialogue(DialogueLineFirestore firestoreDialogue)
    {
        return new DialogueLine(
            firestoreDialogue.characterName,
            firestoreDialogue.dialogueText,
            firestoreDialogue.selectedVoiceId // Make sure this is passed
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

    // Add these events at the top of StoryManager class
    public System.Action OnStoriesLoadingStarted;
    public System.Action<List<StoryData>> OnStoriesLoaded; // WITH parameters
    public System.Action<string> OnStoriesLoadFailed;

    // Update the LoadStories method:
    public async void LoadStories()
    {
        // Prevent double loading
        if (_isLoading)
        {
            Debug.Log("⚠️ LoadStories already in progress, skipping...");
            return;
        }

        _isLoading = true;
        OnStoriesLoadingStarted?.Invoke();

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
                    OnStoriesLoaded?.Invoke(allStories); // PASS the stories list
                    _isLoading = false;
                    return;
                }
                else
                {
                    OnStoriesLoadFailed?.Invoke("Failed to load from Firestore");
                }
            }

            // Fallback to local loading
            LoadStoriesLocal();
            LoadPublishedStories();
            OnStoriesLoaded?.Invoke(allStories); // PASS the stories list
        }
        catch (System.Exception ex)
        {
            OnStoriesLoadFailed?.Invoke($"Load failed: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }


    private async Task<bool> LoadStoriesFromFirestoreAsync()
    {
        try
        {
            if (!IsFirebaseReady)
            {
                Debug.LogError("❌ No user logged in, cannot load from Firestore");
                return false;
            }

            string currentTeachId = GetCurrentTeacherId();
            if (string.IsNullOrEmpty(currentTeachId))
            {
                Debug.LogError("❌ No teacher ID found, cannot load stories");
                return false;
            }

            Debug.Log($"📂 Loading stories for teacher: {currentTeachId}");

            var storiesQuery = _firestore
                .Collection("createdStories")
                .WhereEqualTo("teachId", currentTeachId);

            var snapshot = await storiesQuery.GetSnapshotAsync();
            var documents = snapshot.Documents.ToList();

            Debug.Log($"📚 Found {documents.Count} stories in Firestore for teacher {currentTeachId}");

            // ✅ KEY FIX: Initialize list with nulls for all 6 slots
            var loadedStories = new List<StoryData>(6);
            for (int i = 0; i < 6; i++)
            {
                loadedStories.Add(null);
            }

            foreach (var storyDoc in documents)
            {
                var firestoreStory = storyDoc.ConvertTo<StoryDataFirestore>();

                if (firestoreStory.teachId != currentTeachId)
                {
                    Debug.LogWarning($"⚠️ Skipping story {firestoreStory.storyId}");
                    continue;
                }

                var dialogues = await LoadDialoguesFromFirestore(storyDoc.Id);
                var questions = await LoadQuestionsFromFirestore(storyDoc.Id);

                var unityStory = MapToUnityStory(firestoreStory, dialogues, questions);

                // ✅ KEY FIX: Place story in its designated slot
                int slotIndex = firestoreStory.storyIndex;
                if (slotIndex >= 0 && slotIndex < 6)
                {
                    loadedStories[slotIndex] = unityStory;
                    Debug.Log($"📍 Placed story '{unityStory.storyTitle}' in slot {slotIndex}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ Story has invalid index {slotIndex}, skipping");
                }
            }

            allStories = loadedStories;
            Debug.Log($"✅ Loaded stories into correct slots for teacher: {currentTeachId}");
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

        // ✅ Only save non-null stories
        var nonNullStories = allStories.Where(s => s != null).ToList();

        string json = JsonUtility.ToJson(new StoryListWrapper { stories = nonNullStories }, true);
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

            // ✅ Ensure we have 6 slots
            allStories = new List<StoryData>(6);
            for (int i = 0; i < 6; i++)
            {
                allStories.Add(null);
            }

            // Place stories in their correct slots based on storyIndex
            if (wrapper.stories != null)
            {
                foreach (var story in wrapper.stories)
                {
                    if (story != null && story.storyIndex >= 0 && story.storyIndex < 6)
                    {
                        allStories[story.storyIndex] = story;
                    }
                }
            }

            Debug.Log($"📚 Loaded local stories for teacher: {GetCurrentTeacherId()}");
            Debug.Log($"📁 File: {filePath}");
        }
        else
        {
            // Initialize with 6 null slots
            allStories = new List<StoryData>(6);
            for (int i = 0; i < 6; i++)
            {
                allStories.Add(null);
            }
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

    // === FIRESTORE PUBLISHING METHODS ===

    /// <summary>
    /// Safely publishes a story to a class in Firestore - handles missing stories gracefully
    /// </summary>
    public async Task<bool> PublishStoryToFirestore(string storyId, string classCode, string className)
    {
        try
        {
            if (!IsFirebaseReady)
            {
                Debug.LogWarning("⚠️ No user logged in, skipping Firestore publish");
                return false;
            }

            if (string.IsNullOrEmpty(storyId))
            {
                Debug.LogWarning("⚠️ Story ID is null or empty, skipping Firestore publish");
                return false;
            }

            Debug.Log($"📤 Publishing story {storyId} to class {classCode}");

            // Get the story document reference
            var storyRef = _firestore.Collection("createdStories").Document(storyId);
            var snapshot = await storyRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                Debug.LogWarning($"⚠️ Story {storyId} does not exist in Firestore - saving it first");

                // Try to save the story first
                var currentStory = GetCurrentStory();
                if (currentStory != null && currentStory.storyId == storyId)
                {
                    bool saveSuccess = await SaveStoryToFirestore(currentStory);
                    if (!saveSuccess)
                    {
                        Debug.LogWarning($"⚠️ Could not save story {storyId} to Firestore");
                        return false;
                    }

                    // Now get the fresh snapshot
                    snapshot = await storyRef.GetSnapshotAsync();
                    if (!snapshot.Exists)
                    {
                        Debug.LogWarning($"⚠️ Still cannot find story {storyId} after saving");
                        return false;
                    }
                }
                else
                {
                    Debug.LogWarning($"⚠️ Cannot find current story to save to Firestore");
                    return false;
                }
            }

            var firestoreStory = snapshot.ConvertTo<StoryDataFirestore>();

            // Update assigned classes
            if (firestoreStory.assignedClasses == null)
            {
                firestoreStory.assignedClasses = new List<string>();
            }

            if (!firestoreStory.assignedClasses.Contains(classCode))
            {
                firestoreStory.assignedClasses.Add(classCode);
                Debug.Log($"✅ Added class {classCode} to assigned classes");
            }

            // Update publishing status
            firestoreStory.isPublished = firestoreStory.assignedClasses.Count > 0;
            firestoreStory.updatedAt = Timestamp.GetCurrentTimestamp();

            // Save back to Firestore
            await storyRef.SetAsync(firestoreStory);

            Debug.Log($"✅ Successfully published story to class {className} ({classCode})");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"⚠️ Failed to publish story to Firestore: {ex.Message}");
            return false;
        }
    }


    /// <summary>
    /// Safely unpublishes a story from a class in Firestore - won't crash if story doesn't exist
    /// </summary>
    public async Task<bool> UnpublishStoryFromFirestore(string storyId, string classCode)
    {
        try
        {
            if (!IsFirebaseReady)
            {
                Debug.LogWarning("⚠️ No user logged in, skipping Firestore unpublish");
                return false;
            }

            if (string.IsNullOrEmpty(storyId))
            {
                Debug.LogWarning("⚠️ Story ID is null or empty, skipping Firestore unpublish");
                return false;
            }

            Debug.Log($"📤 Unpublishing story {storyId} from class {classCode}");

            // Get the story document reference
            var storyRef = _firestore.Collection("createdStories").Document(storyId);
            var snapshot = await storyRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                Debug.LogWarning($"⚠️ Story {storyId} does not exist in Firestore - this is OK for local-only stories");
                return true; // Return true because local cleanup is what matters
            }

            var firestoreStory = snapshot.ConvertTo<StoryDataFirestore>();

            // Verify ownership (optional safety check)
            string currentTeachId = GetCurrentTeacherId();
            if (firestoreStory.teachId != currentTeachId)
            {
                Debug.LogWarning($"⚠️ Permission warning: Story belongs to teacher {firestoreStory.teachId}");
                // Continue anyway - might be cleaning up old data
            }

            // Remove class from assigned classes
            if (firestoreStory.assignedClasses != null && firestoreStory.assignedClasses.Contains(classCode))
            {
                firestoreStory.assignedClasses.Remove(classCode);
                Debug.Log($"✅ Removed class {classCode} from assigned classes");
            }

            // Update publishing status
            firestoreStory.isPublished = firestoreStory.assignedClasses?.Count > 0;
            firestoreStory.updatedAt = Timestamp.GetCurrentTimestamp();

            // Save back to Firestore
            await storyRef.SetAsync(firestoreStory);

            Debug.Log($"✅ Successfully unpublished story from class {classCode}");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"⚠️ Failed to unpublish story from Firestore: {ex.Message} - continuing with local cleanup");
            return false; // Don't throw, just log and continue
        }
    }

    // === FIRESTORE PUBLISHED STORIES FETCHING (FOR STUDENTS) ===

    /// <summary>
/// Fetch published stories for a specific class from Firestore (for students)
/// </summary>
public async Task<List<PublishedStory>> GetPublishedStoriesFromFirestore(string classCode)
{
    try
    {
        if (!IsFirebaseReady || _firestore == null)
        {
            Debug.LogError("Firebase not ready, cannot fetch published stories");
            return new List<PublishedStory>();
        }

        Debug.Log($"📥 Fetching published stories for class: {classCode}");

        // Query stories that have this class in their assignedClasses array
        var storiesQuery = _firestore
            .Collection("createdStories")
            .WhereArrayContains("assignedClasses", classCode)
            .WhereEqualTo("isPublished", true);

        var snapshot = await storiesQuery.GetSnapshotAsync();
        var publishedStories = new List<PublishedStory>();

        // Get class details including teacher name
        var classDetails = await GetClassDetailsFromFirestore(classCode);
        
        foreach (var storyDoc in snapshot.Documents)
        {
            var firestoreStory = storyDoc.ConvertTo<StoryDataFirestore>();
            
            // Create PublishedStory from Firestore data with actual class details
            var publishedStory = new PublishedStory
            {
                storyId = firestoreStory.storyId,
                storyTitle = firestoreStory.title,
                classCode = classCode,
                className = classDetails.className ?? "Class",
                publishDate = firestoreStory.updatedAt != null ? firestoreStory.updatedAt.ToDateTime().ToString("MMM dd, yyyy") : "Unknown"
            };
            
            publishedStories.Add(publishedStory);
        }

        Debug.Log($"✅ Found {publishedStories.Count} published stories for class {classCode}");
        return publishedStories;
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"❌ Failed to fetch published stories from Firestore: {ex.Message}");
        return new List<PublishedStory>();
    }
}

private async Task<ClassDetailsModel> GetClassDetailsFromFirestore(string classCode)
{
    try
    {
        var classQuery = _firestore
            .Collection("classes")
            .WhereEqualTo("classCode", classCode)
            .Limit(1);

        var snapshot = await classQuery.GetSnapshotAsync();
        var classDocs = snapshot.Documents.ToList();
        if (classDocs.Count > 0)
        {
            var classData = classDocs[0].ToDictionary();
            var classDetails = new ClassDetailsModel
            {
                classCode = classCode,
                className = classData.ContainsKey("className") ? classData["className"].ToString() : "Unknown Class",
                classLevel = classData.ContainsKey("classLevel") ? classData["classLevel"].ToString() : "",
                teacherName = "Unknown Teacher", // We'll get this next
                isActive = classData.ContainsKey("isActive") ? (bool)classData["isActive"] : true
            };

            // Get teacher name if teachId exists
            if (classData.ContainsKey("teachId"))
            {
                string teacherId = classData["teachId"].ToString();
                classDetails.teacherName = await GetTeacherNameFromFirestore(teacherId);
            }

            return classDetails;
        }
    }
    catch (System.Exception ex)
    {
        Debug.LogWarning($"Failed to fetch class details: {ex.Message}");
    }
    
    return new ClassDetailsModel(classCode, "Unknown Class", "", "Unknown Teacher");
}

    private async Task<string> GetTeacherNameFromFirestore(string teacherId)
    {
        try
        {
            var teacherQuery = _firestore
                .Collection("teachers")
                .WhereEqualTo("teachId", teacherId)
                .Limit(1);

            var snapshot = await teacherQuery.GetSnapshotAsync();
            if (snapshot.Documents.Count() > 0) // ✅ FIXED: Use .Count property
            {
                var teacherDoc = snapshot.Documents.First();
                var teacherData = teacherDoc.ToDictionary();
                string firstName = teacherData.ContainsKey("teachFirstName") ? teacherData["teachFirstName"].ToString() : "";
                string lastName = teacherData.ContainsKey("teachLastName") ? teacherData["teachLastName"].ToString() : "";

                string fullName = $"{firstName} {lastName}".Trim();
                return string.IsNullOrEmpty(fullName) ? "Unknown Teacher" : fullName;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Failed to fetch teacher name: {ex.Message}");
        }

        return "Unknown Teacher";
    }

}
