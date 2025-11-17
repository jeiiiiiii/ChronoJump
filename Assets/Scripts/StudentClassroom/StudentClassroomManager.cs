using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using System;
using System.Linq;

public class StudentClassroomManager : MonoBehaviour
{
    [Header("UI References")]
    public ClassInfo classInfoComponent;
    public Transform storiesContainer;
    public GameObject storyItemPrefab;
    public Button refreshButton;
    public TextMeshProUGUI noStoriesText;

    [Header("Leaderboard")]
    public StudentClassroomLeaderboard leaderboardComponent;
    public Button refreshLeaderboardButton;

    [Header("Story Management")]
    public List<PublishedStory> availableStories = new List<PublishedStory>();

    [Header("Student Info")]
    public TextMeshProUGUI studentNameText;

    private StudentClassData currentClass;
    private bool isInitialized = false;
    private bool isLoadingStories = false;
    private bool isDestroyed = false; // ✅ ADD THIS FLAG

    private void OnEnable()
    {
        isDestroyed = false; // ✅ Reset flag
        ClassInfo.OnClassDataLoaded += OnClassDataLoaded;
        OnStudentNameChanged += HandleStudentNameChanged;
    }

    private void OnDisable()
    {
        isDestroyed = true; // ✅ Set flag when disabling
        ClassInfo.OnClassDataLoaded -= OnClassDataLoaded;
        OnStudentNameChanged -= HandleStudentNameChanged;
    }

    public static System.Action<string, StudentClassData> OnStudentDataChanged;

    private void SetStudentName(string name)
    {
        // ✅ Check if destroyed
        if (isDestroyed || studentNameText == null) return;

        studentNameText.text = name;

        ClassInfo classInfo = FindAnyObjectByType<ClassInfo>();
        StudentClassData classData = null;
        if (classInfo != null && classInfo.IsDataLoaded())
        {
            classData = classInfo.GetCurrentClassData();
        }

        OnStudentDataChanged?.Invoke(name, classData);
    }

    private void Start()
    {
        // ✅ Validate UI references at start
        if (!ValidateUIReferences())
        {
            Debug.LogError("❌ StudentClassroomManager: Critical UI references missing!");
            return;
        }

        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshStories);

        if (classInfoComponent != null && classInfoComponent.IsDataLoaded())
        {
            InitializeClassroom();
        }
        else
        {
            Debug.Log("Waiting for ClassInfo to load data...");
        }

        if (refreshLeaderboardButton != null)
            refreshLeaderboardButton.onClick.AddListener(RefreshLeaderboard);
    }

    // ✅ NEW: Validate all critical UI references
    private bool ValidateUIReferences()
    {
        bool isValid = true;

        if (storiesContainer == null)
        {
            Debug.LogError("❌ Stories Container is not assigned!");
            isValid = false;
        }

        if (storyItemPrefab == null)
        {
            Debug.LogError("❌ Story Item Prefab is not assigned!");
            isValid = false;
        }

        if (classInfoComponent == null)
        {
            Debug.LogError("❌ ClassInfo component is not assigned!");
            isValid = false;
        }

        return isValid;
    }

    private void OnClassDataLoaded(StudentClassData classData)
    {
        // ✅ Check if destroyed
        if (isDestroyed) return;

        Debug.Log($"📢 Received class data loaded event: {classData?.classCode ?? "NULL"}");

        if (!isInitialized)
        {
            InitializeClassroom();
        }
    }

    private void InitializeClassroom()
    {
        // ✅ Check if destroyed
        if (isDestroyed) return;

        if (isInitialized)
        {
            Debug.Log("Already initialized, skipping...");
            return;
        }

        if (classInfoComponent != null)
        {
            currentClass = classInfoComponent.GetCurrentClassData();
            Debug.Log($"Initialized classroom with class: {currentClass?.className ?? "NULL"}, Code: {currentClass?.classCode ?? "NULL"}");
        }
        else
        {
            Debug.LogError("ClassInfo component not assigned!");
            return;
        }

        isInitialized = true;

        LoadStudentName();
        LoadPublishedStories();
    }

    private async void LoadPublishedStories()
    {
        // ✅ Check if destroyed
        if (isDestroyed) return;

        if (isLoadingStories)
        {
            Debug.Log("⚠️ Stories are already loading, skipping...");
            return;
        }

        if (currentClass == null || string.IsNullOrEmpty(currentClass.classCode))
        {
            Debug.LogError($"❌ No class data available. CurrentClass null: {currentClass == null}, ClassCode: {currentClass?.classCode ?? "NULL"}");
            ShowNoStoriesMessage("No class joined");
            return;
        }

        Debug.Log($"📚 Loading stories for class: {currentClass.classCode}, Teacher: {currentClass.teacherName}");

        isLoadingStories = true;
        ClearStoryItems();
        ShowNoStoriesMessage("Loading stories...");

        try
        {
            var stories = await GetPublishedStoriesFromFirestore(currentClass.classCode);

            // ✅ Check if destroyed after async operation
            if (isDestroyed || this == null) return;

            if (stories.Count == 0)
            {
                ShowNoStoriesMessage("No stories published yet");
                Debug.Log($"ℹ️ No stories found for class: {currentClass.classCode}");
            }
            else
            {
                HideNoStoriesMessage();
                availableStories = stories;
                DisplayStories();
                Debug.Log($"✅ Loaded {stories.Count} stories for class: {currentClass.classCode}");
            }
        }
        catch (System.Exception ex)
        {
            // ✅ Check if destroyed
            if (isDestroyed || this == null) return;

            Debug.LogError($"❌ Failed to load stories: {ex.Message}");
            ShowNoStoriesMessage("Failed to load stories. Please try again.");
        }
        finally
        {
            isLoadingStories = false;
        }
    }

    public static System.Action<string> OnStudentNameChanged;

    private void HandleStudentNameChanged(string newName)
    {
        // ✅ Check if destroyed
        if (isDestroyed || studentNameText == null) return;

        Debug.Log($"🔄 StudentClassroomManager: Name changed via event to {newName}");

        studentNameText.text = newName;
        Debug.Log($"✅ Updated classroom display name to: {newName}");
    }

    private void LoadStudentName()
    {
        // ✅ Check if destroyed
        if (isDestroyed || studentNameText == null) return;

        studentNameText.text = "Loading...";
        Debug.Log("🎯 LoadStudentName() called");

        FirebaseManager.Instance.GetUserData(userData =>
        {
            // ✅ Check if destroyed in callback
            if (isDestroyed || this == null || studentNameText == null) return;

            if (userData == null)
            {
                Debug.LogError("❌ userData is NULL");
                SetStudentName("Student");
                return;
            }

            Debug.Log($"✅ Got user data: {userData.displayName}");

            FirebaseManager.Instance.GetStudentByUserId(userData.userId, studentData =>
            {
                // ✅ Check if destroyed in nested callback
                if (isDestroyed || this == null || studentNameText == null) return;

                if (studentData == null)
                {
                    Debug.LogError("❌ studentData is NULL");
                    SetStudentName(userData.displayName ?? "Student");
                    return;
                }

                SetStudentName(studentData.studName ?? userData.displayName ?? "Student");
                Debug.Log($"🎉 FINAL: '{studentNameText.text}'");
            });
        });
    }

    private async Task<List<PublishedStory>> GetPublishedStoriesFromFirestore(string classCode)
    {
        try
        {
            Debug.Log($"[DIRECT] Fetching stories for class: '{classCode}'");

            if (FirebaseManager.Instance?.DB == null)
            {
                Debug.Log("ℹ️ Firebase not available, no stories loaded");
                return new List<PublishedStory>();
            }

            var firestore = FirebaseManager.Instance.DB;

            var storiesQuery = firestore
                .Collection("createdStories")
                .WhereArrayContains("assignedClasses", classCode)
                .WhereEqualTo("isPublished", true);

            var snapshot = await storiesQuery.GetSnapshotAsync();

            // ✅ Check if destroyed after await
            if (isDestroyed || this == null)
            {
                Debug.Log("⚠️ Component destroyed during story fetch, aborting");
                return new List<PublishedStory>();
            }

            var stories = new List<PublishedStory>();

            foreach (var storyDoc in snapshot.Documents)
            {
                var data = storyDoc.ToDictionary();
                stories.Add(new PublishedStory
                {
                    storyId = storyDoc.Id,
                    storyTitle = data.ContainsKey("title") ? data["title"].ToString() : "Untitled",
                    classCode = classCode,
                    className = currentClass?.className ?? "Class",
                    publishDate = "Recent"
                });
            }

            Debug.Log($"[DIRECT] Found {stories.Count} stories");
            return stories;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"⚠️ Couldn't fetch stories: {ex.Message}");
            return new List<PublishedStory>();
        }
    }

    private void DisplayStories()
    {
        // ✅ Check if destroyed
        if (isDestroyed) return;

        foreach (PublishedStory story in availableStories)
        {
            CreateStoryItem(story);
        }
    }

    private void CreateStoryItem(PublishedStory story)
    {
        // ✅ Enhanced validation
        if (isDestroyed || storyItemPrefab == null || storiesContainer == null)
        {
            if (!isDestroyed)
                Debug.LogError("❌ Story item prefab or container not assigned");
            return;
        }

        // ✅ Validate container is a scene object
        if (storiesContainer.gameObject.scene.name == null)
        {
            Debug.LogError("❌ Stories container is a prefab, not a scene object!");
            return;
        }

        GameObject storyItem = Instantiate(storyItemPrefab, storiesContainer);
        StoryItemUI storyItemUI = storyItem.GetComponent<StoryItemUI>();

        if (storyItemUI != null)
        {
            storyItemUI.SetupStory(story, OnPlayStory);
        }
        else
        {
            Debug.LogError("StoryItemUI component not found on prefab");
        }
    }

    private async void OnPlayStory(PublishedStory story)
    {
        // ✅ Check if destroyed
        if (isDestroyed) return;

        Debug.Log($"Starting story: {story.storyTitle} (ID: {story.storyId})");

        try
        {
            var storyData = await LoadStoryFromFirestore(story.storyId);

            // ✅ Check if destroyed after async
            if (isDestroyed || this == null) return;

            if (storyData != null)
            {
                string storyJson = JsonUtility.ToJson(storyData);
                StudentPrefs.SetString("CurrentStoryData", storyJson);
                StudentPrefs.SetString("SelectedStoryID", story.storyId);
                StudentPrefs.SetString("SelectedStoryTitle", story.storyTitle);
                StudentPrefs.SetString("PlayingFromClass", currentClass.classCode);
                StudentPrefs.Save();

                Debug.Log($"✅ Loaded story directly: {story.storyTitle}");
                UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
            }
            else
            {
                Debug.LogError($"Failed to load story from Firestore!");
                ShowNoStoriesMessage("Could not load story. Please try again.");
            }
        }
        catch (System.Exception ex)
        {
            if (isDestroyed || this == null) return;

            Debug.LogError($"Error loading story: {ex.Message}");
            ShowNoStoriesMessage("Error loading story. Please try again.");
        }
    }

    private async Task<StoryData> LoadStoryFromFirestore(string storyId)
    {
        string localStoryKey = $"CachedStory_{storyId}";
        string cachedStoryJson = StudentPrefs.GetString(localStoryKey, "");

        int currentVersion = await GetStoryVersionFromFirestore(storyId);

        // ✅ Check if destroyed after await
        if (isDestroyed || this == null) return null;

        StoryData story = null;

        if (!string.IsNullOrEmpty(cachedStoryJson))
        {
            try
            {
                story = JsonUtility.FromJson<StoryData>(cachedStoryJson);

                if (story != null && story.dialogues != null && story.dialogues.Count > 0 &&
                    story.storyVersion == currentVersion)
                {
                    Debug.Log($"✅ Loaded story from local cache (v{story.storyVersion}): {story.storyTitle}");

                    await LoadVoiceAssignmentsForStory(storyId, story.dialogues);
                    
                    // ✅ Check if destroyed after await
                    if (isDestroyed || this == null) return null;
                    
                    return story;
                }
                else
                {
                    Debug.Log($"🔄 Cache outdated or invalid. Cached v{story?.storyVersion ?? 0}, Firestore v{currentVersion}. Fetching fresh...");
                    story = null;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"⚠️ Failed to load cached story, fetching from Firebase: {ex.Message}");
            }
        }

        try
        {
            if (FirebaseManager.Instance?.DB == null)
            {
                Debug.LogError("Firebase not ready");
                return null;
            }

            var firestore = FirebaseManager.Instance.DB;
            var storyDoc = await firestore.Collection("createdStories").Document(storyId).GetSnapshotAsync();

            // ✅ Check if destroyed after await
            if (isDestroyed || this == null) return null;

            if (!storyDoc.Exists)
            {
                Debug.LogError($"Story document {storyId} not found in Firestore");
                return null;
            }

            var storyData = storyDoc.ToDictionary();

            var dialogues = await LoadDialoguesFromFirestore(storyId);
            if (isDestroyed || this == null) return null;

            var questions = await LoadQuestionsFromFirestore(storyId);
            if (isDestroyed || this == null) return null;

            await LoadVoiceAssignmentsForStory(storyId, dialogues);
            if (isDestroyed || this == null) return null;

            story = new StoryData
            {
                storyId = storyId,
                storyTitle = storyData.ContainsKey("title") ? storyData["title"].ToString() : "Untitled",
                storyDescription = storyData.ContainsKey("description") ? storyData["description"].ToString() : "",
                backgroundPath = storyData.ContainsKey("backgroundUrl") ? storyData["backgroundUrl"].ToString() : "",
                character1Path = storyData.ContainsKey("character1Url") ? storyData["character1Url"].ToString() : "",
                character2Path = storyData.ContainsKey("character2Url") ? storyData["character2Url"].ToString() : "",
                dialogues = dialogues,
                quizQuestions = questions,
                assignedClasses = storyData.ContainsKey("assignedClasses") ?
                    ((List<object>)storyData["assignedClasses"]).ConvertAll(x => x.ToString()) :
                    new List<string>(),
                storyVersion = currentVersion
            };

            string storyJson = JsonUtility.ToJson(story);
            StudentPrefs.SetString(localStoryKey, storyJson);
            StudentPrefs.Save();

            Debug.Log($"✅ Loaded story from Firestore and cached locally (v{story.storyVersion}): {story.storyTitle}");
            return story;
        }
        catch (System.Exception ex)
        {
            if (isDestroyed || this == null) return null;

            Debug.LogError($"❌ Failed to load story from Firestore: {ex.Message}");
            return null;
        }
    }

    private async Task<int> GetStoryVersionFromFirestore(string storyId)
    {
        try
        {
            if (FirebaseManager.Instance?.DB == null)
            {
                Debug.LogWarning("Firebase not ready, using default version");
                return 1;
            }

            var firestore = FirebaseManager.Instance.DB;
            var storyDoc = await firestore.Collection("createdStories").Document(storyId).GetSnapshotAsync();

            if (storyDoc.Exists)
            {
                var storyData = storyDoc.ToDictionary();
                if (storyData.ContainsKey("storyVersion"))
                {
                    return Convert.ToInt32(storyData["storyVersion"]);
                }
            }

            return 1;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"⚠️ Failed to get story version: {ex.Message}");
            return 1;
        }
    }

    private async Task LoadVoiceAssignmentsForStory(string storyId, List<DialogueLine> dialogues)
    {
        try
        {
            Debug.Log($"🎤 Loading voice assignments for story: {storyId}");

            bool hasChanges = false;

            for (int i = 0; i < dialogues.Count; i++)
            {
                string voiceKey = $"{storyId}_Dialogue_{i}_VoiceId";
                string voiceId = TeacherPrefs.GetString(voiceKey, "");

                if (!string.IsNullOrEmpty(voiceId))
                {
                    if (dialogues[i].selectedVoiceId != voiceId)
                    {
                        dialogues[i].selectedVoiceId = voiceId;
                        hasChanges = true;

                        var voice = VoiceLibrary.GetVoiceById(voiceId);
                        Debug.Log($"✅ Loaded voice for dialogue {i}: {voice.voiceName}");
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(dialogues[i].selectedVoiceId))
                    {
                        dialogues[i].selectedVoiceId = VoiceLibrary.GetDefaultVoice().voiceId;
                        hasChanges = true;
                        Debug.LogWarning($"⚠️ No voice found for dialogue {i}, using default");
                    }
                }
            }

            if (hasChanges)
            {
                string localStoryKey = $"CachedStory_{storyId}";
                string currentStoryJson = StudentPrefs.GetString(localStoryKey, "");
                if (!string.IsNullOrEmpty(currentStoryJson))
                {
                    try
                    {
                        var currentStory = JsonUtility.FromJson<StoryData>(currentStoryJson);
                        if (currentStory != null)
                        {
                            currentStory.dialogues = dialogues;
                            string updatedStoryJson = JsonUtility.ToJson(currentStory);
                            StudentPrefs.SetString(localStoryKey, updatedStoryJson);
                            StudentPrefs.Save();
                            Debug.Log($"💾 Updated cached story with voice assignments");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"❌ Failed to update cached story: {ex.Message}");
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to load voice assignments: {ex.Message}");
        }
    }

    private async Task<List<DialogueLine>> LoadDialoguesFromFirestore(string storyId)
    {
        var dialogues = new List<DialogueLine>();

        try
        {
            if (FirebaseManager.Instance?.DB == null)
            {
                Debug.LogError("Firebase not available for loading dialogues");
                return dialogues;
            }

            var firestore = FirebaseManager.Instance.DB;
            Debug.Log($"📖 Loading dialogues for story: {storyId}");

            var dialoguesSnapshot = await firestore
                .Collection("createdStories")
                .Document(storyId)
                .Collection("dialogues")
                .OrderBy("orderIndex")
                .GetSnapshotAsync();

            Debug.Log($"📄 Found {dialoguesSnapshot.Documents.Count()} dialogue documents");

            foreach (var dialogueDoc in dialoguesSnapshot.Documents)
            {
                try
                {
                    var dialogueData = dialogueDoc.ConvertTo<DialogueLineFirestore>();

                    Debug.Log($"💬 Processing dialogue: {dialogueData.characterName} - {dialogueData.dialogueText}");

                    if (!string.IsNullOrEmpty(dialogueData.dialogueText))
                    {
                        dialogues.Add(new DialogueLine(
                            dialogueData.characterName ?? "Unknown",
                            dialogueData.dialogueText
                        ));
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ Skipping empty dialogue for character: {dialogueData.characterName}");
                    }
                }
                catch (System.Exception docEx)
                {
                    Debug.LogError($"❌ Error processing dialogue document {dialogueDoc.Id}: {docEx.Message}");

                    try
                    {
                        var data = dialogueDoc.ToDictionary();
                        string characterName = data.ContainsKey("characterName") ? data["characterName"].ToString() : "Unknown";
                        string dialogueText = data.ContainsKey("dialogueText") ? data["dialogueText"].ToString() : "";

                        if (!string.IsNullOrEmpty(dialogueText))
                        {
                            dialogues.Add(new DialogueLine(characterName, dialogueText));
                        }
                    }
                    catch (System.Exception fallbackEx)
                    {
                        Debug.LogError($"❌ Fallback also failed: {fallbackEx.Message}");
                    }
                }
            }

            Debug.Log($"✅ Successfully loaded {dialogues.Count} dialogues");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to load dialogues: {ex.Message}");
            Debug.LogError($"Stack trace: {ex.StackTrace}");
        }

        return dialogues;
    }

    private async Task<List<Question>> LoadQuestionsFromFirestore(string storyId)
    {
        var questions = new List<Question>();

        try
        {
            if (FirebaseManager.Instance?.DB == null)
            {
                Debug.LogError("Firebase not available for loading questions");
                return questions;
            }

            var firestore = FirebaseManager.Instance.DB;
            Debug.Log($"📖 Loading questions for story: {storyId}");

            var questionsSnapshot = await firestore
                .Collection("createdStories")
                .Document(storyId)
                .Collection("questions")
                .OrderBy("orderIndex")
                .GetSnapshotAsync();

            Debug.Log($"📄 Found {questionsSnapshot.Documents.Count()} question documents");

            foreach (var questionDoc in questionsSnapshot.Documents)
            {
                try
                {
                    var questionData = questionDoc.ConvertTo<QuestionFirestore>();

                    Debug.Log($"❓ Processing question: {questionData.questionText}");
                    Debug.Log($"📝 Choices count: {questionData.choices?.Count ?? 0}");

                    if (questionData.choices != null && questionData.choices.Count >= 2)
                    {
                        questions.Add(new Question(
                            questionData.questionText ?? "No question text",
                            questionData.choices.ToArray(),
                            questionData.correctAnswerIndex
                        ));
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ Skipping question with insufficient choices: {questionData.questionText}");
                    }
                }
                catch (System.Exception docEx)
                {
                    Debug.LogError($"❌ Error processing question document {questionDoc.Id}: {docEx.Message}");

                    try
                    {
                        var data = questionDoc.ToDictionary();
                        string questionText = data.ContainsKey("questionText") ? data["questionText"].ToString() : "No question text";

                        List<string> choices = new List<string>();
                        if (data.ContainsKey("choices") && data["choices"] is List<object> choicesList)
                        {
                            choices = choicesList.ConvertAll(x => x?.ToString() ?? "");
                        }

                        int correctIndex = data.ContainsKey("correctAnswerIndex") ?
                            Convert.ToInt32(data["correctAnswerIndex"]) : 0;

                        if (choices.Count >= 2)
                        {
                            questions.Add(new Question(questionText, choices.ToArray(), correctIndex));
                        }
                    }
                    catch (System.Exception fallbackEx)
                    {
                        Debug.LogError($"❌ Fallback also failed: {fallbackEx.Message}");
                    }
                }
            }

            Debug.Log($"✅ Successfully loaded {questions.Count} questions");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to load questions: {ex.Message}");
            Debug.LogError($"Stack trace: {ex.StackTrace}");
        }

        return questions;
    }

    public void JoinNewClass(string classCode)
    {
        StudentPrefs.SetString("JoinedClassCode", classCode);
        StudentPrefs.Save();

        isInitialized = false;
        InitializeClassroom();
    }

    private void ClearStoryItems()
    {
        if (isDestroyed || storiesContainer == null) return;

        foreach (Transform child in storiesContainer)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }

    private void ShowNoStoriesMessage(string message)
    {
        if (isDestroyed || noStoriesText == null) return;

        noStoriesText.text = message;
        noStoriesText.gameObject.SetActive(true);
    }

    private void HideNoStoriesMessage()
    {
        if (isDestroyed || noStoriesText == null) return;

        noStoriesText.gameObject.SetActive(false);
    }

    public void RefreshStories()
    {
        if (isDestroyed || isLoadingStories)
        {
            Debug.Log("⚠️ Cannot refresh - component destroyed or loading");
            return;
        }

        Debug.Log("Refreshing stories...");

        if (classInfoComponent != null)
        {
            currentClass = classInfoComponent.GetCurrentClassData();
        }

        LoadPublishedStories();
    }

    private void RefreshLeaderboard()
    {
        if (isDestroyed || leaderboardComponent == null) return;

        leaderboardComponent.RefreshLeaderboard();
    }

    public void OnStoryPublished(PublishedStory newStory)
    {
        if (isDestroyed || currentClass == null || newStory == null) return;

        availableStories.Add(newStory);
        CreateStoryItem(newStory);
        HideNoStoriesMessage();
    }

    public void ClearStoryCache()
    {
        if (currentClass == null || string.IsNullOrEmpty(currentClass.classCode))
            return;

        var storiesToClear = availableStories.Where(s => s.classCode == currentClass.classCode).ToList();

        foreach (var story in storiesToClear)
        {
            string cacheKey = $"CachedStory_{story.storyId}";
            if (StudentPrefs.HasKey(cacheKey))
            {
                StudentPrefs.DeleteKey(cacheKey);
                Debug.Log($"🗑️ Cleared cache for: {story.storyTitle}");
            }
        }

        StudentPrefs.Save();
        LoadPublishedStories();

        Debug.Log("✅ Story cache cleared and refreshed");
    }

    private void DebugStudentDataFlow()
    {
        Debug.Log("🔍 Starting student data flow debug...");

        FirebaseManager.Instance.GetUserData(userData =>
        {
            if (isDestroyed || this == null) return;

            if (userData == null)
            {
                Debug.LogError("❌ STEP 1: GetUserData returned NULL");
                return;
            }

            Debug.Log($"✅ STEP 1: Got user data - UserId: {userData.userId}, DisplayName: {userData.displayName}");

            FirebaseManager.Instance.GetStudentByUserId(userData.userId, studentData =>
            {
                if (isDestroyed || this == null) return;

                if (studentData == null)
                {
                    Debug.LogError("❌ STEP 2: GetStudentByUserId returned NULL");
                    return;
                }

                Debug.Log($"✅ STEP 2: Got student data - StudId: {studentData.studId}, StudName: {studentData.studName}, UserId: {studentData.userId}");
            });
        });
    }

    private void OnDestroy()
    {
        // ✅ Mark as destroyed
        isDestroyed = true;
        Debug.Log("🔴 StudentClassroomManager destroyed");
    }
}
