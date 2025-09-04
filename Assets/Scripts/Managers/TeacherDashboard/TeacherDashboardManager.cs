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
    
    private bool _isLoadingProgress = false;
    private bool _isLoadingLeaderboard = false;
    private bool _isViewingAllStudents = false;
    private bool _isViewingAllLeaderboard = false;


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
            
            if (!string.IsNullOrEmpty(_previousSelectedClassCode) && 
                _dashboardState.teacherData.classCode.ContainsKey(_previousSelectedClassCode))
            {
                RestoreClassSelection(_previousSelectedClassCode);
            }
            else
            {
                SelectFirstClass();
            }
        }
    }

    private void UpdateDashboardView()
    {
        string teacherName = $"{_dashboardState.teacherData.title} {_dashboardState.teacherData.teachLastName}";
        dashboardView.UpdateTeacherInfo(teacherName, null);

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
            classListView.SelectClassByCode(classCode);
        }
    }

    private void OnClassSelected(string classCode, string className)
    {
        _dashboardState.selectedClassCode = classCode;
        _dashboardState.selectedClassName = className;
        
        _previousSelectedClassCode = classCode;

        dashboardView.UpdateClassSelection(classCode, className);

        ShowLoadingStates();
        
        LoadStudentLeaderboard(classCode);
        LoadStudentProgress(classCode); 
    }

    private void ShowLoadingStates()
    {
        if (studentProgressView != null)
        {
            studentProgressView.ShowLoadingState(); 
        }
        
        if (studentLeaderboardView != null)
        {
            // Pass the correct view flag based on current state
            studentLeaderboardView.ShowLoadingState(_isViewingAllLeaderboard); 
        }
    }

    private void LoadStudentProgress(string classCode)
    {
        if (_isLoadingProgress) return; 

        _isLoadingProgress = true;
        dashboardView.SetProgressEmptyMessages(false);

        if (_dashboardState.cachedStudents.ContainsKey(classCode))
        {
            _isLoadingProgress = false;
            var cached = _dashboardState.cachedStudents[classCode];
            studentProgressView.ClearLoadingState();

            if (cached.Count == 0)
            {
                dashboardView.SetProgressEmptyMessages(true);
                dashboardView.SetViewAllProgressButtonVisible(false); 
            }
            else
            {
                dashboardView.SetViewAllProgressButtonVisible(true);  
                studentProgressView.ShowStudentProgress(cached, _isViewingAllStudents);
            }
            return;
        }

        studentProgressView.ShowLoadingState();

        FirebaseManager.Instance.GetStudentsInClass(classCode, students =>
        {
            _isLoadingProgress = false;

            var list = students ?? new List<StudentModel>();
            _dashboardState.currentStudents = list;
            _dashboardState.cachedStudents[classCode] = list;

            studentProgressView.ClearLoadingState();

            if (list.Count == 0)
            {
                dashboardView.SetProgressEmptyMessages(true);
                dashboardView.SetViewAllProgressButtonVisible(false);
            }
            else
            {
                dashboardView.SetViewAllProgressButtonVisible(true);
                studentProgressView.ShowStudentProgress(list, _isViewingAllStudents);
            }
        });
    }

    private void LoadStudentLeaderboard(string classCode)
    {
        if (_isLoadingLeaderboard) return;

        _isLoadingLeaderboard = true;
        dashboardView.SetLeaderboardEmptyMessages(false);

        if (_dashboardState.cachedLeaderboards.ContainsKey(classCode))
        {
            _isLoadingLeaderboard = false;
            var cached = _dashboardState.cachedLeaderboards[classCode];
            studentLeaderboardView.ClearLeaderboard();

            if (cached.Count == 0)
            {
                dashboardView.SetLeaderboardEmptyMessages(true);
                dashboardView.SetViewAllLeaderboardButtonVisible(false); 
            }
            else
            {
                dashboardView.SetViewAllLeaderboardButtonVisible(true);  
                // Pass the _isViewingAllLeaderboard flag to show in correct view
                studentLeaderboardView.ShowStudentLeaderboard(cached, _isViewingAllLeaderboard);
            }
            return;
        }

        studentLeaderboardView.ShowLoadingState(_isViewingAllLeaderboard);

        FirebaseManager.Instance.GetStudentLeaderboard(classCode, leaderboardData =>
        {
            _isLoadingLeaderboard = false;

            var list = leaderboardData ?? new List<LeaderboardStudentModel>();
            _dashboardState.cachedLeaderboards[classCode] = list;

            if (list.Count == 0)
            {
                studentLeaderboardView.ClearLeaderboard();
                dashboardView.SetLeaderboardEmptyMessages(true);
                dashboardView.SetViewAllLeaderboardButtonVisible(false);
            }
            else
            {
                dashboardView.SetViewAllLeaderboardButtonVisible(true);
                // Pass the _isViewingAllLeaderboard flag to show in correct view
                studentLeaderboardView.ShowStudentLeaderboard(list, _isViewingAllLeaderboard);
            }
        });
    }


    private IEnumerator ShowDashboardAfterRender()
    {
        yield return new WaitForEndOfFrame();
        dashboardView.SetDashboardInteractable(true);
    }

    public void OnCreateNewClassClicked()
    {
        dashboardView.ShowCreateClassPanel();
    }

    public void OnViewLeaderboardClickedFull()
    {
        _isViewingAllLeaderboard = true;
        dashboardView.ShowLeaderboardPage();

        if (!_isLoadingLeaderboard && !string.IsNullOrEmpty(_dashboardState.selectedClassCode))
        {
            LoadStudentLeaderboard(_dashboardState.selectedClassCode);
        }
    }

    public void OnBackToMainMenuClicked()
    {
        SceneManager.LoadScene("TitleScreen");
    }
    
    public void OnViewStudentProgressClickedFull()
    {
        _isViewingAllStudents = true;
        dashboardView.ShowStudentProgressPage();

        if (!_isLoadingProgress && !string.IsNullOrEmpty(_dashboardState.selectedClassCode))
        {
            LoadStudentProgress(_dashboardState.selectedClassCode);
        }
    }

 public void OnBackToLandingPageClicked()
{
    _isViewingAllStudents = false;
    _isViewingAllLeaderboard = false;
    dashboardView.ShowLandingPage();

    if (!_isLoadingProgress && !string.IsNullOrEmpty(_dashboardState.selectedClassCode))
    {
        LoadStudentProgress(_dashboardState.selectedClassCode);
    }

    if (!_isLoadingLeaderboard && !string.IsNullOrEmpty(_dashboardState.selectedClassCode))
    {
        LoadStudentLeaderboard(_dashboardState.selectedClassCode);
    }

    StartCoroutine(ShowDashboardAfterRender());
}



    public void RefreshDashboard()
    {
        _isLoadingProgress = false;
        _isLoadingLeaderboard = false;
        
        LoadTeacherData();
    }
    
    public void RefreshDashboardAndSelectClass(string newClassCode)
    {
        _previousSelectedClassCode = newClassCode;
        _isLoadingProgress = false;
        _isLoadingLeaderboard = false;
        
        LoadTeacherData();
    }

    public void RefreshLeaderboard()
    {
        if (!string.IsNullOrEmpty(_dashboardState.selectedClassCode) && !_isLoadingLeaderboard)
        {
            LoadStudentLeaderboard(_dashboardState.selectedClassCode);
        }
    }
}