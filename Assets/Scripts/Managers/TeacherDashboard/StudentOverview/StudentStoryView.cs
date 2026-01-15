using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class StudentStoryView : MonoBehaviour
{
    [Header("Story Info")]
    public TextMeshProUGUI storyNameText;
    
    [Header("Quiz Attempts")]
    public GameObject quizAttemptsSection;
    public GameObject quizAttemptPrefab;
    public Transform quizAttemptsContent;
    public GameObject noAttemptsMessage;
    
    private StoryProgress _story;

    public void SetupStory(StoryProgress story)
    {
        _story = story;
        
        Debug.Log($"🎯 StudentStoryView.SetupStory called for: {story.storyTitle} ({story.storyName})");
        
        if (storyNameText != null)
        {
            // Use the format: "storyTitle (storyName)"
            string displayName = $"{story.storyTitle} ({story.storyName})";
            storyNameText.text = displayName;
            Debug.Log($"📖 Story display set to: {displayName}");
        }
        else
        {
            Debug.LogError("❌ storyNameText is NULL in StudentStoryView!");
        }
        
        UpdateQuizAttemptsDisplay();
    }

    private void UpdateQuizAttemptsDisplay()
    {
        if (quizAttemptsSection != null)
        {
            // Always show the quiz attempts section (no toggle)
            quizAttemptsSection.SetActive(true);
            Debug.Log($"📊 Quiz attempts section active: true");
        }
        else
        {
            Debug.LogError("❌ quizAttemptsSection is NULL in StudentStoryView!");
        }
            
        if (_story.quizAttempts.Count > 0)
        {
            CreateQuizAttemptItems();
            if (noAttemptsMessage != null)
                noAttemptsMessage.SetActive(false);
            Debug.Log($"✅ Showing {_story.quizAttempts.Count} quiz attempts");
        }
        else
        {
            // Show empty state message
            if (noAttemptsMessage != null)
            {
                noAttemptsMessage.SetActive(true);
                Debug.Log("ℹ️ No quiz attempts - showing empty state");
            }
            ClearQuizAttemptItems();
        }
    }

    private void CreateQuizAttemptItems()
    {
        if (quizAttemptsContent == null || quizAttemptPrefab == null)
        {
            Debug.LogError("❌ quizAttemptsContent or quizAttemptPrefab is NULL!");
            return;
        }

        ClearQuizAttemptItems();

        Debug.Log($"🎯 Creating {_story.quizAttempts.Count} quiz attempt items");

        // Create new attempt items
        foreach (var attempt in _story.quizAttempts.OrderBy(a => a.attemptNumber))
        {
            GameObject attemptItem = Instantiate(quizAttemptPrefab, quizAttemptsContent);
            var attemptView = attemptItem.GetComponent<StudentQuizAttemptView>();
            
            if (attemptView != null)
            {
                attemptView.SetupAttempt(attempt);
                Debug.Log($"✅ Created attempt {attempt.attemptNumber} with score {attempt.score}");
            }
            else
            {
                Debug.LogError($"❌ StudentQuizAttemptView component missing on prefab for attempt {attempt.attemptNumber}");
            }
        }
    }

    private void ClearQuizAttemptItems()
    {
        if (quizAttemptsContent != null)
        {
            foreach (Transform child in quizAttemptsContent)
            {
                if (child.gameObject != noAttemptsMessage) // Don't destroy the empty state message
                    Destroy(child.gameObject);
            }
            Debug.Log("🧹 Cleared quiz attempt items");
        }
    }
}