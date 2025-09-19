using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

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
    [SerializeField] private Transform createdListParent; // Parent where published stories appear
    [SerializeField] private GameObject createdStoryPrefab; // Prefab for published story items

    [SerializeField] private GameObject deletePopUp;
    [SerializeField] private Button deleteBackButton;
    [SerializeField] private Button deleteConfirmButton;
    [SerializeField] private GameObject publishPopUp;
    [SerializeField] private Button publishBackButton;
    [SerializeField] private Button publishConfirmButton;
    
    [Header("Class List Display")]
    [SerializeField] private Transform classListParent; // Parent object where class items will be instantiated
    [SerializeField] private GameObject classItemPrefab; // Prefab for individual class items
    [SerializeField] private TextMeshProUGUI noClassesText; // Text to show when no classes exist
    
    [Header("Created Section")]
    [SerializeField] private TextMeshProUGUI createdSectionText; // Reference to the text in CreatedSection
    [SerializeField] private GameObject placeholderText; // Add this line - drag the placeholder text GameObject here
    
    [Header("Loading State")]
    [SerializeField] private GameObject loadingIndicator; // Optional loading indicator
    
    private List<GameObject> _instantiatedClassItems = new List<GameObject>();
    private string _selectedClassCode;
    private string _selectedDisplayName;
    private PublishedStory _storyToDelete; // Add this line

    private void Start()
    {
        SetupButtonListeners();
        LoadClassData();
        InitializeCreatedSectionText();
    
        // Hide the created list initially and show placeholder
        if (createdListParent != null)
        {
            createdListParent.gameObject.SetActive(false);
        }
    
        ShowPlaceholderText(true); // Add this line
    }

    private void OnEnable()
    {
        // Subscribe to class data events
        ClassDataSync.OnClassDataUpdated += OnClassDataUpdated;
        ClassDataSync.OnNewClassCreated += OnNewClassAdded;
        ClassDataSync.OnClassDeleted += OnClassRemoved;
        ClassDataSync.OnClassEdited += OnClassUpdated;
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        ClassDataSync.OnClassDataUpdated -= OnClassDataUpdated;
        ClassDataSync.OnNewClassCreated -= OnNewClassAdded;
        ClassDataSync.OnClassDeleted -= OnClassRemoved;
        ClassDataSync.OnClassEdited -= OnClassUpdated;
    }

    private void InitializeCreatedSectionText()
    {
        if (createdSectionText != null)
        {
            createdSectionText.text = "Section"; // Default text
        }
        else
        {
            Debug.LogWarning("CreatedSectionText reference is missing! Please assign it in the inspector.");
        }
    }

    private void SetupButtonListeners()
    {
        // Setup button click events
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

        // Check if ClassDataSync exists, create if needed
        if (ClassDataSync.Instance == null)
        {
            // Create ClassDataSync if it doesn't exist
            GameObject syncObject = new GameObject("ClassDataSync");
            syncObject.AddComponent<ClassDataSync>();
        }

        // Load class data
        if (ClassDataSync.Instance.IsDataLoaded())
        {
            // Use cached data
            var classData = ClassDataSync.Instance.GetCachedClassData();
            DisplayClasses(classData);
            ShowLoadingState(false);
        }
        else
        {
            // Load from Firebase
            ClassDataSync.Instance.LoadTeacherClassData(classData =>
            {
                DisplayClasses(classData);
                ShowLoadingState(false);
            });
        }
    }

    private void DisplayClasses(Dictionary<string, List<string>> classData)
    {
        // Clear existing class items
        ClearClassList();

        if (classData == null || classData.Count == 0)
        {
            ShowNoClassesMessage(true);
            return;
        }

        ShowNoClassesMessage(false);

        // Create UI items for each class
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
    
        // Find and set the text component (works with both prefabs)
        TextMeshProUGUI classText = classItem.GetComponentInChildren<TextMeshProUGUI>();
        if (classText != null)
        {
            classText.text = displayName;
        }

        // Store reference for cleanup
        _instantiatedClassItems.Add(classItem);

        // Add click functionality
        Button classButton = classItem.GetComponent<Button>();
        if (classButton != null)
        {
            classButton.onClick.AddListener(() => OnClassItemClicked(classCode, displayName, classButton));
        }
    }

    private void ClearClassList()
    {
        // Destroy all instantiated class items
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
        // Reset all buttons to normal state
        foreach (GameObject item in _instantiatedClassItems)
        {
            Button btn = item.GetComponent<Button>();
            if (btn != null)
            {
                // You can modify the visual appearance here
                // For example, change color, add outline, etc.
                ColorBlock colors = btn.colors;
                colors.normalColor = Color.white; // Default color
                btn.colors = colors;
            }
        }

        // Highlight the selected button
        if (selectedButton != null)
        {
            ColorBlock colors = selectedButton.colors;
            colors.normalColor = Color.yellow; // Highlighted color - you can change this
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
        // Refresh the display
        LoadClassData();
    }

    private void OnClassRemoved(string classCode)
    {
        Debug.Log($"Class removed: {classCode}");
        // If the removed class was selected, reset the selection
        if (_selectedClassCode == classCode)
        {
            _selectedClassCode = null;
            _selectedDisplayName = null;
            InitializeCreatedSectionText();
        }
        // Refresh the display
        LoadClassData();
    }

    private void OnClassUpdated(string classCode, string displayName)
    {
        Debug.Log($"Class updated: {displayName} ({classCode})");
        // If the updated class was selected, update the CreatedSection text
        if (_selectedClassCode == classCode)
        {
            _selectedDisplayName = displayName;
            UpdateCreatedSectionText(displayName);
        }
        // Refresh the display
        LoadClassData();
    }

    #endregion

    #region Button Click Handlers

    private void OnCreatorModeClicked()
    {
        Debug.Log("Creator's Mode button clicked in Story Publish");
        
        if (SceneNavigationManager.Instance != null)
        {
            SceneNavigationManager.Instance.GoToCreatorMode();
        }
        else
        {
            Debug.LogError("SceneNavigationManager not found! Make sure it exists in the scene.");
        }
    }

    private void OnDashboardClicked()
    {
        Debug.Log("Dashboard button clicked in Story Publish");
        
        if (SceneNavigationManager.Instance != null)
        {
            SceneNavigationManager.Instance.GoToTeacherDashboard();
        }
        else
        {
            Debug.LogError("SceneNavigationManager not found! Make sure it exists in the scene.");
        }
    }

    private void OnBackClicked()
    {
        Debug.Log("Back button clicked in Story Publish - implement as needed");
        // For now, just log the click. You mentioned to leave this alone for now.
        // In the future, this might go back to a previous scene or menu
    }

    private void OnClassItemClicked(string classCode, string displayName, Button clickedButton)
    {
        Debug.Log($"Class item clicked: {displayName} ({classCode})");
    
        // Store the selected class information
        _selectedClassCode = classCode;
        _selectedDisplayName = displayName;
    
        // Update the CreatedSection text to match the clicked class
        UpdateCreatedSectionText(displayName);
    
        // Update button visual states to show selection
        UpdateClassButtonSelection(clickedButton);
    
        // Show the created list parent and refresh the stories for this class
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
    
        // Show publish confirmation popup
        if (publishPopUp != null)
        publishPopUp.SetActive(true);
    }

    private void RefreshCreatedStoriesList()
    {
        if (createdListParent == null || string.IsNullOrEmpty(_selectedClassCode))
        return;
    
        // Clear existing items
        foreach (Transform child in createdListParent)
        {
            DestroyImmediate(child.gameObject);
        }
    
        // Get published stories for selected class
        var publishedStories = StoryManager.Instance.GetPublishedStoriesForClass(_selectedClassCode);
    
        // Show/hide placeholder based on whether there are stories
        bool hasStories = publishedStories != null && publishedStories.Count > 0;
        ShowPlaceholderText(!hasStories);
    
        // Only show the created list if there are stories
        if (hasStories)
        {
            createdListParent.gameObject.SetActive(true);
        
        // Create UI items for each published story
        foreach (var publishedStory in publishedStories)
        {
            CreatePublishedStoryItem(publishedStory);
        }
        }
        else
        {
            createdListParent.gameObject.SetActive(false);
        }
    }

    private void CreatePublishedStoryItem(PublishedStory publishedStory)
    {
        if (createdStoryPrefab == null || createdListParent == null)
        return;
    
        GameObject storyItem = Instantiate(createdStoryPrefab, createdListParent);
    
        // Find and set the text components (adjust these component names to match your prefab)
        TextMeshProUGUI titleText = storyItem.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI dateText = storyItem.transform.Find("DateText")?.GetComponent<TextMeshProUGUI>();
        Button deleteButton = storyItem.transform.Find("DeleteButton")?.GetComponent<Button>();
    
        if (titleText != null)
            titleText.text = publishedStory.storyTitle;
    
        if (dateText != null)
        {
            // Fix: Use current date if publishDate is invalid or old
            DateTime currentDate = DateTime.Now;
            dateText.text = currentDate.ToString("MMMM d, yyyy");
        }
    
        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(() => OnDeletePublishedStory(publishedStory));
        }
    }

    private void OnDeletePublishedStory(PublishedStory publishedStory)
    {
        // Store the story to delete
        _storyToDelete = publishedStory;
    
        // Show delete confirmation popup
        if (deletePopUp != null)
        deletePopUp.SetActive(true);
    }

    #endregion

    #region Popup Handlers
    private void OnDeleteBack()
    {
        // Hide delete popup and clear stored story
        if (deletePopUp != null)
        deletePopUp.SetActive(false);
    
        _storyToDelete = null;
        Debug.Log("Delete cancelled");
    }

    private void OnDeleteConfirm()
    {   
        if (_storyToDelete != null)
        {
            // Actually delete the story
            StoryManager.Instance.DeletePublishedStory(_storyToDelete.storyId, _storyToDelete.classCode);
            RefreshCreatedStoriesList();
            Debug.Log($"Deleted published story: {_storyToDelete.storyTitle}");
        
            // Clear stored story
            _storyToDelete = null;
        }
    
        // Hide delete popup
        if (deletePopUp != null)
        deletePopUp.SetActive(false);
    }

    private void OnPublishBack()
    {
        // Hide publish popup
        if (publishPopUp != null)
        publishPopUp.SetActive(false);
    
        Debug.Log("Publish cancelled");
    }

    private void OnPublishConfirm()
    {
        // Actually publish the story
        StoryManager.Instance.PublishStory(
        StoryManager.Instance.currentStory,
        _selectedClassCode,
        _selectedDisplayName
    );
    
    // Hide popup and refresh list
    if (publishPopUp != null)
        publishPopUp.SetActive(false);
    
        RefreshCreatedStoriesList();
        Debug.Log($"Story published to class: {_selectedDisplayName}");
    }

    #endregion

    /// <summary>
    /// Shows or hides the placeholder text
    /// </summary>
    private void ShowPlaceholderText(bool show)
    {   
        if (placeholderText != null)
        {
            placeholderText.SetActive(show);
        }
    }

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

    private void OnDestroy()
    {
        // Clean up button listeners
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
    }
}