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
    
    [Header("Delete Confirmation")]
    [SerializeField] private GameObject deleteConfirmPopup;
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
    
    public void OnStoryButtonClicked(int storyIndex)
    {
        // Check if this slot actually has content
        if (storyIndex < 0 || storyIndex >= ImageStorage.HasBackground.Length)
            return;

        bool hasContent = ImageStorage.HasBackground[storyIndex];

        if (!hasContent)
        {
            Debug.Log("Story button clicked but it's empty (index " + storyIndex + ")");
            return; // do nothing if the slot is empty
        }

        // Otherwise, set the current story and open the action popup
        SetCurrentStory(storyIndex);

        if (actionPopup != null)
            actionPopup.SetActive(true);

        Debug.Log("Story button clicked. Current story index = " + storyIndex);
    }


    public void SetCurrentStory(int storyIndex)
    {
        currentStoryIndex = storyIndex;
        UpdateButtonStates();
    }
    
    void UpdateButtonStates()
    {
        if (currentStoryIndex < 0 || currentStoryIndex >= ImageStorage.HasBackground.Length)
            return;

        bool hasContent = ImageStorage.HasBackground[currentStoryIndex];
        
        if (viewButton != null)
            viewButton.interactable = hasContent;
            
        if (editButton != null)
            editButton.interactable = hasContent;
            
        if (deleteButton != null)
            deleteButton.interactable = hasContent;
    }
    
    public void EditStory()
    {
        // if (currentStoryIndex < 0) return;
        
        // ImageStorage.CurrentStoryIndex = currentStoryIndex;
        SceneManager.LoadScene("CreateNewAddTitleScene");
    }
    
    public void ViewStory()
    {
        // if (currentStoryIndex < 0) return;
        
        // ImageStorage.CurrentStoryIndex = currentStoryIndex;
        SceneManager.LoadScene("GameScene");
    }
    
    public void ShowDeleteConfirmation()
    {
        if (deleteConfirmPopup != null)
        {
            deleteConfirmPopup.SetActive(true);
        }
        else
        {
            DeleteStory();
        }
    }
    
    public void DeleteStory()
    {
        if (currentStoryIndex < 0) 
        {
            Debug.LogWarning("DeleteStory called but no current story is selected!");
            return;
        }
        
        if (ImageStorage.StoryBackgrounds[currentStoryIndex] != null)
        {
            Destroy(ImageStorage.StoryBackgrounds[currentStoryIndex]);
        }
        
        ImageStorage.StoryBackgrounds[currentStoryIndex] = null;
        ImageStorage.HasBackground[currentStoryIndex] = false;
        
        if (ImageStorage.CurrentStoryIndex == currentStoryIndex)
        {
            ImageStorage.CurrentStoryIndex = -1;
            ImageStorage.UploadedTexture = null;
        }
        
        CloseDeleteConfirmation();
        ClosePopup();
        
        if (backgroundManager != null)
            backgroundManager.RefreshBackgrounds();
        
        Debug.Log($"Story {currentStoryIndex + 1} deleted successfully.");
    }
    
    public void ClosePopup()
    {
        if (actionPopup != null)
            actionPopup.SetActive(false);

        currentStoryIndex = -1;
    }
    
    public void CloseDeleteConfirmation()
    {
        if (deleteConfirmPopup != null)
            deleteConfirmPopup.SetActive(false);
    }
}
