using UnityEngine;

/// <summary>
/// Wrapper for PlayerPrefs that automatically scopes keys to the current student.
/// This makes saved data student-specific instead of global.
/// </summary>
public static class StudentPrefs
{
    private const string DefaultStudentId = "DefaultStudent";

    /// <summary>
    /// Gets the current student ID. 
    /// Replace this with however you track the logged-in student.
    /// </summary>
    private static string CurrentStudentId
{
    get
    {
        // Use the same student ID that GameProgressManager uses
        if (GameProgressManager.Instance != null && 
            GameProgressManager.Instance.CurrentStudentState != null && 
            !string.IsNullOrEmpty(GameProgressManager.Instance.CurrentStudentState.StudentId))
        {
            return GameProgressManager.Instance.CurrentStudentState.StudentId;
        }
        
        // Fallback to Firebase user ID if GameProgressManager not available
        if (FirebaseManager.Instance != null && 
            FirebaseManager.Instance.CurrentUser != null && 
            !string.IsNullOrEmpty(FirebaseManager.Instance.CurrentUser.UserId))
        {
            return FirebaseManager.Instance.CurrentUser.UserId;
        }
        
        return DefaultStudentId; // final fallback
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
        string fullKey = BuildKey(key);
        PlayerPrefs.SetString(fullKey, value);
        Debug.Log($"[StudentPrefs] SAVED string for student '{CurrentStudentId}': {fullKey} = {value}");
    }

    public static string GetString(string key, string defaultValue = "")
    {
        string fullKey = BuildKey(key);
        string value = PlayerPrefs.GetString(fullKey, defaultValue);
        Debug.Log($"[StudentPrefs] LOADED string for student '{CurrentStudentId}': {fullKey} = {value}");
        return value;
    }

    // ---------------------------
    // Int Methods
    // ---------------------------
    public static void SetInt(string key, int value)
    {
        string fullKey = BuildKey(key);
        PlayerPrefs.SetInt(fullKey, value);
        Debug.Log($"[StudentPrefs] SAVED int for student '{CurrentStudentId}': {fullKey} = {value}");
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        string fullKey = BuildKey(key);
        int value = PlayerPrefs.GetInt(fullKey, defaultValue);
        Debug.Log($"[StudentPrefs] LOADED int for student '{CurrentStudentId}': {fullKey} = {value}");
        return value;
    }

    // ---------------------------
    // Float Methods
    // ---------------------------
    public static void SetFloat(string key, float value)
    {
        string fullKey = BuildKey(key);
        PlayerPrefs.SetFloat(fullKey, value);
        Debug.Log($"[StudentPrefs] SAVED float for student '{CurrentStudentId}': {fullKey} = {value}");
    }

    public static float GetFloat(string key, float defaultValue = 0f)
    {
        string fullKey = BuildKey(key);
        float value = PlayerPrefs.GetFloat(fullKey, defaultValue);
        Debug.Log($"[StudentPrefs] LOADED float for student '{CurrentStudentId}': {fullKey} = {value}");
        return value;
    }

    // ---------------------------
    // Utility Methods
    // ---------------------------
    public static bool HasKey(string key)
    {
        string fullKey = BuildKey(key);
        bool exists = PlayerPrefs.HasKey(fullKey);
        Debug.Log($"[StudentPrefs] CHECK key for student '{CurrentStudentId}': {fullKey} exists? {exists}");
        return exists;
    }

    public static void DeleteKey(string key)
    {
        string fullKey = BuildKey(key);
        PlayerPrefs.DeleteKey(fullKey);
        Debug.Log($"[StudentPrefs] DELETED key for student '{CurrentStudentId}': {fullKey}");
    }

    /// <summary>
    /// Saves PlayerPrefs to disk.
    /// </summary>
    public static void Save()
    {
        PlayerPrefs.Save();
        Debug.Log($"[StudentPrefs] PlayerPrefs SAVED to disk for student '{CurrentStudentId}'.");
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
                Debug.Log($"[StudentPrefs] [{CurrentStudentId}] {fullKey} = {PlayerPrefs.GetString(fullKey, "(not a string)")}");
            }
            else
            {
                Debug.Log($"[StudentPrefs] [{CurrentStudentId}] {fullKey} not found.");
            }
        }
    }
}
