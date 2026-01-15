using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AddCharacter : MonoBehaviour
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
        SceneManager.LoadScene("CreateNewAddTitleScene");
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

        SceneManager.LoadScene("CreateNewAddFrameScene");
    }

    public void Try()
    {
        SceneManager.LoadScene("GameScene");
    }
}