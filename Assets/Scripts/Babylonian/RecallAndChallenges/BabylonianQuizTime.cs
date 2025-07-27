using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using static SumerianFifthRecallChallenges;

public class QuizTimeManagerBabylonian : MonoBehaviour
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
                    question = "Sino ang tinaguriang pinakadakilang hari ng Babylonia?"
                },
                answers = new Answer[] {
                    new Answer { text = "Sargon I", isCorrect = false },
                    new Answer { text = "Ashurbanipal", isCorrect = false },
                    new Answer { text = "Nebuchadnezzar II", isCorrect = false },
                    new Answer { text = "Hammurabi", isCorrect = true },
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q2
                    question = "Ano ang tawag sa hanay ng batas na ipinagawa ni Hammurabi?"
                },
                answers = new Answer[] {
                    new Answer { text = "Kodigo ni Hammurabi", isCorrect = true },
                    new Answer { text = "Kodigo ng Katarungan ni Hammurabi", isCorrect = false },
                    new Answer { text = "Batas sa Kalakalan ni Hammurabi", isCorrect = false },
                    new Answer { text = "Alituntunin ng Babilonya", isCorrect = false },
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q3
                    question = "Ano ang pangunahing layunin ng Kodigo ni Hammurabi?"
                },
                answers = new Answer[] {
                    new Answer { text = "Magbigay ng kapangyarihan sa mga pari", isCorrect = false },
                    new Answer { text = "Magturo ng pagsamba kay Marduk", isCorrect = false },
                    new Answer { text = "Palakasin ang sandatahang-lakas", isCorrect = false },
                    new Answer { text = "Panatilihin ang kaayusan sa lipunan", isCorrect = true },
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q4
                    question = "Ano ang prinsipyo ng parusa sa Kodigo ni Hammurabi?"
                },
                answers = new Answer[] {
                    new Answer { text = "Buhay kapalit ng buhay", isCorrect = false },
                    new Answer { text = "Mata sa mata, ngipin sa ngipin", isCorrect = true },
                    new Answer { text = "Lahat ay pantay-pantay", isCorrect = false },
                    new Answer { text = "Isang batas para sa lahat", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q5
                    question = " Ayon kay Zul, saan nanggagaling ang tubig na ginagamit sa irigasyon ng mga sakahan sa Uruk?"
                },
                answers = new Answer[] {
                    new Answer { text = "Aklat ni Hammurabi", isCorrect = false },
                    new Answer { text = "Awit ng Uruk", isCorrect = false },
                    new Answer { text = "Enuma Elish", isCorrect = false },
                    new Answer { text = " Epikong Gilgamesh", isCorrect = true }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q6
                    question = "Anong lungsod ang pinalawak ni Hammurabi upang maging imperyo?"
                },
                answers = new Answer[] {
                    new Answer { text = "Ur", isCorrect = false },
                    new Answer { text = "Babylonian", isCorrect = true },
                    new Answer { text = "Akkad", isCorrect = false },
                    new Answer { text = "Maynila", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q7
                    question = "Anong paraan ang ginamit ni Hammurabi upang mapalawak ang kanyang nasasakupan?"
                },
                answers = new Answer[] {
                    new Answer { text = "Diplomasya, kasunduan, at digmaan", isCorrect = true },
                    new Answer { text = "Digmaan lamang", isCorrect = false },
                    new Answer { text = "Kalakalan", isCorrect = false },
                    new Answer { text = "Pananakop ng dagat", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q8
                    question = "Sino si Marduk ayon sa paniniwala ng mga taga-Babylonia?"
                },
                answers = new Answer[] {
                    new Answer { text = "Hari ng Babylonia", isCorrect = false },
                    new Answer { text = "Dakilang pari", isCorrect = false },
                    new Answer { text = "Pangunahing diyos ng imperyo", isCorrect = true },
                    new Answer { text = "Sugo ng mga diyos", isCorrect = false }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q9
                    question = "Aling imbensyon ng mga Sumerian ang nakatulong sa pag-unlad ng transportasyon sa lupa?"
                },
                answers = new Answer[] {
                    new Answer { text = "Mag-ulat ng kasaysayan", isCorrect = false },
                    new Answer { text = "Mang-aliw lamang sa mga tao", isCorrect = false },
                    new Answer { text = "Magpakita ng lakas ng mga hari", isCorrect = false },
                    new Answer { text = "Magturo ng aral at ipreserba ang kultura", isCorrect = true }
                }
            },
            new Question {
                questionLine = new DialogueLine {
                    //Q10
                    question = "Ano ang pangunahing dahilan ng pagbagsak ng Babylonia pagkatapos ni Hammurabi?"
                },
                answers = new Answer[] {
                    new Answer { text = "Pagkawala ng mga diyos", isCorrect = false },
                    new Answer { text = "Rebolusyon ng mamamayan", isCorrect = false },
                    new Answer { text = "Pananakop ng Griyego", isCorrect = false },
                    new Answer { text = "Kawalan ng matalinong pamumuno", isCorrect = true }
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
        PlayerProgressManager.UnlockCivilization("Assyrian");

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
