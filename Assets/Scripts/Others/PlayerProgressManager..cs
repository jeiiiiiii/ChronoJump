using UnityEngine;

/// <summary>
/// Bridge class that redirects calls to GameProgressManager for backward compatibility.
/// This ensures existing code that calls PlayerProgressManager still works.
/// You can eventually remove this class once all references are updated.
/// </summary>
public static class PlayerProgressManager
{
    private static readonly string[] allCivilizations = { "Sumerian", "Akkadian", "Babylonian", "Assyrian", "Indus" };

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
        if (GameProgressManager.Instance == null || GameProgressManager.Instance.CurrentStudentState == null)
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