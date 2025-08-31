using System.Collections.Generic;
using UnityEngine;

public interface IDashboardView
{
    void ShowLandingPage();
    void ShowEmptyLandingPage();
    void ShowStudentProgressPage();
    void ShowLeaderboardPage();
    void ShowCreateClassPanel();
    void UpdateTeacherInfo(string teacherName, Sprite profileIcon);
    void UpdateClassSelection(string classCode, string className);
    void SetDashboardInteractable(bool interactable);
}

public interface IClassListView
{
    void ClearClassList();
    void AddClassToList(string classCode, string className, System.Action<string, string> onClassSelected);
    void SelectFirstClass();
}

public interface IStudentProgressView
{
    void ClearStudentList();
    void AddStudentToList(StudentModel student);
    void ShowStudentProgress(List<StudentModel> students);
}