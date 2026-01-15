using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Linq;

/// <summary>
/// Manages the Story Publish scene UI and handles button interactions
/// </summary>
public class StoryPublishManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button creatorModeButton;
    [SerializeField] private Button dashboardButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button publishButton;
    [SerializeField] private Transform createdListParent;
    [SerializeField] private GameObject createdStoryPrefab;
    [SerializeField] private GameObject deletePopUp;
    [SerializeField] private Button deleteBackButton;
    [SerializeField] private Button deleteConfirmButton;
    [SerializeField] private GameObject publishPopUp;
    [SerializeField] private Button publishBackButton;
    [SerializeField] private Button publishConfirmButton;

    [Header("Class List Display")]
    [SerializeField] private Transform classListParent;
    [SerializeField] private GameObject classItemPrefab;
    [SerializeField] private TextMeshProUGUI noClassesText;

    [Header("Created Section")]
    [SerializeField] private TextMeshProUGUI createdSectionText;
    [SerializeField] private GameObject placeholderText;

    [Header("Loading State")]
    [SerializeField] private GameObject loadingIndicator;

    private List<GameObject> _instantiatedClassItems = new List<GameObject>();
    private string _selectedClassCode;
    private string _selectedDisplayName;
    private PublishedStory _storyToDelete;

    private void Start()
    {
        EnsureSceneNavigationManager();
        SetupButtonListeners();
        LoadClassData();
        InitializeCreatedSectionText();
        

        if (createdListParent != null)
        {
            createdListParent.gameObject.SetActive(false);
        }

        ShowPlaceholderText(true);
    }

    private void OnEnable()
    {
        ClassDataSync.OnClassDataUpdated += OnClassDataUpdated;
        ClassDataSync.OnNewClassCreated += OnNewClassAdded;
        ClassDataSync.OnClassDeleted += OnClassRemoved;
        ClassDataSync.OnClassEdited += OnClassUpdated;
    }

    private void OnDisable()
    {
        ClassDataSync.OnClassDataUpdated -= OnClassDataUpdated;
        ClassDataSync.OnNewClassCreated -= OnNewClassAdded;
        ClassDataSync.OnClassDeleted -= OnClassRemoved;
        ClassDataSync.OnClassEdited -= OnClassUpdated;
    }

    private void InitializeCreatedSectionText()
    {
        if (createdSectionText != null)
        {
            createdSectionText.text = "Section";
        }
        else
        {
            Debug.LogWarning("CreatedSectionText reference is missing! Please assign it in the inspector.");
        }
    }

    private void SetupButtonListeners()
    {
        if (creatorModeButton != null)
        {
            creatorModeButton.onClick.AddListener(OnCreatorModeClicked);
        }
        else
        {
            Debug.LogWarning("Creator Mode button reference is missing!");
        }

        if (dashboardButton != null)
        {
            dashboardButton.onClick.AddListener(OnDashboardClicked);
        }
        else
        {
            Debug.LogWarning("Dashboard button reference is missing!");
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }
        else
        {
            Debug.LogWarning("Back button reference is missing!");
        }
        if (publishButton != null)
        {
            publishButton.onClick.AddListener(OnPublishButtonClicked);
        }
        if (deleteBackButton != null)
            deleteBackButton.onClick.AddListener(OnDeleteBack);

        if (deleteConfirmButton != null)
            deleteConfirmButton.onClick.AddListener(OnDeleteConfirm);

        if (publishBackButton != null)
            publishBackButton.onClick.AddListener(OnPublishBack);

        if (publishConfirmButton != null)
            publishConfirmButton.onClick.AddListener(OnPublishConfirm);
        else
        {
            Debug.LogWarning("Publish button reference is missing!");
        }
    }

    private void LoadClassData()
    {
        ShowLoadingState(true);

        if (ClassDataSync.Instance == null)
        {
            GameObject syncObject = new GameObject("ClassDataSync");
            syncObject.AddComponent<ClassDataSync>();
        }

        if (ClassDataSync.Instance.IsDataLoaded())
        {
            var classData = ClassDataSync.Instance.GetCachedClassData();
            DisplayClasses(classData);
            ShowLoadingState(false);
        }
        else
        {
            ClassDataSync.Instance.LoadTeacherClassData(classData =>
            {
                DisplayClasses(classData);
                ShowLoadingState(false);
            });
        }
    }

    private void DisplayClasses(Dictionary<string, List<string>> classData)
    {
        ClearClassList();

        if (classData == null || classData.Count == 0)
        {
            ShowNoClassesMessage(true);
            return;
        }

        ShowNoClassesMessage(false);

        foreach (var classEntry in classData)
        {
            string classCode = classEntry.Key;
            string classLevel = classEntry.Value[0];
            string className = classEntry.Value[1];
            string displayName = $"{classLevel} - {className}";

            CreateClassItem(classCode, displayName);
        }

        Debug.Log($"Displayed {classData.Count} classes in Story Publish");
    }

    private void CreateClassItem(string classCode, string displayName)
    {
        if (classItemPrefab == null || classListParent == null)
        {
            Debug.LogError("Class item prefab or parent is not assigned!");
            return;
        }

        GameObject classItem = Instantiate(classItemPrefab, classListParent);

        TextMeshProUGUI classText = classItem.GetComponentInChildren<TextMeshProUGUI>();
        if (classText != null)
        {
            classText.text = displayName;
        }

        _instantiatedClassItems.Add(classItem);

        Button classButton = classItem.GetComponent<Button>();
        if (classButton != null)
        {
            classButton.onClick.AddListener(() => OnClassItemClicked(classCode, displayName, classButton));
        }
    }

    private void ClearClassList()
    {
        foreach (GameObject item in _instantiatedClassItems)
        {
            if (item != null)
            {
                DestroyImmediate(item);
            }
        }
        _instantiatedClassItems.Clear();
    }

    private void ShowNoClassesMessage(bool show)
    {
        if (noClassesText != null)
        {
            noClassesText.gameObject.SetActive(show);
            if (show)
            {
                noClassesText.text = "This is where you can see your classes.";
            }
        }
    }

    private void ShowLoadingState(bool show)
    {
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(show);
        }
    }

    /// <summary>
    /// Updates the CreatedSection text to match the selected class
    /// </summary>
    private void UpdateCreatedSectionText(string displayName)
    {
        if (createdSectionText != null)
        {
            createdSectionText.text = displayName;
            Debug.Log($"Updated CreatedSection text to: {displayName}");
        }
        else
        {
            Debug.LogWarning("CreatedSectionText reference is missing!");
        }
    }

    /// <summary>
    /// Updates the visual state of class buttons to show selection
    /// </summary>
    private void UpdateClassButtonSelection(Button selectedButton)
    {
        foreach (GameObject item in _instantiatedClassItems)
        {
            Button btn = item.GetComponent<Button>();
            if (btn != null)
            {
                ColorBlock colors = btn.colors;
                colors.normalColor = Color.white;
                btn.colors = colors;
            }
        }

        if (selectedButton != null)
        {
            ColorBlock colors = selectedButton.colors;
            colors.normalColor = Color.yellow;
            selectedButton.colors = colors;
        }
    }

    #region Event Handlers

    private void OnClassDataUpdated(Dictionary<string, List<string>> classData)
    {
        Debug.Log("Received class data update in Story Publish");
        DisplayClasses(classData);
    }

    private void OnNewClassAdded(string classCode, string displayName)
    {
        Debug.Log($"New class added: {displayName} ({classCode})");
        LoadClassData();
    }

    private void OnClassRemoved(string classCode)
    {
        Debug.Log($"Class removed: {classCode}");
        if (_selectedClassCode == classCode)
        {
            _selectedClassCode = null;
            _selectedDisplayName = null;
            InitializeCreatedSectionText();
        }
        LoadClassData();
    }

    private void OnClassUpdated(string classCode, string displayName)
    {
        Debug.Log($"Class updated: {displayName} ({classCode})");
        if (_selectedClassCode == classCode)
        {
            _selectedDisplayName = displayName;
            UpdateCreatedSectionText(displayName);
        }
        LoadClassData();
    }

    #endregion

    #region Button Click Handlers

    private void OnBackClicked()
    {
        Debug.Log("Back button clicked in Story Publish");

        if (SceneNavigationManager.Instance != null)
        {
            // Use intelligent back navigation
            SceneNavigationManager.Instance.GoBack();
        }
        else
        {
            Debug.LogError("SceneNavigationManager not found!");
            SceneManager.LoadScene("TitleScreen");
        }
    }

    private void OnCreatorModeClicked()
    {
        Debug.Log("Creator's Mode button clicked in Story Publish");

        if (SceneNavigationManager.Instance != null)
        {
            // This will set previous scene to StoryPublish before navigating
            SceneNavigationManager.Instance.GoToCreatorMode();
        }
        else
        {
            SceneManager.LoadScene("Creator'sModeScene");
        }
    }

    private void OnDashboardClicked()
    {
        Debug.Log("Dashboard button clicked in Story Publish");

        if (dashboardButton != null)
            dashboardButton.interactable = false;

        if (SceneNavigationManager.Instance != null)
        {
            // This will set previous scene to StoryPublish before navigating
            SceneNavigationManager.Instance.GoToTeacherDashboard();
        }
        else
        {
            SceneManager.LoadScene("TeacherDashboard");
        }
    }

    private void OnClassItemClicked(string classCode, string displayName, Button clickedButton)
    {
        Debug.Log($"Class item clicked: {displayName} ({classCode})");

        _selectedClassCode = classCode;
        _selectedDisplayName = displayName;

        UpdateCreatedSectionText(displayName);

        UpdateClassButtonSelection(clickedButton);

        if (createdListParent != null)
        {
            createdListParent.gameObject.SetActive(true);
        }
        RefreshCreatedStoriesList();
    }

    private void OnPublishButtonClicked()
    {
        if (string.IsNullOrEmpty(_selectedClassCode))
        {
            Debug.LogWarning("No class selected for publishing!");
            return;
        }

        if (StoryManager.Instance?.currentStory == null)
        {
            Debug.LogWarning("No current story to publish!");
            return;
        }

        if (publishPopUp != null)
            publishPopUp.SetActive(true);
    }

    private async void RefreshCreatedStoriesList()
    {
        // ✅ CRITICAL FIX: Check if this component is still valid
        if (this == null || createdListParent == null || string.IsNullOrEmpty(_selectedClassCode))
        {
            Debug.LogWarning("RefreshCreatedStoriesList called but component or parent is destroyed");
            return;
        }

        Debug.Log($"=== REFRESHING STORIES FOR CLASS: {_selectedClassCode} ===");

        if (StoryManager.Instance == null)
        {
            Debug.LogError("❌ StoryManager.Instance is NULL");
            ShowPlaceholderText(true);
            return;
        }

        // ✅ STEP 1: Validate published stories against Firestore FIRST
        if (StoryManager.Instance.UseFirestore)
        {
            await StoryManager.Instance.ValidatePublishedStories();
        }

        // ✅ Clear UI with null checks
        if (createdListParent != null)
        {
            foreach (Transform child in createdListParent)
            {
                if (child != null && child.gameObject != null)
                {
                    Destroy(child.gameObject);
                }
            }

            if (createdListParent != null && createdListParent.gameObject != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)createdListParent);
            }
        }

        // Get stories from LOCAL storage (now validated)
        var publishedStories = StoryManager.Instance.GetPublishedStoriesForClass(_selectedClassCode);

        Debug.Log($"📊 Validated local stories found: {(publishedStories == null ? "NULL" : publishedStories.Count.ToString())}");
    

        // If no local stories found AND we're using Firestore, try to sync from Firestore
        if ((publishedStories == null || publishedStories.Count == 0) && StoryManager.Instance.UseFirestore)
        {
            Debug.Log("🔄 No local stories found, syncing from Firestore...");
            ShowLoadingState(true);

            bool syncSuccess = await SyncPublishedStoriesFromFirestore(_selectedClassCode);

            // ✅ Check if component is still valid after async operation
            if (this == null || createdListParent == null)
            {
                Debug.LogWarning("Component destroyed during async operation");
                return;
            }

            if (syncSuccess)
            {
                // Now get the freshly synced local stories
                publishedStories = StoryManager.Instance.GetPublishedStoriesForClass(_selectedClassCode);
                Debug.Log($"✅ Firestore sync complete. Found {publishedStories?.Count ?? 0} stories");
            }
            else
            {
                Debug.LogWarning("❌ Firestore sync failed");
            }

            ShowLoadingState(false);
        }

        // ✅ Final check before updating UI
        if (this == null || createdListParent == null)
        {
            Debug.LogWarning("Component destroyed before UI update");
            return;
        }

        // Debug all found stories
        if (publishedStories != null)
        {
            foreach (var s in publishedStories)
            {
                Debug.Log($"[DEBUG] PublishedStory: {s.storyTitle} (ID: {s.storyId}) for class {s.classCode}");
            }
        }

        Debug.Log($"Refreshing stories for class {_selectedClassCode}. Found {publishedStories?.Count ?? 0} published stories");

        bool hasStories = publishedStories != null && publishedStories.Count > 0;
        ShowPlaceholderText(!hasStories);

        if (hasStories && createdListParent != null)
        {
            createdListParent.gameObject.SetActive(true);

            foreach (var publishedStory in publishedStories)
            {
                // ✅ Check before each UI update
                if (this == null || createdListParent == null)
                {
                    Debug.LogWarning("Component destroyed during story item creation");
                    return;
                }

                Debug.Log($"Creating UI for story: {publishedStory.storyTitle} (ID: {publishedStory.storyId}) in class: {publishedStory.classCode}");
                CreatePublishedStoryItem(publishedStory);
            }
        }
        else if (createdListParent != null)
        {
            createdListParent.gameObject.SetActive(false);
            Debug.Log("No stories found, hiding list");
        }
    }


    // ✅ Also update OnDestroy to handle cleanup better
    private void OnDestroy()
    {
        // Stop all coroutines to prevent callbacks after destruction
        StopAllCoroutines();

        // Remove button listeners
        if (creatorModeButton != null)
            creatorModeButton.onClick.RemoveAllListeners();

        if (dashboardButton != null)
            dashboardButton.onClick.RemoveAllListeners();

        if (backButton != null)
            backButton.onClick.RemoveAllListeners();

        if (deleteBackButton != null)
            deleteBackButton.onClick.RemoveAllListeners();

        if (deleteConfirmButton != null)
            deleteConfirmButton.onClick.RemoveAllListeners();

        if (publishBackButton != null)
            publishBackButton.onClick.RemoveAllListeners();

        if (publishConfirmButton != null)
            publishConfirmButton.onClick.RemoveAllListeners();

        Debug.Log("StoryPublishManager destroyed - cleaned up all listeners and coroutines");
    }


    // ✅ Also add null checks to helper methods
    private void ShowPlaceholderText(bool show)
    {
        if (this != null && placeholderText != null)
        {
            placeholderText.SetActive(show);
        }
    }


    /// <summary>
    /// Syncs published stories from Firestore and saves them locally
    /// </summary>
    private async Task<bool> SyncPublishedStoriesFromFirestore(string classCode)
    {
        if (!StoryManager.Instance.UseFirestore) 
        {
            Debug.Log("Firestore not enabled, skipping sync");
            return false;
        }
        
        try
        {
            Debug.Log($"🔄 Starting Firestore sync for class: {classCode}");
            
            // Use the existing method in StoryManager to fetch published stories from Firestore
            List<PublishedStory> firestoreStories = await StoryManager.Instance.GetPublishedStoriesFromFirestore(classCode);
            
            if (firestoreStories != null && firestoreStories.Count > 0)
            {
                Debug.Log($"📥 Retrieved {firestoreStories.Count} stories from Firestore for class {classCode}");
                
                // Save each story locally using the existing PublishStory method
                foreach (var story in firestoreStories)
                {
                    await SavePublishedStoryToLocal(story);
                }
                
                Debug.Log($"✅ Synced {firestoreStories.Count} stories from Firestore to local storage for class {classCode}");
                return true;
            }
            else
            {
                Debug.Log($"ℹ️ No stories found in Firestore for class {classCode}");
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Firestore sync failed for class {classCode}: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Saves a published story to local storage using existing StoryManager methods
    /// </summary>
    private async Task SavePublishedStoryToLocal(PublishedStory story)
    {
        try
        {
            // Get existing local stories for this class
            var localStories = StoryManager.Instance.GetPublishedStoriesForClass(story.classCode);
            
            // Check if story already exists locally
            bool exists = localStories?.Any(s => s.storyId == story.storyId) ?? false;
            
            if (!exists)
            {
                Debug.Log($"💾 Saving story to local storage: {story.storyTitle} (ID: {story.storyId})");
                
                // Use the existing PublishStory method to save locally
                // This will automatically handle the local storage
                var storyData = StoryManager.Instance.allStories.FirstOrDefault(s => s?.storyId == story.storyId);
                if (storyData != null)
                {
                    bool success = StoryManager.Instance.PublishStory(storyData, story.classCode, story.className);
                    if (success)
                    {
                        Debug.Log($"✅ Successfully saved story to local storage: {story.storyTitle}");
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ Story already exists locally: {story.storyTitle}");
                    }
                }
                else
                {
                    Debug.LogWarning($"⚠️ Could not find story data for: {story.storyTitle} (ID: {story.storyId})");
                    // Create a minimal story data for local storage
                    var tempStory = new StoryData
                    {
                        storyId = story.storyId,
                        storyTitle = story.storyTitle
                    };
                    StoryManager.Instance.PublishStory(tempStory, story.classCode, story.className);
                }
                
                // Small delay to ensure save operation completes
                await Task.Delay(10);
            }
            else
            {
                Debug.Log($"ℹ️ Story already exists locally: {story.storyTitle}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Failed to save story to local storage: {e.Message}");
        }
    }

    private void CreatePublishedStoryItem(PublishedStory publishedStory)
    {
        if (createdStoryPrefab == null || createdListParent == null)
            return;

        GameObject storyItem = Instantiate(createdStoryPrefab, createdListParent);

        TextMeshProUGUI titleText = storyItem.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI dateText = storyItem.transform.Find("DateText")?.GetComponent<TextMeshProUGUI>();
        Button deleteButton = storyItem.transform.Find("DeleteButton")?.GetComponent<Button>();

        if (titleText != null)
            titleText.text = publishedStory.storyTitle;

        if (dateText != null)
        {
            dateText.text = publishedStory.publishDate;
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(() => OnDeletePublishedStory(publishedStory));
        }
    }

    private void OnDeletePublishedStory(PublishedStory publishedStory)
    {
        _storyToDelete = publishedStory;

        if (deletePopUp != null)
            deletePopUp.SetActive(true);
    }

    #endregion

    #region Popup Handlers
    private void OnDeleteBack()
    {
        if (deletePopUp != null)
            deletePopUp.SetActive(false);

        _storyToDelete = null;
        Debug.Log("Delete cancelled");
    }

    private async void OnPublishConfirm()
    {
        if (string.IsNullOrEmpty(_selectedClassCode) || StoryManager.Instance.currentStory == null)
        {
            Debug.LogWarning("⚠️ Cannot publish: No class selected or no current story");
            return;
        }

        // Always save locally first (this should never fail)
        bool localSuccess = StoryManager.Instance.PublishStory(
            StoryManager.Instance.currentStory,
            _selectedClassCode,
            _selectedDisplayName
        );

        // Try Firestore publishing (optional - won't block if it fails)
        bool firestoreSuccess = false;
        if (StoryManager.Instance.UseFirestore)
        {
            firestoreSuccess = await StoryManager.Instance.PublishStoryToFirestore(
                StoryManager.Instance.currentStory.storyId,
                _selectedClassCode,
                _selectedDisplayName
            );
            
            if (!firestoreSuccess)
            {
                Debug.LogWarning("⚠️ Firestore publish failed, but local publish succeeded");
            }
            else
            {
                Debug.Log("✅ Story published to Firestore successfully");
            }
        }

        if (publishPopUp != null)
            publishPopUp.SetActive(false);

        RefreshCreatedStoriesList();
        
        if (localSuccess)
        {
            Debug.Log($"✅ Story published to class: {_selectedDisplayName}");
        }
    }

    private async void OnDeleteConfirm()
    {
        if (_storyToDelete != null)
        {
            // Always remove locally first (this should never fail)
            StoryManager.Instance.DeletePublishedStory(_storyToDelete.storyId, _storyToDelete.classCode);

            // Try Firestore unpublishing (optional - won't block if it fails)
            if (StoryManager.Instance.UseFirestore)
            {
                bool firestoreSuccess = await StoryManager.Instance.UnpublishStoryFromFirestore(
                    _storyToDelete.storyId,
                    _storyToDelete.classCode
                );

                if (!firestoreSuccess)
                {
                    Debug.LogWarning("⚠️ Firestore unpublish failed, but local delete succeeded");
                }
            }

            StartCoroutine(DelayedRefreshStoriesList());
            Debug.Log($"🗑️ Deleted published story: {_storyToDelete.storyTitle}");
            _storyToDelete = null;
        }

        if (deletePopUp != null)
            deletePopUp.SetActive(false);
    }

    private System.Collections.IEnumerator DelayedRefreshStoriesList()
    {
        yield return null;
        RefreshCreatedStoriesList();
    }

    private void OnPublishBack()
    {
        if (publishPopUp != null)
            publishPopUp.SetActive(false);

        Debug.Log("Publish cancelled");
    }

    #endregion

    /// <summary>
    /// Public method to get the currently selected class (if needed by other scripts)
    /// </summary>
    public string GetSelectedClassCode()
    {
        return _selectedClassCode;
    }

    /// <summary>
    /// Public method to get the currently selected display name (if needed by other scripts)
    /// </summary>
    public string GetSelectedDisplayName()
    {
        return _selectedDisplayName;
    }


    private void EnsureSceneNavigationManager()
    {
        if (SceneNavigationManager.Instance == null)
        {
            GameObject navigationObject = new GameObject("SceneNavigationManager");
            navigationObject.AddComponent<SceneNavigationManager>();
            Debug.Log("Created SceneNavigationManager instance");
        }
    }
}