using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;


public class StudentLeaderboardView : MonoBehaviour, IStudentLeaderboardView
{
    [Header("Landing Page Leaderboard")]
    public GameObject landingLeaderboardRowPrefab;
    public Transform landingLeaderboardList;
    public GameObject landingLoadingSpinnerPrefab;
    public TextMeshProUGUI landingEmptyMessage;

    [Header("Full Leaderboard Page")]
    public GameObject fullLeaderboardRowPrefab;
    public Transform fullLeaderboardList;
    public GameObject fullLoadingSpinnerPrefab;
    public TextMeshProUGUI fullEmptyMessage;

    // -----------------------
    // Clearing / Loading
    // -----------------------
    public void ClearLeaderboard()
    {
        if (landingLeaderboardList != null)
        {
            for (int i = landingLeaderboardList.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(landingLeaderboardList.GetChild(i).gameObject);
            }
        }

        if (fullLeaderboardList != null)
        {
            for (int i = fullLeaderboardList.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(fullLeaderboardList.GetChild(i).gameObject);
            }
        }
    }

    // Choose which spinner (landing or full) to show
    public void ShowLoadingState(bool viewAll = false)
    {
        // Clear rows and existing loading indicators in the target list
        ClearLoadingState(viewAll);
        // Clear leaderboard content first (like StudentProgressView does)
        ClearLeaderboard();

        if (!viewAll)
            ClearLoadingState(true); // also clear full just in case
        else
            ClearLoadingState(false);

        SetEmptyMessage(false, viewAll);

        Transform targetList = viewAll ? fullLeaderboardList : landingLeaderboardList;
        GameObject spinnerPrefab = viewAll ? fullLoadingSpinnerPrefab : landingLoadingSpinnerPrefab;

        if (spinnerPrefab != null && targetList != null)
        {
            GameObject spinner = Instantiate(spinnerPrefab, targetList);
            spinner.name = "LoadingSpinner";
            spinner.transform.localScale = Vector3.one;
        }
        else if (targetList != null)
        {
            GameObject loadingObj = new GameObject("LoadingText");
            loadingObj.transform.SetParent(targetList, false);

            TMP_Text text = loadingObj.AddComponent<TextMeshProUGUI>();
            text.text = "Loading...";
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 24;
        }
    }

    // Clear only within the chosen list
    public void ClearLoadingState(bool viewAll)
    {
        Transform targetList = viewAll ? fullLeaderboardList : landingLeaderboardList;
        if (targetList == null) return;

        for (int i = targetList.childCount - 1; i >= 0; i--)
        {
            Transform child = targetList.GetChild(i);
            if (child.name.Contains("LoadingSpinner") || child.name.Contains("LoadingText"))
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    // -----------------------
    // Add rows
    // -----------------------

    // This public method matches the interface exactly (2 params).
    // It will add a row to the landing preview by default.
    public void AddStudentToLeaderboard(LeaderboardStudentModel student, int ranking)
    {
        // default to landing preview to satisfy interface contract
        AddStudentToLeaderboard(student, ranking, false);
    }

    // Internal overloaded method that supports fullView flag
    private void AddStudentToLeaderboard(LeaderboardStudentModel student, int ranking, bool fullView)
    {
        Transform targetList = fullView ? fullLeaderboardList : landingLeaderboardList;
        GameObject prefabToUse = fullView ? fullLeaderboardRowPrefab : landingLeaderboardRowPrefab;

        if (targetList == null || prefabToUse == null)
        {
            Debug.LogWarning("Missing prefab or target list for leaderboard view.");
            return;
        }

        GameObject leaderboardRow = Instantiate(prefabToUse, targetList);
        var leaderboardRowComponent = leaderboardRow.GetComponent<LeaderboardRowView>();
        if (leaderboardRowComponent != null)
        {
            leaderboardRowComponent.SetupStudent(student, ranking);
        }

        leaderboardRow.SetActive(true);
    }

    // Keep the helper that creates a default row
    public void AddDefaultLeaderboardRow(int ranking)
    {
        // Default goes to landing list (consistent with interface)
        Transform targetList = landingLeaderboardList;
        GameObject prefabToUse = landingLeaderboardRowPrefab;

        if (targetList == null || prefabToUse == null)
        {
            Debug.LogWarning("Missing prefab or target list for default leaderboard row.");
            return;
        }

        GameObject leaderboardRow = Instantiate(prefabToUse, targetList);
        var leaderboardRowComponent = leaderboardRow.GetComponent<LeaderboardRowView>();
        if (leaderboardRowComponent != null)
        {
            leaderboardRowComponent.SetDefaultStudent(ranking);
        }

        leaderboardRow.SetActive(true);
    }

    // -----------------------
    // Show leaderboard (preview vs full)
    // -----------------------

    // Backward-compatible signature
    public void ShowStudentLeaderboard(List<LeaderboardStudentModel> students)
    {
        ShowStudentLeaderboard(students, false);
    }

    // New overload with viewAll flag
    // New overload with viewAll flag
    public void ShowStudentLeaderboard(List<LeaderboardStudentModel> students, bool viewAll)
    {
        ClearLeaderboard();
        ClearLoadingState(viewAll);

        if (students == null)
        {
            SetEmptyMessage(true, viewAll);
            return;
        }

        // 🚨 Filter out removed students before sorting
        students = students
            .Where(s => !s.isRemoved)
            .OrderByDescending(s => s.overallScore) // higher score first
            .ToList();

        if (students.Count == 0)
        {
            SetEmptyMessage(true, viewAll);
            return;
        }

        SetEmptyMessage(false, viewAll);

        if (viewAll)
        {
            for (int i = 0; i < students.Count; i++)
            {
                AddStudentToLeaderboard(students[i], i + 1, true);
            }
        }
        else
        {
            int count = Mathf.Min(10, students.Count);
            for (int i = 0; i < count; i++)
            {
                AddStudentToLeaderboard(students[i], i + 1, false);
            }
        }
    }


    // -----------------------
    // Empty message handling
    // -----------------------
    private void SetEmptyMessage(bool active, bool viewAll)
    {
        if (viewAll)
        {
            if (fullEmptyMessage != null) fullEmptyMessage.gameObject.SetActive(active);
            if (landingEmptyMessage != null) landingEmptyMessage.gameObject.SetActive(false);
        }
        else
        {
            if (landingEmptyMessage != null) landingEmptyMessage.gameObject.SetActive(active);
            if (fullEmptyMessage != null) fullEmptyMessage.gameObject.SetActive(false);
        }
    }
}