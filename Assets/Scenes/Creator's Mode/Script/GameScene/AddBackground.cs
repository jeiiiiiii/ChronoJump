using UnityEngine;
using UnityEngine.UI;

public class BackgroundSetter : MonoBehaviour
{
    public RawImage backgroundImage;

    private StoryData CurrentStory => StoryManager.Instance.currentStory;

    void Start()
    {
        if (CurrentStory == null)
        {
            Debug.LogWarning("⚠ No current story found in StoryManager.");
            return;
        }

        // 1. If story already has a saved background path → load from disk
        if (!string.IsNullOrEmpty(CurrentStory.backgroundPath))
        {
            Texture2D savedBackground = ImageStorage.LoadImage(CurrentStory.backgroundPath);
            if (savedBackground != null)
            {
                ApplyTexture(savedBackground);
                Debug.Log($"📖 Loaded background for story '{CurrentStory.storyTitle}' from: {CurrentStory.backgroundPath}");
                return;
            }
        }

        // 2. If no saved path but user uploaded during session → just apply (don't save)
        if (ImageStorage.UploadedTexture != null)
        {
            ApplyTexture(ImageStorage.UploadedTexture);
            Debug.Log($"🖼 Applied temporary uploaded background for story '{CurrentStory.storyTitle}'");
            
            // ❌ REMOVED: The redundant SaveTextureToFile call
            // ImageUploader.cs already calls ImageStorage.SaveCurrentImageToStory() when images are uploaded
            // So we don't need to save again here
        }
    }

    private void ApplyTexture(Texture2D tex)
    {
        backgroundImage.texture = tex;

        AspectRatioFitter fitter = backgroundImage.GetComponent<AspectRatioFitter>();
        if (fitter != null)
        {
            fitter.aspectRatio = (float)tex.width / tex.height;
        }
    }

    // ❌ COMPLETELY REMOVE these methods - they're not needed anymore
    // private Texture2D LoadTextureFromFile(string path) { ... }
    // private string SaveTextureToFile(Texture2D tex, string storyId) { ... }
}