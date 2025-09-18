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

        // Legacy fallback (still uses ID now)
        PlayerPrefs.SetInt(GetKey(data.Id), 1);
        PlayerPrefs.Save();
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

        return PlayerPrefs.GetInt(GetKey(data.Id), 0) == 1;
    }

    private static string GetKey(string achievementId) => $"ACH_{achievementId}_unlocked";
}
