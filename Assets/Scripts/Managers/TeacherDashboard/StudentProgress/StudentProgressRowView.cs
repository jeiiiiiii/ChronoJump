using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StudentProgressRowView : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI studentNameText;
    public DynamicProgressBar progressBar;
    
    [Header("Additional Progress UI")]
    public TextMeshProUGUI ProgressStudChapter;
    public TextMeshProUGUI ProgressStudStory;
    public TextMeshProUGUI ProgressSuccessRate;
    public TextMeshProUGUI ProgressStudOverallScore;
    
    [Header("Delete Mode UI")]
    public Button removeButton;
    
    private StudentModel _currentStudent;
    private TeacherDashboardManager _dashboardManager;
    
    public void SetupStudent(StudentModel student, bool showRemoveButton = false, TeacherDashboardManager dashboardManager = null)
    {
        // Store references
        _currentStudent = student;
        _dashboardManager = dashboardManager;
        
        // Set basic student info
        if (studentNameText != null)
            studentNameText.text = student.studName;
        
        // Use the progress data from the student model
        if (student.studentProgress != null)
        {
            SetProgressData(student.studentProgress);
        }
        else
        {
            SetDefaultProgressData();
        }
        
        // NEW: Handle remove button visibility and setup
        SetupRemoveButton(showRemoveButton);
    }
    
    // NEW: Setup remove button
    private void SetupRemoveButton(bool showRemoveButton)
    {
        if (removeButton != null)
        {
            Debug.Log($"Setting up remove button for {_currentStudent?.studName}, show: {showRemoveButton}");
            removeButton.gameObject.SetActive(showRemoveButton);
            
            if (showRemoveButton)
            {
                // Clear previous listeners to avoid duplicates
                removeButton.onClick.RemoveAllListeners();
                // Add click listener
                removeButton.onClick.AddListener(OnRemoveButtonClicked);
            }
        }
        else
        {
            Debug.LogWarning($"Remove button is null for student {_currentStudent?.studName}");
        }
    }
    
    // NEW: Handle remove button click
    private void OnRemoveButtonClicked()
    {
        if (_dashboardManager != null && _currentStudent != null)
        {
            _dashboardManager.OnRemoveStudentClicked(_currentStudent);
        }
        else
        {
            Debug.LogWarning("Dashboard manager or student reference is missing!");
        }
    }
    
    // Keep existing method for backward compatibility
    public void SetupStudent(StudentModel student)
    {
        SetupStudent(student, false, null);
    }
    
    private void SetProgressData(Dictionary<string, object> progressData)
    {
        // Set overall score
        if (progressData.ContainsKey("overallScore") && ProgressStudOverallScore != null)
        {
            ProgressStudOverallScore.text = progressData["overallScore"]?.ToString() ?? "0";
        }

        // Set success rate
        if (progressData.ContainsKey("successRate"))
        {
            object rawValue = progressData["successRate"];

            float percentage = 0f;

            if (rawValue is double d) percentage = (float)d;
            else if (rawValue is float f) percentage = f;
            else if (rawValue is int i) percentage = i;
            else
            {
                // Assume string
                string str = rawValue.ToString().Replace("%", "").Trim();
                float.TryParse(str, out percentage);
            }

            // If value looks like 0-1, treat it as normalized and scale up
            if (percentage > 0f && percentage <= 1f)
                percentage *= 100f;

            // Update UI
            if (ProgressSuccessRate != null)
                ProgressSuccessRate.text = Mathf.RoundToInt(percentage) + "%";

            if (progressBar != null)
                progressBar.SetProgress(percentage);
        }

        // Set current story data if available
        if (progressData.ContainsKey("currentStory") && 
            progressData["currentStory"] is Dictionary<string, object> currentStory)
        {
            // Set story name
            if (currentStory.ContainsKey("storyName") && ProgressStudStory != null)
            {
                ProgressStudStory.text = currentStory["storyName"]?.ToString() ?? "No Story";
            }

            // Set chapter name from nested chapter data
            if (currentStory.ContainsKey("chapter") && 
                currentStory["chapter"] is Dictionary<string, object> chapterData)
            {
                if (chapterData.ContainsKey("chaptName") && ProgressStudChapter != null)
                {
                    ProgressStudChapter.text = chapterData["chaptName"]?.ToString() ?? "No Chapter";
                }
            }
            else if (ProgressStudChapter != null)
            {
                ProgressStudChapter.text = "No Chapter";
            }
        }
        else
        {
            SetDefaultStoryData();
        }
    }
    
    private void SetDefaultProgressData()
    {
        if (ProgressStudOverallScore != null)
            ProgressStudOverallScore.text = "0";
        
        if (ProgressSuccessRate != null)
            ProgressSuccessRate.text = "0%";
        
        SetDefaultStoryData();
    }

    private void SetDefaultStoryData()
    {
        if (ProgressStudStory != null)
            ProgressStudStory.text = "No Story";
        
        if (ProgressStudChapter != null)
            ProgressStudChapter.text = "No Chapter";
    }
    
    private float CalculateProgressPercentage(System.Collections.Generic.Dictionary<string, object> progress)
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