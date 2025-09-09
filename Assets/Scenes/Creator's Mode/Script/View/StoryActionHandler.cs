using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StoryActionHandler : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject actionPopup;
    [SerializeField] private Button editButton;
    [SerializeField] private Button viewButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button backButton;
    
    [Header("Scene Names")]
    [SerializeField] private string editSceneName = "CreateNewAddFrameScene";
    [SerializeField] private string viewSceneName = "ViewCreatedStoriesScene"; // Scene for viewing/playing story
    
    [Header("Delete Confirmation")]
    [SerializeField] private GameObject deleteConfirmPopup; // Optional: confirmation popup
    [SerializeField] private Button confirmDeleteButton;
    [SerializeField] private Button cancelDeleteButton;
    
    private int currentStoryIndex = -1;
    private ViewStoriesBackgroundManager backgroundManager;
    
    void Start()
    {
        // Set up button listeners
        if (editButton != null)
            editButton.onClick.AddListener(EditStory);
            
        if (viewButton != null)
            viewButton.onClick.AddListener(ViewStory);
            
        if (deleteButton != null)
            deleteButton.onClick.AddListener(ShowDeleteConfirmation);
            
        if (backButton != null)
            backButton.onClick.AddListener(ClosePopup);
            
        // Delete confirmation buttons
        if (confirmDeleteButton != null)
            confirmDeleteButton.onClick.AddListener(DeleteStory);
            
        if (cancelDeleteButton != null)
            cancelDeleteButton.onClick.AddListener(CloseDeleteConfirmation);

        if (deleteConfirmPopup != null)
            deleteConfirmPopup.SetActive(false);
        
        // Get reference to background manager
        backgroundManager = FindObjectOfType<ViewStoriesBackgroundManager>();
    }
    
    public void SetCurrentStory(int storyIndex)
    {
        currentStoryIndex = storyIndex;
        
        // Update button states based on story content
        UpdateButtonStates();
    }
    
    void UpdateButtonStates()
    {
        bool hasContent = ImageStorage.HasBackground[currentStoryIndex];
        
        // Enable/disable buttons based on story state
        if (viewButton != null)
            viewButton.interactable = hasContent;
            
        if (editButton != null)
            editButton.interactable = hasContent;
            
        if (deleteButton != null)
            deleteButton.interactable = hasContent;
    }
    
    public void EditStory()
    {
        if (currentStoryIndex < 0) return;
        
        // Set the story to edit
        ImageStorage.CurrentStoryIndex = currentStoryIndex;
        
        // Load the editing scene
        SceneManager.LoadScene(editSceneName);
    }
    
    public void ViewStory()
    {
        if (currentStoryIndex < 0) return;
        
        // Set the story to view
        ImageStorage.CurrentStoryIndex = currentStoryIndex;
        
        // Load the viewing scene
        SceneManager.LoadScene(viewSceneName);
    }
    
    public void ShowDeleteConfirmation()
    {
        if (deleteConfirmPopup != null)
        {
            deleteConfirmPopup.SetActive(true);
        }
        else
        {
            // No confirmation popup, delete immediately
            DeleteStory();
        }
    }
    
    public void DeleteStory()
    {
        if (currentStoryIndex < 0) return;
        
        // Clear the story data
        if (ImageStorage.StoryBackgrounds[currentStoryIndex] != null)
        {
            // Destroy the texture to free memory
            Destroy(ImageStorage.StoryBackgrounds[currentStoryIndex]);
        }
        
        ImageStorage.StoryBackgrounds[currentStoryIndex] = null;
        ImageStorage.HasBackground[currentStoryIndex] = false;
        
        // If this was the current story being edited, clear it
        if (ImageStorage.CurrentStoryIndex == currentStoryIndex)
        {
            ImageStorage.CurrentStoryIndex = -1;
            ImageStorage.UploadedTexture = null;
        }
        
        // Close popups
        CloseDeleteConfirmation();
        ClosePopup();
        
        // Refresh the display
        if (backgroundManager != null)
        {
            backgroundManager.RefreshBackgrounds();
        }
        
        Debug.Log($"Story {currentStoryIndex + 1} deleted successfully.");
    }
    
    public void ClosePopup()
    {
        if (actionPopup != null)
        {
            actionPopup.SetActive(false);
        }
        currentStoryIndex = -1;
    }
    
    public void CloseDeleteConfirmation()
    {
        if (deleteConfirmPopup != null)
        {
            deleteConfirmPopup.SetActive(false);
        }
    }
}