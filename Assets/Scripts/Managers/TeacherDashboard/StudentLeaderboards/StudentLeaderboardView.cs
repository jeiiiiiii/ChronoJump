using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StudentLeaderboardView : MonoBehaviour, IStudentLeaderboardView
{
    [Header("Student Leaderboard")]
    public GameObject leaderboardRowPrefab;
    public Transform leaderboardList;

    [Header("Loading UI")]
    public GameObject loadingSpinnerPrefab;

    public void ClearLeaderboard()
    {
        foreach (Transform child in leaderboardList)
        {
            Destroy(child.gameObject);
        }
    }

    public void ShowLoadingState()
    {
        ClearLeaderboard();

        if (loadingSpinnerPrefab != null)
        {
            GameObject spinner = Instantiate(loadingSpinnerPrefab, leaderboardList);
            spinner.name = "LoadingSpinner";
            spinner.transform.localScale = Vector3.one;
        }
        else
        {
            GameObject loadingObj = new GameObject("LoadingText");
            loadingObj.transform.SetParent(leaderboardList, false);

            TMP_Text text = loadingObj.AddComponent<TextMeshProUGUI>();
            text.text = "Loading...";
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 24;
        }
    }

    public void ClearLoadingState()
    {
        foreach (Transform child in leaderboardList)
        {
            if (child.name.Contains("LoadingSpinner") || child.name.Contains("LoadingText"))
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void AddStudentToLeaderboard(LeaderboardStudentModel student, int ranking)
    {
        GameObject leaderboardRow = Instantiate(leaderboardRowPrefab, leaderboardList);
        
        var leaderboardRowComponent = leaderboardRow.GetComponent<LeaderboardRowView>();
        if (leaderboardRowComponent != null)
        {
            leaderboardRowComponent.SetupStudent(student, ranking);
        }
        
        leaderboardRow.SetActive(true);
    }

    public void AddDefaultLeaderboardRow(int ranking)
    {
        GameObject leaderboardRow = Instantiate(leaderboardRowPrefab, leaderboardList);

        var leaderboardRowComponent = leaderboardRow.GetComponent<LeaderboardRowView>();
        if (leaderboardRowComponent != null)
        {
            leaderboardRowComponent.SetDefaultStudent(ranking);
        }

        leaderboardRow.SetActive(true);
    }

    public void ShowStudentLeaderboard(List<LeaderboardStudentModel> students)
    {
        ClearLeaderboard();

        if (students == null || students.Count == 0)
        {
            return;
        }

        students.Sort((a, b) => b.overallScore.CompareTo(a.overallScore));

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "TeacherDashboard")
        {
            int count = Mathf.Min(10, students.Count);
            for (int i = 0; i < count; i++)
            {
                AddStudentToLeaderboard(students[i], i + 1);
            }
        }
        else
        {
            for (int i = 0; i < students.Count; i++)
            {
                AddStudentToLeaderboard(students[i], i + 1);
            }
        }
    }
}
