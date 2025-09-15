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
            Texture2D savedBackground = LoadTextureFromFile(CurrentStory.backgroundPath);
            if (savedBackground != null)
            {
                ApplyTexture(savedBackground);
                Debug.Log($"📖 Loaded background for story '{CurrentStory.storyTitle}'");
                return;
            }
        }

        // 2. If no saved path but user uploaded during session → apply and save
        if (ImageStorage.UploadedTexture != null)
        {
            ApplyTexture(ImageStorage.UploadedTexture);
            Debug.Log($"🖼 Applied uploaded background for story '{CurrentStory.storyTitle}'");

            // ⚡ Save the uploaded background as this story’s permanent background
            string path = SaveTextureToFile(ImageStorage.UploadedTexture, CurrentStory.storyId);
            CurrentStory.backgroundPath = path;
            StoryManager.Instance.SaveStories();
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

    private Texture2D LoadTextureFromFile(string path)
    {
        if (!System.IO.File.Exists(path)) return null;
        byte[] fileData = System.IO.File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
        return tex;
    }

    private string SaveTextureToFile(Texture2D tex, string storyId)
    {
        string folder = Application.persistentDataPath + "/Backgrounds/";
        if (!System.IO.Directory.Exists(folder))
            System.IO.Directory.CreateDirectory(folder);

        string path = folder + storyId + ".png";
        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        return path;
    }
}
