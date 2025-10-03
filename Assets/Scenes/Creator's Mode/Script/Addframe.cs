using UnityEngine;
using UnityEngine.SceneManagement;

public class AddFrame : MonoBehaviour
{
    public void MainMenu()
    {
        SceneManager.LoadScene("Creator'sModeScene");
    }
    
    public void Back()
    {
        SceneManager.LoadScene("CreateNewAddCharacterScene");
    }
    
    public void Next()
    {
        DebugBackgroundInfo(); // Add this line here temporarily
        
        var story = StoryManager.Instance.currentStory;
        if (story == null)
        {
            Debug.LogError("❌ No current story!");
            return;
        }

        // Better validation - check if background actually exists
        if (!HasValidBackground(story))
        {
            ValidationManager.Instance.ShowWarning(
                "Background Required",
                "Please add a background image before proceeding!",
                null,
                () => { /* Stay on current scene */ }
            );
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
        
        // Check if the image file actually exists
        if (!ImageStorage.ImageExists(story.backgroundPath))
        {
            Debug.Log($"❌ Background image doesn't exist at path: {story.backgroundPath}");
            return false;
        }
        
        Debug.Log($"✅ Valid background found: {story.backgroundPath}");
        return true;
    }
    
    // ADD THE DEBUG METHOD HERE:
    public void DebugBackgroundInfo()
    {
        var story = StoryManager.Instance.currentStory;
        if (story != null)
        {
            Debug.Log($"🔍 Background Path: '{story.backgroundPath}'");
            Debug.Log($"🔍 Is null or empty: {string.IsNullOrEmpty(story.backgroundPath)}");
            Debug.Log($"🔍 Image exists: {ImageStorage.ImageExists(story.backgroundPath)}");
        }
        else
        {
            Debug.Log("❌ No current story");
        }
    }
}