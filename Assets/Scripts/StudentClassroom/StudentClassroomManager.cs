using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

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

    private void LoadPublishedStories()
    {
        if (currentClass == null || string.IsNullOrEmpty(currentClass.classCode))
        {
            Debug.LogError($"No class data available. CurrentClass null: {currentClass == null}, ClassCode: {currentClass?.classCode ?? "NULL"}");
            ShowNoStoriesMessage("No class joined");
            return;
        }

        ClearStoryItems();

        availableStories = GetPublishedStoriesForClass(currentClass.classCode);

        if (availableStories.Count == 0)
        {
            ShowNoStoriesMessage("No stories published yet");
        }
        else
        {
            HideNoStoriesMessage();
            DisplayStories();
        }
    }

    private List<PublishedStory> GetPublishedStoriesForClass(string classCode)
    {
        if (StoryManager.Instance == null)
        {
            Debug.LogError("StoryManager instance not found!");
            return new List<PublishedStory>();
        }

        Debug.Log($"[DEBUG] Requesting stories for class: '{classCode}'");
        Debug.Log($"[DEBUG] StoryManager has {StoryManager.Instance.publishedStories.Count} total published stories");

        foreach (var story in StoryManager.Instance.publishedStories)
        {
            Debug.Log($"[DEBUG] Published Story: {story.storyTitle} | Class: '{story.classCode}' | Match: {story.classCode == classCode}");
        }

        var stories = StoryManager.Instance.GetPublishedStoriesForClass(classCode);
        Debug.Log($"[DEBUG] GetPublishedStoriesForClass returned {stories.Count} stories for '{classCode}'");

        return stories;
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

    private void OnPlayStory(PublishedStory story)
    {
        Debug.Log($"Starting story: {story.storyTitle} (ID: {story.storyId})");

        if (StoryManager.Instance != null)
        {
            var actualStory = StoryManager.Instance.allStories.Find(s => s.storyId == story.storyId);
            if (actualStory != null)
            {
                StoryManager.Instance.SetCurrentStory(actualStory);

                PlayerPrefs.SetString("SelectedStoryID", story.storyId);
                PlayerPrefs.SetString("SelectedStoryTitle", story.storyTitle);
                PlayerPrefs.SetString("PlayingFromClass", currentClass.classCode);
                PlayerPrefs.Save();

                Debug.Log($"Loading story gameplay for: {story.storyTitle}");
                UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
            }
            else
            {
                Debug.LogError($"Story with ID {story.storyId} not found in StoryManager!");
            }
        }
        else
        {
            Debug.LogError("StoryManager instance not found!");
        }
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
        Debug.Log("Refreshing stories...");

        if (classInfoComponent != null)
        {
            currentClass = classInfoComponent.GetCurrentClassData();
        }

        LoadPublishedStories();
    }

    public void JoinNewClass(string classCode)
    {
        PlayerPrefs.SetString("JoinedClassCode", classCode);
        PlayerPrefs.Save();

        isInitialized = false;
        InitializeClassroom();
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