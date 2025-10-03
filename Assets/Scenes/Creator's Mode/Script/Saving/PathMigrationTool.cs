using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class PathMigrationTool
{
    // Constants for image types (ADD THESE)
    private const string IMAGE_TYPE_BACKGROUND = "background";
    private const string IMAGE_TYPE_CHARACTER1 = "char1";
    private const string IMAGE_TYPE_CHARACTER2 = "char2";

    [System.Serializable]
    public class MigrationResult
    {
        public int storiesProcessed = 0;
        public int pathsMigrated = 0;
        public int errors = 0;
        public List<string> migratedFiles = new List<string>();
    }

    // Main migration method
    public static MigrationResult MigrateAllStoriesToRelativePaths()
    {
        var result = new MigrationResult();
        
        if (StoryManager.Instance == null)
        {
            Debug.LogError("❌ StoryManager not available for migration");
            result.errors++;
            return result;
        }

        Debug.Log("🔄 Starting path migration from absolute to relative paths...");

        foreach (var story in StoryManager.Instance.allStories)
        {
            if (story == null) continue;

            result.storiesProcessed++;
            Debug.Log($"🔧 Processing story: {story.storyTitle}");

            // Migrate background path
            if (!string.IsNullOrEmpty(story.backgroundPath))
            {
                string newPath = ConvertToRelativePath(story.backgroundPath, story.storyIndex, IMAGE_TYPE_BACKGROUND);
                if (newPath != null)
                {
                    story.backgroundPath = newPath;
                    result.pathsMigrated++;
                    result.migratedFiles.Add($"Background: {story.backgroundPath}");
                }
            }

            // Migrate character1 path
            if (!string.IsNullOrEmpty(story.character1Path))
            {
                string newPath = ConvertToRelativePath(story.character1Path, story.storyIndex, IMAGE_TYPE_CHARACTER1);
                if (newPath != null)
                {
                    story.character1Path = newPath;
                    result.pathsMigrated++;
                    result.migratedFiles.Add($"Character1: {story.character1Path}");
                }
            }

            // Migrate character2 path
            if (!string.IsNullOrEmpty(story.character2Path))
            {
                string newPath = ConvertToRelativePath(story.character2Path, story.storyIndex, IMAGE_TYPE_CHARACTER2);
                if (newPath != null)
                {
                    story.character2Path = newPath;
                    result.pathsMigrated++;
                    result.migratedFiles.Add($"Character2: {story.character2Path}");
                }
            }
        }

        // Save the migrated stories
        StoryManager.Instance.SaveStories();
        Debug.Log($"✅ Migration complete! Processed {result.storiesProcessed} stories, migrated {result.pathsMigrated} paths");

        return result;
    }

    // Convert absolute path to relative path
    private static string ConvertToRelativePath(string absolutePath, int storyIndex, string imageType)
    {
        if (string.IsNullOrEmpty(absolutePath))
            return null;

        // Check if it's already a relative path
        if (IsRelativePath(absolutePath))
        {
            Debug.Log($"ℹ️ Path is already relative: {absolutePath}");
            return absolutePath;
        }

        // Check if file exists
        if (!File.Exists(absolutePath))
        {
            Debug.LogWarning($"⚠️ File not found, skipping: {absolutePath}");
            return null;
        }

        try
        {
            string teachId = StoryManager.Instance.GetCurrentTeacherId();
            if (string.IsNullOrEmpty(teachId)) teachId = "default";
            string safeTeachId = teachId.Replace("/", "_").Replace("\\", "_");

            // Create new relative path structure
            string relativeDir = Path.Combine(safeTeachId, $"story_{storyIndex}");
            string relativePath = Path.Combine(relativeDir, $"{imageType}.png");
            string newAbsolutePath = Path.Combine(Application.persistentDataPath, relativePath);

            // Create directory if it doesn't exist
            string newAbsoluteDir = Path.GetDirectoryName(newAbsolutePath);
            if (!Directory.Exists(newAbsoluteDir))
            {
                Directory.CreateDirectory(newAbsoluteDir);
            }

            // Copy file to new location
            File.Copy(absolutePath, newAbsolutePath, true);
            Debug.Log($"📁 Migrated: {absolutePath} → {relativePath}");

            return relativePath;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to migrate path {absolutePath}: {ex.Message}");
            return null;
        }
    }

    // Check if path is already relative
    private static bool IsRelativePath(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        
        // Relative paths shouldn't start with drive letters or root slashes
        return !Path.IsPathRooted(path) && 
               !path.StartsWith(Application.persistentDataPath) &&
               !path.Contains(":\\") && 
               !path.StartsWith("/") &&
               !path.StartsWith("\\");
    }

    // Clean up old absolute path files (use with caution!)
    public static void CleanupOldAbsolutePaths()
    {
        Debug.Log("🧹 Cleaning up old absolute path files...");
        int deletedCount = 0;

        foreach (var story in StoryManager.Instance.allStories)
        {
            if (story == null) continue;

            // After migration, these should be relative paths
            // Old absolute paths might still exist, but we'll keep them as backup
            Debug.Log($"ℹ️ Story '{story.storyTitle}': Background={story.backgroundPath}, Char1={story.character1Path}, Char2={story.character2Path}");
        }

        Debug.Log($"ℹ️ Cleanup complete. {deletedCount} old files deleted (kept as backup for safety)");
    }

    // Validation method to check migration status
    public static void ValidateMigration()
    {
        Debug.Log("🔍 Validating path migration...");
        int relativePaths = 0;
        int absolutePaths = 0;
        int missingFiles = 0;

        foreach (var story in StoryManager.Instance.allStories)
        {
            if (story == null) continue;

            CheckPath(story.backgroundPath, ref relativePaths, ref absolutePaths, ref missingFiles);
            CheckPath(story.character1Path, ref relativePaths, ref absolutePaths, ref missingFiles);
            CheckPath(story.character2Path, ref relativePaths, ref absolutePaths, ref missingFiles);
        }

        Debug.Log($"📊 Migration Validation:");
        Debug.Log($"   Relative Paths: {relativePaths}");
        Debug.Log($"   Absolute Paths: {absolutePaths}");
        Debug.Log($"   Missing Files: {missingFiles}");

        if (absolutePaths > 0)
        {
            Debug.LogWarning($"⚠️ Still found {absolutePaths} absolute paths that need migration");
        }
    }

    private static void CheckPath(string path, ref int relative, ref int absolute, ref int missing)
    {
        if (string.IsNullOrEmpty(path)) return;

        if (IsRelativePath(path))
        {
            relative++;
            // Check if file exists in new location
            string absolutePath = Path.Combine(Application.persistentDataPath, path);
            if (!File.Exists(absolutePath))
            {
                missing++;
                Debug.LogWarning($"⚠️ Missing migrated file: {path}");
            }
        }
        else
        {
            absolute++;
            Debug.LogWarning($"⚠️ Found absolute path: {path}");
        }
    }

    // Method to check if migration is needed
    public static bool CheckIfMigrationNeeded()
    {
        if (StoryManager.Instance == null) return false;

        foreach (var story in StoryManager.Instance.allStories)
        {
            if (story == null) continue;
            
            if (!string.IsNullOrEmpty(story.backgroundPath) && 
                !IsRelativePath(story.backgroundPath))
                return true;
                
            if (!string.IsNullOrEmpty(story.character1Path) && 
                !IsRelativePath(story.character1Path))
                return true;
                
            if (!string.IsNullOrEmpty(story.character2Path) && 
                !IsRelativePath(story.character2Path))
                return true;
        }
        return false;
    }
}