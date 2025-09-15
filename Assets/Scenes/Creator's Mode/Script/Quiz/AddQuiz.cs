using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Question
{
    public string questionText;
    public string[] choices;
    public int correctAnswerIndex;

    public Question(string q, string[] c, int correct)
    {
        questionText = q;
        choices = c;
        correctAnswerIndex = correct;
    }
}

public class AddQuiz : MonoBehaviour
{
    [Header("TMP Input Fields")]
    public TMP_InputField questionInput;
    public TMP_InputField choice1Input;
    public TMP_InputField choice2Input;
    public TMP_InputField choice3Input;
    public TMP_InputField choice4Input;

    [Header("Toggles for Correct Answer")]
    public Toggle toggle1;
    public Toggle toggle2;
    public Toggle toggle3;
    public Toggle toggle4;

    [Header("Buttons")]
    public Button addQuestionButton;
    public Button nextButton;

    // Reference to current story’s quiz list
    public static List<Question> quizQuestions;

    void Start()
    {
        addQuestionButton.onClick.AddListener(AddQuestion);
        nextButton.onClick.AddListener(GoNext);

        // Ensure only one toggle can be active at once
        toggle1.onValueChanged.AddListener((isOn) => { if (isOn) UncheckOthers(0); });
        toggle2.onValueChanged.AddListener((isOn) => { if (isOn) UncheckOthers(1); });
        toggle3.onValueChanged.AddListener((isOn) => { if (isOn) UncheckOthers(2); });
        toggle4.onValueChanged.AddListener((isOn) => { if (isOn) UncheckOthers(3); });

        var story = StoryManager.Instance.currentStory;
        if (story == null)
        {
            // Auto-create a story if none exists
            story = StoryManager.Instance.CreateNewStory("Default Story");
            Debug.LogWarning("⚠️ No current story found. Created a new one: " + story.storyTitle);
        }

        // Make sure the story has its own quiz list
        if (story.quizQuestions == null)
            story.quizQuestions = new List<Question>();

        quizQuestions = story.quizQuestions;

        Debug.Log($"📖 Ready to add quiz for story '{story.storyTitle}'. Current questions: {quizQuestions.Count}");
    }

    void AddQuestion()
    {
        var story = StoryManager.Instance.currentStory;
        if (story == null)
        {
            Debug.LogError("❌ No active story. Cannot add question.");
            return;
        }

        string qText = questionInput.text.Trim();
        if (string.IsNullOrEmpty(qText))
        {
            Debug.LogWarning("⚠ Question cannot be empty!");
            return;
        }

        string[] choices = {
            choice1Input.text.Trim(),
            choice2Input.text.Trim(),
            choice3Input.text.Trim(),
            choice4Input.text.Trim()
        };

        int correctIndex = GetCorrectAnswerIndex();
        if (correctIndex == -1)
        {
            Debug.LogWarning("⚠ Please select a correct answer!");
            return;
        }

        Question newQ = new Question(qText, choices, correctIndex);
        quizQuestions.Add(newQ);

        Debug.Log($"✅ Added Question to '{story.storyTitle}': {qText} (Correct: {choices[correctIndex]})");

        // Save to JSON immediately
        StoryManager.Instance.SaveStories();

        // Reset inputs
        ResetInputs();
    }

    int GetCorrectAnswerIndex()
    {
        if (toggle1.isOn) return 0;
        if (toggle2.isOn) return 1;
        if (toggle3.isOn) return 2;
        if (toggle4.isOn) return 3;
        return -1;
    }

    void UncheckOthers(int keepIndex)
    {
        toggle1.isOn = keepIndex == 0;
        toggle2.isOn = keepIndex == 1;
        toggle3.isOn = keepIndex == 2;
        toggle4.isOn = keepIndex == 3;
    }

    void ResetInputs()
    {
        questionInput.text = "";
        choice1Input.text = "";
        choice2Input.text = "";
        choice3Input.text = "";
        choice4Input.text = "";
        toggle1.isOn = false;
        toggle2.isOn = false;
        toggle3.isOn = false;
        toggle4.isOn = false;
    }

    void GoNext()
    {
        if (quizQuestions == null)
        {
            Debug.LogWarning("⚠ Cannot go next, no quiz loaded.");
            return;
        }

        Debug.Log($"➡ Moving to review scene. Total Questions: {quizQuestions.Count}");
        SceneManager.LoadScene("ReviewQuestionsScene");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Creator'sModeScene");
    }

    public void Back()
    {
        SceneManager.LoadScene("CreateNewAddFrameScene");
    }
}
