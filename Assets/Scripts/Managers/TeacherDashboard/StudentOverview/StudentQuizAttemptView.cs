using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
            dateCompletedText.text = attempt.dateCompleted.ToString("MMM dd, yyyy 'at' hh:mm tt");

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
