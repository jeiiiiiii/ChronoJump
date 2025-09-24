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

        // FIXED: Check the type and call the appropriate method
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.CurrentStudentState != null)
        {
            if (data.Type == "artifact")
            {
                GameProgressManager.Instance.AddArtifact(data.Id);
            }
            else // achievement or any other type
            {
                GameProgressManager.Instance.AddAchievement(data.Id);
            }
            return;
        }

        // Legacy fallback (still uses ID now) - now with StudentPrefs
        // FIXED: Use different keys for achievements vs artifacts
        string key = data.Type == "artifact" ? GetArtifactKey(data.Id) : GetAchievementKey(data.Id);
        StudentPrefs.SetInt(key, 1);
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
        {
            // FIXED: Check the appropriate list based on type
            if (data.Type == "artifact")
                return GameProgressManager.Instance.IsArtifactUnlocked(data.Id);
            else
                return GameProgressManager.Instance.IsAchievementUnlocked(data.Id);
        }

        // FIXED: Check the appropriate key based on type
        string key = data.Type == "artifact" ? GetArtifactKey(data.Id) : GetAchievementKey(data.Id);
        return StudentPrefs.GetInt(key, 0) == 1;
    }

    private static string GetAchievementKey(string achievementId) => $"ACH_{achievementId}_unlocked";
    private static string GetArtifactKey(string artifactId) => $"ART_{artifactId}_unlocked";

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

        Debug.Log("Migrating achievements and artifacts from legacy PlayerPrefs to StudentPrefs");

        foreach (var achievement in AchievementRegistry.AchievementsById.Values)
        {
            string legacyKey = GetAchievementKey(achievement.Id);
            
            // Check if this achievement/artifact was unlocked in PlayerPrefs
            if (PlayerPrefs.GetInt(legacyKey, 0) == 1)
            {
                // FIXED: Use the appropriate key based on type
                string newKey = achievement.Type == "artifact" ? GetArtifactKey(achievement.Id) : GetAchievementKey(achievement.Id);
                StudentPrefs.SetInt(newKey, 1);
                
                // Clean up the PlayerPrefs entry
                PlayerPrefs.DeleteKey(legacyKey);
                
                Debug.Log($"Migrated {achievement.Type} {achievement.Id} from PlayerPrefs to StudentPrefs");
            }
        }
        
        StudentPrefs.Save();
        PlayerPrefs.Save();
        Debug.Log("Achievement and artifact migration completed");
    }
}