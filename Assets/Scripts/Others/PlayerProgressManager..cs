using UnityEngine;

/// <summary>
/// Bridge class that redirects calls to GameProgressManager for backward compatibility.
/// This ensures existing code that calls PlayerProgressManager still works.
/// You can eventually remove this class once all references are updated.
/// </summary>
public static class PlayerProgressManager
{
    private static readonly string[] allCivilizations = { "Sumerian", "Akkadian", "Babylonian", "Assyrian", "Harappa", "Sining", "HuangHe" };

    /// <summary>
    /// Unlocks a civilization through GameProgressManager.
    /// This is a bridge method for backward compatibility.
    /// </summary>
    public static void UnlockCivilization(string civName)
    {
        if (!IsValidCivilization(civName))
        {
            Debug.LogWarning("Invalid civilization name: " + civName);
            return;
        }

        // Use GameProgressManager if available
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.CurrentStudentState != null)
        {
            GameProgressManager.Instance.UnlockCivilization(civName);
            Debug.Log($"Unlocked civilization {civName} through GameProgressManager");
        }
        else
        {
            // Fallback to StudentPrefs if GameProgressManager is not available
            StudentPrefs.SetInt(civName + "_unlocked", 1);
            StudentPrefs.Save();
            Debug.LogWarning($"GameProgressManager not available. Unlocked {civName} using StudentPrefs fallback.");
        }
    }

    /// <summary>
    /// Unlocks a chapter through GameProgressManager.
    /// This is a bridge method for backward compatibility.
    /// </summary>
    public static void UnlockChapter(string chapterId)
    {
        if (string.IsNullOrEmpty(chapterId))
        {
            Debug.LogWarning("Invalid chapter ID: " + chapterId);
            return;
        }

        // Use GameProgressManager if available
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.CurrentStudentState != null)
        {
            GameProgressManager.Instance.UnlockChapter(chapterId);
            Debug.Log($"Unlocked chapter {chapterId} through GameProgressManager");
        }
        else
        {
            // Fallback to StudentPrefs if GameProgressManager is not available
            // Load existing unlocked chapters
            var chaptersJson = StudentPrefs.GetString("unlockedChapters", "");
            var unlockedChapters = new System.Collections.Generic.List<string>();
            
            if (!string.IsNullOrEmpty(chaptersJson))
            {
                try
                {
                    var wrapper = JsonUtility.FromJson<StringListWrapper>(chaptersJson);
                    if (wrapper != null && wrapper.list != null)
                    {
                        unlockedChapters = wrapper.list;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to parse unlockedChapters from StudentPrefs: {e.Message}");
                }
            }

            // Add the new chapter if not already unlocked
            if (!unlockedChapters.Contains(chapterId))
            {
                unlockedChapters.Add(chapterId);
                
                // Save back to StudentPrefs
                var newChaptersJson = JsonUtility.ToJson(new StringListWrapper { list = unlockedChapters });
                StudentPrefs.SetString("unlockedChapters", newChaptersJson);
                StudentPrefs.Save();
                
                Debug.Log($"Unlocked chapter {chapterId} using StudentPrefs fallback.");
            }
            else
            {
                Debug.Log($"Chapter {chapterId} was already unlocked in StudentPrefs.");
            }
        }
    }

    /// <summary>
    /// Checks if a civilization is unlocked through GameProgressManager.
    /// Falls back to StudentPrefs if GameProgressManager is not available.
    /// </summary>
    public static bool IsCivilizationUnlocked(string civName)
    {
        if (!IsValidCivilization(civName))
        {
            Debug.LogWarning("Invalid civilization name: " + civName);
            return false;
        }

        // Use GameProgressManager if available
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.CurrentStudentState != null)
        {
            return GameProgressManager.Instance.IsCivilizationUnlocked(civName);
        }
        else
        {
            // Fallback to StudentPrefs
            bool isUnlocked = StudentPrefs.GetInt(civName + "_unlocked", civName == "Sumerian" ? 1 : 0) == 1;
            Debug.LogWarning($"GameProgressManager not available. Checking {civName} unlock status using StudentPrefs fallback: {isUnlocked}");
            return isUnlocked;
        }
    }

    /// <summary>
    /// Checks if a chapter is unlocked through GameProgressManager.
    /// Falls back to StudentPrefs if GameProgressManager is not available.
    /// </summary>
    public static bool IsChapterUnlocked(string chapterId)
    {
        if (string.IsNullOrEmpty(chapterId))
        {
            Debug.LogWarning("Invalid chapter ID: " + chapterId);
            return false;
        }

        // Use GameProgressManager if available
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.CurrentStudentState != null)
        {
            return GameProgressManager.Instance.IsChapterUnlocked(chapterId);
        }
        else
        {
            // Fallback to StudentPrefs
            var chaptersJson = StudentPrefs.GetString("unlockedChapters", "");
            
            if (!string.IsNullOrEmpty(chaptersJson))
            {
                try
                {
                    var wrapper = JsonUtility.FromJson<StringListWrapper>(chaptersJson);
                    if (wrapper != null && wrapper.list != null)
                    {
                        bool isUnlocked = wrapper.list.Contains(chapterId);
                        Debug.LogWarning($"GameProgressManager not available. Checking {chapterId} unlock status using StudentPrefs fallback: {isUnlocked}");
                        return isUnlocked;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to parse unlockedChapters from StudentPrefs: {e.Message}");
                }
            }
            
            // Default: CH001 is always unlocked
            bool defaultUnlocked = (chapterId == "CH001");
            Debug.LogWarning($"GameProgressManager not available. Using default unlock status for {chapterId}: {defaultUnlocked}");
            return defaultUnlocked;
        }
    }

    private static bool IsValidCivilization(string civName)
    {
        foreach (string civ in allCivilizations)
        {
            if (civ == civName) return true;
        }
        return false;
    }

    /// <summary>
    /// Migrates any existing PlayerPrefs civilization data to GameProgressManager.
    /// Call this once when a student logs in to ensure data consistency.
    /// </summary>
    public static void MigrateToGameProgressManager()
    {
        if (GameProgressManager.Instance == null || GameProgressManager.Instance.CurrentStudentState != null)
        {
            Debug.LogWarning("Cannot migrate: GameProgressManager not available or no student logged in");
            return;
        }

        Debug.Log("Migrating civilizations from legacy PlayerPrefs to GameProgressManager");

        foreach (string civName in allCivilizations)
        {
            // Check if this civilization was unlocked in PlayerPrefs
            if (PlayerPrefs.GetInt(civName + "_unlocked", 0) == 1)
            {
                // Migrate to GameProgressManager
                GameProgressManager.Instance.UnlockCivilization(civName);
                
                // Clean up the PlayerPrefs entry
                PlayerPrefs.DeleteKey(civName + "_unlocked");
                
                Debug.Log($"Migrated civilization {civName} from PlayerPrefs to GameProgressManager");
            }
        }
        
        PlayerPrefs.Save();
        Debug.Log("Civilization migration completed");
    }

    /// <summary>
    /// Migrates legacy PlayerPrefs civilization data to StudentPrefs for fallback scenarios
    /// </summary>
    public static void MigrateFromLegacyPlayerPrefs()
    {
        Debug.Log("Migrating civilizations from legacy PlayerPrefs to StudentPrefs");

        foreach (string civName in allCivilizations)
        {
            string legacyKey = civName + "_unlocked";
            
            // Check if this civilization was unlocked in PlayerPrefs and not already in StudentPrefs
            if (PlayerPrefs.GetInt(legacyKey, 0) == 1 && StudentPrefs.GetInt(legacyKey, 0) == 0)
            {
                // Migrate to StudentPrefs
                StudentPrefs.SetInt(legacyKey, 1);
                
                // Clean up the PlayerPrefs entry
                PlayerPrefs.DeleteKey(legacyKey);
                
                Debug.Log($"Migrated civilization {civName} from PlayerPrefs to StudentPrefs");
            }
        }
        
        StudentPrefs.Save();
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs to StudentPrefs civilization migration completed");
    }
}