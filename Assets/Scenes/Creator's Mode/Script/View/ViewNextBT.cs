using UnityEngine;
using UnityEngine.SceneManagement;

public class NextButtonHandler : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "NextScene"; // Set in inspector
    
    public void OnNextButtonClicked()
    {
        // Save the currently uploaded image to the current story
        ImageStorage.SaveCurrentImageToStory();
        
        // Load the next scene
        SceneManager.LoadScene(nextSceneName);
    }
    
    // Alternative method if you want to validate before saving
    public void OnNextButtonClickedWithValidation()
    {
        if (ImageStorage.UploadedTexture != null)
        {
            // Save the image to current story
            ImageStorage.SaveCurrentImageToStory();
            
            // Load next scene
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            // Show warning that no image is uploaded
            Debug.LogWarning("Please upload a background image before proceeding.");
            // You could show a UI popup here instead
        }
    }
}