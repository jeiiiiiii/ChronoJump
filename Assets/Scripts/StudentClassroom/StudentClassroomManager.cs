using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using System;

public class StudentClassroomManager : MonoBehaviour
{
    [Header("UI References")]
    public ClassInfo classInfoComponent;
    public Transform storiesContainer;
    public GameObject storyItemPrefab;
    public Button refreshButton;
    public TextMeshProUGUI noStoriesText;

    [Header("Story Management")]
    public List<PublishedStory> availableStories = new List<PublishedStory>();

    private StudentClassData currentClass;
    private bool isInitialized = false;
    private bool isLoadingStories = false;

    private void OnEnable()
    {
        // Subscribe to ClassInfo data loaded event
        ClassInfo.OnClassDataLoaded += OnClassDataLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe from event
        ClassInfo.OnClassDataLoaded -= OnClassDataLoaded;
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


// ✅ ADD THIS METHOD TO LOAD STORY FROM FIRESTORE
private async Task<StoryData> LoadStoryFromFirestore(string storyId)
{
    try
    {
        if (FirebaseManager.Instance?.DB == null)
        {
            Debug.LogError("Firebase not ready");
            return null;
        }

        var firestore = FirebaseManager.Instance.DB;
        
        // Get the main story document
        var storyDoc = await firestore.Collection("createdStories").Document(storyId).GetSnapshotAsync();
        
        if (!storyDoc.Exists)
        {
            Debug.LogError($"Story document {storyId} not found in Firestore");
            return null;
        }

        var storyData = storyDoc.ToDictionary();
        
        // Load dialogues
        var dialogues = await LoadDialoguesFromFirestore(storyId);
        
        // Load questions
        var questions = await LoadQuestionsFromFirestore(storyId);

        // Create StoryData object
        var story = new StoryData
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
                new List<string>()
        };

        Debug.Log($"✅ Loaded story from Firestore: {story.storyTitle}");
        return story;
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"❌ Failed to load story from Firestore: {ex.Message}");
        return null;
    }
}

// ✅ ADD THESE HELPER METHODS
private async Task<List<DialogueLine>> LoadDialoguesFromFirestore(string storyId)
{
    var dialogues = new List<DialogueLine>();
    
    try
    {
        var firestore = FirebaseManager.Instance.DB;
        var dialoguesSnapshot = await firestore
            .Collection("createdStories")
            .Document(storyId)
            .Collection("dialogues")
            .OrderBy("orderIndex")
            .GetSnapshotAsync();

        foreach (var dialogueDoc in dialoguesSnapshot.Documents)
        {
            var data = dialogueDoc.ToDictionary();
            dialogues.Add(new DialogueLine(
                data.ContainsKey("characterName") ? data["characterName"].ToString() : "",
                data.ContainsKey("dialogueText") ? data["dialogueText"].ToString() : ""
            ));
        }
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"Failed to load dialogues: {ex.Message}");
    }
    
    return dialogues;
}

    private async Task<List<Question>> LoadQuestionsFromFirestore(string storyId)
    {
        var questions = new List<Question>();

        try
        {
            var firestore = FirebaseManager.Instance.DB;
            var questionsSnapshot = await firestore
                .Collection("createdStories")
                .Document(storyId)
                .Collection("questions")
                .GetSnapshotAsync();

            foreach (var questionDoc in questionsSnapshot.Documents)
            {
                var data = questionDoc.ToDictionary();

                var choices = data.ContainsKey("choices") ?
                    ((List<object>)data["choices"]).ConvertAll(x => x.ToString()).ToArray() :
                    new string[0];

                int correctIndex = data.ContainsKey("correctAnswerIndex") ?
                    Convert.ToInt32(data["correctAnswerIndex"]) : 0;

                questions.Add(new Question(
                    data.ContainsKey("questionText") ? data["questionText"].ToString() : "",
                    choices,
                    correctIndex
                ));
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load questions: {ex.Message}");
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

    public void OnStoryPublished(PublishedStory newStory)
    {
        if (currentClass != null && newStory != null)
        {
            availableStories.Add(newStory);
            CreateStoryItem(newStory);
            HideNoStoriesMessage();
        }
    }
}
