// FIXED: PlayerAchievementManager.cs - Consistent saving/loading
using UnityEngine;

public static class ChinaPlayerAchievementManager
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

        // Primary: Use GameProgressManager if available
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
            
            // FIXED: Also save to StudentPrefs for UI consistency
            string key = data.Type == "artifact" ? GetArtifactKey(data.Id) : GetAchievementKey(data.Id);
            StudentPrefs.SetInt(key, 1);
            StudentPrefs.Save();
            return;
        }

        // Fallback: Direct StudentPrefs save
        string fallbackKey = data.Type == "artifact" ? GetArtifactKey(data.Id) : GetAchievementKey(data.Id);
        StudentPrefs.SetInt(fallbackKey, 1);
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

        // FIXED: Check GameProgress first, then fall back to StudentPrefs
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.CurrentStudentState != null)
        {
            bool isUnlockedInGameProgress;
            
            if (data.Type == "artifact")
                isUnlockedInGameProgress = GameProgressManager.Instance.IsArtifactUnlocked(data.Id);
            else
                isUnlockedInGameProgress = GameProgressManager.Instance.IsAchievementUnlocked(data.Id);
            
            // If unlocked in GameProgress, also ensure StudentPrefs is synced
            if (isUnlockedInGameProgress)
            {
                string key = data.Type == "artifact" ? GetArtifactKey(data.Id) : GetAchievementKey(data.Id);
                if (StudentPrefs.GetInt(key, 0) != 1)
                {
                    StudentPrefs.SetInt(key, 1);
                    StudentPrefs.Save();
                    Debug.Log($"Synced {data.Type} {data.Id} to StudentPrefs from GameProgress");
                }
            }
            
            return isUnlockedInGameProgress;
        }

        // Fallback: Check StudentPrefs directly
        string fallbackKey = data.Type == "artifact" ? GetArtifactKey(data.Id) : GetAchievementKey(data.Id);
        return StudentPrefs.GetInt(fallbackKey, 0) == 1;
    }

    private static string GetAchievementKey(string achievementId) => $"ACH_{achievementId}_unlocked";
    private static string GetArtifactKey(string artifactId) => $"ART_{artifactId}_unlocked";

    /// <summary>
    /// FIXED: Sync achievements/artifacts from GameProgress to StudentPrefs after loading
    /// Call this after GameProgress is loaded to ensure UI consistency
    /// </summary>
    public static void SyncToStudentPrefs()
    {
        if (GameProgressManager.Instance?.CurrentStudentState?.GameProgress == null)
        {
            Debug.LogWarning("Cannot sync to StudentPrefs: GameProgress not available");
            return;
        }

        var gp = GameProgressManager.Instance.CurrentStudentState.GameProgress;
        
        Debug.Log("Syncing achievements and artifacts from GameProgress to StudentPrefs");

        // Sync achievements
        if (gp.unlockedAchievements != null)
        {
            foreach (string achievementId in gp.unlockedAchievements)
            {
                string key = GetAchievementKey(achievementId);
                StudentPrefs.SetInt(key, 1);
                Debug.Log($"Synced achievement {achievementId} to StudentPrefs");
            }
        }

        // Sync artifacts
        if (gp.unlockedArtifacts != null)
        {
            foreach (string artifactId in gp.unlockedArtifacts)
            {
                string key = GetArtifactKey(artifactId);
                StudentPrefs.SetInt(key, 1);
                Debug.Log($"Synced artifact {artifactId} to StudentPrefs");
            }
        }
        
        StudentPrefs.Save();
        Debug.Log("Achievement and artifact sync to StudentPrefs completed");
    }

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
            // Check old PlayerPrefs keys (both achievement and artifact used same pattern)
            string legacyKey = $"ACH_{achievement.Id}_unlocked";
            
            if (PlayerPrefs.GetInt(legacyKey, 0) == 1)
            {
                // Use the appropriate new key based on type
                string newKey = achievement.Type == "artifact" ? GetArtifactKey(achievement.Id) : GetAchievementKey(achievement.Id);
                StudentPrefs.SetInt(newKey, 1);
                
                // Also add to GameProgress if available
                if (achievement.Type == "artifact")
                {
                    GameProgressManager.Instance.AddArtifact(achievement.Id);
                }
                else
                {
                    GameProgressManager.Instance.AddAchievement(achievement.Id);
                }
                
                // Clean up the PlayerPrefs entry
                PlayerPrefs.DeleteKey(legacyKey);
                
                Debug.Log($"Migrated {achievement.Type} {achievement.Id} from PlayerPrefs to StudentPrefs and GameProgress");
            }
        }
        
        StudentPrefs.Save();
        PlayerPrefs.Save();
        Debug.Log("Achievement and artifact migration completed");
    }
}