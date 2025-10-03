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

        // ✅ FIXED: More robust validation
        bool isValidStory = false;
        StoryData story = null;

        if (storyIndex >= 0 && storyIndex < stories.Count)
        {
            story = stories[storyIndex];
            isValidStory = story != null && !string.IsNullOrEmpty(story.createdAt);
        }

        if (!isValidStory)
        {
            Debug.LogWarning($"Invalid or unsaved story at index: {storyIndex}");
            return;
        }

        // Show popup only for valid, saved stories
        SetCurrentStory(storyIndex);
        if (actionPopup != null)
            actionPopup.SetActive(true);

        Debug.Log($"Story action popup shown. Index = {storyIndex}, Title = {story.storyTitle}");
    }
    void UpdateButtonStates()
    {
        var stories = StoryManager.Instance.allStories; // ✅ Use allStories

        if (currentStoryIndex < 0 || currentStoryIndex >= stories.Count)
        {
            // If no valid story, disable all buttons
            if (viewButton != null) viewButton.interactable = false;
            if (editButton != null) editButton.interactable = false;
            if (deleteButton != null) deleteButton.interactable = false;
            if (publishButton != null) publishButton.interactable = false;
            return;
        }

        var story = stories[currentStoryIndex];
        // Enable buttons if story is saved and not null
        bool isSaved = story != null && !string.IsNullOrEmpty(story.createdAt);

        if (viewButton != null) viewButton.interactable = isSaved;
        if (editButton != null) editButton.interactable = isSaved;
        if (deleteButton != null) deleteButton.interactable = isSaved;
        if (publishButton != null) publishButton.interactable = isSaved;

        Debug.Log($"🔘 Button states updated - Story valid: {isSaved}");
    }



    public void SetCurrentStory(int storyIndex)
    {
        currentStoryIndex = storyIndex;
        // ✅ FIXED: Use allStories consistently
        if (storyIndex >= 0 && storyIndex < StoryManager.Instance.allStories.Count)
        {
            StoryManager.Instance.currentStory = StoryManager.Instance.allStories[storyIndex];
        }
        UpdateButtonStates();
    }

    public void EditStory()
    {
        if (currentStoryIndex < 0) return;
        // ✅ FIXED: Use allStories consistently
        if (currentStoryIndex < StoryManager.Instance.allStories.Count)
        {
            StoryManager.Instance.currentStory = StoryManager.Instance.allStories[currentStoryIndex];
            SceneManager.LoadScene("CreateNewAddTitleScene");
        }
    }

    public void ViewStory()
    {
        if (currentStoryIndex < 0) return;
        // ✅ FIXED: Use allStories consistently
        if (currentStoryIndex < StoryManager.Instance.allStories.Count)
        {
            StoryManager.Instance.currentStory = StoryManager.Instance.allStories[currentStoryIndex];
            SceneManager.LoadScene("GameScene");
        }
    }

    public void OnPublishedStoriesClicked()
    {
        Debug.Log("Published Stories button clicked!");
        SceneManager.LoadScene("StoryPublish");
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
        int storyIndexToDelete = currentStoryIndex;
        var stories = StoryManager.Instance.allStories;

        if (storyIndexToDelete < 0 || storyIndexToDelete >= stories.Count)
        {
            Debug.LogWarning($"DeleteStory called but no valid story is selected! Index: {storyIndexToDelete}");
            return;
        }

        StoryData storyToDelete = stories[storyIndexToDelete];

        if (storyToDelete != null)
        {
            Debug.Log($"🗑️ Deleting story: {storyToDelete.storyTitle} (Index: {storyIndexToDelete})");

            // ✅ FIXED: Delete from Firebase if using Firestore
            if (StoryManager.Instance.UseFirestore && !string.IsNullOrEmpty(storyToDelete.storyId))
            {
                DeleteStoryFromFirestore(storyToDelete.storyId);
            }

            // ✅ UPDATED: Delete local files using ImageStorage with relative paths
            ImageStorage.DeleteImage(storyToDelete.backgroundPath);
            ImageStorage.DeleteImage(storyToDelete.character1Path);
            ImageStorage.DeleteImage(storyToDelete.character2Path);

            // Remove from list
            if (storyIndexToDelete < stories.Count)
            {
                stories.RemoveAt(storyIndexToDelete);
            }

            // Remove from published stories
            RemoveFromPublishedStories(storyToDelete.storyId);

            // Save changes
            StoryManager.Instance.SaveStories();

            // Reset selection and clear temp data
            if (StoryManager.Instance.currentStory == storyToDelete)
                StoryManager.Instance.currentStory = null;

            if (ImageStorage.CurrentStoryIndex == storyIndexToDelete)
            {
                ImageStorage.UploadedTexture = null;
                ImageStorage.uploadedTexture1 = null;
                ImageStorage.uploadedTexture2 = null;
            }

            // Refresh UI
            var viewStoriesScene = FindFirstObjectByType<ViewCreatedStoriesScene>();
            if (viewStoriesScene != null)
            {
                viewStoriesScene.RefreshBackgrounds();
            }

            Debug.Log($"🗑️ Story at slot {storyIndexToDelete} deleted successfully.");
        }

        CloseDeleteConfirmation();
        ClosePopup();
    }


    // ✅ UPDATED: Use StoryManager's integrated Firestore method
    private async void DeleteStoryFromFirestore(string storyId)
    {
        if (StoryManager.Instance.UseFirestore && StoryManager.Instance.IsFirebaseReady)
        {
            try
            {
                bool success = await StoryManager.Instance.DeleteStoryFromFirestore(storyId);

                if (success)
                {
                    Debug.Log($"✅ Story {storyId} deleted from Firestore");
                }
                else
                {
                    Debug.LogError($"❌ Failed to delete story {storyId} from Firestore");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Error deleting story from Firestore: {ex.Message}");
            }
        }
    }


    // ✅ NEW: Method to remove from published stories
    private void RemoveFromPublishedStories(string storyId)
    {
        if (StoryManager.Instance.publishedStories != null)
        {
            int removedCount = StoryManager.Instance.publishedStories.RemoveAll(p => p.storyId == storyId);
            if (removedCount > 0)
            {
                Debug.Log($"✅ Removed {removedCount} published story entries");
            }
        }
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

    // ✅ NEW: Method to be called when popup is shown via StorySelector
    public void OnPopupShown(int storyIndex)
    {
        SetCurrentStory(storyIndex);
        UpdateButtonStates();
    }

}

