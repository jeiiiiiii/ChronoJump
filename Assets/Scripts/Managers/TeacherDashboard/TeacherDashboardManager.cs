using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeacherDashboardManager : MonoBehaviour
{
   
    [Header("Views")]
    public TeacherDashboardView dashboardView;
    public ClassListView classListView;
    public StudentProgressView studentProgressView;
    public StudentLeaderboardView studentLeaderboardView;
    public CreateClassView createClassView;

    [Header("State")]
    private DashboardState _dashboardState;
    
    private string _previousSelectedClassCode;
    
    // Add loading state tracking
    private bool _isLoadingProgress = false;
    private bool _isLoadingLeaderboard = false;

    private void Awake()
    {
        InitializeState();
        LoadTeacherData();
    }

    private void InitializeState()
    {
        _dashboardState = new DashboardState();
    }

    private void LoadTeacherData()
    {
        FirebaseManager.Instance.GetUserData(userData =>
        {
            if (userData?.role?.ToLower() != "teacher")
            {
                Debug.LogError("User is not a teacher.");
                return;
            }

            // Use FirebaseManager to get teacher data
            FirebaseManager.Instance.GetTeacherData(userData.userId, OnTeacherDataLoaded);
        });
    }

    private void OnTeacherDataLoaded(TeacherModel teacherData)
    {
        if (teacherData == null)
        {
            Debug.LogError("No teacher data found.");
            return;
        }

        _dashboardState.teacherData = teacherData;
        UpdateDashboardView();

        if (_dashboardState.HasClasses)
        {
            SetupClassList();
            
            // Try to restore previous selection, otherwise select first class
            if (!string.IsNullOrEmpty(_previousSelectedClassCode) && 
                _dashboardState.teacherData.classCode.ContainsKey(_previousSelectedClassCode))
            {
                // Restore previous selection
                RestoreClassSelection(_previousSelectedClassCode);
            }
            else
            {
                // Select first class (new user or no previous selection)
                SelectFirstClass();
            }
        }
    }

    private void UpdateDashboardView()
    {
        string teacherName = $"{_dashboardState.teacherData.title} {_dashboardState.teacherData.teachLastName}";
        dashboardView.UpdateTeacherInfo(teacherName, null); // Add profile icon handling if needed

        if (_dashboardState.HasClasses)
        {
            dashboardView.ShowLandingPage();
            StartCoroutine(ShowDashboardAfterRender());
        }
        else
        {
            dashboardView.ShowEmptyLandingPage();
        }
    }

    private void SetupClassList()
    {
        classListView.ClearClassList();

        foreach (var classEntry in _dashboardState.teacherData.classCode)
        {
            string classCode = classEntry.Key;
            string className = $"{classEntry.Value[0]} - {classEntry.Value[1]}";

            classListView.AddClassToList(classCode, className, OnClassSelected);
        }
    }

    private void SelectFirstClass()
    {
        if (_dashboardState.HasClasses)
        {
            var firstClass = _dashboardState.teacherData.classCode.First();
            string classCode = firstClass.Key;
            string className = $"{firstClass.Value[0]} - {firstClass.Value[1]}";

            OnClassSelected(classCode, className);
            classListView.SelectFirstClass();
        }
    }
    
    private void RestoreClassSelection(string classCode)
    {
        if (_dashboardState.teacherData.classCode.ContainsKey(classCode))
        {
            var classData = _dashboardState.teacherData.classCode[classCode];
            string className = $"{classData[0]} - {classData[1]}";
            
            OnClassSelected(classCode, className);
            
            // Add this line to highlight the button in the UI
            classListView.SelectClassByCode(classCode);
        }
    }

    private void OnClassSelected(string classCode, string className)
    {
        _dashboardState.selectedClassCode = classCode;
        _dashboardState.selectedClassName = className;
        
        // Store current selection
        _previousSelectedClassCode = classCode;

        dashboardView.UpdateClassSelection(classCode, className);
        
        // Show loading states immediately
        ShowLoadingStates();
        
        // Load data with priority - leaderboard first (usually faster), then progress
        LoadStudentLeaderboard(classCode);
        LoadStudentProgress(classCode);
    }

    private void ShowLoadingStates()
    {
        // Show loading indicators instead of empty views
        if (studentProgressView != null)
        {
            studentProgressView.ShowLoadingState(); 
        }
        
        if (studentLeaderboardView != null)
        {
            studentLeaderboardView.ShowLoadingState(); 
        }
    }

    private void LoadStudentProgress(string classCode)
    {
        if (_isLoadingProgress) return; 

        _isLoadingProgress = true;
        dashboardView.SetProgressEmptyMessages(false);

        // Check cache first
        if (_dashboardState.cachedStudents.ContainsKey(classCode))
        {
            _isLoadingProgress = false;
            var cached = _dashboardState.cachedStudents[classCode];
            studentProgressView.ClearLoadingState();

            if (cached.Count == 0)
            {
                dashboardView.SetProgressEmptyMessages(true);
            }
            else
            {
                studentProgressView.ShowStudentProgress(cached);
            }
            return;
        }

        // Not cached → fetch from Firebase
        studentProgressView.ShowLoadingState();

        FirebaseManager.Instance.GetStudentsInClass(classCode, students =>
        {
            _isLoadingProgress = false;

            var list = students ?? new List<StudentModel>();
            _dashboardState.currentStudents = list;

            // Store in cache
            _dashboardState.cachedStudents[classCode] = list;

            studentProgressView.ClearLoadingState();

            if (list.Count == 0)
            {
                dashboardView.SetProgressEmptyMessages(true);
            }
            else
            {
                studentProgressView.ShowStudentProgress(list);
            }
        });
    }



    private void LoadStudentLeaderboard(string classCode)
    {
        if (_isLoadingLeaderboard) return;

        _isLoadingLeaderboard = true;
        dashboardView.SetLeaderboardEmptyMessages(false);

        // Check cache first
        if (_dashboardState.cachedLeaderboards.ContainsKey(classCode))
        {
            _isLoadingLeaderboard = false;
            var cached = _dashboardState.cachedLeaderboards[classCode];
            studentLeaderboardView.ClearLeaderboard();

            if (cached.Count == 0)
            {
                dashboardView.SetLeaderboardEmptyMessages(true);
            }
            else
            {
                studentLeaderboardView.ShowStudentLeaderboard(cached);
            }
            return;
        }

        // Not cached → fetch from Firebase
        FirebaseManager.Instance.GetStudentLeaderboard(classCode, leaderboardData =>
        {
            _isLoadingLeaderboard = false;

            var list = leaderboardData ?? new List<LeaderboardStudentModel>();

            // Store in cache
            _dashboardState.cachedLeaderboards[classCode] = list;

            if (list.Count == 0)
            {
                studentLeaderboardView.ClearLeaderboard();
                dashboardView.SetLeaderboardEmptyMessages(true);
            }
            else
            {
                studentLeaderboardView.ShowStudentLeaderboard(list);
            }
        });
    }



    private IEnumerator ShowDashboardAfterRender()
    {
        yield return new WaitForEndOfFrame();
        dashboardView.SetDashboardInteractable(true);
    }

    // Public methods for UI events
    public void OnCreateNewClassClicked()
    {
        dashboardView.ShowCreateClassPanel();
    }

    public void OnViewStudentProgressClicked()
    {
        dashboardView.ShowStudentProgressPage();
    }

    public void OnViewLeaderboardClicked()
    {
        dashboardView.ShowLeaderboardPage();
        
        // Only refresh if not currently loading
        if (!_isLoadingLeaderboard && !string.IsNullOrEmpty(_dashboardState.selectedClassCode))
        {
            LoadStudentLeaderboard(_dashboardState.selectedClassCode);
        }
    }

    public void OnBackToMainMenuClicked()
    {
        SceneManager.LoadScene("TitleScreen");
    }

    public void RefreshDashboard()
    {
        // Reset loading states
        _isLoadingProgress = false;
        _isLoadingLeaderboard = false;
        
        LoadTeacherData();
    }
    
    // Method to select a newly created class
    public void RefreshDashboardAndSelectClass(string newClassCode)
    {
        _previousSelectedClassCode = newClassCode;
        
        // Reset loading states
        _isLoadingProgress = false;
        _isLoadingLeaderboard = false;
        
        LoadTeacherData();
    }

    // Method to refresh leaderboard data
    public void RefreshLeaderboard()
    {
        if (!string.IsNullOrEmpty(_dashboardState.selectedClassCode) && !_isLoadingLeaderboard)
        {
            LoadStudentLeaderboard(_dashboardState.selectedClassCode);
        }
    }
}