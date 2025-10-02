using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

[System.Serializable]
public class StudentClassData
{
    public string classCode;
    public string teacherName;
    public string className;
    public string classLevel;

    public StudentClassData(string code, string teacher, string classTitle, string level = "")
    {
        classCode = code;
        teacherName = teacher;
        className = classTitle;
        classLevel = level;
    }
}

public class ClassInfo : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI teacherNameText;
    public TextMeshProUGUI classNameText;
    public Button backButton;

    [Header("Class Data")]
    [SerializeField] private StudentClassData currentClassData;

    // Event to notify when class data is loaded
    public static event Action<StudentClassData> OnClassDataLoaded;

    private bool isDataLoaded = false;

    private void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(GoBackToMainMenu);

        LoadClassInfo();
    }

    private void LoadClassInfo()
    {
        string joinedClassCode = PlayerPrefs.GetString("JoinedClassCode", "");
        Debug.Log($"LoadClassInfo: Looking for class code: '{joinedClassCode}'");

        if (!string.IsNullOrEmpty(joinedClassCode))
        {
            LoadClassDataFromCache(joinedClassCode);
        }
        else
        {
            LoadClassDataFromFirebase();
        }
    }

    private void LoadClassDataFromCache(string classCode)
    {
        currentClassData = GetClassDataByCode(classCode);

        if (currentClassData != null)
        {
            Debug.Log($"Loaded from cache: {currentClassData.className}");
            UpdateUI();
            NotifyDataLoaded();
        }
        else
        {
            LoadClassDataFromFirebase();
        }
    }

    private StudentClassData GetClassDataByCode(string classCode)
    {
        Debug.Log($"Looking for class data for code: {classCode}");

        // Priority 1: Check registered class data from Firebase (stored in PlayerPrefs)
        string studentClassJson = PlayerPrefs.GetString("RegisteredClassData", "");
        if (!string.IsNullOrEmpty(studentClassJson))
        {
            var registeredClass = JsonUtility.FromJson<StudentClassData>(studentClassJson);
            if (registeredClass.classCode == classCode)
            {
                Debug.Log($"✅ Found registered class data from cache");
                return registeredClass;
            }
        }

        // Priority 2: Check ClassDataSync (for teachers only - students won't have this)
        if (ClassDataSync.Instance != null && ClassDataSync.Instance.IsDataLoaded())
        {
            var classData = ClassDataSync.Instance.GetCachedClassData();

            if (classData.ContainsKey(classCode))
            {
                var classInfo = classData[classCode];
                string classLevel = classInfo.Count > 0 ? classInfo[0] : "";
                string className = classInfo.Count > 1 ? classInfo[1] : "";
                string teacherName = classInfo.Count > 2 ? classInfo[2] : "Unknown Teacher";

                return new StudentClassData(classCode, teacherName, className, classLevel);
            }
        }

        // Priority 3: Check individual class cache in PlayerPrefs
        string classDataJson = PlayerPrefs.GetString("StudentClassData_" + classCode, "");
        if (!string.IsNullOrEmpty(classDataJson))
        {
            Debug.Log($"✅ Found class data in individual cache");
            return JsonUtility.FromJson<StudentClassData>(classDataJson);
        }

        Debug.LogWarning($"⚠️ No cached data found for {classCode} - will fetch from Firebase");
        return null;
    }

    private void LoadClassDataFromFirebase()
    {
        if (FirebaseManager.Instance == null || FirebaseManager.Instance.CurrentUser == null)
        {
            Debug.LogError("Firebase not ready or user not logged in");
            SetDefaultValues();
            NotifyDataLoaded(); // Still notify even if failed
            return;
        }

        string userId = FirebaseManager.Instance.CurrentUser.UserId;
        Debug.Log($"Fetching class data from Firebase for user: {userId}");

        FirebaseManager.Instance.StudentService.GetStudentDataByUserId(userId, (studentData) =>
        {
            if (studentData != null && !string.IsNullOrEmpty(studentData.classCode))
            {
                Debug.Log($"Found student class: {studentData.classCode}");

                PlayerPrefs.SetString("JoinedClassCode", studentData.classCode);
                PlayerPrefs.Save();

                FirebaseManager.Instance.GetClassDetails(studentData.classCode, (classDetails) =>
                {
                    if (classDetails != null)
                    {
                        currentClassData = classDetails.ToStudentClassData();

                        PlayerPrefs.SetString("RegisteredClassData", JsonUtility.ToJson(currentClassData));
                        PlayerPrefs.Save();

                        UpdateUI();
                        NotifyDataLoaded();
                    }
                    else
                    {
                        Debug.LogError("Failed to get class details");
                        SetDefaultValues();
                        NotifyDataLoaded();
                    }
                });
            }
            else
            {
                Debug.LogError("Student is not enrolled in any class");
                SetDefaultValues();
                NotifyDataLoaded();
            }
        });
    }

    private void UpdateUI()
    {
        if (currentClassData != null)
        {
            if (teacherNameText != null)
                teacherNameText.text = currentClassData.teacherName;

            if (classNameText != null)
            {
                if (!string.IsNullOrEmpty(currentClassData.classLevel))
                    classNameText.text = $"{currentClassData.classLevel} - {currentClassData.className}";
                else
                    classNameText.text = currentClassData.className;
            }
        }
    }

    private void SetDefaultValues()
    {
        if (teacherNameText != null)
            teacherNameText.text = "Teacher Name";

        if (classNameText != null)
            classNameText.text = "Class Name";
    }

    private void NotifyDataLoaded()
    {
        isDataLoaded = true;
        OnClassDataLoaded?.Invoke(currentClassData);
        Debug.Log($"✅ ClassInfo data loaded and notified. ClassCode: {currentClassData?.classCode ?? "NULL"}");
    }

    public StudentClassData GetCurrentClassData()
    {
        return currentClassData;
    }

    public bool IsDataLoaded()
    {
        return isDataLoaded;
    }

    public void SetClassData(StudentClassData newClassData)
    {
        currentClassData = newClassData;
        UpdateUI();
    }

    private void GoBackToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScreen");
    }

    public static void SaveStudentClassData(StudentClassData classData)
    {
        string json = JsonUtility.ToJson(classData);
        PlayerPrefs.SetString("StudentClassData_" + classData.classCode, json);
        PlayerPrefs.Save();
    }
}