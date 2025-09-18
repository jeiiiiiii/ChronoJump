using UnityEngine;
using UnityEngine.UI;
using System.IO;

public static class ImageStorage
{
    public static Texture2D UploadedTexture; // Temporary storage for currently uploaded image

    // Store multiple story backgrounds (saved when "Next" is clicked)
    public static Texture2D[] StoryBackgrounds = new Texture2D[6]; // For 6 stories

    public static Texture2D uploadedTexture1;
    public static Texture2D uploadedTexture2;

    // Track which stories have backgrounds
    public static bool[] HasBackground = new bool[6];

    // Current story being edited (set when creating/editing a story)
    public static int CurrentStoryIndex = -1;

    // Method to save current uploaded image to specific story (called on "Next" click)
     public static void SaveCurrentImageToStory()
    {
        var story = StoryManager.Instance.currentStory;
        if (story == null)
        {
            Debug.LogWarning("❌ No current story, cannot save images.");
            return;
        }

        if (CurrentStoryIndex < 0)
        {
            Debug.LogWarning("❌ CurrentStoryIndex not set, cannot save images.");
            return;
        }

        string rootFolder = Path.Combine(Application.persistentDataPath, "StoryImages");
        if (!Directory.Exists(rootFolder))
            Directory.CreateDirectory(rootFolder);

        // ✅ Background
        if (UploadedTexture != null)
        {
            string bgPath = Path.Combine(rootFolder, $"story_{CurrentStoryIndex}_background.png");
            File.WriteAllBytes(bgPath, UploadedTexture.EncodeToPNG());
            story.backgroundPath = bgPath;
            Debug.Log($"✅ Saved background → {bgPath}");
        }

        // ✅ Character 1
        if (uploadedTexture1 != null)
        {
            string char1Path = Path.Combine(rootFolder, $"story_{CurrentStoryIndex}_char1.png");
            File.WriteAllBytes(char1Path, uploadedTexture1.EncodeToPNG());
            story.character1Path = char1Path;
            Debug.Log($"✅ Saved character 1 → {char1Path}");
        }

        // ✅ Character 2
        if (uploadedTexture2 != null)
        {
            string char2Path = Path.Combine(rootFolder, $"story_{CurrentStoryIndex}_char2.png");
            File.WriteAllBytes(char2Path, uploadedTexture2.EncodeToPNG());
            story.character2Path = char2Path;
            Debug.Log($"✅ Saved character 2 → {char2Path}");
        }

        // ✅ Save metadata (title, description, dialogues, etc.)
        StoryManager.Instance.SaveStories();

        Debug.Log($"📂 All images saved for Story {CurrentStoryIndex} in {rootFolder}");
    }



    // Method to get background for specific story
    public static Texture2D GetBackgroundForStory(int storyIndex)
    {
        if (storyIndex >= 0 && storyIndex < 6 && HasBackground[storyIndex])
        {
            return StoryBackgrounds[storyIndex];
        }
        return null;
    }

    // Check if current story has a saved background
    public static bool CurrentStoryHasBackground()
    {
        return CurrentStoryIndex >= 0 && CurrentStoryIndex < 6 && HasBackground[CurrentStoryIndex];
    }
    
        public static Texture2D LoadImage(string path)
    {
        if (!File.Exists(path)) return null;
        byte[] bytes = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        return tex;
    }
}