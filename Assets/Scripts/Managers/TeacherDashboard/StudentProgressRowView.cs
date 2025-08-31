using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StudentProgressRowView : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI studentNameText;
    public Image studentProfileImage;
    public DynamicProgressBar progressBar;
    
    public void SetupStudent(StudentModel student)
    {
        if (studentNameText != null)
            studentNameText.text = student.studName;
        
        // Calculate progress percentage from student.progress dictionary
        float progressPercentage = CalculateProgressPercentage(student.progress);
        
        if (progressBar != null)
            progressBar.SetProgress(progressPercentage);
            
        // Set profile image if available
    }
    
    private float CalculateProgressPercentage(System.Collections.Generic.Dictionary<string, string> progress)
    {
        if (progress == null || progress.Count == 0)
            return 0f;
        
        int completedLevels = 0;
        foreach (var level in progress)
        {
            if (level.Value == "completed" || level.Value == "done")
            {
                completedLevels++;
            }
        }
        
        return (float)completedLevels / progress.Count * 100f;
    }
}