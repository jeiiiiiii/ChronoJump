using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public static class ImageStorage
{
    public static Texture2D UploadedTexture;
    public static Texture2D uploadedTexture1;
    public static Texture2D uploadedTexture2;
    public static int CurrentStoryIndex = -1;

    // Constants for image types
    public const string IMAGE_TYPE_BACKGROUND = "background";
    public const string IMAGE_TYPE_CHARACTER1 = "char1";
    public const string IMAGE_TYPE_CHARACTER2 = "char2";

    // Method to save current uploaded image to specific story
    public static void SaveCurrentImageToStory()
    {
        if (StoryManager.Instance == null)
        {
            Debug.LogError("❌ StoryManager.Instance is null - cannot save images.");
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

        // ✅ Background
        if (UploadedTexture != null)
        {
            string relativePath = SaveTextureWithRelativePath(UploadedTexture, CurrentStoryIndex, IMAGE_TYPE_BACKGROUND);
            story.backgroundPath = relativePath; // Store relative path only
            Debug.Log($"✅ Saved background → {relativePath}");
        }

        // ✅ Character 1
        if (uploadedTexture1 != null)
        {
            string relativePath = SaveTextureWithRelativePath(uploadedTexture1, CurrentStoryIndex, IMAGE_TYPE_CHARACTER1);
            story.character1Path = relativePath;
            Debug.Log($"✅ Saved character 1 → {relativePath}");
        }

        // ✅ Character 2
        if (uploadedTexture2 != null)
        {
            string relativePath = SaveTextureWithRelativePath(uploadedTexture2, CurrentStoryIndex, IMAGE_TYPE_CHARACTER2);
            story.character2Path = relativePath;
            Debug.Log($"✅ Saved character 2 → {relativePath}");
        }

        // ✅ Save metadata
        StoryManager.Instance.SaveStories();
        Debug.Log($"📂 All images saved for Story {CurrentStoryIndex}");
    }

    // NEW: Save texture and return relative path
    private static string SaveTextureWithRelativePath(Texture2D texture, int storyIndex, string imageType)
    {
        string teachId = StoryManager.Instance.GetCurrentTeacherId();
        if (string.IsNullOrEmpty(teachId)) teachId = "default";
        string safeTeachId = teachId.Replace("/", "_").Replace("\\", "_");

        // Create relative path structure: teacherId/story_{index}/{imageType}.png
        string relativeDir = Path.Combine(safeTeachId, $"story_{storyIndex}");
        string relativePath = Path.Combine(relativeDir, $"{imageType}.png");
        
        // Convert to absolute path for saving
        string absolutePath = Path.Combine(Application.persistentDataPath, relativePath);
        
        // Ensure directory exists
        string absoluteDir = Path.GetDirectoryName(absolutePath);
        if (!Directory.Exists(absoluteDir))
        {
            Directory.CreateDirectory(absoluteDir);
        }

        // Save the file
        File.WriteAllBytes(absolutePath, texture.EncodeToPNG());
        
        // Return only the relative path for storage
        return relativePath;
    }

    // UPDATED: Load image from relative path
    public static Texture2D LoadImage(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
        {
            Debug.LogWarning("⚠️ Relative path is null or empty");
            return null;
        }

        // Convert relative path to absolute path
        string absolutePath = Path.Combine(Application.persistentDataPath, relativePath);
        
        if (!File.Exists(absolutePath)) 
        {
            Debug.LogWarning($"⚠️ Image file not found: {absolutePath} (relative: {relativePath})");
            return null;
        }
        
        try
        {
            byte[] bytes = File.ReadAllBytes(absolutePath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);
            Debug.Log($"✅ Loaded image from: {absolutePath}");
            return tex;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to load image from {absolutePath}: {ex.Message}");
            return null;
        }
    }

    // NEW: Convert relative path to absolute path
    public static string GetAbsolutePath(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return null;
        return Path.Combine(Application.persistentDataPath, relativePath);
    }

    // NEW: Check if image file exists (using relative path)
    public static bool ImageExists(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return false;
        string absolutePath = GetAbsolutePath(relativePath);
        return File.Exists(absolutePath);
    }

    // NEW: Delete image file (using relative path)
    public static void DeleteImage(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return;
        
        string absolutePath = GetAbsolutePath(relativePath);
        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
            Debug.Log($"🗑️ Deleted image: {absolutePath}");
        }
    }

    // NEW: For Firebase Storage - generate cloud path from relative path
    public static string GetCloudStoragePath(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return null;
        return $"story_images/{relativePath}";
    }

    // NEW: For Firebase Storage - convert download URL to relative path
    public static string GetRelativePathFromUrl(string downloadUrl)
    {
        // Extract the relative path from Firebase Storage URL
        // Example: https://firebasestorage.googleapis.com/.../default/story_0/background.png
        // Should return: default/story_0/background.png
        if (string.IsNullOrEmpty(downloadUrl)) return null;
        
        try
        {
            Uri uri = new Uri(downloadUrl);
            string path = uri.AbsolutePath;
            
            // Find the story_images part and get everything after it
            const string prefix = "/story_images/";
            int prefixIndex = path.IndexOf(prefix);
            if (prefixIndex >= 0)
            {
                return path.Substring(prefixIndex + prefix.Length);
            }
            
            return path.TrimStart('/');
        }
        catch
        {
            return null;
        }
    }

    // Rest of your existing methods remain the same, but updated to use relative paths:
    public static Texture2D GetBackgroundForStory(int storyIndex)
    {
        if (StoryManager.Instance == null) return null;
        if (storyIndex < 0 || storyIndex >= StoryManager.Instance.allStories.Count) return null;

        var story = StoryManager.Instance.allStories[storyIndex];
        if (story == null || string.IsNullOrEmpty(story.backgroundPath)) return null;

        return LoadImage(story.backgroundPath); // Now uses relative path
    }

    public static bool CurrentStoryHasBackground()
    {
        if (StoryManager.Instance == null) return false;
        var story = StoryManager.Instance.currentStory;
        return story != null && !string.IsNullOrEmpty(story.backgroundPath) && ImageExists(story.backgroundPath);
    }

    public static bool StoryHasImages(int storyIndex)
    {
        if (StoryManager.Instance == null || storyIndex < 0 || storyIndex >= StoryManager.Instance.allStories.Count)
            return false;

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