using UnityEngine;

public static class PlayerProgressManager
{
private static readonly string[] allCivilizations = { "Sumerian", "Akkadian", "Babylonian", "Assyrian", "Indus" };



    public static void UnlockCivilization(string civName)
    {
        if (IsValidCivilization(civName))
        {
            PlayerPrefs.SetInt(civName + "_unlocked", 1);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogWarning("Invalid civilization name: " + civName);
        }
    }

    public static bool IsCivilizationUnlocked(string civName)
    {
        if (!IsValidCivilization(civName))
        {
            Debug.LogWarning("Invalid civilization name: " + civName);
            return false;
        }

        return PlayerPrefs.GetInt(civName + "_unlocked", civName == "Sumerian" ? 1 : 0) == 1;
    }

    private static bool IsValidCivilization(string civName)
    {
        foreach (string civ in allCivilizations)
        {
            if (civ == civName) return true;
        }
        return false;
    }
}
