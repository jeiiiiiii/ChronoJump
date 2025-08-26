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

    public static List<Question> quizQuestions = new List<Question>();

    void Start()
    {
        addQuestionButton.onClick.AddListener(AddQuestion);
        nextButton.onClick.AddListener(GoNext);

        // Ensure only one toggle can be active at once (manual radio button logic)
        toggle1.onValueChanged.AddListener((isOn) => { if (isOn) UncheckOthers(0); });
        toggle2.onValueChanged.AddListener((isOn) => { if (isOn) UncheckOthers(1); });
        toggle3.onValueChanged.AddListener((isOn) => { if (isOn) UncheckOthers(2); });
        toggle4.onValueChanged.AddListener((isOn) => { if (isOn) UncheckOthers(3); });
    }

    void AddQuestion()
    {
        string qText = questionInput.text;
        string[] choices = new string[4];
        choices[0] = choice1Input.text;
        choices[1] = choice2Input.text;
        choices[2] = choice3Input.text;
        choices[3] = choice4Input.text;

        if (string.IsNullOrEmpty(qText))
        {
            Debug.LogWarning("⚠ Question cannot be empty!");
            return;
        }

        int correctIndex = GetCorrectAnswerIndex();
        if (correctIndex == -1)
        {
            Debug.LogWarning("⚠ Please select a correct answer!");
            return;
        }

        Question newQ = new Question(qText, choices, correctIndex);
        quizQuestions.Add(newQ);

        Debug.Log($"✅ Added Question: {qText} (Correct: {choices[correctIndex]})");

        // Reset inputs
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
        if (keepIndex != 0) toggle1.isOn = false;
        if (keepIndex != 1) toggle2.isOn = false;
        if (keepIndex != 2) toggle3.isOn = false;
        if (keepIndex != 3) toggle4.isOn = false;
    }

    void GoNext()
    {
        Debug.Log("➡ Moving to review scene. Total Questions: " + quizQuestions.Count);
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
