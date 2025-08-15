using UnityEngine;

public static class PlayerAchievementManager
{
    private static readonly string[] allAchievements = {
        "Scribe",
        "Master",
        "Rise",
        "Strategist",
        "Keeper",
        "Fear",
        "Guardian",
        "Tablet",
        "Sword",
        "Stone",
        "Belt",
    };

    public static void UnlockAchievement(string achievementName)
    {
        if (IsValidAchievement(achievementName))
        {
            PlayerPrefs.SetInt(achievementName + "_unlocked", 1);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogWarning("Invalid achievement name: " + achievementName);
        }
    }

    public static bool IsAchievementUnlocked(string achievementName)
    {
        if (!IsValidAchievement(achievementName))
        {
            Debug.LogWarning("Invalid achievement name: " + achievementName);
            return false;
        }

        return PlayerPrefs.GetInt(achievementName + "_unlocked", 0) == 1;
    }

    private static bool IsValidAchievement(string achievementName)
    {
        foreach (string ach in allAchievements)
        {
            if (ach == achievementName) return true;
        }
        return false;
    }
}