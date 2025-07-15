using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class SumerianThirdRecallChallenges : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueLine
    {
        public string characterName;
        public string line;
    }

    [System.Serializable]
    public struct Answer
    {
        public string text;
        public bool isCorrect;
    }

    public Button[] answerButtons;
    public Button nextButton;
    public TextMeshProUGUI dialogueText;

    public int currentDialogueIndex = 0;
    public DialogueLine[] dialogueLines;
    private Answer[] answers;
    private bool hasAnswered = false;

    public Image[] heartImages;
    private bool isShowingOligarkiyaDialogue = false;
    private bool isShowingMonarkiyaDialogue = false;
    private bool isShowingTheocracyDialogue = false;
    public AudioSource finishAudioSource;


    void Start()
    {
        nextButton.gameObject.SetActive(false);
        UpdateHeartsUI();

        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "PATESI NINURTA",
                line = " Dayuhan, sa iyong nakita at narinig, masasabi mo ba kung anong uri ng pamahalaan ang mayroon kami sa Uruk?"
            },
        };

        ShowDialogue();
    }
    private DialogueLine[] Theocracy = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = " PLAYER",
            line = " Theocracy… Ang pamahalaang pinamumunuan ng isang pinunong-pari."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Tumpak! Talagang lumalalim na ang pagkaunawa mo sa kabihasnan ng Sumer."
        },
        new DialogueLine
        {
            characterName = "PATESI NINURTA",
            line = " Karapat-dapat kang dalhin sa susunod na bahagi ng aming lungsod. Halina at ipapakita ko sa inyo ang iba pa naming kaayusan."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Ngayon ay panahon namang masdan ang kanilang mga imbensyon at kalakalan... Tara?"
        },
    };
    private DialogueLine[] Oligarkiya = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = "Oligarkiya...?" },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Hindi. Sa ganitong kabihasnan, ang kapangyarihan ay nagmumula sa paniniwala sa mga diyos. Subukan mong alalahanin kung sino ang tunay na may hawak ng pamahalaan dito."
        },
        new DialogueLine
        {
            characterName = "PATESI NINURTA",
            line = " Ang pamahalaan ay hindi lamang tungkol sa kapangyarihan ng tao, kundi sa pananampalatayang nakaangkla sa aming mga diyos."
        },
    };
    private DialogueLine[] Monarkiya = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = "Monarkiya…?" },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Hindi. Sa ganitong kabihasnan, ang kapangyarihan ay nagmumula sa paniniwala sa mga diyos. Subukan mong alalahanin kung sino ang tunay na may hawak ng pamahalaan dito."
        },
        new DialogueLine
        {
            characterName = "PATESI NINURTA",
            line = " Ang pamahalaan ay hindi lamang tungkol sa kapangyarihan ng tao, kundi sa pananampalatayang nakaangkla sa aming mga diyos."
        },
    };

    void SetAnswers()
    {
        answers = new Answer[]
        {
            new Answer { text = "Theocracy", isCorrect = true },
            new Answer { text = "Oligarkiya", isCorrect = false },
            new Answer { text = "Monarkiya", isCorrect = false },
        };

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i;

            TMP_Text buttonText = answerButtons[i].GetComponentInChildren<TMP_Text>();
            if (buttonText != null) buttonText.text = answers[i].text;

            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() =>
            {
                Debug.Log("✅ CLICKED: " + answers[index].text);
                OnAnswerSelected(answers[index]);
            });
        }
    }

    void ShowDialogue()
    {
        DialogueLine line = dialogueLines[currentDialogueIndex];
        dialogueText.text = $"<b>{line.characterName}</b>: {line.line}";

        // Only set answers for the first question
        if (currentDialogueIndex == 0)
        {
            SetAnswers();
            foreach (Button btn in answerButtons)
            {
                btn.interactable = true;
                btn.gameObject.SetActive(true);
            }
            nextButton.gameObject.SetActive(false);
        }
        else
        {

            if (currentDialogueIndex == dialogueLines.Length - 1)
            {
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(() =>
                {
                    if (finishAudioSource != null)
                        finishAudioSource.Play();
                    nextButton.interactable = false;
                    Invoke(nameof(LoadNextScene), 2f); // 2 seconds delay, adjust as needed
                });
            }
            else
            {
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(() =>
                {
                    currentDialogueIndex++;
                    ShowDialogue();
                });
            }
        }

        hasAnswered = false;
    }
    void UpdateHeartsUI()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            heartImages[i].enabled = i < GameState.hearts;
        }
    }

    void OnAnswerSelected(Answer selected)
    {

        if (selected.isCorrect)
        {
            isShowingTheocracyDialogue = true;
            currentDialogueIndex = 0;
            dialogueLines = Theocracy;
            ShowDialogue();

            nextButton.gameObject.SetActive(true);
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() =>
            {
                currentDialogueIndex++;
                if (currentDialogueIndex < dialogueLines.Length - 1)
                {
                    ShowDialogue();
                }
                else
                {
                    // Show the last line
                    ShowDialogue();
                    nextButton.onClick.RemoveAllListeners();
                    nextButton.onClick.AddListener(() =>
                    {
                        SceneManager.LoadScene("SumerianSceneFive");
                    });
                }
            });
        }
        else
        {
            GameState.hearts--;
            UpdateHeartsUI();

            if (GameState.hearts <= 0)
            {
                dialogueText.text = "<b>ENKI</b>: Uliting muli. Wala ka nang natitirang puso.";
                nextButton.gameObject.SetActive(false);
                foreach (Button btn in answerButtons)
                    btn.interactable = false;
                Invoke(nameof(LoadGameOverScene), 2f);
                return;
            }

            if (selected.text == "Oligarkiya")
            {
                isShowingOligarkiyaDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = Oligarkiya;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextOligarkiyaDialogue);
            }
            else if (selected.text == "Monarkiya")
            {
                isShowingMonarkiyaDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = Monarkiya;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextMonarkiyaDialogue);
            }
            else
            {
                foreach (Button btn in answerButtons)
                {
                    btn.interactable = true;
                }
                nextButton.gameObject.SetActive(false);
                hasAnswered = false;
            }
        }

        void ShowNextTheocracyDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextTheocracyDialogue);
            }
        }

        void ShowNextOligarkiyaDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextOligarkiyaDialogue);
            }
        }

        void ShowNextMonarkiyaDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextMonarkiyaDialogue);
            }
        }
    }
    void LoadGameOverScene()
    {
        SceneManager.LoadScene("GameOver");
    }
    void LoadNextScene()
    {
        SceneManager.LoadScene("SumerianSceneFive");
    }
}
