using System.IO;
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
    public void GameScene()
    {
        SceneManager.LoadScene("PreviewScene");
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
    // In AddFrame.cs - add debug logging to the ValidateBackground method
    private bool ValidateBackground(StoryData story)
    {
        Debug.Log($"🔍 ValidateBackground called for story: {story.storyTitle}");

        if (string.IsNullOrEmpty(story.backgroundPath))
        {
            Debug.Log("❌ Background path is null or empty");
            return false;
        }

        Debug.Log($"📁 Background path: {story.backgroundPath}");

        // Check if it's a relative path
        string absolutePath = ImageStorage.GetAbsolutePath(story.backgroundPath);
        Debug.Log($"📁 Absolute path: {absolutePath}");

        bool fileExists = File.Exists(absolutePath);
        Debug.Log($"📁 File exists: {fileExists}");

        if (!fileExists)
        {
            // Check if UploadedTexture exists as fallback
            bool hasUploadedTexture = ImageStorage.UploadedTexture != null;
            Debug.Log($"🖼 UploadedTexture exists: {hasUploadedTexture}");

            if (hasUploadedTexture)
            {
                Debug.Log($"🖼 UploadedTexture dimensions: {ImageStorage.UploadedTexture.width}x{ImageStorage.UploadedTexture.height}");
            }
        }

        return fileExists || ImageStorage.UploadedTexture != null;
    }

}