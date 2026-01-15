using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Threading.Tasks;

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

    // ✅ NEW: Async method that handles S3 downloads
    public static async Task<Texture2D> LoadImageAsync(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("⚠️ Path is null or empty");
            return null;
        }

        // Handle S3 URLs with automatic download
        if (IsS3Url(path))
        {
            Debug.Log($"🔍 S3 URL detected in LoadImageAsync: {path}");
            string localCachePath = GetLocalCachePathFromS3Url(path);
            
            if (!string.IsNullOrEmpty(localCachePath))
            {
                string absoluteLocalPath = Path.Combine(Application.persistentDataPath, localCachePath);
                
                // ✅ STEP 1: Check if local file exists
                if (File.Exists(absoluteLocalPath))
                {
                    Debug.Log($"✅ Loading from local cache: {localCachePath}");
                    return LoadImageFromAbsolutePath(absoluteLocalPath);
                }
                else
                {
                    // ✅ STEP 2: Local file doesn't exist - download from S3
                    Debug.Log($"⬇️ Local cache missing, downloading from S3: {path}");
                    
                    // Make sure S3 service is ready
                    if (!S3StorageService.Instance.IsReady)
                    {
                        Debug.LogError("❌ S3 service not ready for download");
                        return null;
                    }
                    
                    Texture2D downloadedTexture = await S3StorageService.Instance.DownloadImage(path);
                    if (downloadedTexture != null)
                    {
                        // ✅ STEP 3: Save to local cache for future use
                        await SaveTextureToLocalCache(downloadedTexture, localCachePath);
                        Debug.Log($"✅ Downloaded and cached: {localCachePath}");
                        return downloadedTexture;
                    }
                    else
                    {
                        Debug.LogError($"❌ Failed to download from S3: {path}");
                        return null;
                    }
                }
            }
            return null;
        }

        // Handle local files with existing synchronous method
        return LoadImage(path);
    }

    // ✅ NEW: Save texture to local cache
    private static async Task SaveTextureToLocalCache(Texture2D texture, string localCachePath)
    {
        try
        {
            string absolutePath = Path.Combine(Application.persistentDataPath, localCachePath);
            
            // Ensure directory exists
            string directory = Path.GetDirectoryName(absolutePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Save the texture
            byte[] pngData = texture.EncodeToPNG();
            await File.WriteAllBytesAsync(absolutePath, pngData);
            Debug.Log($"💾 Saved to local cache: {absolutePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to save texture to cache: {ex.Message}");
        }
    }

    // ✅ NEW: Helper method to load from absolute path
    private static Texture2D LoadImageFromAbsolutePath(string absolutePath)
    {
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

    // UPDATED: Load image from relative path OR S3 URL (synchronous - for backward compatibility)
    public static Texture2D LoadImage(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("⚠️ Path is null or empty");
            return null;
        }

        // ✅ FIX: Convert S3 URL to local cache path BEFORE loading
        if (IsS3Url(path))
        {
            Debug.Log($"🔍 S3 URL detected in LoadImage: {path}");
            string localCachePath = GetLocalCachePathFromS3Url(path);
            if (!string.IsNullOrEmpty(localCachePath))
            {
                string absoluteLocalPath = Path.Combine(Application.persistentDataPath, localCachePath);
                if (File.Exists(absoluteLocalPath))
                {
                    Debug.Log($"✅ Loading from local cache: {localCachePath}");
                    return LoadImageFromAbsolutePath(absoluteLocalPath);
                }
                else
                {
                    Debug.LogWarning($"⚠️ Local cache file not found: {absoluteLocalPath}");
                    Debug.LogWarning($"💡 Use LoadImageAsync() to automatically download from S3");
                    return null;
                }
            }
            return null;
        }

        // Handle local relative paths (existing code)
        string absolutePath = Path.Combine(Application.persistentDataPath, path);

        if (!File.Exists(absolutePath))
        {
            Debug.LogWarning($"⚠️ Image file not found: {absolutePath} (relative: {path})");
            return null;
        }

        return LoadImageFromAbsolutePath(absolutePath);
    }

    // ✅ UPDATED: Check if image file exists - FIXED to actually check local files
    public static bool ImageExists(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;

        // If it's an S3 URL, check if we have a local cached version
        if (IsS3Url(path))
        {
            string localCachePath = GetLocalCachePathFromS3Url(path);
            if (!string.IsNullOrEmpty(localCachePath))
            {
                string absoluteLocalPath = Path.Combine(Application.persistentDataPath, localCachePath);
                bool localExists = File.Exists(absoluteLocalPath);
                
                if (localExists)
                {
                    Debug.Log($"✅ Found local cached version of S3 URL: {localCachePath}");
                    return true;
                }
                else
                {
                    Debug.Log($"🔍 S3 URL detected, local cache missing but exists in cloud: {path}");
                    return true; // Assume it exists in cloud
                }
            }
            return true; // Assume S3 URLs exist
        }

        // For local files, check if file exists
        string absolutePath = GetAbsolutePath(path);
        bool exists = File.Exists(absolutePath);
        Debug.Log($"🔍 Local file {(exists ? "exists" : "not found")}: {path}");
        return exists;
    }

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

    // NEW: Check if a path is an S3 URL
    public static bool IsS3Url(string path)
    {
        return !string.IsNullOrEmpty(path) &&
               (path.StartsWith("https://") || path.StartsWith("http://")) &&
               path.Contains("s3") &&
               path.Contains("amazonaws.com");
    }

    // ✅ IMPROVED: Convert S3 URL to local cache path
    public static string GetLocalCachePathFromS3Url(string s3Url)
    {
        if (!IsS3Url(s3Url)) return null;

        try
        {
            Uri uri = new Uri(s3Url);
            string path = uri.AbsolutePath;

            // Remove "/images/" prefix if present
            const string imagesPrefix = "/images/";
            if (path.StartsWith(imagesPrefix))
            {
                path = path.Substring(imagesPrefix.Length);
            }
            else if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }

            Debug.Log($"🔗 Converted S3 URL '{s3Url}' to local path: '{path}'");
            return path;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to parse S3 URL '{s3Url}': {ex.Message}");
            return null;
        }
    }


    // NEW: Convert relative path to absolute path (only for local files)
    public static string GetAbsolutePath(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return null;
        return Path.Combine(Application.persistentDataPath, relativePath);
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
