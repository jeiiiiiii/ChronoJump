using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using static SumerianFifthRecallChallenges;

public class QuizTimeManagerNile : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueLine
    {
        public string question;
    }

    [System.Serializable]
    public struct Question
    {
        public DialogueLine questionLine;
        public Answer[] answers;
    }

    public Question[] quizQuestions;
    private int currentQuestionIndex = 0;

    public TMP_Text dialogueText;
    public Button[] answerButtons;
    public Button nextButton;
    private bool hasAnswered = false;

    public float timePerQuestion = 10f;
    private float currentTime;
    public TMP_Text timerText;
    private bool timerRunning = false;
    public AudioSource finishAudioSource;

    private List<int> wrongAnswers = new List<int>();

    public TMP_Text resultTextUI;
    public TMP_Text ScoreTextUI;

    private Button selectedButton = null;
    private Answer selectedAnswer;




    void Start()
    {
        GameState.ResetScore();
        quizQuestions = new Question[]
        {
            new Question {
                questionLine = new DialogueLine {
                    //Q1
                    question = "Bakit tinawag ni Herodotus na \"Gift of the Nile\" ang Egypt?"
                },
                answers = new Answer[] {
                    new Answer { text = "ahil sa ginto na matatagpuan sa ilog", isCorrect = false },
                    new Answer { text = "Dahil sa annual flooding na nagdadala ng matabang lupa para sa pagtatanim", isCorrect = true },
                    new Answer { text = "Dahil sa transportation na dulot ng ilog", isCorrect = false },
                    new Answer { text = "Dahil sa mga diyos na naninirahan sa ilog", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q2
                    question = "Ano ang sinisimbolo ng Nile River ayon kay Imhotep?"
                },
                answers = new Answer[] {
                    new Answer { text = "Buhay, diyos, at Egypt mism", isCorrect = true },
                    new Answer { text = "Simpleng water source lang", isCorrect = false },
                    new Answer { text = "Kalaban ng mga Egyptian", isCorrect = false },
                    new Answer { text = "Highway lang para sa trade", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q3
                    question = "Ano ang pananaw ni Imhotep tungkol sa \"pagbaha ng Nile\"?"
                },
                answers = new Answer[] {
                    new Answer { text = "Nakakasira tulad ng Yellow River", isCorrect = false },
                    new Answer { text = "Random at walang pattern", isCorrect = false },
                    new Answer { text = "Hindi importante", isCorrect = false },
                    new Answer { text = "Predictable at maaaring paghanda kaya blessing ito", isCorrect = true }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q4
                    question = "Sino ang kinikilalang babae na farmer na nagtatanim ng wheat at barley sa lambak ng Nile?"
                },
                answers = new Answer[] {
                    new Answer { text = "Fu Hao", isCorrect = false },
                    new Answer { text = "Merit", isCorrect = true },
                    new Answer { text = "Daro", isCorrect = false },
                    new Answer { text = "Matrika", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q5
                    question = "Ano ang dahilan kung bakit naging stable ang Egyptian civilization?"
                },
                answers = new Answer[] {
                    new Answer { text = "Malakas na militar", isCorrect = false },
                    new Answer { text = "Mayaman sa ginto", isCorrect = false },
                    new Answer { text = "Surplus production na nagpahintulot ng specialized jobs", isCorrect = true },
                    new Answer { text = "Malaking populasyon", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q6
                    question = "Ano ang natural barrier na nagprotekta sa Egypt mula sa mga invaders?"
                },
                answers = new Answer[] {
                    new Answer { text = "Mediterranean Sea at Red Sea", isCorrect = false },
                    new Answer { text = "Mataas na bundok", isCorrect = false },
                    new Answer { text = "Malawak na disyerto sa magkabilang panig ng Nile", isCorrect = true },
                    new Answer { text = "Building material", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q7
                    question = "Ano ang simbolismo ng papyrus ayon kay Imhotep?"
                },
                answers = new Answer[] {
                    new Answer { text = "Foundation ng Egyptian writing at record keeping", isCorrect = true },
                    new Answer { text = "Decorative plant lang", isCorrect = false },
                    new Answer { text = "Pagkain ng mga hayop", isCorrect = false },
                    new Answer { text = "Walang kahalagahan", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q8
                    question = "Ano ang sinabi ni Chrono tungkol sa Nile bilang transportation?"
                },
                answers = new Answer[] {
                    new Answer { text = "indi ginagamit para sa travel", isCorrect = false },
                    new Answer { text = "Delikado at hindi safe", isCorrect = false },
                    new Answer { text = "Dumadaloy patungo sa hilaga madaling travel gamit ang current, at pabalik gamit ang sail", isCorrect = true },
                    new Answer { text = "Para sa military lang", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q9
                    question = "Ano ang pangunahing vulnerability ng Egyptian civilization?"
                },
                answers = new Answer[] {
                    new Answer { text = "Kakulangan ng military power", isCorrect = false },
                    new Answer { text = "Complete dependency sa Nile River para sa tubig at pagkain", isCorrect = true },
                    new Answer { text = "Kakulangan ng natural resources", isCorrect = false },
                    new Answer { text = "Masyadong malaking populasyon", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q10
                    question = "Ano ang pangunahing aral na naiwan ng Nile River sa Egyptian civilization?"
                },
                answers = new Answer[] {
                    new Answer { text = "Ang tubig ay walang halaga", isCorrect = false },
                    new Answer { text = "Ang militar ay mas importante", isCorrect = false },
                    new Answer { text = "Ang proteksyon ay walang kahalagahan", isCorrect = false },
                    new Answer { text = "Ang pinakasimpleng bagay (tubig) ay nagiging foundation ng dakilang sibilisasyon—pero dependency ay may price", isCorrect = true }
                }
            }
        };
        ShowQuestion();
        ShuffleQuestionsAndAnswers();
    }

    void ShowQuestion()
    {

        selectedButton = null;
        selectedAnswer = new Answer();

        timerText.color = Color.white;
        var current = quizQuestions[currentQuestionIndex];

        dialogueText.text = current.questionLine.question;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            TMP_Text buttonText = answerButtons[i].GetComponentInChildren<TMP_Text>();
            buttonText.text = current.answers[i].text;

            int index = i;
            answerButtons[i].onClick.RemoveAllListeners();
            Button btn = answerButtons[i];
            answerButtons[i].onClick.AddListener(() =>
            {
                OnAnswerSelected(current.answers[index], btn);
            });



            answerButtons[i].interactable = true;
            answerButtons[i].gameObject.SetActive(true);
        }

        currentTime = timePerQuestion;
        timerRunning = true;
        nextButton.gameObject.SetActive(false);
        hasAnswered = false;
    }


    void OnAnswerSelected(Answer answer, Button button)
    {

        selectedButton = button;
        selectedAnswer = answer;

        // Allow player to proceed
        nextButton.gameObject.SetActive(true);
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(ConfirmAnswer);
    }

    void ConfirmAnswer()
    {
        if (selectedAnswer.isCorrect)
        {
            GameState.score++;
        }
        else
        {
            wrongAnswers.Add(currentQuestionIndex);
        }

        timerRunning = false;
        NextQuestion();
    }

    void NextQuestion()
    {
        currentQuestionIndex++;

        if (currentQuestionIndex < quizQuestions.Length)
        {
            ShowQuestion();
        }
        else
        {
            ShowQuizResult();
        }
    }

        private void Shuffle<T>(T[] array)
    {
        System.Random rng = new System.Random();
        int n = array.Length;
        while (n > 1)
        {
            int k = rng.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }

    // 🔹 Call this after quizQuestions are created
    private void ShuffleQuestionsAndAnswers()
    {
        // Shuffle the questions
        Shuffle(quizQuestions);

        // Shuffle each question's answers
        foreach (var q in quizQuestions)
        {
            Shuffle(q.answers);
        }
    }

    void ShowQuizResult()
    {
        PlayerProgressManager.UnlockCivilization("Kingdom");
        
        timerText.color = Color.white;
        timerText.gameObject.SetActive(false);

        foreach (Button btn in answerButtons)
        {
            btn.gameObject.SetActive(false);
        }

        string resultText = "";
        
        dialogueText.gameObject.SetActive(false);

        if (finishAudioSource != null)
            finishAudioSource.Play();

        string ScoreText = $"Tapos na ang pagsusulit! \nScore: {GameState.score}/{quizQuestions.Length}\n\n";
        if (wrongAnswers.Count > 0)
        {
            resultText += "Ang mga numero kung saan ka mali\n";
            foreach (int idx in wrongAnswers)
            {
                var q = quizQuestions[idx];
                string correct = "";
                foreach (var a in q.answers)
                {
                    if (a.isCorrect)
                    {
                        correct = a.text;
                        break;
                    }
                }
                resultText += $"{idx + 1}. Tamang sagot: {correct}\n";
            }
        }
        else
        {
            resultText += "Perfect! Lahat ng sagot mo ay tama!";
        }

        resultTextUI.text = resultText;
        ScoreTextUI.text = ScoreText;

        GameState.UpdateProgressManager();

        // Saving to firebase
        bool passed = GameState.score >= 8; 
        GameProgressManager.Instance.RecordQuizAttempt(
            "CH001_ST002",
            GameState.score,
            quizQuestions.Length,
            passed
        );

        if (GameState.score <= 7)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("NileCoordinateSelect");
                UpdateSaveAfterQuizCompletion();
            });
        }
        else
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("AkkadianArtifactScene");
                PlayerAchievementManager.UnlockAchievement("Sword");
                UpdateSaveAfterQuizCompletion();
            });
        }
    }

    void Update()
    {
        if (timerRunning)
        {
            currentTime -= Time.deltaTime;
            timerText.text = "Time: " + Mathf.Ceil(currentTime).ToString();

            if (currentTime <= 0)
            {
                timerRunning = false;
                TimeUp();
            }
        }
    }
    void TimeUp()
    {
        if (currentTime < 3f)
            timerText.color = Color.red;
        else
            timerText.color = Color.white;


        dialogueText.text = "Ubos na ang oras mo!";
        hasAnswered = true;

        foreach (Button btn in answerButtons)
            btn.interactable = false;

        nextButton.gameObject.SetActive(true);
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(NextQuestion);
    }

    // Add this method to update saves after quiz completion
private void UpdateSaveAfterQuizCompletion()
{
    if (SaveLoadManager.Instance != null && GameProgressManager.Instance?.CurrentStudentState != null)
    {
        // Determine the next civilization's first scene based on current quiz
        string nextScene = GetNextCivilizationScene();
        
        // Update all slots that contain the current chapter's data
        for (int slot = 1; slot <= 4; slot++)
        {
            if (SaveLoadManager.Instance.HasSaveFile(slot))
            {
                var saveData = SaveLoadManager.Instance.GetSaveData(slot);
                if (saveData != null && IsCurrentChapterSave(saveData.currentScene))
                {
                    // Update this save to point to the next civilization's first scene
                    SaveLoadManager.Instance.UpdateSaveSlot(slot, nextScene, 0);
                    
                    Debug.Log($"Updated slot {slot} after quiz completion to: {nextScene}");
                }
            }
        }
    }
}

// Helper method to determine the next civilization's first scene
private string GetNextCivilizationScene()
{
    string currentScene = SceneManager.GetActiveScene().name;
    
    switch (currentScene)
    {
        case "SumerianQuizTime":
            return "AkkadianSceneOne"; // Progress to Akkadian
        
        case "AkkadianQuizTime":
            return "BabylonianSceneOne"; // Progress to Babylonian
        
        case "BabylonianQuizTime":
            return "AssyrianSceneOne"; // Progress to Assyrian
        
        //case "AssyrianQuizTime":
        //    return "FinalReview"; // Progress to next
        
        default:
            Debug.LogWarning($"Unknown quiz scene: {currentScene}, defaulting to TitleScreen");
            return "TitleScreen";
    }
}

    // Helper method to check if a save is from the current chapter
    private bool IsCurrentChapterSave(string sceneName)
    {
        string currentQuizScene = SceneManager.GetActiveScene().name;

        if (currentQuizScene.Contains("Sumerian"))
        {
            return sceneName.Contains("Sumerian");
        }
        else if (currentQuizScene.Contains("Akkadian"))
        {
            return sceneName.Contains("Akkadian");
        }
        else if (currentQuizScene.Contains("Babylonian"))
        {
            return sceneName.Contains("Babylonian");
        }
        else if (currentQuizScene.Contains("Assyrian"))
        {
            return sceneName.Contains("Assyrian");
        }
        else if (currentQuizScene.Contains("Harappa"))
        {
            return sceneName.Contains("Harappa");
        }
        else if (currentQuizScene.Contains("Sining"))
        {
            return sceneName.Contains("Sining");
        }
        else if (currentQuizScene.Contains("Nile"))
        {
            return sceneName.Contains("Nile");
        }
        else if (currentQuizScene.Contains("Shang"))
        {
            return sceneName.Contains("Shang");
        }

        return false;
    }
}
