using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages class data synchronization between scenes and provides events for data changes
/// </summary>
public class ClassDataSync : MonoBehaviour
{
    public static ClassDataSync Instance { get; private set; }

    public static event Action<Dictionary<string, List<string>>> OnClassDataUpdated;
    public static event Action<string, string> OnNewClassCreated;
    public static event Action<string> OnClassDeleted;
    public static event Action<string, string> OnClassEdited;

    private Dictionary<string, List<string>> _cachedClassData = new Dictionary<string, List<string>>();
    private bool _isDataLoaded = false;

    private void Awake()
    {
        // Singleton pattern - persist across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("ClassDataSync instance created and marked as DontDestroyOnLoad");
        }
        else
        {
            Debug.Log("Destroying duplicate ClassDataSync instance");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Load teacher's class data from Firebase
    /// </summary>
    public void LoadTeacherClassData(Action<Dictionary<string, List<string>>> callback = null)
    {
        if (FirebaseManager.Instance == null)
        {
            Debug.LogError("FirebaseManager instance not found");
            callback?.Invoke(new Dictionary<string, List<string>>());
            return;
        }

        FirebaseManager.Instance.GetUserData(userData =>
        {
            if (userData?.role?.ToLower() != "teacher")
            {
                Debug.LogWarning("User is not a teacher or no user data found");
                callback?.Invoke(new Dictionary<string, List<string>>());
                return;
            }

            FirebaseManager.Instance.GetTeacherData(userData.userId, teacherData =>
            {
                if (teacherData?.classCode != null)
                {
                    _cachedClassData = new Dictionary<string, List<string>>(teacherData.classCode);
                    _isDataLoaded = true;
                    
                    Debug.Log($"Loaded {_cachedClassData.Count} classes for teacher");
                    
                    // Notify all listeners that class data has been updated
                    OnClassDataUpdated?.Invoke(_cachedClassData);
                    callback?.Invoke(_cachedClassData);
                }
                else
                {
                    Debug.Log("No class data found for teacher");
                    _cachedClassData.Clear();
                    _isDataLoaded = true;
                    OnClassDataUpdated?.Invoke(_cachedClassData);
                    callback?.Invoke(_cachedClassData);
                }
            });
        });
    }

    /// <summary>
    /// Get cached class data (if available)
    /// </summary>
    public Dictionary<string, List<string>> GetCachedClassData()
    {
        return new Dictionary<string, List<string>>(_cachedClassData);
    }

    /// <summary>
    /// Check if class data is loaded
    /// </summary>
    public bool IsDataLoaded()
    {
        return _isDataLoaded;
    }

    /// <summary>
    /// Notify that a new class has been created
    /// </summary>
    public void NotifyClassCreated(string classCode, string className, string classLevel)
    {
        // Add to cached data
        _cachedClassData[classCode] = new List<string> { classLevel, className };
        
        Debug.Log($"Class created notification: {className} ({classCode})");
        
        // Notify listeners
        OnNewClassCreated?.Invoke(classCode, $"{classLevel} - {className}");
        OnClassDataUpdated?.Invoke(_cachedClassData);
    }

    /// <summary>
    /// Notify that a class has been deleted
    /// </summary>
    public void NotifyClassDeleted(string classCode)
    {
        // Remove from cached data
        if (_cachedClassData.ContainsKey(classCode))
        {
            _cachedClassData.Remove(classCode);
            Debug.Log($"Class deleted notification: {classCode}");
            
            // Notify listeners
            OnClassDeleted?.Invoke(classCode);
            OnClassDataUpdated?.Invoke(_cachedClassData);
        }
    }

    /// <summary>
    /// Notify that a class has been edited
    /// </summary>
    public void NotifyClassEdited(string classCode, string newClassName)
    {
        // Update cached data
        if (_cachedClassData.ContainsKey(classCode))
        {
            string classLevel = _cachedClassData[classCode][0]; // Keep the same class level
            _cachedClassData[classCode] = new List<string> { classLevel, newClassName };
            
            Debug.Log($"Class edited notification: {classCode} -> {newClassName}");
            
            // Notify listeners
            OnClassEdited?.Invoke(classCode, $"{classLevel} - {newClassName}");
            OnClassDataUpdated?.Invoke(_cachedClassData);
        }
    }

    /// <summary>
    /// Force refresh data from Firebase
    /// </summary>
    public void RefreshClassData(Action<Dictionary<string, List<string>>> callback = null)
    {
        Debug.Log("Forcing refresh of class data from Firebase");
        _isDataLoaded = false;
        LoadTeacherClassData(callback);
    }

    /// <summary>
    /// Update cached data without triggering events (used internally)
    /// </summary>
    public void UpdateCachedData(Dictionary<string, List<string>> newData)
    {
        _cachedClassData = new Dictionary<string, List<string>>(newData);
        _isDataLoaded = true;
        Debug.Log($"Updated cached data with {_cachedClassData.Count} classes");
    }

    /// <summary>
    /// Clear cached data (useful for logout scenarios)
    /// </summary>
    public void ClearCache()
    {
        _cachedClassData.Clear();
        _isDataLoaded = false;
        Debug.Log("Class data cache cleared");
    }

    private void OnDestroy()
    {
        // Clean up events when destroyed
        OnClassDataUpdated = null;
        OnNewClassCreated = null;
        OnClassDeleted = null;
        OnClassEdited = null;
    }
}