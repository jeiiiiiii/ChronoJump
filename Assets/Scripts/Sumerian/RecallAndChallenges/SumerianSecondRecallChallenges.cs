using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class SumerianSecondRecallChallenges : MonoBehaviour
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
    private bool isShowingDisyertoDialogue = false;
    private bool isShowingImbakNaUlanDialogue = false;
    private bool isShowingTigrisAndEuphratesDialogue = false;
    public AudioSource finishAudioSource;

    public SpriteRenderer PlayercharacterRenderer;
    public SpriteRenderer ChronocharacterRenderer;

    public Sprite PlayerSmile;
    public Sprite PlayerReflective;
    public Sprite PlayerCurious;
    public Sprite PlayerEager;
    public Sprite PlayerEmbarassed;
    public Sprite ChronoCheerful;
    public Sprite ChronoSad;
    public Sprite ChronoSmile;
    public Sprite ZulNeutral;
    public Sprite ZulFriendly;
    public SpriteRenderer BlurBG;

    void Start()
    {
        nextButton.gameObject.SetActive(false);
        UpdateHeartsUI();

        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "ZUL",
                line = "Ano, alam mo ba kung saan talaga nanggagaling ang tubig na dinadala sa mga kanal na ito?"
            },
        };

        ShowDialogue();
    }
    private DialogueLine[] TigrisAndEuphrates = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Tigris at Euphrates!"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Ayos! Talagang nakikinig ka. Iyan ang puso ng buhay sa Mesopotamia."
        },
        new DialogueLine
        {
            characterName = "ZUL",
            line = "Marunong kang makinig, bata. Maraming batang sumasama sa sakahan ang hindi pa rin alam 'yan."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Ngayong naunawaan mo na ang kahalagahan ng irigasyon, may isa pa akong gustong ipakita. Halina’t magtungo tayo sa harap ng templo. Doon mo makikilala ang pinunong-pari na si Ninurta."
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = "Pinunong-pari?"
        },
        new DialogueLine
        {
            characterName = "ZUL",
            line = "Ay, si Patesi Ninurta , ang tagapamagitan namin sa mga diyos. Siya rin ang nagpapatakbo sa mga gawain sa lungsod."
        },
    };
    private DialogueLine[] Disyerto = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = "Disyerto...?" },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Hindi sapat ang ulan dito. At ang disyerto ay lalong hindi mapagkakatiwalaan. Tandaan mo , ang mga ilog ang bumuhay sa kabihasnang ito."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Pumiling muli!"
        },
    };
    private DialogueLine[] ImbakNaUlan = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = "Imbak na ulan...?" },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Hindi sapat ang ulan dito. At ang disyerto ay lalong hindi mapagkakatiwalaan. Tandaan mo , ang mga ilog ang bumuhay sa kabihasnang ito."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Pumiling muli!"
        },
    };

    void SetAnswers()
    {
        answers = new Answer[]
        {
            new Answer { text = "Disyerto", isCorrect = false },
            new Answer { text = "Tigris at Euphrates", isCorrect = true },
            new Answer { text = "Imbak na ulan", isCorrect = false },
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
        
    if (dialogueLines == TigrisAndEuphrates)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerSmile;
                    ChronocharacterRenderer.sprite = ChronoSmile;
                    break;
                case 1:
                    PlayercharacterRenderer.sprite = PlayerEager;
                    ChronocharacterRenderer.sprite = ChronoCheerful;
                    break;
                case 2:
                    PlayercharacterRenderer.sprite = PlayerSmile;
                    ChronocharacterRenderer.sprite = ZulFriendly;
                    break;
                case 3:
                    foreach (Button btn in answerButtons)
                    {
                        btn.gameObject.SetActive(false);
                    }
                    
                    foreach (Image heart in heartImages)
                    {
                        heart.gameObject.SetActive(false);
                    }

                    BlurBG.gameObject.SetActive(false);
                    ChronocharacterRenderer.sprite = ChronoCheerful;
                    break;
                case 4:
                    PlayercharacterRenderer.sprite = PlayerCurious;
                    break;
                case 5:
                    ChronocharacterRenderer.sprite = ZulNeutral;
                    break;
            }
        }
        else if (dialogueLines == Disyerto)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    ChronocharacterRenderer.sprite = ChronoSad;
                    break;
                case 1:

                    PlayercharacterRenderer.sprite = PlayerEmbarassed;
                    break;
            }
        }
        else if (dialogueLines == ImbakNaUlan)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    ChronocharacterRenderer.sprite = ChronoSad;
                    break;
                case 1:
                    PlayercharacterRenderer.sprite = PlayerEmbarassed;
                    break;
            }
        }
        else
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    ChronocharacterRenderer.sprite = ZulNeutral;
                    break;
            }
        }
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
            isShowingTigrisAndEuphratesDialogue = true;
            currentDialogueIndex = 0;
            dialogueLines = TigrisAndEuphrates;
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
                    ShowDialogue();
                    nextButton.onClick.RemoveAllListeners();
                    nextButton.onClick.AddListener(() =>
                    {
                        SceneManager.LoadScene("SumerianSceneFour");
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

            if (selected.text == "Disyerto")
            {
                isShowingDisyertoDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = Disyerto;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextDisyertoDialogue);
            }
            else if (selected.text == "Imbak na ulan")
            {
                isShowingImbakNaUlanDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = ImbakNaUlan;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextImbakNaUlanDialogue);
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

        void ShowNextCuneiformDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextCuneiformDialogue);
            }
        }

        void ShowNextDisyertoDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextDisyertoDialogue);
            }
        }

        void ShowNextImbakNaUlanDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextImbakNaUlanDialogue);
            }
        }
    }
    void LoadGameOverScene()
    {
        SceneManager.LoadScene("GameOver");
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("SumerianSceneFour");
    }
}
