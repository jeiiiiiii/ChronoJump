using UnityEngine;

public static class PlayerAchievementManager
{
    public static void UnlockAchievement(string achievementNameOrId)
    {
        // Allow both ID ("AC001") or Name ("Scribe")
        AchievementModel data = null;

        if (AchievementRegistry.AchievementsById.ContainsKey(achievementNameOrId))
            data = AchievementRegistry.AchievementsById[achievementNameOrId];
        else if (AchievementRegistry.AchievementsByName.ContainsKey(achievementNameOrId))
            data = AchievementRegistry.AchievementsByName[achievementNameOrId];

        if (data == null)
        {
            Debug.LogWarning("Invalid achievement: " + achievementNameOrId);
            return;
        }

        // Always save using ID internally
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.CurrentStudentState != null)
        {
            GameProgressManager.Instance.AddAchievement(data.Id);
            return;
        }

        // Legacy fallback (still uses ID now) - now with StudentPrefs
        StudentPrefs.SetInt(GetKey(data.Id), 1);
        StudentPrefs.Save();
    }

    public static bool IsAchievementUnlocked(string achievementNameOrId)
    {
        AchievementModel data = null;

        if (AchievementRegistry.AchievementsById.ContainsKey(achievementNameOrId))
            data = AchievementRegistry.AchievementsById[achievementNameOrId];
        else if (AchievementRegistry.AchievementsByName.ContainsKey(achievementNameOrId))
            data = AchievementRegistry.AchievementsByName[achievementNameOrId];

        if (data == null) return false;

        if (GameProgressManager.Instance != null && GameProgressManager.Instance.CurrentStudentState != null)
            return GameProgressManager.Instance.IsAchievementUnlocked(data.Id);

        return StudentPrefs.GetInt(GetKey(data.Id), 0) == 1;
    }

    private static string GetKey(string achievementId) => $"ACH_{achievementId}_unlocked";

    /// <summary>
    /// Migration method to convert old PlayerPrefs achievements to StudentPrefs
    /// Call this once when a student logs in to ensure data consistency.
    /// </summary>
    public static void MigrateFromLegacyPlayerPrefs()
    {
        if (GameProgressManager.Instance?.CurrentStudentState == null)
        {
            Debug.LogWarning("Cannot migrate achievements: no student logged in");
            return;
        }

        Debug.Log("Migrating achievements from legacy PlayerPrefs to StudentPrefs");

        foreach (var achievement in AchievementRegistry.AchievementsById.Values)
        {
            string legacyKey = GetKey(achievement.Id);
            
            // Check if this achievement was unlocked in PlayerPrefs
            if (PlayerPrefs.GetInt(legacyKey, 0) == 1)
            {
                // Migrate to StudentPrefs
                StudentPrefs.SetInt(legacyKey, 1);
                
                // Clean up the PlayerPrefs entry
                PlayerPrefs.DeleteKey(legacyKey);
                
                Debug.Log($"Migrated achievement {achievement.Id} from PlayerPrefs to StudentPrefs");
            }
        }
        
        StudentPrefs.Save();
        PlayerPrefs.Save();
        Debug.Log("Achievement migration completed");
    }
}