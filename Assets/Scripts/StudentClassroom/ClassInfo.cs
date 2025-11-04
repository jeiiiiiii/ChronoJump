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

        // Migrate existing data to StudentPrefs
        MigrateToStudentPrefs();

        LoadClassInfo();
    }

    private void LoadClassInfo()
    {
        // ✅ Use StudentPrefs instead of PlayerPrefs
        string joinedClassCode = StudentPrefs.GetString("JoinedClassCode", "");
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

    private StudentClassData GetClassDataByCode(string classCode)
    {
        Debug.Log($"🔍 Looking for class data for code: {classCode}");

        // Priority 1: Check registered class data from Firebase (stored in StudentPrefs)
        string studentClassJson = StudentPrefs.GetString("RegisteredClassData", "");
        if (!string.IsNullOrEmpty(studentClassJson))
        {
            var registeredClass = JsonUtility.FromJson<StudentClassData>(studentClassJson);
            if (registeredClass.classCode == classCode)
            {
                Debug.Log($"✅ Found registered class data from cache: {registeredClass.className}, Teacher: {registeredClass.teacherName}");
                return registeredClass;
            }
            else
            {
                Debug.Log($"❌ Cached class code mismatch: Expected {classCode}, Found {registeredClass.classCode}");
            }
        }
        else
        {
            Debug.Log("ℹ️ No RegisteredClassData found in StudentPrefs");
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

                Debug.Log($"✅ Found class data from ClassDataSync: {className}, Teacher: {teacherName}");
                return new StudentClassData(classCode, teacherName, className, classLevel);
            }
            else
            {
                Debug.Log($"❌ Class {classCode} not found in ClassDataSync");
            }
        }
        else
        {
            Debug.Log("ℹ️ ClassDataSync not available or not loaded");
        }

        // Priority 3: Check individual class cache in StudentPrefs
        string classDataJson = StudentPrefs.GetString("StudentClassData_" + classCode, "");
        if (!string.IsNullOrEmpty(classDataJson))
        {
            var cachedClass = JsonUtility.FromJson<StudentClassData>(classDataJson);
            Debug.Log($"✅ Found class data in individual cache: {cachedClass.className}, Teacher: {cachedClass.teacherName}");
            return cachedClass;
        }
        else
        {
            Debug.Log($"ℹ️ No individual cache found for class: {classCode}");
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
            NotifyDataLoaded();
            return;
        }

        string userId = FirebaseManager.Instance.CurrentUser.UserId;
        Debug.Log($"🔍 Fetching class data from Firebase for STUDENT user: {userId}");

        FirebaseManager.Instance.StudentService.GetStudentDataByUserId(userId, (studentData) =>
        {
            if (studentData != null && !string.IsNullOrEmpty(studentData.classCode))
            {
                Debug.Log($"✅ Found student class: {studentData.classCode} for student {userId}");

                // ✅ Use StudentPrefs instead of PlayerPrefs
                StudentPrefs.SetString("JoinedClassCode", studentData.classCode);
                StudentPrefs.Save();

                FirebaseManager.Instance.GetClassDetails(studentData.classCode, (classDetails) =>
                {
                    if (classDetails != null)
                    {
                        currentClassData = classDetails.ToStudentClassData();
                        Debug.Log($"✅ Loaded class details: {currentClassData.className}, Teacher: {currentClassData.teacherName}");

                        // ✅ Use StudentPrefs instead of PlayerPrefs
                        StudentPrefs.SetString("RegisteredClassData", JsonUtility.ToJson(currentClassData));
                        StudentPrefs.Save();

                        UpdateUI();
                        NotifyDataLoaded();
                    }
                    else
                    {
                        Debug.LogError("❌ Failed to get class details");
                        SetDefaultValues();
                        NotifyDataLoaded();
                    }
                });
            }
            else
            {
                Debug.LogError($"❌ Student {userId} is not enrolled in any class");
                SetDefaultValues();
                NotifyDataLoaded();
            }
        });
    }

    public static void SaveStudentClassData(StudentClassData classData)
    {
        // ✅ Use StudentPrefs instead of PlayerPrefs
        string json = JsonUtility.ToJson(classData);
        StudentPrefs.SetString("StudentClassData_" + classData.classCode, json);
        StudentPrefs.Save();
    }


    private void LoadClassDataFromCache(string classCode)
    {
        currentClassData = GetClassDataByCode(classCode);

        if (currentClassData != null)
        {
            Debug.Log($"✅ Loaded from cache: {currentClassData.className}, Teacher: {currentClassData.teacherName}");
            UpdateUI();
            NotifyDataLoaded();

            // 🔄 ADD THIS: Always refresh from Firebase to ensure we have the latest data
            Debug.Log("🔄 Refreshing class data from Firebase for latest information...");
            LoadClassDataFromFirebase();
        }
        else
        {
            Debug.Log("❌ No cached data found, loading from Firebase...");
            LoadClassDataFromFirebase();
        }
    }

    private void UpdateUI()
    {
        if (currentClassData != null)
        {
            if (teacherNameText != null)
                teacherNameText.text = "Teacher " + currentClassData.teacherName;

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

    private void MigrateToStudentPrefs()
    {
        // Migrate JoinedClassCode
        string joinedClassCode = PlayerPrefs.GetString("JoinedClassCode", "");
        if (!string.IsNullOrEmpty(joinedClassCode) && !StudentPrefs.HasKey("JoinedClassCode"))
        {
            StudentPrefs.SetString("JoinedClassCode", joinedClassCode);
            Debug.Log($"🔄 Migrated JoinedClassCode: {joinedClassCode}");
        }

        // Migrate RegisteredClassData
        string registeredClassData = PlayerPrefs.GetString("RegisteredClassData", "");
        if (!string.IsNullOrEmpty(registeredClassData) && !StudentPrefs.HasKey("RegisteredClassData"))
        {
            StudentPrefs.SetString("RegisteredClassData", registeredClassData);
            Debug.Log($"🔄 Migrated RegisteredClassData");
        }

        // Migrate individual class data
        string classCode = StudentPrefs.GetString("JoinedClassCode", "");
        if (!string.IsNullOrEmpty(classCode))
        {
            string individualClassData = PlayerPrefs.GetString("StudentClassData_" + classCode, "");
            if (!string.IsNullOrEmpty(individualClassData) && !StudentPrefs.HasKey("StudentClassData_" + classCode))
            {
                StudentPrefs.SetString("StudentClassData_" + classCode, individualClassData);
                Debug.Log($"🔄 Migrated individual class data for: {classCode}");
            }
        }

        StudentPrefs.Save();
    }

}
