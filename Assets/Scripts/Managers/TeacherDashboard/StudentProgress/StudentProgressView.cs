using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class StudentProgressView : MonoBehaviour, IStudentProgressView
{
    [Header("Student Progress")]
    public GameObject studentProgressPrefab;
    public Transform studentProgressList;

    [Header("Loading UI")]
    public GameObject loadingSpinnerPrefab;


    public void ClearStudentList()
    {
        foreach (Transform child in studentProgressList)
        {
            Destroy(child.gameObject);
        }
    }

    public void ShowLoadingState()
    {
        ClearStudentList();

        if (loadingSpinnerPrefab != null)
        {
            GameObject spinner = Instantiate(loadingSpinnerPrefab, studentProgressList);
            spinner.name = "LoadingSpinner";
            spinner.transform.localScale = Vector3.one;
        }
        else
        {
            GameObject loadingObj = new GameObject("LoadingText");
            loadingObj.transform.SetParent(studentProgressList, false);

            TMP_Text text = loadingObj.AddComponent<TextMeshProUGUI>();
            text.text = "Loading...";
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 24;
        }
    }

    public void ClearLoadingState()
    {
        foreach (Transform child in studentProgressList)
        {
            if (child.name.Contains("LoadingSpinner") || child.name.Contains("LoadingText"))
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void AddStudentToList(StudentModel student)
    {
        GameObject studentRow = Instantiate(studentProgressPrefab, studentProgressList);

        var studentRowComponent = studentRow.GetComponent<StudentProgressRowView>();
        if (studentRowComponent != null)
        {
            studentRowComponent.SetupStudent(student);
        }

        studentRow.SetActive(true);
    }

    public void ShowStudentProgress(List<StudentModel> students)
    {
        ClearStudentList();

        students = students.OrderBy(s => s.studName).ToList();

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "TeacherDashboard")
        {
            int count = Mathf.Min(5, students.Count);
            for (int i = 0; i < count; i++)
            {
                AddStudentToList(students[i]);
            }
        }
        else
        {
            foreach (var student in students)
            {
                AddStudentToList(student);
            }
        }
    }
}
