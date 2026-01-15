using UnityEngine;

/// <summary>
/// Model class for storing class details retrieved from Firebase
/// Used when students need to access information about the class they joined
/// </summary>
[System.Serializable]
public class ClassDetailsModel
{
    [Header("Class Information")]
    public string classCode;
    public string className;
    public string classLevel;
    public string teacherName;
    public bool isActive;

    /// <summary>
    /// Default constructor
    /// </summary>
    public ClassDetailsModel()
    {
        classCode = "";
        className = "";
        classLevel = "";
        teacherName = "";
        isActive = true;
    }

    /// <summary>
    /// Constructor with parameters
    /// </summary>
    public ClassDetailsModel(string code, string name, string level, string teacher, bool active = true)
    {
        classCode = code;
        className = name;
        classLevel = level;
        teacherName = teacher;
        isActive = active;
    }

    /// <summary>
    /// Get display name combining level and class name
    /// </summary>
    public string GetDisplayName()
    {
        if (string.IsNullOrEmpty(classLevel))
            return className;

        return $"{classLevel} - {className}";
    }

    /// <summary>
    /// Check if the class data is valid
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(classCode) &&
               !string.IsNullOrEmpty(className) &&
               !string.IsNullOrEmpty(teacherName) &&
               isActive;
    }

    /// <summary>
    /// Convert to StudentClassData for compatibility
    /// </summary>
    public StudentClassData ToStudentClassData()
    {
        return new StudentClassData(classCode, teacherName, className, classLevel);
    }

    /// <summary>
    /// Debug string representation
    /// </summary>
    public override string ToString()
    {
        return $"Class: {GetDisplayName()} | Teacher: {teacherName} | Code: {classCode} | Active: {isActive}";
    }
}