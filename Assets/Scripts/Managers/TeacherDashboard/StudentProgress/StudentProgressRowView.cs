using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StudentProgressRowView : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI studentNameText;
    public Image studentProfileImage;
    public DynamicProgressBar progressBar;
    
    [Header("Additional Progress UI")]
    public TextMeshProUGUI ProgressStudChapter;
    public TextMeshProUGUI ProgressStudStory;
    public TextMeshProUGUI ProgressSuccessRate;
    public TextMeshProUGUI ProgressStudOverallScore;
    
    public void SetupStudent(StudentModel student)
    {
        // Set basic student info
        if (studentNameText != null)
            studentNameText.text = student.studName;
        
        // Set profile image if available
        // Keep existing profile image logic here
        
        // Use the progress data from the student model
        if (student.studentProgress != null)
        {
            SetProgressData(student.studentProgress);
        }
        else
        {
            SetDefaultProgressData();
        }
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