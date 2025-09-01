using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StudentProgressView : MonoBehaviour, IStudentProgressView
{
    [Header("Student Progress")]
    public GameObject studentProgressPrefab;
    public Transform studentProgressList;
    
    public void ClearStudentList()
    {
        foreach (Transform child in studentProgressList)
        {
            Destroy(child.gameObject);
        }
    }
    
    public void AddStudentToList(StudentModel student)
    {
        GameObject studentRow = Instantiate(studentProgressPrefab, studentProgressList);
        
        // Configure the student row with the student data
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

        // sort here instead of service
        students = students.OrderBy(s => s.studName).ToList();

        // If scene is Teachers Landing Page, show five student rows only, else show all students
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "TeacherDashboard")
        {
            int count = Mathf.Min(5, students.Count);
            for (int i = 0; i < count; i++)
            {
                AddStudentToList(students[i]);
            }
            return;
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