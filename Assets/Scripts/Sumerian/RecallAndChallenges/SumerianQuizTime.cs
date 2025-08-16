using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using static SumerianFifthRecallChallenges;

public class QuizTimeManagerSumerian : MonoBehaviour
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
        quizQuestions = new Question[]
        {
            new Question {
                questionLine = new DialogueLine {
                    //Q1
                    question = "Sino ang tinatawag na “patesi” sa lungsod ng Uruk?"
                },
                answers = new Answer[] {
                    new Answer { text = " Pinuno ng mga manggagawa", isCorrect = false },
                    new Answer { text = "Pinunong-pari na tagapamagitan ng mga diyos at mamamayan", isCorrect = true },
                    new Answer { text = "Mangangalakal ng lungsod", isCorrect = false },
                    new Answer { text = "Sundalong tagapagbantay", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q2
                    question = "Ano ang tawag sa sistema ng pamahalaang pinamumunuan ng pinunong-pari sa Sumer?"
                },
                answers = new Answer[] {
                    new Answer { text = "Monarkiya", isCorrect = false },
                    new Answer { text = "Demokrasya", isCorrect = false },
                    new Answer { text = "Theocracy", isCorrect = true },
                    new Answer { text = "Oligarkiya", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q3
                    question = "Ano ang layunin ng mga kanal at irigasyon na ginawa ng mga Sumerian?"
                },
                answers = new Answer[] {
                    new Answer { text = "Upang mapabilis ang kalakalan", isCorrect = false },
                    new Answer { text = "Upang hadlangan ang mga kalaban ", isCorrect = false },
                    new Answer { text = " Upang gawing paliguan ang mga lungsod", isCorrect = false },
                    new Answer { text = "Upang pamahalaan ang daloy ng tubig para sa sakahan", isCorrect = true }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q4
                    question = "Ang Sumerian script na tinatawag na “cuneiform” ay isinulat gamit ang stylus sa anong uri ng materyal?"
                },
                answers = new Answer[] {
                    new Answer { text = "Piraso ng kahoy", isCorrect = false },
                    new Answer { text = "Papel mula sa papyrus", isCorrect = false },
                    new Answer { text = "Tabletang luwad (clay tablet)", isCorrect = true },
                    new Answer { text = "Balat ng hayop", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q5
                    question = " Ayon kay Zul, saan nanggagaling ang tubig na ginagamit sa irigasyon ng mga sakahan sa Uruk?"
                },
                answers = new Answer[] {
                    new Answer { text = "Ilog Tigris at Euphrates", isCorrect = true },
                    new Answer { text = "Disyerto", isCorrect = false },
                    new Answer { text = "Imbak na ulan ", isCorrect = false },
                    new Answer { text = " Bukal sa ilalim ng lupa", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q6
                    question = "Ano ang tawag sa sistema ng palitan ng produkto na ginagamit ng mga Sumerian bago pa magkaroon ng salapi?"
                },
                answers = new Answer[] {
                    new Answer { text = "Transaksyon", isCorrect = false },
                    new Answer { text = "Monopolyo", isCorrect = false },
                    new Answer { text = "Barter", isCorrect = true },
                    new Answer { text = "Komersyo", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q7
                    question = "Ayon kay Enki, bakit mahalaga ang pagsusulat ng mga tala sa clay tablet?"
                },
                answers = new Answer[] {
                    new Answer { text = "Para sa pagtatala ng ani, batas, at kasunduan", isCorrect = true },
                    new Answer { text = "Para sa pagsulat ng tula", isCorrect = false },
                    new Answer { text = "Para sa panlibang", isCorrect = false },
                    new Answer { text = "Para sa pagbebenta sa merkado", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q8
                    question = "Sa eksena ng palengke sa Uruk, ano ang layunin ng paggamit ng gulong ayon kay Ishma?"
                },
                answers = new Answer[] {
                    new Answer { text = "Pang-aliw sa mga bata", isCorrect = false },
                    new Answer { text = "Palamuti sa templo", isCorrect = false },
                    new Answer { text = "Para mapabilis ang pagdadala ng produkto at paglalakbay sa lupa", isCorrect = true },
                    new Answer { text = "Bilang alay sa mga diyos", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q9
                    question = "Aling imbensyon ng mga Sumerian ang nakatulong sa pag-unlad ng transportasyon sa lupa?"
                },
                answers = new Answer[] {
                    new Answer { text = "Layag", isCorrect = false },
                    new Answer { text = "Gulong", isCorrect = true },
                    new Answer { text = "Kompas", isCorrect = false },
                    new Answer { text = "Kabayo", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q10
                    question = "Alin sa mga sumusunod ang pangunahing layunin ng mga batas ni Ur-Nammu?"
                },
                answers = new Answer[] {
                    new Answer { text = "Patawan ng parusa ang mayayaman", isCorrect = false },
                    new Answer { text = "Itaguyod ang kapangyarihan ng hari", isCorrect = false },
                    new Answer { text = "Mapalawak ang teritoryo ng Sumer", isCorrect = false },
                    new Answer { text = "Itaguyod ang katarungan", isCorrect = true }
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
        PlayerProgressManager.UnlockCivilization("Akkadian");
        
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

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("CoordinateSelect");
        });
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
