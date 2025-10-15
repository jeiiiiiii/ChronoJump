using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class StudentQuizAttemptView : MonoBehaviour
{
    [Header("Attempt Info")]
    public TextMeshProUGUI attemptNumberText;
    public TextMeshProUGUI dateCompletedText;
    public TextMeshProUGUI remarksText;
    public TextMeshProUGUI scoreText;

    public void SetupAttempt(QuizAttempt attempt)
    {
        if (attemptNumberText != null)
            attemptNumberText.text = $"{attempt.attemptNumber}";

        if (dateCompletedText != null)
        {
            // Use the local time instead of UTC
            DateTime localDate = attempt.dateCompleted;
            
            // Ensure we're working with local time
            if (attempt.dateCompleted.Kind == DateTimeKind.Utc)
            {
                localDate = attempt.dateCompleted.ToLocalTime();
            }
            
            dateCompletedText.text = localDate.ToString("MMM dd, yyyy 'at' hh:mm tt");
            
            // Debug log to verify the conversion
            Debug.Log($"📅 Date conversion - UTC: {attempt.dateCompleted:MMM dd, yyyy 'at' hh:mm tt} -> Local: {localDate:MMM dd, yyyy 'at' hh:mm tt}");
        }

        if (remarksText != null)
        {
            remarksText.text = attempt.remarks;
            // Set color based on pass/fail
            if (ColorUtility.TryParseHtmlString(attempt.remarksColor, out Color color))
            {
                remarksText.color = color;
            }
        }

        if (scoreText != null)
            scoreText.text = $"{attempt.score}";
    }
}