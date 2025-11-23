using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using static SumerianFifthRecallChallenges;

public class QuizTimeManagerHuangHe : MonoBehaviour
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

    [System.Serializable]
    public struct WrongAnswerInfo
    {
        public string questionText;
        public string correctAnswerText;
        public int questionNumber;
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

    private List<WrongAnswerInfo> wrongAnswers = new List<WrongAnswerInfo>();

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
                    question = "Bakit tinawag na \"China's Sorrow\" ang Yellow River?"
                },
                answers = new Answer[] {
                    new Answer { text = "Dahil sa dilim ng tubig nito", isCorrect = false },
                    new Answer { text = "Dahil sa mapaminsalang pagbaha na lumalamon sa mga tahanan", isCorrect = true },
                    new Answer { text = "Dahil sa mga patay na isda", isCorrect = false },
                    new Answer { text = "Dahil sa masamang diyos ng ilog", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q2
                    question = "Ano ang sinisimbolo ng Yellow River ayon kay Huang?"
                },
                answers = new Answer[] {
                    new Answer { text = "Blessing at sumpa minsan nagbibigay ng buhay, minsan kamatayan", isCorrect = true },
                    new Answer { text = "Sumpa ng mga diyos", isCorrect = false },
                    new Answer { text = "Dakilang kalaban ng China", isCorrect = false },
                    new Answer { text = "Simpleng water source lang", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q3
                    question = "Ano ang pananaw ni Huang tungkol sa \"proteksyon\"?"
                },
                answers = new Answer[] {
                    new Answer { text = "Ang China ay protektado ng malalakas na hukbo", isCorrect = false },
                    new Answer { text = "Ang China ay walang proteksyon", isCorrect = false },
                    new Answer { text = "Ang China ay protektado ng mga diyos", isCorrect = false },
                    new Answer { text = "Ang China ay protektado ng natural barriers: Himalayas, Taklamakan, Tibet, Gobi", isCorrect = true }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q4
                    question = "Sino ang kinikilalang batang magsasaka na nagtatanim ng millet at wheat sa North China Plain?"
                },
                answers = new Answer[] {
                    new Answer { text = "Huang", isCorrect = false },
                    new Answer { text = "Li", isCorrect = true },
                    new Answer { text = "Daro", isCorrect = false },
                    new Answer { text = "Ravi", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q5
                    question = "Ano ang pangunahing dahilan kung bakit nag-organize ang mga sinaunang Tsino sa North China Plain?"
                },
                answers = new Answer[] {
                    new Answer { text = "Upang magtayo ng mga templo", isCorrect = false },
                    new Answer { text = "Upang makipagdigma sa mga karatig-bansa", isCorrect = false },
                    new Answer { text = "Upang kontrolin ang pagbaha at magtulungan sa survival", isCorrect = true },
                    new Answer { text = "Upang magpatayo ng palasyo", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q6
                    question = "Ano ang naging foundation ng organized society sa China ayon kay Chrono?"
                },
                answers = new Answer[] {
                    new Answer { text = "Ang takot sa mga hari", isCorrect = false },
                    new Answer { text = "Ang relihiyon at mga templo", isCorrect = false },
                    new Answer { text = "Ang collective work dahil sa pangangailangan ng survival", isCorrect = true },
                    new Answer { text = "Ang militar na kapangyarihan", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q7
                    question = "Ano ang simbolismo ng natural barriers ayon kay Chrono?"
                },
                answers = new Answer[] {
                    new Answer { text = "Proteksyon at isolation blessing at curse", isCorrect = true },
                    new Answer { text = "Mga hadlang sa pag-unlad", isCorrect = false },
                    new Answer { text = "Tahanan ng mga diyos", isCorrect = false },
                    new Answer { text = "Walang kahalagahan", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q8
                    question = "Ano ang sinabi ni Chrono tungkol sa relationship ng tao at kalikasan?"
                },
                answers = new Answer[] {
                    new Answer { text = "Ang kalikasan ay kaaway", isCorrect = false },
                    new Answer { text = "Ang kalikasan ay kaibigan", isCorrect = false },
                    new Answer { text = "Ang kalikasan ay kapwa kailangan mong matutong makisama rito", isCorrect = true },
                    new Answer { text = "Ang kalikasan ay hindi mahalaga", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q9
                    question = "Ano ang dahilan ng isolation ng China mula sa ibang civilizations?"
                },
                answers = new Answer[] {
                    new Answer { text = "Dahil sa takot sa dayuhan", isCorrect = false },
                    new Answer { text = "Dahil sa natural barriers na mahirap tawirin", isCorrect = true },
                    new Answer { text = "Dahil sa walang interest sa trade", isCorrect = false },
                    new Answer { text = "Dahil sa malakas na military", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q10
                    question = "Ano ang pangunahing aral na naiwan ng Yellow River sa Chinese civilization?"
                },
                answers = new Answer[] {
                    new Answer { text = "Ang kalikasan ay dapat pigilan", isCorrect = false },
                    new Answer { text = "Ang tunay na lakas ay nasa espada", isCorrect = false },
                    new Answer { text = "Ang takot ay nagdudulot ng kapayapaan", isCorrect = false },
                    new Answer { text = "Ang pinakamalakas na sibilisasyon ay nagsisimula sa pinakamahirap na kondisyon—luha nagiging pag-asa", isCorrect = true }
                }
            }
        };
        ShuffleQuestionsAndAnswers();
        ShowQuestion();
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
        // 🔧 FIX: Store question details when answer is wrong
        var currentQuestion = quizQuestions[currentQuestionIndex];
        
        if (selectedAnswer.isCorrect)
        {
            GameState.score++;
        }
        else
        {
            // Find the correct answer
            string correctAnswer = "";
            foreach (var a in currentQuestion.answers)
            {
                if (a.isCorrect)
                {
                    correctAnswer = a.text;
                    break;
                }
            }

            // Store wrong answer info
            wrongAnswers.Add(new WrongAnswerInfo
            {
                questionText = currentQuestion.questionLine.question,
                correctAnswerText = correctAnswer,
                questionNumber = currentQuestionIndex + 1
            });
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
        PlayerProgressManager.UnlockCivilization("Shang");
        
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
        // NEW:
        if (wrongAnswers.Count > 0)
        {
            resultText += "Ang mga tanong kung saan ka mali:\n\n";
            foreach (var wrongInfo in wrongAnswers)
            {
                resultText += $"{wrongInfo.questionNumber}. ";
                resultText += $"Tamang sagot: {wrongInfo.correctAnswerText}\n";
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
            "CH003_ST007",
            GameState.score,
            quizQuestions.Length,
            passed
        );

        if (GameState.score <= 7)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("HuangheCoordinateSelect");
                UpdateSaveAfterQuizCompletion();
            });
        }
        else
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("HuangheCoordinateSelect");
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
        
        // 🔧 FIX: Record timeout as wrong answer
        var currentQuestion = quizQuestions[currentQuestionIndex];
        string correctAnswer = "";
        foreach (var a in currentQuestion.answers)
        {
            if (a.isCorrect)
            {
                correctAnswer = a.text;
                break;
            }
        }

        wrongAnswers.Add(new WrongAnswerInfo
        {
            questionText = currentQuestion.questionLine.question,
            correctAnswerText = correctAnswer,
            questionNumber = currentQuestionIndex + 1
        });

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
            
            case "AssyrianQuizTime":
                return "HarappaSceneOne"; // Progress to Harappa
            
            case "HarappaQuizTime":
                return "SiningSceneOne"; // Progress to Sining
            
            case "SiningQuizTime":
                return "HuangHeSceneOne"; // Progress to HuangHe
            
            case "HuangHeQuizTime":
                return "ShangSceneOne"; // Progress to Shang
            
            case "ShangQuizTime":
                return "NileSceneOne"; // Progress to Nile
            
            case "NileQuizTime":
                return "KingdomSceneOne"; // Progress to Kingdom
        
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
        else if (currentQuizScene.Contains("HuangHe"))
        {
            return sceneName.Contains("HuangHe");
        }
        else if (currentQuizScene.Contains("Shang"))
        {
            return sceneName.Contains("Shang");
        }
        else if (currentQuizScene.Contains("Nile"))
        {
            return sceneName.Contains("Nile");
        }
        else if (currentQuizScene.Contains("Kingdom"))
        {
            return sceneName.Contains("Kingdom");
        }

        return false;
    }
}
