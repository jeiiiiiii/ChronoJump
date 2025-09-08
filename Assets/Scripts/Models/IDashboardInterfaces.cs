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
    // Events for class actions (these should be properties, not fields)
    System.Action<string, string> OnDeleteClassRequested { get; set; }
    System.Action<string, string> OnEditClassRequested { get; set; }
    
    // Basic list management methods
    void ClearClassList();
    void AddClassToList(string classCode, string className, System.Action<string, string> onClassSelected);
    void RemoveClassFromList(string classCode);
    
    // Selection management methods
    void SelectFirstClass();
    void SelectClassByCode(string classCode);
}

public interface IStudentProgressView
{
    void ClearStudentList();
    void AddStudentToList(StudentModel student);
    void ShowStudentProgress(List<StudentModel> students);
}