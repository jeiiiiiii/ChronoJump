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

    // Reference to current story's quiz list
    public static List<Question> quizQuestions;

    void Start()
    {
        // Add character limit validation
        if (questionInput != null)
        {
            questionInput.characterLimit = ValidationManager.Instance.maxQuestionLength;
        }

        if (choice1Input != null) choice1Input.characterLimit = ValidationManager.Instance.maxChoiceLength;
        if (choice2Input != null) choice2Input.characterLimit = ValidationManager.Instance.maxChoiceLength;
        if (choice3Input != null) choice3Input.characterLimit = ValidationManager.Instance.maxChoiceLength;
        if (choice4Input != null) choice4Input.characterLimit = ValidationManager.Instance.maxChoiceLength;
        
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
            Debug.LogWarning("No current story found. Created a new one: " + story.storyTitle);
        }

        // Make sure the story has its own quiz list
        if (story.quizQuestions == null)
            story.quizQuestions = new List<Question>();

        quizQuestions = story.quizQuestions;

        Debug.Log($"Ready to add quiz for story '{story.storyTitle}'. Current questions: {quizQuestions.Count}");
    }

    void AddQuestion()
    {
        var story = StoryManager.Instance.currentStory;
        if (story == null)
        {
            Debug.LogError("No active story. Cannot add question.");
            return;
        }

        string qText = questionInput.text.Trim();
        
        // Validate question text
        var questionValidation = ValidationManager.Instance.ValidateQuestion(qText);
        if (!questionValidation.isValid)
        {
            ValidationManager.Instance.ShowWarning(
                "Question Required",
                questionValidation.message,
                null,
                () => { questionInput.Select(); }
            );
            return;
        }

        string[] choices = {
            choice1Input.text.Trim(),
            choice2Input.text.Trim(),
            choice3Input.text.Trim(),
            choice4Input.text.Trim()
        };

        // Validate choices
        var choicesValidation = ValidationManager.Instance.ValidateChoices(choices);
        if (!choicesValidation.isValid)
        {
            ValidationManager.Instance.ShowWarning(
                "Choices Validation",
                choicesValidation.message,
                null,
                () => { 
                    if (string.IsNullOrEmpty(choices[0])) choice1Input.Select();
                    else if (string.IsNullOrEmpty(choices[1])) choice2Input.Select();
                    else if (string.IsNullOrEmpty(choices[2])) choice3Input.Select();
                    else if (string.IsNullOrEmpty(choices[3])) choice4Input.Select();
                }
            );
            return;
        }

        int correctIndex = GetCorrectAnswerIndex();
        if (correctIndex == -1)
        {
            ValidationManager.Instance.ShowWarning(
                "Correct Answer Required",
                "Please select which choice is the correct answer by checking one of the boxes!",
                null,
                null
            );
            return;
        }

        Question newQ = new Question(qText, choices, correctIndex);
        quizQuestions.Add(newQ);

        Debug.Log($"Added Question to '{story.storyTitle}': {qText} (Correct: {choices[correctIndex]})");

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
        Debug.Log("GoNext() called");
        
        if (quizQuestions == null)
        {
            Debug.LogWarning("Cannot go next, no quiz loaded.");
            return;
        }

        Debug.Log($"Current question count: {quizQuestions.Count}");

        // Check if at least one question has been added
        if (quizQuestions.Count == 0)
        {
            Debug.Log("VALIDATION FAILED: No questions added");
            ValidationManager.Instance.ShowWarning(
                "No Questions Added",
                "You must add at least one question before proceeding to the review!",
                null,
                () => { questionInput.Select(); }
            );
            Debug.Log("Returning from GoNext - should NOT load scene");
            return; // STOP HERE - don't proceed
        }

        // Check if there are unsaved inputs
        if (HasUnsavedQuestion())
        {
            Debug.Log("VALIDATION FAILED: Unsaved question detected");
            ValidationManager.Instance.ShowWarning(
                "Unsaved Question",
                "You have unsaved input. Please add the question first or clear the fields.",
                null,
                () => { questionInput.Select(); }
            );
            Debug.Log("Returning from GoNext - should NOT load scene");
            return; // STOP HERE - don't proceed
        }

        // All validation passed - proceed to next scene
        Debug.Log($"VALIDATION PASSED: Moving to review scene. Total Questions: {quizQuestions.Count}");
        SceneManager.LoadScene("ReviewQuestionsScene");
    }

    // Helper method to check if there's unsaved input
    private bool HasUnsavedQuestion()
    {
        bool hasQuestionText = !string.IsNullOrEmpty(questionInput.text.Trim());
        bool hasAnyChoice = !string.IsNullOrEmpty(choice1Input.text.Trim()) ||
                           !string.IsNullOrEmpty(choice2Input.text.Trim()) ||
                           !string.IsNullOrEmpty(choice3Input.text.Trim()) ||
                           !string.IsNullOrEmpty(choice4Input.text.Trim());
        bool hasSelectedAnswer = toggle1.isOn || toggle2.isOn || toggle3.isOn || toggle4.isOn;

        return hasQuestionText || hasAnyChoice || hasSelectedAnswer;
    }

    // Optional: Add a button to clear unsaved inputs
    public void ClearInputs()
    {
        ResetInputs();
        Debug.Log("Inputs cleared");
    }

    public void MainMenu()
    {
        // Check if there are unsaved questions
        if (HasUnsavedQuestion())
        {
            ValidationManager.Instance.ShowWarning(
                "Unsaved Changes",
                "You have unsaved input. Please add the question first or clear the fields.",
                null,
                null
            );
            return; // STOP HERE - don't navigate
        }

        SceneManager.LoadScene("Creator'sModeScene");
    }

    public void Back()
    {
        // Check if there are unsaved questions
        if (HasUnsavedQuestion())
        {
            ValidationManager.Instance.ShowWarning(
                "Unsaved Changes",
                "You have unsaved input. Please add the question first or clear the fields.",
                null,
                null
            );
            return; // STOP HERE - don't navigate
        }

        SceneManager.LoadScene("CreateNewAddFrameScene");
    }
}