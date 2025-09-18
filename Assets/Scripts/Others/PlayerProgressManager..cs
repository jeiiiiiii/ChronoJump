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
            // Fallback to PlayerPrefs if GameProgressManager is not available
            PlayerPrefs.SetInt(civName + "_unlocked", 1);
            PlayerPrefs.Save();
            Debug.LogWarning($"GameProgressManager not available. Unlocked {civName} using PlayerPrefs fallback.");
        }
    }

    /// <summary>
    /// Checks if a civilization is unlocked through GameProgressManager.
    /// Falls back to PlayerPrefs if GameProgressManager is not available.
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
            // Fallback to PlayerPrefs
            bool isUnlocked = PlayerPrefs.GetInt(civName + "_unlocked", civName == "Sumerian" ? 1 : 0) == 1;
            Debug.LogWarning($"GameProgressManager not available. Checking {civName} unlock status using PlayerPrefs fallback: {isUnlocked}");
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
}