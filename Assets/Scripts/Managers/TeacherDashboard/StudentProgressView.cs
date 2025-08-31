using System.Collections.Generic;
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
        
        foreach (var student in students)
        {
            AddStudentToList(student);
        }
    }
}