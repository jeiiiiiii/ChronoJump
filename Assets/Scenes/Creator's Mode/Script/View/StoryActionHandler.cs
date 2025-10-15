using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;

public class StoryActionHandler : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject actionPopup;
    [SerializeField] private Button editButton;
    [SerializeField] private Button viewButton;
    [SerializeField] private Button publishButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button backButton;

    [Header("Story Details UI")]
    [SerializeField] private TextMeshProUGUI storyTitleText;
    [SerializeField] private TextMeshProUGUI storyDescriptionText;
    [SerializeField] private TextMeshProUGUI dateCreatedText;
    [SerializeField] private TextMeshProUGUI dateUpdatedText;

    [Header("Delete Confirmation")]
    [SerializeField] private GameObject deleteConfirmPopup;
    [SerializeField] private Button confirmDeleteButton;
    [SerializeField] private Button cancelDeleteButton;

    private int currentStoryIndex = -1;

    void Start()
    {
        if (editButton != null) editButton.onClick.AddListener(EditStory);
        if (viewButton != null) viewButton.onClick.AddListener(ViewStory);
        if (publishButton != null) publishButton.onClick.AddListener(OnPublishedStoriesClicked);
        if (deleteButton != null) deleteButton.onClick.AddListener(ShowDeleteConfirmation);
        if (backButton != null) backButton.onClick.AddListener(ClosePopup);

        if (confirmDeleteButton != null) confirmDeleteButton.onClick.AddListener(DeleteStory);
        if (cancelDeleteButton != null) cancelDeleteButton.onClick.AddListener(CloseDeleteConfirmation);

        if (deleteConfirmPopup != null) deleteConfirmPopup.SetActive(false);
    }

    public void OnStoryButtonClicked(int storyIndex)
    {
        var stories = StoryManager.Instance.allStories;
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

        SetCurrentStory(storyIndex);
        UpdateStoryDetailsUI(story);
        
        if (actionPopup != null)
            actionPopup.SetActive(true);

        Debug.Log($"Story action popup shown. Index = {storyIndex}, Title = {story.storyTitle}");
    }

    private void UpdateStoryDetailsUI(StoryData story)
{
    if (story == null)
    {
        ClearStoryDetailsUI();
        return;
    }

    if (storyTitleText != null)
    {
        storyTitleText.text = string.IsNullOrEmpty(story.storyTitle) 
            ? "Untitled Story" 
            : story.storyTitle;
    }

    if (storyDescriptionText != null)
    {
        storyDescriptionText.text = string.IsNullOrEmpty(story.storyDescription) 
            ? "No description available" 
            : story.storyDescription;
    }

    if (dateCreatedText != null)
{
    string createdDisplay = string.IsNullOrEmpty(story.createdAt) 
        ? "Created: Unknown" 
        : $"Created: {FormatDateForDisplay(story.createdAt)}";
    dateCreatedText.text = createdDisplay;
}

        if (dateUpdatedText != null)
        {
            string updatedDisplay = string.IsNullOrEmpty(story.updatedAt)
                ? "Updated: Not modified"
                : $"Updated: {FormatDateForDisplay(story.updatedAt)}";
            dateUpdatedText.text = updatedDisplay;
        }

}

private string FormatDateForDisplay(string dateString)
{
    if (string.IsNullOrEmpty(dateString))
        return "Unknown";
    
    // First try parsing as ISO format (from Firestore)
    if (DateTime.TryParse(dateString, out DateTime date))
    {
        return date.ToString("MMM dd, yyyy hh:mm tt");
    }
    
    // If that fails, try common formats
    string[] formats = {
        "yyyy-MM-dd HH:mm:ss",
        "MM/dd/yyyy HH:mm:ss",
        "dd/MM/yyyy HH:mm:ss",
        "yyyy-MM-ddTHH:mm:ssZ",
        "MMM dd, yyyy hh:mm tt"
    };
    
    foreach (string format in formats)
    {
        if (DateTime.TryParseExact(dateString, format, System.Globalization.CultureInfo.InvariantCulture, 
            System.Globalization.DateTimeStyles.None, out date))
        {
            return date.ToString("MMM dd, yyyy hh:mm tt");
        }
    }
    
    // If all parsing fails, return the original string
    return dateString;
}



    private void ClearStoryDetailsUI()
    {
        if (storyTitleText != null) storyTitleText.text = "No Story Selected";
        if (storyDescriptionText != null) storyDescriptionText.text = "";
        if (dateCreatedText != null) dateCreatedText.text = "";
        if (dateUpdatedText != null) dateUpdatedText.text = "";
    }

    void UpdateButtonStates()
    {
        var stories = StoryManager.Instance.allStories;

        if (currentStoryIndex < 0 || currentStoryIndex >= stories.Count)
        {
            if (viewButton != null) viewButton.interactable = false;
            if (editButton != null) editButton.interactable = false;
            if (deleteButton != null) deleteButton.interactable = false;
            if (publishButton != null) publishButton.interactable = false;
            return;
        }

        var story = stories[currentStoryIndex];
        bool isSaved = story != null && !string.IsNullOrEmpty(story.createdAt);

        if (viewButton != null) viewButton.interactable = isSaved;
        if (editButton != null) editButton.interactable = isSaved;
        if (deleteButton != null) deleteButton.interactable = isSaved;
        if (publishButton != null) publishButton.interactable = isSaved;

        Debug.Log($"Button states updated - Story valid: {isSaved}");
    }

    public void SetCurrentStory(int storyIndex)
    {
        currentStoryIndex = storyIndex;
        if (storyIndex >= 0 && storyIndex < StoryManager.Instance.allStories.Count)
        {
            StoryData story = StoryManager.Instance.allStories[storyIndex];
            StoryManager.Instance.currentStory = story;
            UpdateStoryDetailsUI(story);
        }
        UpdateButtonStates();
    }

    public void EditStory()
    {
        if (currentStoryIndex < 0) return;
        if (currentStoryIndex < StoryManager.Instance.allStories.Count)
        {
            StoryManager.Instance.currentStory = StoryManager.Instance.allStories[currentStoryIndex];
            SceneManager.LoadScene("CreateNewAddTitleScene");
        }
    }

    public void ViewStory()
    {
        if (currentStoryIndex < 0) return;
        if (currentStoryIndex < StoryManager.Instance.allStories.Count)
        {
            StoryManager.Instance.currentStory = StoryManager.Instance.allStories[currentStoryIndex];
            SceneManager.LoadScene("GameScene");
        }
    }

    public void OnPublishedStoriesClicked()
    {
        Debug.Log("Published Stories button clicked in Creator's Mode");

        if (SceneNavigationManager.Instance != null)
        {
            // This will set previous scene to Creator'sModeScene before navigating
            SceneNavigationManager.Instance.GoToStoryPublish();
        }
        else
        {
            SceneManager.LoadScene("StoryPublish");
        }
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

    if (storyIndexToDelete < 0 || storyIndexToDelete >= StoryManager.Instance.allStories.Count)
    {
        Debug.LogWarning($"DeleteStory called but no valid story is selected! Index: {storyIndexToDelete}");
        return;
    }

    StoryData storyToDelete = StoryManager.Instance.allStories[storyIndexToDelete];

    if (storyToDelete != null)
    {
        Debug.Log($"=== DELETING STORY ===");
        Debug.Log($"Story: {storyToDelete.storyTitle}");
        Debug.Log($"Slot Index: {storyIndexToDelete}");
        Debug.Log($"Story's storyIndex field: {storyToDelete.storyIndex}");
        
        Debug.Log("Before deletion:");
        for (int i = 0; i < StoryManager.Instance.allStories.Count; i++)
        {
            var s = StoryManager.Instance.allStories[i];
            Debug.Log($"  [{i}] = {(s != null ? s.storyTitle : "NULL")}");
        }

        // Delete from Firestore (including subcollections)
        if (StoryManager.Instance.UseFirestore && !string.IsNullOrEmpty(storyToDelete.storyId))
        {
            DeleteStoryFromFirestore(storyToDelete.storyId);
        }

        // Delete image files
        ImageStorage.DeleteImage(storyToDelete.backgroundPath);
        ImageStorage.DeleteImage(storyToDelete.character1Path);
        ImageStorage.DeleteImage(storyToDelete.character2Path);

        // ✅ CRITICAL: Set to null, don't remove from list
        StoryManager.Instance.allStories[storyIndexToDelete] = null;
        
        Debug.Log("After setting to null:");
        for (int i = 0; i < StoryManager.Instance.allStories.Count; i++)
        {
            var s = StoryManager.Instance.allStories[i];
            Debug.Log($"  [{i}] = {(s != null ? s.storyTitle : "NULL")}");
        }

        RemoveFromPublishedStories(storyToDelete.storyId);
        
        // Save after deletion
        StoryManager.Instance.SaveStories();

        if (StoryManager.Instance.currentStory == storyToDelete)
            StoryManager.Instance.currentStory = null;

        if (ImageStorage.CurrentStoryIndex == storyIndexToDelete)
        {
            ImageStorage.UploadedTexture = null;
            ImageStorage.uploadedTexture1 = null;
            ImageStorage.uploadedTexture2 = null;
        }

        // Refresh the UI
        var viewStoriesScene = FindFirstObjectByType<ViewCreatedStoriesScene>();
        if (viewStoriesScene != null)
        {
            viewStoriesScene.RefreshBackgrounds();
        }

        Debug.Log($"=== DELETION COMPLETE ===");
    }

    CloseDeleteConfirmation();
    ClosePopup();
}

    // Updated Firestore deletion with subcollection cleanup
    private async void DeleteStoryFromFirestore(string storyId)
    {
        if (StoryManager.Instance.UseFirestore && StoryManager.Instance.IsFirebaseReady)
        {
            try
            {
                bool success = await StoryManager.Instance.DeleteStoryFromFirestore(storyId);

                if (success)
                {
                    Debug.Log($"Story {storyId} and all subcollections deleted from Firestore");
                }
                else
                {
                    Debug.LogError($"Failed to delete story {storyId} from Firestore");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error deleting story from Firestore: {ex.Message}");
            }
        }
    }



    private void RemoveFromPublishedStories(string storyId)
    {
        if (StoryManager.Instance.publishedStories != null)
        {
            int removedCount = StoryManager.Instance.publishedStories.RemoveAll(p => p.storyId == storyId);
            if (removedCount > 0)
            {
                Debug.Log($"Removed {removedCount} published story entries");
            }
        }
    }

    private void TryDeleteFile(string path)
    {
        if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
            Debug.Log($"Deleted file: {path}");
        }
    }

    public void ClosePopup()
    {
        if (actionPopup != null) actionPopup.SetActive(false);
        ClearStoryDetailsUI();
        currentStoryIndex = -1;
    }

    public void CloseDeleteConfirmation()
    {
        if (deleteConfirmPopup != null) deleteConfirmPopup.SetActive(false);
    }

    public void OnPopupShown(int storyIndex)
    {
        SetCurrentStory(storyIndex);
        UpdateButtonStates();
    }
}