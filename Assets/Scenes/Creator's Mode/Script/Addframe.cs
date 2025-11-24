using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AddFrame : MonoBehaviour
{
    [Header("Button State Management")]
    public Button nextButton; // Assign in Inspector
    public GameObject loadingIndicator; // Optional loading spinner/text

    private bool isLoadingFromS3 = false;

    private void Start()
    {
        UpdateButtonState();
        CheckIfImagesAreLoading();
    }

    private void Update()
    {
        // Continuously check loading state while on this scene
        CheckIfImagesAreLoading();
    }

    /// <summary>
    /// Check if any images are currently being loaded from S3
    /// </summary>
    private void CheckIfImagesAreLoading()
    {
        bool previousState = isLoadingFromS3;

        // Check if ImageUploader exists and is loading
        var uploader = FindObjectOfType<ImageUploader>();
        if (uploader != null)
        {
            isLoadingFromS3 = uploader.IsLoading;
        }
        else
        {
            isLoadingFromS3 = false;
        }

        // Update button state if loading status changed
        if (previousState != isLoadingFromS3)
        {
            UpdateButtonState();
        }
    }

    private void UpdateButtonState()
    {
        if (nextButton != null)
        {
            nextButton.interactable = !isLoadingFromS3;
        }

        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(isLoadingFromS3);
        }

        if (isLoadingFromS3)
        {
            Debug.Log("⏳ Next button disabled - images loading from S3");
        }
        else
        {
            Debug.Log("✅ Next button enabled - all images loaded");
        }
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Creator'sModeScene");
    }
    
    public void Back()
    {
        SceneManager.LoadScene("CreateNewAddCharacterScene");
    }

    public void GameScene()
    {
        SceneManager.LoadScene("PreviewScene");
    }
    
    public void Next()
    {
        // Prevent proceeding if still loading from S3
        if (isLoadingFromS3)
        {
            if (ValidationManager.Instance != null)
            {
                ValidationManager.Instance.ShowWarning(
                    "Please Wait",
                    "Images are still loading from cloud storage. Please wait a moment...",
                    null,
                    () => { /* Stay on current scene */ }
                );
            }
            else
            {
                Debug.LogWarning("⚠️ Cannot proceed - images still loading from S3");
            }
            return;
        }

        var story = StoryManager.Instance.currentStory;
        if (story == null)
        {
            Debug.LogError("❌ No current story!");
            return;
        }

        // Validate background
        if (!HasValidBackground(story))
        {
            if (ValidationManager.Instance != null)
            {
                ValidationManager.Instance.ShowWarning(
                    "Background Required",
                    "Please add a background image before proceeding!",
                    null,
                    () => { /* Stay on current scene */ }
                );
            }
            return;
        }

        SceneManager.LoadScene("CreateNewAddDialogueScene");
    }
    
    public void Game()
    {
        SceneManager.LoadScene("GameScene");
    }
    
    private bool HasValidBackground(StoryData story)
    {
        if (string.IsNullOrEmpty(story.backgroundPath))
        {
            Debug.Log("❌ Background path is null or empty");
            return false;
        }
        
        // Check if the path is just a default or empty value
        if (story.backgroundPath == "default" || story.backgroundPath == "none" || story.backgroundPath == "empty")
        {
            Debug.Log($"❌ Background path is default value: {story.backgroundPath}");
            return false;
        }
        
        // For S3 URLs, assume valid if path exists (already loaded or will be loaded)
        if (ImageStorage.IsS3Url(story.backgroundPath))
        {
            Debug.Log($"✅ S3 URL found for background: {story.backgroundPath}");
            return true;
        }
        
        // For local files, check if image exists
        if (!ImageStorage.ImageExists(story.backgroundPath))
        {
            Debug.Log($"❌ Background image doesn't exist at path: {story.backgroundPath}");
            return false;
        }
        
        Debug.Log($"✅ Valid background found: {story.backgroundPath}");
        return true;
    }

    public void DebugBackgroundInfo()
    {
        var story = StoryManager.Instance.currentStory;
        if (story != null)
        {
            Debug.Log($"🔍 Background Path: '{story.backgroundPath}'");
            Debug.Log($"🔍 Is null or empty: {string.IsNullOrEmpty(story.backgroundPath)}");
            Debug.Log($"🔍 Is S3 URL: {ImageStorage.IsS3Url(story.backgroundPath)}");
            Debug.Log($"🔍 Image exists: {ImageStorage.ImageExists(story.backgroundPath)}");
            Debug.Log($"🔍 Is Loading: {isLoadingFromS3}");
        }
        else
        {
            Debug.Log("❌ No current story");
        }
    }
}
