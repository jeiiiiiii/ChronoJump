using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using static SumerianFifthRecallChallenges;

public class QuizTimeManagerAkkadian : MonoBehaviour
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
                    question = "Sino ang kinikilalang tagapagtatag ng kauna-unahang imperyo sa kasaysayan?"
                },
                answers = new Answer[] {
                    new Answer { text = "Ur-Nammu", isCorrect = false },
                    new Answer { text = "Sargon I ", isCorrect = true },
                    new Answer { text = "Hammurabi", isCorrect = false },
                    new Answer { text = "Naram-Sin", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q2
                    question = "Ano ang tawag sa sistema ng pamahalaang pinamumunuan ng pinunong-pari sa Sumer?"
                },
                answers = new Answer[] {
                    new Answer { text = "Akkad", isCorrect = true },
                    new Answer { text = "Uruk", isCorrect = false },
                    new Answer { text = "Babylon", isCorrect = false },
                    new Answer { text = "Larsa", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q3
                    question = "Ano ang naging pangunahing paraan ni Sargon upang pag-isahin ang mga lungsod-estado?"
                },
                answers = new Answer[] {
                    new Answer { text = "Diplomasiya", isCorrect = false },
                    new Answer { text = "Pagtatayo ng templo ", isCorrect = false },
                    new Answer { text = "Pananampalataya", isCorrect = false },
                    new Answer { text = "Lakas ng militar at mahusay na pamahalaan", isCorrect = true }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q4
                    question = "Ano ang sinabi ni Sargon tungkol sa pagpapanatili ng isang malawak na imperyo?"
                },
                answers = new Answer[] {
                    new Answer { text = "Kailangan umasa sa mga diyos", isCorrect = false },
                    new Answer { text = "Kailangan ng sistemang gumagana kahit wala ang pinuno", isCorrect = true },
                    new Answer { text = "Magtiwala sa kapalaran", isCorrect = false },
                    new Answer { text = "Dapat matakot sa hari ang lahat", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q5
                    question = "Ano ang ginawa ni Sargon upang mapanatili ang kaayusan sa imperyo?"
                },
                answers = new Answer[] {
                    new Answer { text = "Ipinagbawal ang digmaan", isCorrect = false },
                    new Answer { text = "Nagtayo ng mga templo", isCorrect = false },
                    new Answer { text = "Nagtalaga ng mga gobernador sa bawat bahagi ", isCorrect = true },
                    new Answer { text = "Nagpatupad ng bagong relihiyon", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q6
                    question = "Ano ang pangunahing dahilan ng pagbagsak ng Akkadian Empire?"
                },
                answers = new Answer[] {
                    new Answer { text = "Matinding tagtuyot", isCorrect = false },
                    new Answer { text = "Rebolusyon mula sa loob", isCorrect = false },
                    new Answer { text = "Pagsalakay ng Amorite at Hurrian", isCorrect = true },
                    new Answer { text = "Kakulangan sa pagkain", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q7
                    question = "Sino ang sumunod na pinuno matapos si Sargon at sa ilalim ng pamumuno niya humina ang imperyo?"
                },
                answers = new Answer[] {
                    new Answer { text = "Naram-Sin", isCorrect = true },
                    new Answer { text = "Ur-Nammu", isCorrect = false },
                    new Answer { text = "Hammurabi", isCorrect = false },
                    new Answer { text = "Ishtar", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q8
                    question = "Ano ang nangyari matapos bumagsak ang Akkadian Empire?"
                },
                answers = new Answer[] {
                    new Answer { text = "Naibalik ang kapangyarihan ng Akkad", isCorrect = false },
                    new Answer { text = "Nagsimula ang pamumuno ng mga Greek", isCorrect = false },
                    new Answer { text = "Panandaliang pagbawi ng Ur at tunggalian ng Isin at Larsa", isCorrect = true },
                    new Answer { text = "Umusbong agad ang imperyong Babylonia", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q9
                    question = "Ayon kay Sargon, ano ang tunay na layunin ng pamumuno?"
                },
                answers = new Answer[] {
                    new Answer { text = "Pagpapayaman", isCorrect = false },
                    new Answer { text = "Layunin at kaayusan", isCorrect = true },
                    new Answer { text = "Takot at kapangyarihan", isCorrect = false },
                    new Answer { text = "Katapatan sa mga diyos", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q10
                    question = "Ano ang natutunang aral mula sa pamumuno ni Sargon ayon kina Chrono at Player?"
                },
                answers = new Answer[] {
                    new Answer { text = "Ang kapangyarihan ay permanente", isCorrect = false },
                    new Answer { text = "Walang silbi ang pamumuno sa dulo", isCorrect = false },
                    new Answer { text = "Ang bawat pamahalaan ay malakas magpakailanman", isCorrect = false },
                    new Answer { text = "Ang alaala ng mga pinuno ay mananatili sa kasaysayan", isCorrect = true }
                }
            }
        };
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

    void ShowQuizResult()
    {
        PlayerProgressManager.UnlockCivilization("Babylonian");
        
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
                SceneManager.LoadScene("CoordinateSelect");
            });
        }
        else
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("AkkadianArtifactScene");
                PlayerAchievementManager.UnlockAchievement("Sword");
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


}
