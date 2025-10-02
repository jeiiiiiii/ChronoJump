using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StoryActionHandler : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject actionPopup;
    [SerializeField] private Button editButton;
    [SerializeField] private Button viewButton;
    [SerializeField] private Button publishButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button backButton;


    [Header("Delete Confirmation")]
    [SerializeField] private GameObject deleteConfirmPopup;
    [SerializeField] private Button confirmDeleteButton;
    [SerializeField] private Button cancelDeleteButton;

    private int currentStoryIndex = -1;

    void Start()
    {
        // Set up button listeners
        if (editButton != null) editButton.onClick.AddListener(EditStory);
        if (viewButton != null) viewButton.onClick.AddListener(ViewStory);
        if (publishButton != null) publishButton.onClick.AddListener(OnPublishedStoriesClicked);
        if (deleteButton != null) deleteButton.onClick.AddListener(ShowDeleteConfirmation);
        if (backButton != null) backButton.onClick.AddListener(ClosePopup);

        // Delete confirmation buttons
        if (confirmDeleteButton != null) confirmDeleteButton.onClick.AddListener(DeleteStory);
        if (cancelDeleteButton != null) cancelDeleteButton.onClick.AddListener(CloseDeleteConfirmation);

        if (deleteConfirmPopup != null) deleteConfirmPopup.SetActive(false);
    }

    public void OnStoryButtonClicked(int storyIndex)
    {
        var stories = StoryManager.Instance.allStories;

        // Check if this slot actually has content
        if (storyIndex < 0 || storyIndex >= stories.Count)
        {
            Debug.LogWarning($"Invalid story index: {storyIndex}");
            return;
        }

        var story = stories[storyIndex];
        if (story == null || string.IsNullOrEmpty(story.backgroundPath))
        {
            Debug.Log($"Story button clicked but it's empty (index {storyIndex})");
            return; // do nothing if empty
        }

        // Otherwise, set the current story and open the action popup
        SetCurrentStory(storyIndex);
        if (actionPopup != null)
            actionPopup.SetActive(true);

        Debug.Log($"✅ Story button clicked. Current story index = {storyIndex}, Title = {story.storyTitle}");
    }


    public void SetCurrentStory(int storyIndex)
    {
        currentStoryIndex = storyIndex;
        StoryManager.Instance.currentStory = StoryManager.Instance.stories[storyIndex];
        UpdateButtonStates();
    }

    void UpdateButtonStates()
    {
        var stories = StoryManager.Instance.stories;
        if (currentStoryIndex < 0 || currentStoryIndex >= stories.Count) return;

        bool hasContent = !string.IsNullOrEmpty(stories[currentStoryIndex].backgroundPath);
        if (viewButton != null) viewButton.interactable = hasContent;
        if (editButton != null) editButton.interactable = hasContent;
        if (deleteButton != null) deleteButton.interactable = hasContent;
        if (publishButton != null) publishButton.interactable = hasContent;
    }

    public void OnPublishedStoriesClicked()
    {
        Debug.Log("Published Stories button clicked!");
        SceneManager.LoadScene("StoryPublish");
    }

    public void EditStory()
    {
        if (currentStoryIndex < 0) return;
        StoryManager.Instance.currentStory = StoryManager.Instance.stories[currentStoryIndex];
        SceneManager.LoadScene("CreateNewAddTitleScene");
    }

    public void ViewStory()
    {
        if (currentStoryIndex < 0) return;
        StoryManager.Instance.currentStory = StoryManager.Instance.stories[currentStoryIndex];
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
        var stories = StoryManager.Instance.stories;

        if (currentStoryIndex < 0 || currentStoryIndex >= stories.Count)
        {
            Debug.LogWarning("DeleteStory called but no valid story is selected!");
            return;
        }

        // Get the story to delete
        StoryData storyToDelete = stories[currentStoryIndex];

        // Optional: Delete files
        if (storyToDelete != null)
        {
            TryDeleteFile(storyToDelete.backgroundPath);
            TryDeleteFile(storyToDelete.character1Path);
            TryDeleteFile(storyToDelete.character2Path);
        }

        // Remove from persistence
        stories[currentStoryIndex] = null;
        StoryManager.Instance.SaveStories();

        // Reset selection
        if (StoryManager.Instance.currentStory == storyToDelete)
            StoryManager.Instance.currentStory = null;

        // Clear temp uploads
        if (ImageStorage.CurrentStoryIndex == currentStoryIndex)
        {
            ImageStorage.UploadedTexture = null;
            ImageStorage.uploadedTexture1 = null;
            ImageStorage.uploadedTexture2 = null;
        }

        // Refresh UI immediately
        FindFirstObjectByType<ViewCreatedStoriesScene>()?.RefreshBackgrounds();

        // Close popups
        CloseDeleteConfirmation();
        ClosePopup();

        Debug.Log($"🗑 Story at slot {currentStoryIndex} deleted successfully.");
    }


    private void TryDeleteFile(string path)
    {
        if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
            Debug.Log($"🗑 Deleted file: {path}");
        }
    }

    public void ClosePopup()
    {
        if (actionPopup != null) actionPopup.SetActive(false);
        currentStoryIndex = -1;
    }

    public void CloseDeleteConfirmation()
    {
        if (deleteConfirmPopup != null) deleteConfirmPopup.SetActive(false);
    }
}
