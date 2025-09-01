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
    public CreateClassView createClassView;

    [Header("State")]
    private DashboardState _dashboardState;

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
            SelectFirstClass();
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

    private void OnClassSelected(string classCode, string className)
    {
        _dashboardState.selectedClassCode = classCode;
        _dashboardState.selectedClassName = className;

        dashboardView.UpdateClassSelection(classCode, className);
        LoadStudentProgress(classCode);
    }

    private void LoadStudentProgress(string classCode)
    {
        // Use FirebaseManager to get students
        FirebaseManager.Instance.GetStudentsInClass(classCode, students =>
        {
            _dashboardState.currentStudents = students ?? new List<StudentModel>();
            dashboardView.SetEmptyMessages(_dashboardState.currentStudents == null || _dashboardState.currentStudents.Count == 0);
            studentProgressView.ShowStudentProgress(_dashboardState.currentStudents);
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
    }

    public void OnBackToMainMenuClicked()
    {
        SceneManager.LoadScene("TitleScreen");
    }

    public void RefreshDashboard()
    {
        LoadTeacherData();
    }
}
