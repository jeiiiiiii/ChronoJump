using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using static SumerianFifthRecallChallenges;

public class QuizTimeManagerHarappa : MonoBehaviour
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
                    question = "Ano ang pangunahing katangian ng mga lungsod ng Indus Valley tulad ng Mohenjo-daro?"
                },
                answers = new Answer[] {
                    new Answer { text = "Malalaking templo at palasyo", isCorrect = false },
                    new Answer { text = "Grid system at advanced drainage", isCorrect = true },
                    new Answer { text = "Matinding hukbo at pader", isCorrect = false },
                    new Answer { text = "Mataas na pyramids at ziggurats", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q2
                    question = "Ano ang sinisimbolo ng Mohenjo-daro ayon kay Chrono?"
                },
                answers = new Answer[] {
                    new Answer { text = "Lungsod na pinamumunuan ng sistema at dunong, hindi ng takot", isCorrect = true },
                    new Answer { text = "Tahanan ng mga diyos", isCorrect = false },
                    new Answer { text = "Sentro ng digmaan at pananakop", isCorrect = false },
                    new Answer { text = "Lungsod ng mga hari at reyna", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q3
                    question = "Ano ang pananaw ni Daro tungkol sa \"kapayapaan\"?"
                },
                answers = new Answer[] {
                    new Answer { text = "Kapayapaan ay regalo ng mga diyos", isCorrect = false },
                    new Answer { text = "Kapayapaan ay kinukuha sa pamamagitan ng takot", isCorrect = false },
                    new Answer { text = "Kapayapaan ay resulta ng digmaan", isCorrect = false },
                    new Answer { text = "Kapayapaan ay bunga ng dunong at sistema, hindi takot", isCorrect = true }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q4
                    question = "Sino ang kinikilalang tagapag-ukit ng mga clay seals sa Mohenjo-daro?"
                },
                answers = new Answer[] {
                    new Answer { text = "Daro", isCorrect = false },
                    new Answer { text = "Matrika", isCorrect = true },
                    new Answer { text = "Ravi", isCorrect = false },
                    new Answer { text = "Sindhu", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q5
                    question = "Ano ang layunin ng paggamit ng standardized weights ng mga taga-Indus Valley?"
                },
                answers = new Answer[] {
                    new Answer { text = "Para sa religious rituals", isCorrect = false },
                    new Answer { text = "Para sa military purposes", isCorrect = false },
                    new Answer { text = "Para sa patas na kalakalan at palitan", isCorrect = true },
                    new Answer { text = "Para sa construction ng mga gusali", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q6
                    question = "Ano ang naging misteryo ng Indus Valley Civilization na hindi pa rin nalulutas?"
                },
                answers = new Answer[] {
                    new Answer { text = "Ang dahilan ng pagtatayo ng pyramids", isCorrect = false },
                    new Answer { text = "Ang pagkakaroon ng malalaking hukbo", isCorrect = false },
                    new Answer { text = "Ang Indus script na hindi pa rin nababasa", isCorrect = true },
                    new Answer { text = "Ang sistema ng irrigation", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q7
                    question = "Ano ang simbolismo ng clay seals ayon kay Matrika?"
                },
                answers = new Answer[] {
                    new Answer { text = "Mga pangalan, kalakal, at mensahe ngunit mawawala sa kasaysayan", isCorrect = true },
                    new Answer { text = "Mga sandata ng digmaan", isCorrect = false },
                    new Answer { text = "Mga religious offerings", isCorrect = false },
                    new Answer { text = "Mga tool sa construction", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q8
                    question = "Ano ang sinabi ni Chrono tungkol sa irrigation systems at agriculture ng Indus Valley?"
                },
                answers = new Answer[] {
                    new Answer { text = "Ginamit lang para sa mga ceremonial rituals", isCorrect = false },
                    new Answer { text = "Mas advanced kaysa sa modernong sistema", isCorrect = false },
                    new Answer { text = "Lahat ay nakadepende sa Indus River kapag tumitigil ang daloy, tumitigil din sila", isCorrect = true },
                    new Answer { text = "Hindi importante sa kanilang kultura", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q9
                    question = "Ano ang dahilan ng pagbagsak ng Indus Valley Civilization ayon kay Chrono?"
                },
                answers = new Answer[] {
                    new Answer { text = "Pananakop ng mga dayuhang imperyo", isCorrect = false },
                    new Answer { text = "Pagbabago ng klima at pagkatuyo ng Indus River", isCorrect = true },
                    new Answer { text = "Digmaan sa pagitan ng mga lungsod", isCorrect = false },
                    new Answer { text = "Epidemic ng sakit", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q10
                    question = "Ano ang pangunahing aral na naiwan ng Indus Valley Civilization?"
                },
                answers = new Answer[] {
                    new Answer { text = "Ang takot ay nagdudulot ng kapayapaan", isCorrect = false },
                    new Answer { text = "Ang tunay na lakas ay nasa espada", isCorrect = false },
                    new Answer { text = "Ang bawat pamahalaan ay malakas magpakailanman", isCorrect = false },
                    new Answer { text = "Ang imperyo ay tumatagal magpakailanman", isCorrect = true }
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
        PlayerProgressManager.UnlockCivilization("Sining");
        
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
            "CH002_ST005",
            GameState.score,
            quizQuestions.Length,
            passed
        );

        if (GameState.score <= 7)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("IndusCoordinateSelect");
                UpdateSaveAfterQuizCompletion();
            });
        }
        else
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() =>
            {
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

        return false;
    }
}
