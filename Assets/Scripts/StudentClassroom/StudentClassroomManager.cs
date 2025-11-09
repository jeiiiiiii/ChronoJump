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

    private void OnEnable()
    {
        ClassInfo.OnClassDataLoaded += OnClassDataLoaded;
        // NEW: Subscribe to static event
        OnStudentNameChanged += HandleStudentNameChanged;
    }

    private void OnDisable()
    {
        ClassInfo.OnClassDataLoaded -= OnClassDataLoaded;
        // NEW: Unsubscribe from static event
        OnStudentNameChanged -= HandleStudentNameChanged;
    }

    // Change the event
    public static System.Action<string, StudentClassData> OnStudentDataChanged;

    private void SetStudentName(string name)
    {
        if (studentNameText != null)
        {
            studentNameText.text = name;

            ClassInfo classInfo = FindAnyObjectByType<ClassInfo>();
            StudentClassData classData = null;
            if (classInfo != null && classInfo.IsDataLoaded())
            {
                classData = classInfo.GetCurrentClassData();
            }

            OnStudentDataChanged?.Invoke(name, classData);
        }
    }


    private void Start()
    {
        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshStories);

        // Check if ClassInfo already has data loaded
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

    private void OnClassDataLoaded(StudentClassData classData)
    {
        Debug.Log($"📢 Received class data loaded event: {classData?.classCode ?? "NULL"}");

        if (!isInitialized)
        {
            InitializeClassroom();
        }
    }

    private void InitializeClassroom()
    {
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
        // Prevent multiple simultaneous loads
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

        Debug.Log($"🔍 Loading stories for class: {currentClass.classCode}, Teacher: {currentClass.teacherName}");

        isLoadingStories = true;
        ClearStoryItems();
        ShowNoStoriesMessage("Loading stories...");

        try
        {
            // ✅ FIX: Call GetPublishedStoriesFromFirestore directly instead of GetPublishedStoriesFromFirestoreWithRetry
            var stories = await GetPublishedStoriesFromFirestore(currentClass.classCode);

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
            Debug.LogError($"❌ Failed to load stories: {ex.Message}");
            ShowNoStoriesMessage("Failed to load stories. Please try again.");
        }
        finally
        {
            isLoadingStories = false;
        }
    }

    // NEW: Static event for name changes
    public static System.Action<string> OnStudentNameChanged;

    // NEW: Handle the name change event
    private void HandleStudentNameChanged(string newName)
    {
        Debug.Log($"🔄 StudentClassroomManager: Name changed via event to {newName}");

        if (studentNameText != null)
        {
            studentNameText.text = newName;
            Debug.Log($"✅ Updated classroom display name to: {newName}");
        }
    }


    private void LoadStudentName()
    {
        if (studentNameText == null)
        {
            Debug.LogWarning("Student name text component not assigned");
            return;
        }

        studentNameText.text = "Loading...";
        Debug.Log("🎯 LoadStudentName() called");

        // Get current student data from Firebase
        FirebaseManager.Instance.GetUserData(userData =>
        {
            if (userData == null)
            {
                Debug.LogError("❌ userData is NULL");
                SetStudentName("Student");
                return;
            }

            Debug.Log($"✅ Got user data: {userData.displayName}");

            // Get student data from students collection to access studName
            FirebaseManager.Instance.GetStudentByUserId(userData.userId, studentData =>
            {
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

            // Quick check - if Firebase isn't ready, just return empty list
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
        foreach (PublishedStory story in availableStories)
        {
            CreateStoryItem(story);
        }
    }

    private void CreateStoryItem(PublishedStory story)
    {
        if (storyItemPrefab == null || storiesContainer == null)
        {
            Debug.LogError("Story item prefab or container not assigned");
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
        Debug.Log($"Starting story: {story.storyTitle} (ID: {story.storyId})");

        try
        {
            // ✅ LOAD STORY DIRECTLY - NO STORYMANAGER
            var storyData = await LoadStoryFromFirestore(story.storyId);

            if (storyData != null)
            {
                // Store in StudentPrefs for GameScene to use
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
            Debug.LogError($"Error loading story: {ex.Message}");
            ShowNoStoriesMessage("Error loading story. Please try again.");
        }
    }


    private async Task<StoryData> LoadStoryFromFirestore(string storyId)
    {
        // ✅ FIRST: Check local cache WITH VERSION VALIDATION
        string localStoryKey = $"CachedStory_{storyId}";
        string cachedStoryJson = StudentPrefs.GetString(localStoryKey, "");

        // ✅ NEW: Get current version from Firestore to check if cache is stale
        int currentVersion = await GetStoryVersionFromFirestore(storyId);

        StoryData story = null;

        if (!string.IsNullOrEmpty(cachedStoryJson))
        {
            try
            {
                story = JsonUtility.FromJson<StoryData>(cachedStoryJson);

                // ✅ ENHANCED CACHE VALIDATION: Check version AND audio completeness
                if (story != null && story.dialogues != null && story.dialogues.Count > 0 &&
                    story.storyVersion == currentVersion &&
                    !ShouldRefreshCache(story, currentVersion)) // ← NEW CHECK
                {
                    Debug.Log($"✅ Loaded story from local cache (v{story.storyVersion}): {story.storyTitle}");
                    await LoadVoiceAssignmentsForStory(storyId, story.dialogues);
                    return story;
                }
                else
                {
                    Debug.Log($"🔄 Cache outdated or incomplete. Cached v{story?.storyVersion ?? 0}, Firestore v{currentVersion}. Fetching fresh...");
                    story = null; // Force fresh fetch
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"⚠️ Failed to load cached story, fetching from Firebase: {ex.Message}");
            }
        }

        // ✅ SECOND: Fetch from Firebase if no valid local cache
        try
        {
            if (FirebaseManager.Instance?.DB == null)
            {
                Debug.LogError("Firebase not ready");
                return null;
            }

            var firestore = FirebaseManager.Instance.DB;
            var storyDoc = await firestore.Collection("createdStories").Document(storyId).GetSnapshotAsync();

            if (!storyDoc.Exists)
            {
                Debug.LogError($"Story document {storyId} not found in Firestore");
                return null;
            }

            var storyData = storyDoc.ToDictionary();

            // Load dialogues and questions
            var dialogues = await LoadDialoguesFromFirestore(storyId);
            var questions = await LoadQuestionsFromFirestore(storyId);

            // ✅ CRITICAL: Load voice assignments BEFORE creating the story
            await LoadVoiceAssignmentsForStory(storyId, dialogues);

            // Create StoryData object
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
                storyVersion = currentVersion // Set the current version
            };

            // ✅ Save to local cache for future use
            string storyJson = JsonUtility.ToJson(story);
            StudentPrefs.SetString(localStoryKey, storyJson);
            StudentPrefs.Save();

            Debug.Log($"✅ Loaded story from Firestore and cached locally (v{story.storyVersion}): {story.storyTitle}");
            return story;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to load story from Firestore: {ex.Message}");
            return null;
        }

    }

    // ✅ NEW: Method to get current story version from Firestore
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

            return 1; // Default version if not found
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"⚠️ Failed to get story version: {ex.Message}");
            return 1;
        }
    }



    // ✅ NEW: Load voice assignments for all dialogues in a story
    private async Task LoadVoiceAssignmentsForStory(string storyId, List<DialogueLine> dialogues)
    {
        try
        {
            Debug.Log($"🎤 Loading voice assignments for story: {storyId}");

            bool hasChanges = false;

            for (int i = 0; i < dialogues.Count; i++)
            {
                // Use the same key format as VoiceStorageManager
                string voiceKey = $"{storyId}_Dialogue_{i}_VoiceId";

                // Try to load from TeacherPrefs (this works for students too)
                string voiceId = TeacherPrefs.GetString(voiceKey, "");

                if (!string.IsNullOrEmpty(voiceId))
                {
                    // Only update if different
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
                    // Fallback: use default voice
                    if (string.IsNullOrEmpty(dialogues[i].selectedVoiceId))
                    {
                        dialogues[i].selectedVoiceId = VoiceLibrary.GetDefaultVoice().voiceId;
                        hasChanges = true;
                        Debug.LogWarning($"⚠️ No voice found for dialogue {i}, using default");
                    }
                }
            }

            // ✅ CRITICAL: If we made changes, update the cached story
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

    // ✅ FIXED: LoadDialoguesFromFirestore method in StudentClassroomManager.cs
    // Replace your existing method with this one

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
            Debug.Log($"🔍 Loading dialogues for story: {storyId}");

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
                    // Use the Firestore data model
                    var dialogueData = dialogueDoc.ConvertTo<DialogueLineFirestore>();

                    Debug.Log($"💬 Processing dialogue: {dialogueData.characterName} - {dialogueData.dialogueText}");

                    // ✅ CRITICAL FIX: Create DialogueLine with ALL audio metadata
                    if (!string.IsNullOrEmpty(dialogueData.dialogueText))
                    {
                        var dialogue = new DialogueLine(
                            dialogueData.characterName ?? "Unknown",
                            dialogueData.dialogueText
                        );

                        // ✅ NEW: Copy ALL audio-related fields from Firestore
                        dialogue.selectedVoiceId = dialogueData.selectedVoiceId ?? "";
                        dialogue.audioFilePath = dialogueData.audioFilePath ?? "";
                        dialogue.audioFileName = dialogueData.audioFileName ?? "";
                        dialogue.audioStoragePath = dialogueData.audioStoragePath ?? "";
                        dialogue.audioFileSize = dialogueData.audioFileSize;
                        dialogue.hasAudio = dialogueData.hasAudio;

                        // ✅ Log what we loaded
                        if (dialogue.hasAudio)
                        {
                            Debug.Log($"   🎵 Has audio: Local={dialogue.audioFilePath}, S3={dialogue.audioStoragePath}");
                        }
                        else
                        {
                            Debug.Log($"   🔇 No audio for this dialogue");
                        }

                        dialogues.Add(dialogue);
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ Skipping empty dialogue for character: {dialogueData.characterName}");
                    }
                }
                catch (System.Exception docEx)
                {
                    Debug.LogError($"❌ Error processing dialogue document {dialogueDoc.Id}: {docEx.Message}");

                    // Fallback: try dictionary approach
                    try
                    {
                        var data = dialogueDoc.ToDictionary();
                        string characterName = data.ContainsKey("characterName") ? data["characterName"].ToString() : "Unknown";
                        string dialogueText = data.ContainsKey("dialogueText") ? data["dialogueText"].ToString() : "";

                        if (!string.IsNullOrEmpty(dialogueText))
                        {
                            var dialogue = new DialogueLine(characterName, dialogueText);

                            // ✅ Also load audio metadata in fallback
                            if (data.ContainsKey("selectedVoiceId"))
                                dialogue.selectedVoiceId = data["selectedVoiceId"].ToString();

                            if (data.ContainsKey("audioFilePath"))
                                dialogue.audioFilePath = data["audioFilePath"].ToString();

                            if (data.ContainsKey("audioFileName"))
                                dialogue.audioFileName = data["audioFileName"].ToString();

                            if (data.ContainsKey("audioStoragePath"))
                                dialogue.audioStoragePath = data["audioStoragePath"].ToString();

                            if (data.ContainsKey("audioFileSize"))
                                dialogue.audioFileSize = Convert.ToInt64(data["audioFileSize"]);

                            if (data.ContainsKey("hasAudio"))
                                dialogue.hasAudio = Convert.ToBoolean(data["hasAudio"]);

                            dialogues.Add(dialogue);
                        }
                    }
                    catch (System.Exception fallbackEx)
                    {
                        Debug.LogError($"❌ Fallback also failed: {fallbackEx.Message}");
                    }
                }
            }

            Debug.Log($"✅ Successfully loaded {dialogues.Count} dialogues with audio metadata");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to load dialogues: {ex.Message}");
            Debug.LogError($"Stack trace: {ex.StackTrace}");
        }

        return dialogues;
    }


    // ✅ CORRECTED QUESTION LOADING METHOD
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
            Debug.Log($"🔍 Loading questions for story: {storyId}");

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
                    // Use the Firestore data model
                    var questionData = questionDoc.ConvertTo<QuestionFirestore>();

                    Debug.Log($"❓ Processing question: {questionData.questionText}");
                    Debug.Log($"📝 Choices count: {questionData.choices?.Count ?? 0}");

                    // Convert to game model
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

                    // Fallback: try dictionary approach
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
        // ✅ Use StudentPrefs instead of PlayerPrefs
        StudentPrefs.SetString("JoinedClassCode", classCode);
        StudentPrefs.Save();

        isInitialized = false;
        InitializeClassroom();
    }

    private void ClearStoryItems()
    {
        if (storiesContainer == null) return;

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
        if (noStoriesText != null)
        {
            noStoriesText.text = message;
            noStoriesText.gameObject.SetActive(true);
        }
    }

    private void HideNoStoriesMessage()
    {
        if (noStoriesText != null)
        {
            noStoriesText.gameObject.SetActive(false);
        }
    }

    public void RefreshStories()
    {
        if (isLoadingStories)
        {
            Debug.Log("⚠️ Stories are already loading, please wait...");
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
        if (leaderboardComponent != null)
        {
            leaderboardComponent.RefreshLeaderboard();
        }
    }


    public void OnStoryPublished(PublishedStory newStory)
    {
        if (currentClass != null && newStory != null)
        {
            availableStories.Add(newStory);
            CreateStoryItem(newStory);
            HideNoStoriesMessage();
        }
    }

    // Add this method to StudentClassroomManager.cs
    public void ClearStoryCache()
    {
        if (currentClass == null || string.IsNullOrEmpty(currentClass.classCode))
            return;

        // Clear all cached stories for this class
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

        // Reload stories
        LoadPublishedStories();

        Debug.Log("✅ Story cache cleared and refreshed");
    }

    private void DebugStudentDataFlow()
    {
        Debug.Log("🔍 Starting student data flow debug...");

        FirebaseManager.Instance.GetUserData(userData =>
        {
            if (userData == null)
            {
                Debug.LogError("❌ STEP 1: GetUserData returned NULL");
                return;
            }

            Debug.Log($"✅ STEP 1: Got user data - UserId: {userData.userId}, DisplayName: {userData.displayName}");

            FirebaseManager.Instance.GetStudentByUserId(userData.userId, studentData =>
            {
                if (studentData == null)
                {
                    Debug.LogError("❌ STEP 2: GetStudentByUserId returned NULL");
                    return;
                }

                Debug.Log($"✅ STEP 2: Got student data - StudId: {studentData.studId}, StudName: {studentData.studName}, UserId: {studentData.userId}");
            });
        });
    }

    private bool ShouldRefreshCache(StoryData cachedStory, int currentVersion)
    {
        // If versions don't match, definitely refresh
        if (cachedStory.storyVersion != currentVersion)
            return true;

        // If audio is missing but should exist, refresh
        if (cachedStory.dialogues != null)
        {
            foreach (var dialogue in cachedStory.dialogues)
            {
                // If dialogue has a voice but no audio, cache might be stale
                if (!string.IsNullOrEmpty(dialogue.selectedVoiceId) &&
                    !dialogue.hasAudio &&
                    !VoiceLibrary.IsNoVoice(dialogue.selectedVoiceId))
                {
                    Debug.Log($"🔄 Cache refresh needed: Dialogue '{dialogue.characterName}' has voice but no audio");
                    return true;
                }
            }
        }

        return false;
    }
    

}