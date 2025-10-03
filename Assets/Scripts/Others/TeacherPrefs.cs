using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Wrapper for PlayerPrefs that automatically scopes keys to the current teacher.
/// This makes saved data teacher-specific instead of global.
/// </summary>
public static class TeacherPrefs
{
    private const string DefaultTeacherId = "DefaultTeacher";

    /// <summary>
    /// Gets the current teacher ID.
    /// Uses StoryManager's teacher data when available.
    /// </summary>
    /// <summary>
/// Gets the current teacher ID.
/// Uses StoryManager's teacher data when available.
/// </summary>
private static string CurrentTeacherId
{
    get
    {
        // Priority 1: Use StoryManager's teacher data (most reliable)
        if (StoryManager.Instance != null && 
            StoryManager.Instance.IsCurrentUserTeacher())
        {
            string teachId = StoryManager.Instance.GetCurrentTeacherId();
            if (!string.IsNullOrEmpty(teachId) && teachId != "default")
            {
                return teachId;
            }
        }
        
        // Priority 2: Fallback to stored PlayerPrefs
        string storedTeachId = PlayerPrefs.GetString("CurrentTeachId", "");
        if (!string.IsNullOrEmpty(storedTeachId))
        {
            return storedTeachId;
        }
        
        return DefaultTeacherId; // final fallback
    }
}


    /// <summary>
    /// Builds a namespaced key using the teacher ID.
    /// </summary>
    private static string BuildKey(string key)
    {
        return $"{CurrentTeacherId}_{key}";
    }

    // ---------------------------
    // String Methods
    // ---------------------------
    public static void SetString(string key, string value)
    {
        string fullKey = BuildKey(key);
        PlayerPrefs.SetString(fullKey, value);
        Debug.Log($"[TeacherPrefs] SAVED string for teacher '{CurrentTeacherId}': {fullKey} = {value}");
    }

    public static string GetString(string key, string defaultValue = "")
    {
        string fullKey = BuildKey(key);
        string value = PlayerPrefs.GetString(fullKey, defaultValue);
        Debug.Log($"[TeacherPrefs] LOADED string for teacher '{CurrentTeacherId}': {fullKey} = {value}");
        return value;
    }

    // ---------------------------
    // Int Methods
    // ---------------------------
    public static void SetInt(string key, int value)
    {
        string fullKey = BuildKey(key);
        PlayerPrefs.SetInt(fullKey, value);
        Debug.Log($"[TeacherPrefs] SAVED int for teacher '{CurrentTeacherId}': {fullKey} = {value}");
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        string fullKey = BuildKey(key);
        int value = PlayerPrefs.GetInt(fullKey, defaultValue);
        Debug.Log($"[TeacherPrefs] LOADED int for teacher '{CurrentTeacherId}': {fullKey} = {value}");
        return value;
    }

    // ---------------------------
    // Float Methods
    // ---------------------------
    public static void SetFloat(string key, float value)
    {
        string fullKey = BuildKey(key);
        PlayerPrefs.SetFloat(fullKey, value);
        Debug.Log($"[TeacherPrefs] SAVED float for teacher '{CurrentTeacherId}': {fullKey} = {value}");
    }

    public static float GetFloat(string key, float defaultValue = 0f)
    {
        string fullKey = BuildKey(key);
        float value = PlayerPrefs.GetFloat(fullKey, defaultValue);
        Debug.Log($"[TeacherPrefs] LOADED float for teacher '{CurrentTeacherId}': {fullKey} = {value}");
        return value;
    }

    // ---------------------------
    // Bool Methods (stored as int)
    // ---------------------------
    public static void SetBool(string key, bool value)
    {
        string fullKey = BuildKey(key);
        PlayerPrefs.SetInt(fullKey, value ? 1 : 0);
        Debug.Log($"[TeacherPrefs] SAVED bool for teacher '{CurrentTeacherId}': {fullKey} = {value}");
    }

    public static bool GetBool(string key, bool defaultValue = false)
    {
        string fullKey = BuildKey(key);
        int value = PlayerPrefs.GetInt(fullKey, defaultValue ? 1 : 0);
        Debug.Log($"[TeacherPrefs] LOADED bool for teacher '{CurrentTeacherId}': {fullKey} = {value == 1}");
        return value == 1;
    }

    // ---------------------------
    // Utility Methods
    // ---------------------------
    public static bool HasKey(string key)
    {
        string fullKey = BuildKey(key);
        bool exists = PlayerPrefs.HasKey(fullKey);
        Debug.Log($"[TeacherPrefs] CHECK key for teacher '{CurrentTeacherId}': {fullKey} exists? {exists}");
        return exists;
    }

    public static void DeleteKey(string key)
    {
        string fullKey = BuildKey(key);
        PlayerPrefs.DeleteKey(fullKey);
        Debug.Log($"[TeacherPrefs] DELETED key for teacher '{CurrentTeacherId}': {fullKey}");
    }

    /// <summary>
    /// Delete all keys for the current teacher
    /// </summary>
    public static void DeleteAllForCurrentTeacher()
    {
        // This is a bit more complex since we need to find all keys for this teacher
        // For now, we'll rely on individual key deletion
        Debug.Log($"[TeacherPrefs] Use individual DeleteKey() for specific keys for teacher '{CurrentTeacherId}'");
    }

    /// <summary>
    /// Saves PlayerPrefs to disk.
    /// </summary>
    public static void Save()
    {
        PlayerPrefs.Save();
        Debug.Log($"[TeacherPrefs] PlayerPrefs SAVED to disk for teacher '{CurrentTeacherId}'.");
    }

    /// <summary>
    /// Debug helper: prints all keys for the current teacher.
    /// </summary>
    public static void DebugPrintAllKeys()
    {
        Debug.Log($"[TeacherPrefs] === ALL KEYS FOR TEACHER '{CurrentTeacherId}' ===");
        
        // Note: This will show ALL PlayerPrefs keys, but highlight the ones for current teacher
        foreach (string key in GetAllTeacherKeys())
        {
            if (key.StartsWith(CurrentTeacherId + "_"))
            {
                string cleanKey = key.Substring(CurrentTeacherId.Length + 1);
                string value = PlayerPrefs.GetString(key, "(not a string)");
                Debug.Log($"[TeacherPrefs] {cleanKey} = {value}");
            }
        }
        Debug.Log($"[TeacherPrefs] === END KEYS ===");
    }

    /// <summary>
    /// Gets all keys that belong to the current teacher
    /// </summary>
    private static List<string> GetAllTeacherKeys()
    {
        var teacherKeys = new List<string>();
        string teacherPrefix = CurrentTeacherId + "_";
        
        // This is a simplified approach - in a real implementation you might want to track keys
        #if UNITY_EDITOR
        // In editor, we can use reflection to get all keys (for debugging)
        try
        {
            var playerPrefsType = typeof(PlayerPrefs);
            var method = playerPrefsType.GetMethod("GetKeys", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (method != null)
            {
                string[] allKeys = (string[])method.Invoke(null, null);
                foreach (string key in allKeys)
                {
                    if (key.StartsWith(teacherPrefix))
                    {
                        teacherKeys.Add(key);
                    }
                }
            }
        }
        catch
        {
            // Reflection failed, just return empty list
        }
        #endif
        
        return teacherKeys;
    }
}