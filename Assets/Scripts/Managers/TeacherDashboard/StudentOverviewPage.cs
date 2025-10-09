using UnityEngine;
using TMPro;

public class StudentOverviewPage : MonoBehaviour
{
    [Header("Student Info")]
    public TextMeshProUGUI studentNameText;
    public TextMeshProUGUI studentSectionText;
    
    public void SetupStudentOverview(StudentModel student)
    {
        if (studentNameText != null)
            studentNameText.text = student.studName;
            
        if (studentSectionText != null)
            studentSectionText.text = student.classCode;
        
        // Populate progress data
        if (student.studentProgress != null)
        {
            // You can reuse the progress data parsing from StudentProgressRowView
            // or create more detailed overview here
        }
    }
}