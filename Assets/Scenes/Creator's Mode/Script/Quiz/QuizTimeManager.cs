using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.Threading.Tasks;
using System.Linq;

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

    // ✅ NEW: Flag to prevent multiple saves
    private bool isSavingQuizAttempt = false;

    // Quiz attempt data
    private string currentStoryId;
    private string studentStudId; // Changed from studentId to studentStudId

    void Start()
    {
        // Get current story and student info
        LoadQuizContext();

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

    private void LoadQuizContext()
    {
        // Get story ID from StudentPrefs
        currentStoryId = StudentPrefs.GetString("SelectedStoryID", "");

        // Get student studId (not userId)
        studentStudId = GetStudentStudId();

        Debug.Log($"📝 Quiz Context - Story: {currentStoryId}, Student StudId: {studentStudId}");
    }

    private string GetStudentStudId()
    {
        // ✅ FIRST: Try to get from GameProgressManager (most reliable)
        if (GameProgressManager.Instance != null)
        {
            string studIdFromManager = GameProgressManager.Instance.GetCurrentStudentId();
            if (!string.IsNullOrEmpty(studIdFromManager))
            {
                Debug.Log($"✅ Using CurrentStudentId from GameProgressManager: {studIdFromManager}");
                return studIdFromManager;
            }
        }

        // ✅ SECOND: Try StudentPrefs directly
        string studIdFromPrefs = StudentPrefs.GetString("CurrentStudentId", "");
        if (!string.IsNullOrEmpty(studIdFromPrefs))
        {
            Debug.Log($"✅ Using CurrentStudentId from StudentPrefs: {studIdFromPrefs}");
            return studIdFromPrefs;
        }

        // Last resort fallbacks
        string fallbackStudId = StudentPrefs.GetString("StudentId", "");
        if (!string.IsNullOrEmpty(fallbackStudId))
        {
            Debug.LogWarning($"⚠️ Using fallback StudentId: {fallbackStudId}");
            return fallbackStudId;
        }

        if (FirebaseManager.Instance?.CurrentUser != null)
        {
            Debug.LogWarning($"⚠️ CRITICAL: Using Firebase UserId as final fallback - quiz attempts may not be linked to correct student!");
            return FirebaseManager.Instance.CurrentUser.UserId;
        }

        return "unknown_student";
    }


    void Update()
    {
        if (!isQuizActive) return;

        // Countdown
        currentTime -= Time.deltaTime;
        timerText.text = "Time: " + Mathf.Ceil(currentTime).ToString();

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
            Button btn = answerButtons[i];
            int choiceIndex = i;

            TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = currentQuestion.choices[choiceIndex];

            btn.interactable = true;
            btn.image.color = Color.white; // Reset color

            // Clear and re-add listener
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnAnswerSelected(choiceIndex, btn));
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
            int correctIndex = questions[currentQuestionIndex].correctAnswerIndex;

            if (selectedAnswerIndex == correctIndex)
            {
                score++;
            }
            else
            {
                wrongAnswers.Add(currentQuestionIndex);
            }

            // Disable all buttons after confirming
            foreach (Button btn in answerButtons)
            {
                btn.interactable = false;
            }
        }

        // Next button now moves to next question
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(NextQuestion);
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
        if (currentTime < 3f)
            timerText.color = Color.red;
        else
            timerText.color = Color.white;

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

    async void ShowQuizResult()
    {
        isQuizActive = false;

        foreach (Button btn in answerButtons)
        {
            btn.gameObject.SetActive(false);
        }

        questionText.gameObject.SetActive(false);

        string result = "Tapos na ang Quiz!\n";
        result += "Score: " + score + " / " + questions.Length + "\n\n";

        if (wrongAnswers.Count > 0)
        {
            result += "Maling Sagot:\n";
            foreach (int idx in wrongAnswers)
            {
                var q = questions[idx];
                int correctIdx = q.correctAnswerIndex;
                string correctChoice = q.choices[correctIdx];
                result += $"{idx + 1}. Ang sagot mo: {q.questionText} - ";
                result += $" Tamang sagot: {correctChoice}\n";
            }
        }
        else
        {
            result += "🎉 Perfect ka! Walang mali!";
        }

        resultText.text = result;
        timerText.gameObject.SetActive(false);

        // ✅ Disable next button before saving
        nextButton.interactable = false;

        // Save quiz attempt to Firebase (will be skipped for teachers)
        await SaveQuizAttempt();

        // ✅ Re-enable next button after saving completes
        nextButton.onClick.RemoveAllListeners();
        nextButton.interactable = true;
        nextButton.onClick.AddListener(GoToNextScene);
    }

    // ✅ NEW: Separate method for scene transition with protection
    private void GoToNextScene()
    {
        // ✅ Prevent multiple clicks
        if (isSavingQuizAttempt)
        {
            Debug.Log("⚠️ Still saving quiz attempt, please wait...");
            return;
        }

        // ✅ Disable button immediately
        nextButton.interactable = false;

        // Check if user is a teacher or student and load appropriate scene
        if (IsTeacher())
        {
            SceneManager.LoadScene("Creator'sModeScene");
        }
        else
        {
            SceneManager.LoadScene("Classroom");
        }
    }

    private async Task SaveQuizAttempt()
    {
        // ✅ Check if already saving
        if (isSavingQuizAttempt)
        {
            Debug.Log("⚠️ SaveQuizAttempt already in progress, skipping...");
            return;
        }

        // ✅ Skip saving if teacher is logged in
        if (IsTeacher())
        {
            Debug.Log("👨‍🏫 Teacher mode - Skipping quiz attempt save to Firebase");
            return;
        }

        // ✅ Additional check - make sure we have a valid studId
        if (string.IsNullOrEmpty(studentStudId) || studentStudId == "unknown_student")
        {
            Debug.LogError("❌ Cannot save quiz attempt - invalid studId");
            return;
        }

        try
        {
            // ✅ Set flag to prevent multiple saves
            isSavingQuizAttempt = true;

            // ✅ Update result text to show saving status
            resultText.text += "\n\n💾 Saving quiz result...";

            if (FirebaseManager.Instance?.DB == null)
            {
                Debug.LogError("Firebase not available to save quiz attempt");
                resultText.text += "\n❌ Failed to save (no connection)";
                return;
            }

            var firestore = FirebaseManager.Instance.DB;

            // Generate a unique quiz ID
            string quizId = $"{currentStoryId}_{DateTime.Now:yyyyMMdd_HHmmss}";

            // Determine if passed (70% to pass)
            bool isPassed = score >= (questions.Length * 0.7);

            // Get attempt number for this student and story
            int attemptNumber = await GetNextAttemptNumber();

            // Create quiz attempt using your QuizAttemptModel
            var quizAttempt = new QuizAttemptModel(
                quizId: quizId,
                attemptNum: attemptNumber,
                score: score,
                isPassed: isPassed
            );

            // Save to Firestore with nested structure:
            // createdStories/{storyId}/quizAttempts/{studentStudId}/attempts/{attemptId}
            var attemptRef = firestore
                .Collection("createdStories")
                .Document(currentStoryId)
                .Collection("quizAttempts")
                .Document(studentStudId) // ✅ Now using studentStudId instead of userId
                .Collection("attempts")
                .Document(); // Auto-generated ID

            await attemptRef.SetAsync(quizAttempt);

            Debug.Log($"✅ Quiz attempt saved: StudentStudId: {studentStudId}, Attempt #{attemptNumber}, Score: {score}/{questions.Length}, Passed: {isPassed}");

            // ✅ Update result text to show success
            resultText.text = resultText.text.Replace("💾 Saving quiz result...", "✅ Quiz result saved!");

        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Failed to save quiz attempt: {ex.Message}");

            // ✅ Update result text to show error
            resultText.text = resultText.text.Replace("💾 Saving quiz result...", "❌ Failed to save quiz result");
        }
        finally
        {
            // ✅ Always reset the flag when done
            isSavingQuizAttempt = false;
        }
    }

    // Helper method to determine if the current user is a teacher
    private bool IsTeacher()
    {
        // Check the user role saved during login
        string userRole = PlayerPrefs.GetString("UserRole", "student");
        return userRole.ToLower() == "teacher";
    }

    private async Task<int> GetNextAttemptNumber()
    {
        // ✅ Skip if teacher
        if (IsTeacher())
        {
            return 0; // Return 0 or any placeholder for teacher
        }

        // ✅ Additional check - make sure we have a valid studId
        if (string.IsNullOrEmpty(studentStudId) || studentStudId == "unknown_student")
        {
            Debug.LogError("❌ Cannot get next attempt number - invalid studId");
            return 1; // Return 1 as fallback
        }

        try
        {
            var firestore = FirebaseManager.Instance.DB;

            var attemptsRef = firestore
                .Collection("createdStories")
                .Document(currentStoryId)
                .Collection("quizAttempts")
                .Document(studentStudId) // ✅ Now using studentStudId instead of userId
                .Collection("attempts");

            var snapshot = await attemptsRef
                .OrderByDescending("attemptNumber")
                .Limit(1)
                .GetSnapshotAsync();

            if (snapshot.Documents.Count() > 0)
            {
                var lastAttempt = snapshot.Documents.First().ConvertTo<QuizAttemptModel>();
                return lastAttempt.attemptNumber + 1;
            }

            return 1; // First attempt
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Failed to get next attempt number: {ex.Message}");
            return 1;
        }
    }
}
