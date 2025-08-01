using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class BabylonianSecondRecallChallenges : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueLine
    {
        public string characterName;
        public string line;
    }

    public SpriteRenderer PlayercharacterRenderer;
    public SpriteRenderer ChronocharacterRenderer;
    public SpriteRenderer BlurBG;
    
    public Sprite PlayerSmile;
    public Sprite PlayerEager;
    public Sprite PlayerReflective;
    public Sprite PlayerEmbarassed;
    public Sprite ChronoThinking;
    public Sprite ChronoCheerful;
    public Sprite ChronoSad;
    public Sprite ChronoSmile;

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
    private bool isShowingKatarunganDialogue = false;
    private bool isShowingKalakalanDialogue = false;
    private bool isShowingKodigoDialogue = false;
    public AudioSource finishAudioSource;

    void Start()
    {
        nextButton.gameObject.SetActive(false);

        if (GameState.hearts <= 0)
            GameState.hearts = 3;

        UpdateHeartsUI();

        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ano ang tawag sa hanay ng batas na ipinatupad ni Hammurabi upang mapanatili ang kaayusan sa Babylonia?"
            },
        };

        ShowDialogue();
    }

    private DialogueLine[] KodigoLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Kodigo ni Hammurabi"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Tumpak. Ito ang ginamit niyang batayan sa pamumuno."
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Ang dami kong natutunan… pero parang may mas malaki pa siyang plano."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Napansin mo rin? Tara, silipin natin ang susunod na yugto sa kanyang pamumuno."
        },

    };
    private DialogueLine[] KatarunganLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = "Kodigo ng Katarungan ni Hammurabi?" },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Malapit na, pero hindi iyan ang tamang tawag."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Pumiling muli."
        },

    };
    private DialogueLine[] KalakalanLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = "Batas sa Kalakalan ni Hammurabi?" },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Hindi lang sa kalakalan umiikot ang mga batas na ito."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Pumiling muli."
        }
    };

    void SetAnswers()
    {
        answers = new Answer[]
        {
            new Answer { text = "Kodigo ni Hammurabi", isCorrect = true },
            new Answer { text = "Kodigo ng Katarungan ni Hammurabi", isCorrect = false },
            new Answer { text = "Batas sa Kalakalan ni Hammurabi", isCorrect = false },
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

        if (dialogueLines == KodigoLines)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerSmile;
                    ChronocharacterRenderer.sprite = ChronoCheerful;
                    break;
                case 1:
                    PlayercharacterRenderer.sprite = PlayerEager;
                    ChronocharacterRenderer.sprite = ChronoSmile;
                    break;
                case 2:
                    foreach (Button btn in answerButtons)
                    {
                    btn.gameObject.SetActive(false);
                    }
                    foreach (Image heart in heartImages)
                    {
                    heart.gameObject.SetActive(false);
                    }
                    if (BlurBG != null)
                    BlurBG.gameObject.SetActive(false);
                    
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    ChronocharacterRenderer.sprite = ChronoThinking;
                    break;
                case 3:
                    PlayercharacterRenderer.sprite = PlayerSmile;
                    ChronocharacterRenderer.sprite = ChronoSmile;
                    break;
            }
        }
        else if (dialogueLines == KatarunganLines)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerEmbarassed;
                    ChronocharacterRenderer.sprite = ChronoSad;
                    break;
                case 1:
                    PlayercharacterRenderer.sprite = PlayerEmbarassed;
                    ChronocharacterRenderer.sprite = ChronoSad;
                    break;
                case 2:
                    ChronocharacterRenderer.sprite = ChronoThinking;
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    break;
            }
        }
        else if (dialogueLines == KalakalanLines)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerEmbarassed;
                    ChronocharacterRenderer.sprite = ChronoSad;
                    break;
                case 1:
                    PlayercharacterRenderer.sprite = PlayerEmbarassed;
                    ChronocharacterRenderer.sprite = ChronoSad;
                    break;
                case 2:
                    ChronocharacterRenderer.sprite = ChronoThinking;
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    break;
            }
        }
        else
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    ChronocharacterRenderer.sprite = ChronoThinking;
                    break;
            }
        }

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

                if (isShowingKodigoDialogue)
                {
                    if (finishAudioSource != null)
                        finishAudioSource.Play();
                    nextButton.interactable = false;
                    Invoke(nameof(LoadNextScene), 2f);
                }
                else
                {
                    nextButton.onClick.AddListener(() =>
                    {
                        currentDialogueIndex = 0;
                        ShowDialogue();
                    });
                }
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
        foreach (Button btn in answerButtons)
            btn.interactable = false;

        if (selected.isCorrect)
        {
            isShowingKodigoDialogue = true;
            currentDialogueIndex = 0;
            dialogueLines = KodigoLines;
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
                        SceneManager.LoadScene("BabylonianSceneThree");
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
                dialogueText.text = "<b>CHRONO</b>: Uliting muli. Wala ka nang natitirang puso.";
                nextButton.gameObject.SetActive(false);
                foreach (Button btn in answerButtons)
                    btn.interactable = false;
                Invoke(nameof(LoadGameOverScene), 2f);
                return;
            }

            if (selected.text == "Kodigo ng Katarungan ni Hammurabi")
            {
                isShowingKatarunganDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = KatarunganLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextKatarunganDialogue);
            }
            else if (selected.text == "Batas sa Kalakalan ni Hammurabi")
            {
                isShowingKalakalanDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = KalakalanLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextKalakalanDialogue);
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

        void ShowNextKodigoDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextKodigoDialogue);
            }
        }

        void ShowNextKatarunganDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextKatarunganDialogue);
            }
        }

        void ShowNextKalakalanDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextKalakalanDialogue);
            }
        }
    }
    void LoadGameOverScene()
    {
        SceneManager.LoadScene("GameOver");
    }
    
    void LoadNextScene()
    {
        SceneManager.LoadScene("BabylonianSceneThree");
    }

}