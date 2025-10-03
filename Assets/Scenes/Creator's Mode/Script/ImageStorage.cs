using UnityEngine;
using UnityEngine.UI;
using System.IO;

public static class ImageStorage
{
    public static Texture2D UploadedTexture; // Temporary storage for currently uploaded image
    public static Texture2D uploadedTexture1;
    public static Texture2D uploadedTexture2;

    // Current story being edited (set when creating/editing a story)
    public static int CurrentStoryIndex = -1;

    // Method to save current uploaded image to specific story (called on "Next" click)
    public static void SaveCurrentImageToStory()
    {
        // ✅ ADD NULL CHECK WITH BETTER ERROR HANDLING
        if (StoryManager.Instance == null)
        {
            Debug.LogError("❌ StoryManager.Instance is null - cannot save images. Make sure StoryManager is initialized.");
            return;
        }

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

        // Get teacher-specific directory for images
        string teacherBaseDir = GetTeacherImageDirectory();
        if (string.IsNullOrEmpty(teacherBaseDir))
        {
            Debug.LogError("❌ Could not determine teacher directory for images");
            return;
        }

        // ✅ Background
        if (UploadedTexture != null)
        {
            string bgPath = Path.Combine(teacherBaseDir, $"story_{CurrentStoryIndex}_background.png");
            File.WriteAllBytes(bgPath, UploadedTexture.EncodeToPNG());
            story.backgroundPath = bgPath;
            Debug.Log($"✅ Saved background → {bgPath}");
        }

        // ✅ Character 1
        if (uploadedTexture1 != null)
        {
            string char1Path = Path.Combine(teacherBaseDir, $"story_{CurrentStoryIndex}_char1.png");
            File.WriteAllBytes(char1Path, uploadedTexture1.EncodeToPNG());
            story.character1Path = char1Path;
            Debug.Log($"✅ Saved character 1 → {char1Path}");
        }

        // ✅ Character 2
        if (uploadedTexture2 != null)
        {
            string char2Path = Path.Combine(teacherBaseDir, $"story_{CurrentStoryIndex}_char2.png");
            File.WriteAllBytes(char2Path, uploadedTexture2.EncodeToPNG());
            story.character2Path = char2Path;
            Debug.Log($"✅ Saved character 2 → {char2Path}");
        }

        // ✅ Save metadata (title, description, dialogues, etc.)
        // BUT DON'T SAVE TO FIRESTORE YET - only local JSON
        StoryManager.Instance.SaveStories();

        Debug.Log($"📂 All images saved for Story {CurrentStoryIndex} in teacher directory: {teacherBaseDir}");
    }

    // Get teacher-specific image directory
    private static string GetTeacherImageDirectory()
    {
        // ✅ IMPROVED NULL CHECK WITH DETAILED DEBUGGING
        if (StoryManager.Instance == null)
        {
            Debug.LogError("❌ StoryManager.Instance is null in GetTeacherImageDirectory()");
            Debug.LogError("🔍 This usually means:");
            Debug.LogError("   - StoryManager GameObject is not in the scene");
            Debug.LogError("   - StoryManager script is not attached to a GameObject"); 
            Debug.LogError("   - StoryManager hasn't finished initializing yet");
            return null;
        }

        // Use the same pattern as StoryManager for teacher-specific directories
        string teachId = StoryManager.Instance.GetCurrentTeacherId();
        
        if (string.IsNullOrEmpty(teachId))
        {
            Debug.LogWarning("⚠️ Teacher ID is null or empty, using 'default' folder");
            teachId = "default";
        }
        
        string safeTeachId = teachId.Replace("/", "_").Replace("\\", "_");
        
        string teacherBaseDir = Path.Combine(Application.persistentDataPath, safeTeachId, "StoryImages");
        
        // Ensure directory exists
        if (!Directory.Exists(teacherBaseDir))
        {
            Directory.CreateDirectory(teacherBaseDir);
            Debug.Log($"📁 Created teacher image directory: {teacherBaseDir}");
        }

        Debug.Log($"📁 Using teacher image directory: {teacherBaseDir}");
        return teacherBaseDir;
    }

    // UPDATED: Method to get background for specific story (now loads from file path)
    public static Texture2D GetBackgroundForStory(int storyIndex)
    {
        if (StoryManager.Instance == null)
        {
            Debug.LogError("❌ StoryManager.Instance is null");
            return null;
        }

        if (storyIndex < 0 || storyIndex >= StoryManager.Instance.allStories.Count)
        {
            return null;
        }

        var story = StoryManager.Instance.allStories[storyIndex];
        if (story == null || string.IsNullOrEmpty(story.backgroundPath))
        {
            return null;
        }

        return LoadImage(story.backgroundPath);
    }

    // UPDATED: Check if current story has a saved background
    public static bool CurrentStoryHasBackground()
    {
        if (StoryManager.Instance == null)
        {
            Debug.LogError("❌ StoryManager.Instance is null");
            return false;
        }

        var story = StoryManager.Instance.currentStory;
        return story != null && !string.IsNullOrEmpty(story.backgroundPath) && File.Exists(story.backgroundPath);
    }
    
    public static Texture2D LoadImage(string path)
    {
        if (!File.Exists(path)) 
        {
            Debug.LogWarning($"⚠️ Image file not found: {path}");
            return null;
        }
        
        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);
            Debug.Log($"✅ Loaded image from: {path}");
            return tex;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to load image from {path}: {ex.Message}");
            return null;
        }
    }

    // NEW: Get the correct image path for loading (teacher-specific)
    public static string GetTeacherImagePath(string filename)
    {
        string teacherBaseDir = GetTeacherImageDirectory();
        if (string.IsNullOrEmpty(teacherBaseDir)) return null;
        
        return Path.Combine(teacherBaseDir, filename);
    }

    // NEW: Clear temporary images when switching teachers or stories
    public static void ClearTemporaryImages()
    {
        UploadedTexture = null;
        uploadedTexture1 = null;
        uploadedTexture2 = null;
        Debug.Log("🔄 Cleared temporary images from memory");
    }

    // NEW: Check if a story has any images
    public static bool StoryHasImages(int storyIndex)
    {
        if (StoryManager.Instance == null || storyIndex < 0 || storyIndex >= StoryManager.Instance.allStories.Count)
        {
            return false;
        }

        var story = StoryManager.Instance.allStories[storyIndex];
        return story != null && 
               (!string.IsNullOrEmpty(story.backgroundPath) || 
                !string.IsNullOrEmpty(story.character1Path) || 
                !string.IsNullOrEmpty(story.character2Path));
    }

    // NEW: Safe method to check if StoryManager is ready
    public static bool IsReady()
    {
        bool isReady = StoryManager.Instance != null;
        if (!isReady)
        {
            Debug.LogWarning("⚠️ ImageStorage: StoryManager is not ready yet");
        }
        return isReady;
    }
}