using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using static SumerianFifthRecallChallenges;

public class QuizTimeManagerAssyrian : MonoBehaviour
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
                    question = "Ano ang pangunahing paraan na ginamit ni Tiglath-Pileser I upang mapalawak ang imperyong Assyrian?"
                },
                answers = new Answer[] {
                    new Answer { text = "Pagpapalaganap ng sining", isCorrect = false },
                    new Answer { text = "Panunuyo sa mga kalapit na hari", isCorrect = false },
                    new Answer { text = "Ekspedisyong militar at pananakop", isCorrect = true },
                    new Answer { text = "Pagpapakumbaba sa harap ng kalaban", isCorrect = false },
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q2
                    question = "Ano ang sinisimbolo ng lungsod ng Nineveh ayon kay Chrono?"
                },
                answers = new Answer[] {
                    new Answer { text = "Kapangyarihan at babala", isCorrect = true },
                    new Answer { text = "Lungsod ng kapayapaan", isCorrect = false },
                    new Answer { text = "Sentro ng sining", isCorrect = false },
                    new Answer { text = "Tirahan ng mga diyos", isCorrect = false },
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q3
                    question = "Ano ang pananaw ni Tiglath-Pileser tungkol sa “kapayapaan”?"
                },
                answers = new Answer[] {
                    new Answer { text = "Kapayapaan ay regalo sa mga masunurin", isCorrect = false },
                    new Answer { text = "Kapayapaan ay kinukuha sa pamamagitan ng takot", isCorrect = true },
                    new Answer { text = "Kapayapaan ay bunga ng diplomasya", isCorrect = false },
                    new Answer { text = "Kapayapaan ay biyaya ng mga diyos", isCorrect = false },
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q4
                    question = "Sino ang kinikilalang tagapagtatag ng Imperyong Assyrian?"
                },
                answers = new Answer[] {
                    new Answer { text = "Hammurabi", isCorrect = false },
                    new Answer { text = "Tiglath-Pileser I", isCorrect = true },
                    new Answer { text = "Ashurbanipal", isCorrect = false },
                    new Answer { text = "Sargon II", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q5
                    question = "Ano ang layunin ng ekspedisyong militar ng mga Assyrian?"
                },
                answers = new Answer[] {
                    new Answer { text = "Paghahanap ng bagong tirahan", isCorrect = false },
                    new Answer { text = "Pagpapalaganap ng relihiyon", isCorrect = false },
                    new Answer { text = "Pagkuha ng mga rutang pangkalakalan at tributo", isCorrect = true },
                    new Answer { text = "Pagpapalawak ng agrikultura", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q6
                    question = "Ano ang naging ambag ni Ashurbanipal bukod sa pamumuno?"
                },
                answers = new Answer[] {
                    new Answer { text = "Pagpapagawa ng silid-aklatan", isCorrect = true },
                    new Answer { text = "Pagtayo ng ziggurat", isCorrect = false },
                    new Answer { text = "Pagpapalawak ng agrikultura", isCorrect = false },
                    new Answer { text = "Pagpapalaganap ng sining", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q7
                    question = "Ano ang simbolismo ng silid-aklatan ni Ashurbanipal ayon kay Chrono?"
                },
                answers = new Answer[] {
                    new Answer { text = "Dakilang templo ng mga diyos", isCorrect = false },
                    new Answer { text = "Paaralan para sa mga pari", isCorrect = false },
                    new Answer { text = "Alaala ng sibilisasyon sa gitna ng karahasan", isCorrect = true },
                    new Answer { text = "Munting tahanan ng karunungan", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q8
                    question = "Ano ang sinabi ni Chrono tungkol sa mga kalsada at serbisyong postal ng Assyria?"
                },
                answers = new Answer[] {
                    new Answer { text = "Ginamit para sa paglalakbay ng hari", isCorrect = false },
                    new Answer { text = "Ginamit sa paglalaganap ng sining", isCorrect = false },
                    new Answer { text = "Simbolo ng imperyal na kaayusan", isCorrect = true },
                    new Answer { text = "Para lamang sa mga mangangalakal", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q9
                    question = "Ano ang dahilan ng pagbagsak ng Assyrian ayon kay Chrono?"
                },
                answers = new Answer[] {
                    new Answer { text = "Wala itong naging kahulugan", isCorrect = false },
                    new Answer { text = "Panloob na trahedya", isCorrect = false },
                    new Answer { text = "Pagkatalo sa digmaan laban sa Roma", isCorrect = false },
                    new Answer { text = "Sama-samang galit ng mga dating pinaghaharian", isCorrect = true }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q10
                    question = "Sino-sino ang nagtulungan upang pabagsakin ang Assyrian?"
                },
                answers = new Answer[] {
                    new Answer { text = "Babylonians, Greeks, Romans", isCorrect = false },
                    new Answer { text = "Hittites, Lydians, Phoenicians", isCorrect = false },
                    new Answer { text = "Pananakop ng Griyego", isCorrect = false },
                    new Answer { text = "Chaldean, Medes, Persian", isCorrect = true }
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

        // 🔹 Shuffle helper
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
        PlayerProgressManager.UnlockCivilization("Harappa");

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
            "CH001_ST004",
            GameState.score,
            quizQuestions.Length,
            passed
        );

        if (GameState.score <= 7)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("ChapterSelect");
                UpdateSaveAfterQuizCompletion();
            });
        }
        else
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("ClayArtifactScene");
                PlayerAchievementManager.UnlockAchievement("Clay");
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
