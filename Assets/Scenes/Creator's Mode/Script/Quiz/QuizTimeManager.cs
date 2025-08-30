using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class QuizTimeManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI questionText;
    public Button[] answerButtons; // Always 4
    public Button nextButton;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI timerText;

    [Header("Settings")]
    public float timePerQuestion = 10f;

    // Internal state
    private Question[] questions;
    private int currentQuestionIndex = 0;
    private int selectedAnswerIndex = -1;
    private int score = 0;
    private List<int> wrongAnswers = new List<int>();
    private float currentTime;
    private bool isQuizActive = true;

    void Start()
    {
        // Pull teacher-created questions
        if (AddQuiz.quizQuestions.Count == 0)
        {
            Debug.LogError("No questions found in AddQuiz.quizQuestions!");
            resultText.text = "Walang available na tanong.";
            isQuizActive = false;
            return;
        }

        // Copy into local array
        questions = AddQuiz.quizQuestions.ToArray();

        score = 0;
        currentQuestionIndex = 0;
        ShowQuestion();
    }

    void Update()
    {
        if (!isQuizActive) return;

        // Countdown
        currentTime -= Time.deltaTime;
        timerText.text = "Oras: " + Mathf.Ceil(currentTime).ToString();

        // Warning color when time low
        if (currentTime <= 3f)
            timerText.color = Color.red;
        else
            timerText.color = Color.white;

        // Time out
        if (currentTime <= 0f)
        {
            TimeUp();
        }
    }

    void ShowQuestion()
    {
        if (currentQuestionIndex >= questions.Length)
        {
            ShowQuizResult();
            return;
        }

        Question currentQuestion = questions[currentQuestionIndex];
        questionText.text = currentQuestion.questionText;

        // Setup answer buttons
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int choiceIndex = i;
            TextMeshProUGUI btnText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = currentQuestion.choices[choiceIndex];

            answerButtons[i].interactable = true;

            // Clear and re-add listener
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(choiceIndex, answerButtons[choiceIndex]));
        }

        // Reset next button
        nextButton.onClick.RemoveAllListeners();
        nextButton.interactable = false;

        // Reset timer
        currentTime = timePerQuestion;
    }

    void OnAnswerSelected(int answerIndex, Button clickedButton)
    {
        selectedAnswerIndex = answerIndex;
        nextButton.interactable = true;

        // Clear previous listeners and set confirm action
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(ConfirmAnswer);
    }

    void ConfirmAnswer()
    {
        if (selectedAnswerIndex != -1)
        {
            if (selectedAnswerIndex == questions[currentQuestionIndex].correctAnswerIndex)
            {
                score++;
            }
            else
            {
                wrongAnswers.Add(currentQuestionIndex);
            }
        }

        NextQuestion();
    }

    void NextQuestion()
    {
        currentQuestionIndex++;
        selectedAnswerIndex = -1;

        if (currentQuestionIndex < questions.Length)
        {
            ShowQuestion();
        }
        else
        {
            ShowQuizResult();
        }
    }

    void TimeUp()
    {
        foreach (Button btn in answerButtons)
        {
            btn.interactable = false;
        }

        if (selectedAnswerIndex == -1)
        {
            wrongAnswers.Add(currentQuestionIndex);
        }

        nextButton.interactable = true;
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(NextQuestion);

        questionText.text = "Ubos na ang oras mo!";
    }

    void ShowQuizResult()
    {
        isQuizActive = false;

        string result = "Tapos na ang Quiz!\n";
        result += "Score: " + score + " / " + questions.Length + "\n\n";

        if (wrongAnswers.Count > 0)
        {
            result += "Maling Sagot:\n";
            foreach (int idx in wrongAnswers)
            {
                result += "- " + questions[idx].questionText + "\n";
                int correctIdx = questions[idx].correctAnswerIndex;
                string correctChoice = questions[idx].choices[correctIdx];
                result += "   ✔ Tamang sagot: " + correctChoice + "\n";
            }
        }
        else
        {
            result += "🎉 Perfect ka! Walang mali!";
        }

        resultText.text = result;

        nextButton.onClick.RemoveAllListeners();
        nextButton.interactable = true;
        nextButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("ReviewScene");
        });
    }
}
