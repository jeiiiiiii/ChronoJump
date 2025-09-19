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
    private bool _isInDeleteMode = false; // NEW: Track if we're in delete mode

    [Header("Class Management")]
    public ClassManager classManager;

    [Header("Confirmation Dialog")]
    public ConfirmationDialog confirmationDialog; // NEW: Reference to confirmation dialog

    private void Awake()
    {
        InitializeState();
        LoadTeacherData();

        // NEW: Set dashboard manager reference in student progress view
        if (studentProgressView != null)
        {
            studentProgressView.SetDashboardManager(this);
        }

        if (classManager != null)
        {
            classManager.OnClassDeleted += HandleClassDeleted;
            classManager.OnClassEdited += HandleClassEdited;
        }
    }

    private void HandleClassDeleted()
{
    // Get the deleted class code before clearing cache
    string deletedClassCode = _dashboardState.selectedClassCode;
    
    _dashboardState.cachedStudents.Clear();
    _dashboardState.cachedLeaderboards.Clear();

    // NEW: Notify ClassDataSync about the deletion
    if (ClassDataSync.Instance != null && !string.IsNullOrEmpty(deletedClassCode))
    {
        ClassDataSync.Instance.NotifyClassDeleted(deletedClassCode);
    }

    if (_dashboardState.HasClasses)
    {
        SelectFirstClass();
    }
    else
    {
        dashboardView.ShowEmptyLandingPage();
    }
}

    private void HandleClassEdited()
{
    // NEW: Notify ClassDataSync about the edit
    if (ClassDataSync.Instance != null && !string.IsNullOrEmpty(_dashboardState.selectedClassCode))
    {
        // Get the updated class name
        if (_dashboardState.teacherData?.classCode?.ContainsKey(_dashboardState.selectedClassCode) == true)
        {
            var classData = _dashboardState.teacherData.classCode[_dashboardState.selectedClassCode];
            string newClassName = classData[1]; // className is at index 1
            ClassDataSync.Instance.NotifyClassEdited(_dashboardState.selectedClassCode, newClassName);
        }
    }
    
    // Refresh teacher data to get updated class names
    RefreshDashboard();
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
    
    // NEW: Update ClassDataSync with the loaded class data
    if (ClassDataSync.Instance != null && teacherData.classCode != null)
    {
        // Update the cached data in ClassDataSync but don't trigger events to avoid loops
        var cachedData = new Dictionary<string, List<string>>(teacherData.classCode);
        ClassDataSync.Instance.UpdateCachedData(cachedData);
    }
    
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
                // NEW: Pass delete mode state to the view
                studentProgressView.ShowStudentProgress(cached, _isViewingAllStudents, _isInDeleteMode);
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
                // NEW: Pass delete mode state to the view
                studentProgressView.ShowStudentProgress(list, _isViewingAllStudents, _isInDeleteMode);
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

    public void OnPublishedStoriesClicked()
    {
        Debug.Log("Published Stories button clicked!");
        SceneManager.LoadScene("StoryPublish"); // Make sure this matches your scene name exactly
    }

    public void OnViewStudentProgressClickedFull()
    {
        _isViewingAllStudents = true;
        _isInDeleteMode = false; 
        dashboardView.ShowStudentProgressPage();

        if (!_isLoadingProgress && !string.IsNullOrEmpty(_dashboardState.selectedClassCode))
        {
            LoadStudentProgress(_dashboardState.selectedClassCode);
        }
    }

    // NEW: Method for delete student button
    public void OnDeleteStudentProgressClickedFull()
    {
        _isViewingAllStudents = true;
        _isInDeleteMode = true; 
        dashboardView.ShowStudentProgressPage();

        if (!_isLoadingProgress && !string.IsNullOrEmpty(_dashboardState.selectedClassCode))
        {
            LoadStudentProgress(_dashboardState.selectedClassCode);
        }
    }

    // NEW: Method called when a student's remove button is clicked
    public void OnRemoveStudentClicked(StudentModel student)
    {
        if (confirmationDialog != null)
        {
            string title = "Remove Student";
            string message = $"Are you sure you want to remove {student.studName} from the class? This action cannot be undone.";
            
            confirmationDialog.ShowDialog(title, message, 
                () => ConfirmRemoveStudent(student), 
                () => Debug.Log("Remove student cancelled"));
        }
    }

    // NEW: Method called when removal is confirmed
    private void ConfirmRemoveStudent(StudentModel student)
    {
        Debug.Log($"Removing student: {student.studName}");
        // TODO: Implement actual removal logic here
        // This would involve calling Firebase to remove the student from the class
        // and then refreshing the dashboard
        
        // For now, just log the action
        // In the future, you would call something like:
        // FirebaseManager.Instance.RemoveStudentFromClass(_dashboardState.selectedClassCode, student.userId, OnStudentRemoved);
    }

    public void OnBackToLandingPageClicked()
    {
        _isViewingAllStudents = false;
        _isViewingAllLeaderboard = false;
        _isInDeleteMode = false; // NEW: Reset delete mode when going back
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

    private void EnsureClassDataSync()
{
    // Make sure ClassDataSync exists
    if (ClassDataSync.Instance == null)
    {
        GameObject syncObject = new GameObject("ClassDataSync");
        syncObject.AddComponent<ClassDataSync>();
        Debug.Log("Created ClassDataSync instance in TeacherDashboard");
    }
}

public void OnClassCreatedSuccessfully(string classCode, string className, string classLevel)
{
    // This should be called from your CreateClassView after successful class creation
    if (ClassDataSync.Instance != null)
    {
        ClassDataSync.Instance.NotifyClassCreated(classCode, className, classLevel);
    }
    
    // Refresh dashboard to show the new class
    RefreshDashboard();
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

    private void OnDestroy()
    {
        if (classManager != null)
        {
            classManager.OnClassDeleted -= HandleClassDeleted;
            classManager.OnClassEdited -= HandleClassEdited;
        }
    }
}