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
    public StudentOverviewPage studentOverviewPage;


    [Header("State")]
    private DashboardState _dashboardState;

    private string _previousSelectedClassCode;

    private bool _isLoadingProgress = false;
    private bool _isLoadingLeaderboard = false;
    private bool _isViewingAllStudents = false;
    private bool _isViewingAllLeaderboard = false;
    private bool _isInDeleteMode = false;

    [Header("Class Management")]
    public ClassManager classManager;

    [Header("Confirmation Dialog")]
    public ConfirmationDialog confirmationDialog;

    [Header("Loading State")]
    private bool _isInitialLoading = true;

    private void ShowInitialLoadingState(bool show)
    {
        if (dashboardView != null)
        {
            dashboardView.SetLoadingState(show);
        }

        Debug.Log($"🔄 Dashboard loading: {(show ? "SHOWING" : "HIDING")}");
    }

    private void ShowPartialLoadingState(bool classListLoading, bool teacherInfoLoading)
    {
        if (dashboardView != null)
        {
            dashboardView.SetPartialLoadingState(classListLoading, teacherInfoLoading);
        }
    }


    // Update the Awake method:
    private void Awake()
    {
        EnsureSceneNavigationManager();
        InitializeState();

        // Show loading spinner immediately
        ShowInitialLoadingState(true);

        LoadTeacherData();

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


    // Update the OnTeacherDataLoaded method:
    private void OnTeacherDataLoaded(TeacherModel teacherData)
    {
        if (this == null || !gameObject.activeInHierarchy)
        {
            Debug.Log("TeacherDashboardManager was destroyed before teacher data loaded");
            ShowInitialLoadingState(false);
            return;
        }

        if (teacherData == null)
        {
            Debug.LogError("No teacher data found.");
            ShowInitialLoadingState(false);
            return;
        }

        _dashboardState.teacherData = teacherData;

        if (ClassDataSync.Instance != null && teacherData.classCode != null)
        {
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

        // Hide loading after data is processed
        StartCoroutine(HideLoadingAfterDelay());
    }

    // Add this coroutine:
    private IEnumerator HideLoadingAfterDelay()
    {
        yield return new WaitForSeconds(0.3f);
        ShowInitialLoadingState(false);
        _isInitialLoading = false;
        Debug.Log("✅ Dashboard loading complete");
    }

    // Update the UpdateDashboardView method:
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
            ShowInitialLoadingState(false);
            _isInitialLoading = false;
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
        // ✅ Always refresh after deletion so teacherData is up-to-date
        RefreshDashboard();
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
        // Add these debug checks
        if (FirebaseManager.Instance == null)
        {
            Debug.LogError("FirebaseManager.Instance is null!");
            return;
        }

        Debug.Log($"FirebaseManager exists: {FirebaseManager.Instance != null}");
        Debug.Log($"This GameObject active: {gameObject.activeInHierarchy}");

        FirebaseManager.Instance.GetUserData(userData =>
        {
            // Add null check here too
            if (this == null || !gameObject.activeInHierarchy)
            {
                Debug.Log("TeacherDashboardManager was destroyed during GetUserData callback");
                return;
            }

            if (userData?.role?.ToLower() != "teacher")
            {
                Debug.LogError("User is not a teacher.");
                return;
            }

            FirebaseManager.Instance.GetTeacherData(userData.userId, OnTeacherDataLoaded);
        });
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
            studentLeaderboardView.ShowLoadingState(_isViewingAllLeaderboard);
        }
    }

    private List<StudentModel> FilterActiveStudents(List<StudentModel> students)
    {
        if (students == null) return new List<StudentModel>();
        return students.Where(student => student != null && !student.isRemoved).ToList();
    }

    private List<LeaderboardStudentModel> FilterActiveLeaderboardStudents(List<LeaderboardStudentModel> leaderboardStudents)
    {
        if (leaderboardStudents == null) return new List<LeaderboardStudentModel>();
        return leaderboardStudents.Where(student => student != null && !student.isRemoved).ToList();
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
            var activeStudents = FilterActiveStudents(cached);

            studentProgressView.ClearLoadingState();

            if (activeStudents.Count == 0)
            {
                // NEW: Check if we're in delete mode and automatically navigate back
                if (_isInDeleteMode && _isViewingAllStudents)
                {
                    Debug.Log("No more students to delete. Returning to landing page.");
                    OnBackToLandingPageClicked();
                    return;
                }

                dashboardView.SetProgressEmptyMessages(true);
                dashboardView.SetViewAllProgressButtonVisible(false);
                dashboardView.SetDeleteStudentButtonVisible(false);
            }
            else
            {
                dashboardView.SetViewAllProgressButtonVisible(true);
                dashboardView.SetDeleteStudentButtonVisible(true);
                studentProgressView.ShowStudentProgress(activeStudents, _isViewingAllStudents, _isInDeleteMode);
            }
            return;
        }

        studentProgressView.ShowLoadingState();

        FirebaseManager.Instance.GetStudentsInClass(classCode, students =>
        {
            _isLoadingProgress = false;

            var allStudents = students ?? new List<StudentModel>();
            _dashboardState.cachedStudents[classCode] = allStudents;

            var activeStudents = FilterActiveStudents(allStudents);
            _dashboardState.currentStudents = activeStudents;

            studentProgressView.ClearLoadingState();

            if (activeStudents.Count == 0)
            {
                // NEW: Check if we're in delete mode and automatically navigate back
                if (_isInDeleteMode && _isViewingAllStudents)
                {
                    Debug.Log("No more students to delete. Returning to landing page.");
                    OnBackToLandingPageClicked();
                    return;
                }

                dashboardView.SetProgressEmptyMessages(true);
                dashboardView.SetViewAllProgressButtonVisible(false);
                dashboardView.SetDeleteStudentButtonVisible(false);
            }
            else
            {
                dashboardView.SetViewAllProgressButtonVisible(true);
                dashboardView.SetDeleteStudentButtonVisible(true);
                studentProgressView.ShowStudentProgress(activeStudents, _isViewingAllStudents, _isInDeleteMode);
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
            var cachedActiveLeaderboard = _dashboardState.cachedLeaderboards[classCode];

            studentLeaderboardView.ClearLeaderboard();

            if (cachedActiveLeaderboard.Count == 0)
            {
                dashboardView.SetLeaderboardEmptyMessages(true);
                dashboardView.SetViewAllLeaderboardButtonVisible(false);
            }
            else
            {
                dashboardView.SetViewAllLeaderboardButtonVisible(true);
                studentLeaderboardView.ShowStudentLeaderboard(cachedActiveLeaderboard, _isViewingAllLeaderboard);
            }
            return;
        }

        studentLeaderboardView.ShowLoadingState(_isViewingAllLeaderboard);

        FirebaseManager.Instance.GetStudentLeaderboard(classCode, leaderboardData =>
        {
            _isLoadingLeaderboard = false;

            var allLeaderboardData = leaderboardData ?? new List<LeaderboardStudentModel>();
            var activeLeaderboard = FilterActiveLeaderboardStudents(allLeaderboardData);
            _dashboardState.cachedLeaderboards[classCode] = activeLeaderboard;

            if (activeLeaderboard.Count == 0)
            {
                studentLeaderboardView.ClearLeaderboard();
                dashboardView.SetLeaderboardEmptyMessages(true);
                dashboardView.SetViewAllLeaderboardButtonVisible(false);
            }
            else
            {
                dashboardView.SetViewAllLeaderboardButtonVisible(true);
                studentLeaderboardView.ShowStudentLeaderboard(activeLeaderboard, _isViewingAllLeaderboard);
            }
        });
    }

    private IEnumerator ShowDashboardAfterRender()
    {
        yield return new WaitForEndOfFrame();

        // ✅ Just make it interactive, don't change visibility
        dashboardView.SetDashboardInteractable(true);

        Debug.Log("✅ Dashboard now fully interactive");
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
        Debug.Log("Back button clicked in Teacher Dashboard");

        if (SceneNavigationManager.Instance != null)
        {
            // Use intelligent back navigation
            SceneNavigationManager.Instance.GoBack();
        }
        else
        {
            // Fallback
            SceneManager.LoadScene("TitleScreen");
        }
    }


    public void OnPublishedStoriesClicked()
    {
        Debug.Log("Published Stories button clicked!");
        if (SceneNavigationManager.Instance != null)
            SceneNavigationManager.Instance.GoToStoryPublish();
        else
            SceneManager.LoadScene("StoryPublish");
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

    private void ConfirmRemoveStudent(StudentModel student)
    {
        Debug.Log($"Removing student: {student.studName}");

        FirebaseManager.Instance.MarkStudentAsRemoved(student.userId, _dashboardState.selectedClassCode,
            success =>
            {
                if (success)
                {
                    Debug.Log($"Student {student.studName} marked as removed successfully");

                    _dashboardState.cachedStudents.Remove(_dashboardState.selectedClassCode);
                    _dashboardState.cachedLeaderboards.Remove(_dashboardState.selectedClassCode);

                    LoadStudentProgress(_dashboardState.selectedClassCode);
                    LoadStudentLeaderboard(_dashboardState.selectedClassCode);
                }
                else
                {
                    Debug.LogError($"Failed to remove student {student.studName}");
                }
            });
    }

    public void OnBackToStudentProgressClicked()
    {
        Debug.Log("Returning to full student progress view from overview");

        // Just hide the overview page - don't reset it completely
        if (studentOverviewPage != null)
        {
            // studentOverviewPage.ResetPage(); // REMOVE THIS LINE - causing the issue
            studentOverviewPage.gameObject.SetActive(false);
        }

        // Show the full student progress page (keeping the same state)
        dashboardView.ShowStudentProgressPage();

        // Reload the student progress data to ensure it's current
        if (!_isLoadingProgress && !string.IsNullOrEmpty(_dashboardState.selectedClassCode))
        {
            LoadStudentProgress(_dashboardState.selectedClassCode);
        }

        Debug.Log("Successfully returned to full student progress view");
    }




    // NEW: Update the existing back method to handle overview page properly
    public void OnBackToLandingPageClicked()
    {
        Debug.Log("OnBackToLandingPageClicked called");

        _isViewingAllStudents = false;
        _isViewingAllLeaderboard = false;
        _isInDeleteMode = false;

        // NEW: Hide overview page if it's open
        if (studentOverviewPage != null)
        {
            studentOverviewPage.gameObject.SetActive(false);
        }

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

    // NEW: Add this method to provide access to the dashboard state
    public DashboardState GetDashboardState()
    {
        return _dashboardState;
    }

    private void OnDestroy()
    {
        if (classManager != null)
        {
            classManager.OnClassDeleted -= HandleClassDeleted;
            classManager.OnClassEdited -= HandleClassEdited;
        }
    }

    private void EnsureSceneNavigationManager()
    {
        if (SceneNavigationManager.Instance == null)
        {
            GameObject navigationObject = new GameObject("SceneNavigationManager");
            navigationObject.AddComponent<SceneNavigationManager>();
            Debug.Log("Created SceneNavigationManager instance");
        }
    }

    // NEW: Handle overview button click
public void OnStudentOverviewClicked(StudentModel student)
{
    Debug.Log($"🎯 TEACHER DASHBOARD MANAGER: Overview button clicked for student: {student.studName}");
    Debug.Log($"📊 Current state - ViewAll: {_isViewingAllStudents}, DeleteMode: {_isInDeleteMode}");

    // Show the student overview page
    ShowStudentOverviewPage(student);
}

    private void ShowStudentOverviewPage(StudentModel student)
    {
        Debug.Log("🔄 TEACHER DASHBOARD MANAGER: ShowStudentOverviewPage called");

        // Use dashboardView to handle page switching properly
        if (dashboardView != null)
        {
            Debug.Log("✅ Using dashboardView.ShowStudentOverviewPage()");
            dashboardView.ShowStudentOverviewPage();
        }
        else
        {
            Debug.LogError("❌ dashboardView is NULL - cannot show overview page!");
            return;
        }

        // Show and setup the overview page
        if (studentOverviewPage != null)
        {
            Debug.Log($"✅ StudentOverviewPage reference found - setting up student: {student.studName}");

            // SET THE DASHBOARD MANAGER REFERENCE
            studentOverviewPage.SetDashboardManager(this);

            // Make sure student has studId
            if (string.IsNullOrEmpty(student.studId))
            {
                student.studId = student.userId; // Fallback to userId if studId is not available
                Debug.Log($"⚠️ studId not found, using userId as fallback: {student.studId}");
            }
            Debug.Log($"🔄 TEACHER DASHBOARD: Showing student overview for {student.studName}");
            Debug.Log($"📊 StudentOverviewPage active: {studentOverviewPage.gameObject.activeInHierarchy}");

            studentOverviewPage.SetupStudentOverview(student);

            Debug.Log($"✅ TEACHER DASHBOARD: Student overview setup complete");
            Debug.Log($"📊 StudentOverviewPage active after setup: {studentOverviewPage.gameObject.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("❌ StudentOverviewPage reference is NULL!");
        }
    }


}
