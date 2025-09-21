using UnityEngine;

/// <summary>
/// Wrapper for PlayerPrefs that automatically scopes keys to the current student.
/// This makes saved data student-specific instead of global.
/// </summary>
public static class StudentPrefs
{
    /// <summary>
    /// Gets the current student ID. 
    /// Replace this with however you track the logged-in student.
    /// </summary>
    private static string CurrentStudentId
    {
        get
        {
            // Example: If you're using Firebase, replace with FirebaseManager.Instance.CurrentUserId
            // Make sure this returns a unique string for each student.
            return FirebaseManager.Instance != null && !string.IsNullOrEmpty(FirebaseManager.Instance.CurrentUserId)
                ? FirebaseManager.Instance.CurrentUserId
                : "DefaultStudent"; // fallback if no student logged in
        }
    }

    /// <summary>
    /// Builds a namespaced key using the student ID.
    /// </summary>
    private static string BuildKey(string key)
    {
        return $"{CurrentStudentId}_{key}";
    }

    // ---------------------------
    // String Methods
    // ---------------------------
    public static void SetString(string key, string value)
    {
        PlayerPrefs.SetString(BuildKey(key), value);
    }

    public static string GetString(string key, string defaultValue = "")
    {
        return PlayerPrefs.GetString(BuildKey(key), defaultValue);
    }

    // ---------------------------
    // Int Methods
    // ---------------------------
    public static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(BuildKey(key), value);
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        return PlayerPrefs.GetInt(BuildKey(key), defaultValue);
    }

    // ---------------------------
    // Float Methods
    // ---------------------------
    public static void SetFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(BuildKey(key), value);
    }

    public static float GetFloat(string key, float defaultValue = 0f)
    {
        return PlayerPrefs.GetFloat(BuildKey(key), defaultValue);
    }

    // ---------------------------
    // Utility Methods
    // ---------------------------
    public static bool HasKey(string key)
    {
        return PlayerPrefs.HasKey(BuildKey(key));
    }

    public static void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(BuildKey(key));
    }

    /// <summary>
    /// Saves PlayerPrefs to disk.
    /// </summary>
    public static void Save()
    {
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Debug helper: prints all keys for the current student.
    /// </summary>
    public static void DebugPrintKeys(string[] keys)
    {
        foreach (string key in keys)
        {
            string fullKey = BuildKey(key);
            if (PlayerPrefs.HasKey(fullKey))
            {
                Debug.Log($"[{CurrentStudentId}] {fullKey} = {PlayerPrefs.GetString(fullKey, "(not a string)")}");
            }
        }
    }
}
