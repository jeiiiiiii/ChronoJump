using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class StudentProgressView : MonoBehaviour
{
    [Header("Landing Page Progress")]
    public GameObject landingStudentPrefab;
    public Transform landingStudentList;
    public GameObject landingLoadingSpinnerPrefab;
    public TextMeshProUGUI landingEmptyMessage;

    [Header("Full Progress Page")]
    public GameObject fullStudentPrefab;
    public Transform fullStudentList;
    public GameObject fullLoadingSpinnerPrefab;
    public TextMeshProUGUI fullEmptyMessage;

    public void ClearStudentLists()
    {
        if (landingStudentList != null)
        {
            foreach (Transform child in landingStudentList)
            {
                if (!child.name.Contains("LoadingSpinner") && !child.name.Contains("LoadingText") &&
                    (landingEmptyMessage == null || child.gameObject != landingEmptyMessage.gameObject))
                {
                    Destroy(child.gameObject);
                }
            }
        }

        if (fullStudentList != null)
        {
            foreach (Transform child in fullStudentList)
            {
                if (!child.name.Contains("LoadingSpinner") && !child.name.Contains("LoadingText") &&
                    (fullEmptyMessage == null || child.gameObject != fullEmptyMessage.gameObject))
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }


    public void ShowLoadingState(bool viewAll = false)
    {
        ClearLoadingState(viewAll);
        ClearStudentLists();
        SetEmptyMessage(false, viewAll);

        Transform targetList = viewAll ? fullStudentList : landingStudentList;
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

    public void ClearLoadingState(bool viewAll = false)
    {
        Transform targetList = viewAll ? fullStudentList : landingStudentList;
        if (targetList == null) return;

        foreach (Transform child in targetList)
        {
            if (child.name.Contains("LoadingSpinner") || child.name.Contains("LoadingText"))
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void AddStudentToList(StudentModel student, bool fullView)
    {
        Transform targetList = fullView ? fullStudentList : landingStudentList;
        GameObject prefabToUse = fullView ? fullStudentPrefab : landingStudentPrefab;

        if (targetList == null || prefabToUse == null)
        {
            Debug.LogWarning("Missing prefab or target list for student progress view.");
            return;
        }

        GameObject studentRow = Instantiate(prefabToUse, targetList);
        var studentRowComponent = studentRow.GetComponent<StudentProgressRowView>();
        if (studentRowComponent != null)
        {
            studentRowComponent.SetupStudent(student);
        }

        studentRow.SetActive(true);
    }

    // For backward compatibility
    public void ShowStudentProgress(List<StudentModel> students)
    {
        ShowStudentProgress(students, false);
    }

    public void ShowStudentProgress(List<StudentModel> students, bool viewAll)
    {
        ClearStudentLists();
        ClearLoadingState(viewAll);

        if (students == null || students.Count == 0)
        {
            SetEmptyMessage(true, viewAll);
            return;
        }

        SetEmptyMessage(false, viewAll);

        students = students.OrderBy(s => s.studName).ToList();

        if (viewAll)
        {
            foreach (var student in students)
            {
                AddStudentToList(student, true);
            }
        }
        else
        {
            int count = Mathf.Min(5, students.Count);
            for (int i = 0; i < count; i++)
            {
                AddStudentToList(students[i], false);
            }
        }
    }

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
