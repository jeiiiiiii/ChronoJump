using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class StudentClassroomLeaderboard : MonoBehaviour
{
    [Header("Leaderboard UI")]
    public GameObject leaderboardRowPrefab;
    public Transform leaderboardContainer;
    public GameObject loadingSpinnerPrefab;
    public TextMeshProUGUI emptyMessage;

    [Header("References")]
    public ClassInfo classInfoComponent;

    [Header("State")]
    private bool isLoadingLeaderboard = false;
    private bool isInitialized = false;
    private StudentClassData currentClass;
    private GameObject loadingSpinnerInstance;
    private bool isDestroyed = false; // ✅ ADD DESTRUCTION FLAG

    private void OnEnable()
    {
        isDestroyed = false; // ✅ Reset flag
        ClassInfo.OnClassDataLoaded += OnClassDataLoaded;
    }

    private void OnDisable()
    {
        isDestroyed = true; // ✅ Set flag when disabling
        ClassInfo.OnClassDataLoaded -= OnClassDataLoaded;
    }

    private void Start()
    {
        // ✅ Validate UI references
        if (!ValidateUIReferences())
        {
            Debug.LogError("❌ StudentClassroomLeaderboard: Critical UI references missing!");
            return;
        }

        if (classInfoComponent != null && classInfoComponent.IsDataLoaded())
        {
            InitializeLeaderboard();
        }
        else
        {
            Debug.Log("Waiting for ClassInfo to load data...");
            ShowLoadingState();
        }
    }

    // ✅ NEW: Validate all critical UI references
    private bool ValidateUIReferences()
    {
        bool isValid = true;

        if (leaderboardContainer == null)
        {
            Debug.LogError("❌ Leaderboard Container is not assigned!");
            isValid = false;
        }
        else if (leaderboardContainer.gameObject.scene.name == null)
        {
            Debug.LogError("❌ Leaderboard Container is a PREFAB, not a scene object! Please assign from scene hierarchy.");
            isValid = false;
        }

        if (leaderboardRowPrefab == null)
        {
            Debug.LogError("❌ Leaderboard Row Prefab is not assigned!");
            isValid = false;
        }

        if (classInfoComponent == null)
        {
            Debug.LogError("❌ ClassInfo component is not assigned!");
            isValid = false;
        }

        return isValid;
    }

    private void OnClassDataLoaded(StudentClassData classData)
    {
        // ✅ Check if destroyed
        if (isDestroyed || this == null) return;

        Debug.Log($"📢 Student Leaderboard received class data: {classData?.classCode ?? "NULL"}");
        
        if (isInitialized)
        {
            Debug.Log("⚠️ Leaderboard already initialized, skipping duplicate event");
            return;
        }
        
        InitializeLeaderboard();
    }

    private void InitializeLeaderboard()
    {
        // ✅ Check if destroyed
        if (isDestroyed || this == null) return;

        if (isInitialized)
        {
            Debug.Log("⚠️ Leaderboard already initialized, skipping");
            return;
        }

        if (classInfoComponent != null)
        {
            currentClass = classInfoComponent.GetCurrentClassData();
            
            if (currentClass != null && !string.IsNullOrEmpty(currentClass.classCode))
            {
                Debug.Log($"📊 Loading leaderboard for class: {currentClass.classCode}");
                             
                isInitialized = true;
                LoadLeaderboard();
            }
            else
            {
                Debug.LogError("❌ No valid class data available");
                ShowEmptyMessage("No class joined");
            }
        }
        else
        {
            Debug.LogError("❌ ClassInfo component not assigned!");
        }
    }

    private void LoadLeaderboard()
    {
        // ✅ Check if destroyed
        if (isDestroyed || this == null) return;

        if (isLoadingLeaderboard)
        {
            Debug.Log("⚠️ Leaderboard already loading...");
            return;
        }

        if (string.IsNullOrEmpty(currentClass?.classCode))
        {
            Debug.LogError("❌ Cannot load leaderboard: no class code");
            ShowEmptyMessage("No class data available");
            return;
        }

        isLoadingLeaderboard = true;
        ShowLoadingState();
        ClearLeaderboard();

        Debug.Log($"🔄 Fetching leaderboard for class: {currentClass.classCode}");

        FirebaseManager.Instance.GetStudentLeaderboard(currentClass.classCode, leaderboardData =>
        {
            // ✅ Critical: Check if destroyed in callback
            if (isDestroyed || this == null)
            {
                Debug.Log("⚠️ Leaderboard destroyed during data fetch, aborting display");
                return;
            }

            isLoadingLeaderboard = false;
            ClearLoadingState();

            if (leaderboardData == null || leaderboardData.Count == 0)
            {
                Debug.Log("ℹ️ No leaderboard data found");
                ShowEmptyMessage("No students in leaderboard yet");
                return;
            }

            var activeStudents = leaderboardData
                .Where(s => s != null && !s.isRemoved)
                .OrderByDescending(s => s.overallScore)
                .ToList();

            if (activeStudents.Count == 0)
            {
                Debug.Log("ℹ️ No active students in leaderboard");
                ShowEmptyMessage("No students in leaderboard yet");
                return;
            }

            Debug.Log($"✅ Displaying {activeStudents.Count} students in leaderboard");
            HideEmptyMessage();
            DisplayLeaderboard(activeStudents);
        });
    }

    private void DisplayLeaderboard(List<LeaderboardStudentModel> students)
    {
        // ✅ Check if destroyed
        if (isDestroyed || this == null) return;

        ClearLeaderboard();

        for (int i = 0; i < students.Count; i++)
        {
            // ✅ Check if destroyed during loop
            if (isDestroyed || this == null) break;
            
            CreateLeaderboardRow(students[i], i + 1);
        }
    }

    private void CreateLeaderboardRow(LeaderboardStudentModel student, int ranking)
    {
        // ✅ Enhanced validation
        if (isDestroyed || leaderboardRowPrefab == null || leaderboardContainer == null)
        {
            if (!isDestroyed)
                Debug.LogError("❌ Leaderboard prefab or container not assigned");
            return;
        }

        // ✅ Validate container is a scene object
        if (leaderboardContainer.gameObject.scene.name == null)
        {
            Debug.LogError("❌ Cannot create row - container is a PREFAB, not a scene object!");
            return;
        }

        GameObject rowObject = Instantiate(leaderboardRowPrefab, leaderboardContainer);
        LeaderboardRowView rowView = rowObject.GetComponent<LeaderboardRowView>();

        if (rowView != null)
        {
            rowView.SetupStudent(student, ranking);
        }
        else
        {
            Debug.LogError("❌ LeaderboardRowView component not found on prefab");
        }

        rowObject.SetActive(true);
    }

    private void ShowLoadingState()
    {
        // ✅ Check if destroyed
        if (isDestroyed || leaderboardContainer == null) return;

        HideEmptyMessage();

        // ✅ Validate container is a scene object
        if (leaderboardContainer.gameObject.scene.name == null)
        {
            Debug.LogError("❌ Leaderboard Container is a PREFAB, not a scene object! Please assign a GameObject from the scene hierarchy.");
            ShowEmptyMessage("Configuration error - check console");
            return;
        }

        if (loadingSpinnerPrefab != null && leaderboardContainer != null)
        {
            if (loadingSpinnerInstance == null)
            {
                loadingSpinnerInstance = Instantiate(loadingSpinnerPrefab, leaderboardContainer);
                loadingSpinnerInstance.name = "LeaderboardLoadingSpinner";
                
                RectTransform rect = loadingSpinnerInstance.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = Vector2.zero;
                }
            }

            loadingSpinnerInstance.SetActive(true);
        }
        else if (emptyMessage != null)
        {
            ShowEmptyMessage("Loading leaderboard...");
        }
    }

    private void ClearLoadingState()
    {
        // ✅ Check if destroyed
        if (isDestroyed) return;

        if (loadingSpinnerInstance != null)
        {
            loadingSpinnerInstance.SetActive(false);
        }
    }

    private void ClearLeaderboard()
    {
        // ✅ Check if destroyed
        if (isDestroyed || leaderboardContainer == null) return;

        // ✅ Validate container is a scene object
        if (leaderboardContainer.gameObject.scene.name == null)
        {
            Debug.LogError("❌ Leaderboard Container is a PREFAB! Assign a scene GameObject instead.");
            return;
        }

        foreach (Transform child in leaderboardContainer)
        {
            // ✅ Skip if destroyed during iteration
            if (isDestroyed || this == null) break;

            if (child.gameObject != loadingSpinnerInstance)
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }
    }

    private void ShowEmptyMessage(string message)
    {
        // ✅ Check if destroyed
        if (isDestroyed || emptyMessage == null) return;

        emptyMessage.text = message;
        emptyMessage.gameObject.SetActive(true);
    }

    private void HideEmptyMessage()
    {
        // ✅ Check if destroyed
        if (isDestroyed || emptyMessage == null) return;

        emptyMessage.gameObject.SetActive(false);
    }

    public void RefreshLeaderboard()
    {
        // ✅ Check if destroyed
        if (isDestroyed || this == null)
        {
            Debug.Log("⚠️ Cannot refresh - component destroyed");
            return;
        }

        if (isLoadingLeaderboard)
        {
            Debug.Log("⚠️ Leaderboard already loading, please wait...");
            return;
        }

        Debug.Log("🔄 Refreshing leaderboard...");
        
        if (classInfoComponent != null)
        {
            currentClass = classInfoComponent.GetCurrentClassData();
        }

        isInitialized = false;
        InitializeLeaderboard();
    }

    private void OnDestroy()
    {
        // ✅ Mark as destroyed
        isDestroyed = true;

        // ✅ Clean up loading spinner
        if (loadingSpinnerInstance != null)
        {
            Destroy(loadingSpinnerInstance);
            loadingSpinnerInstance = null;
        }

        Debug.Log("🔴 StudentClassroomLeaderboard destroyed");
    }
}
