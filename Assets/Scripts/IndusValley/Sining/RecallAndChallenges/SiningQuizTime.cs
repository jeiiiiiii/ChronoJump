using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using static SumerianFifthRecallChallenges;

public class QuizTimeManagerSining : MonoBehaviour
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
                    question = "Ano ang pangunahing layunin ng Great Bath sa Mohenjo-daro?"
                },
                answers = new Answer[] {
                    new Answer { text = "Para sa pag-iimbak ng tubig", isCorrect = false },
                    new Answer { text = "Para sa ritwal ng kalinisan ", isCorrect = true },
                    new Answer { text = "Para sa sports at laro", isCorrect = false },
                    new Answer { text = "Para sa pakikipag-ugnayan sa mga diyos", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q2
                    question = "Ano ang sinisimbolo ng Great Bath ayon kay Sindhu?"
                },
                answers = new Answer[] {
                    new Answer { text = "Bunga ng libu-libong oras ng pagpaplano bawat detalye ay may layunin", isCorrect = true },
                    new Answer { text = "Kapangyarihan ng hari", isCorrect = false },
                    new Answer { text = "Tahanan ng mga diyos", isCorrect = false },
                    new Answer { text = "Display ng yaman", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q3
                    question = "Ano ang pananaw ni Sindhu tungkol sa \"teknolohiya\"?"
                },
                answers = new Answer[] {
                    new Answer { text = "Teknolohiya ay tungkol sa kung gaano kalaki ang gusali", isCorrect = false },
                    new Answer { text = "Teknolohiya ay para sa digmaan ", isCorrect = false },
                    new Answer { text = "Teknolohiya ay regalo ng mga diyos", isCorrect = false },
                    new Answer { text = " Teknolohiya ay hindi lang tungkol sa kung paano, kundi bakit mo ito ginawa", isCorrect = true }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q4
                    question = "Sino ang kinikilalang batang craftsman na gumagawa ng carnelian at lapis lazuli beads?"
                },
                answers = new Answer[] {
                    new Answer { text = "Matrika", isCorrect = false },
                    new Answer { text = "Ravi", isCorrect = true },
                    new Answer { text = "Daro", isCorrect = false },
                    new Answer { text = "Sindhu", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q5
                    question = "Ano ang layunin ng paggamit ng carnelian beads ng mga taga-Indus Valley?"
                },
                answers = new Answer[] {
                    new Answer { text = "Para sa religious ceremonies", isCorrect = false },
                    new Answer { text = "Para sa digmaan", isCorrect = false },
                    new Answer { text = "Para sa kalakalan hanggang Mesopotamia at ibang rehiyon", isCorrect = true },
                    new Answer { text = "Para sa decoration ng mga palasyo", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q6
                    question = "Ano ang naging innovation ng Indus Valley sa pottery?"
                },
                answers = new Answer[] {
                    new Answer { text = "Gumamit ng malalaking kiln", isCorrect = false },
                    new Answer { text = "Gumamit ng imported clay", isCorrect = false },
                    new Answer { text = "Gumamit ng potter's wheel na mabilis at tumpak", isCorrect = true },
                    new Answer { text = "Gumamit ng gold decoration", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q7
                    question = "Ano ang simbolismo ng bronze metallurgy ayon kay Sindhu?"
                },
                answers = new Answer[] {
                    new Answer { text = "Metal ay tool, hindi sandata ang tunay na lakas ay nasa kung paano mo ito ginagamit", isCorrect = true },
                    new Answer { text = "Yaman at status symbol", isCorrect = false },
                    new Answer { text = "Sandata para sa digmaan", isCorrect = false },
                    new Answer { text = "Offerings para sa mga diyos", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q8
                    question = "Ano ang sinabi ni Chrono tungkol sa sining ng Indus Valley?"
                },
                answers = new Answer[] {
                    new Answer { text = "Ang sining ay dapat malaki upang maging mahalaga", isCorrect = false },
                    new Answer { text = "Ang sining ay para sa mga mayaman lang", isCorrect = false },
                    new Answer { text = "Ang sining ay hindi kailangang maging malaki minsan ang pinakamaganda ay nasa detalye", isCorrect = true },
                    new Answer { text = "Ang sining ay walang kahalagahan", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q9
                    question = " Bakit tumigil ang produksyon ng sining at teknolohiya sa Indus Valley?"
                },
                answers = new Answer[] {
                    new Answer { text = "Sinakop ng ibang imperyo", isCorrect = false },
                    new Answer { text = "Nawala ang trade at sistema dahil sa climate change", isCorrect = true },
                    new Answer { text = "Naubos ang resources", isCorrect = false },
                    new Answer { text = "Nag-rebel ang mga craftsmen", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q10
                    question = "Ano ang pangunahing aral na naiwan ng sining at teknolohiya ng Indus Valley?"
                },
                answers = new Answer[] {
                    new Answer { text = "Ang sining ay para sa mga elite lang", isCorrect = false },
                    new Answer { text = "Ang teknolohiya ay para sa digmaan", isCorrect = false },
                    new Answer { text = "Ang yaman ay mas importante kaysa husay", isCorrect = false },
                    new Answer { text = "Ang sining at teknolohiya ay nabubuhay sa komunidad—kapag naglaho ang komunidad, naglaho din ang lahat", isCorrect = true }
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
        PlayerProgressManager.UnlockCivilization("HuangHe");
        
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
                SceneManager.LoadScene("ChapterSelect");
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


        return false;
    }
}
