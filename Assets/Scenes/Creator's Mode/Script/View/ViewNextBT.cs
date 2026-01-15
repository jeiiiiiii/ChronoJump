using UnityEngine;
using UnityEngine.SceneManagement;

public class NextButtonHandler : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "NextScene"; // Set in inspector

    public void OnNextButtonClicked()
    {
        SaveBackgroundAndProceed();
    }

    public void OnNextButtonClickedWithValidation()
    {
        if (ImageStorage.UploadedTexture != null)
        {
            SaveBackgroundAndProceed();
        }
        else
        {
            Debug.LogWarning("Please upload a background image before proceeding.");
        }
    }

    private void SaveBackgroundAndProceed()
    {
        if (StoryManager.Instance.currentStory != null && ImageStorage.UploadedTexture != null)
        {
            StoryManager.Instance.SaveBackground(ImageStorage.UploadedTexture);
            StoryManager.Instance.SaveStories(); // persist JSON immediately
        }


        // Load the next scene
        SceneManager.LoadScene(nextSceneName);
    }
}
