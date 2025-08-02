using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class BabylonianFirstRecallChallenges : MonoBehaviour
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
    private bool isShowingSargonDialogue = false;
    private bool isShowingAshurbanipalDialogue = false;
    private bool isShowingHammurabiDialogue = false;
    public AudioSource finishAudioSource;

    public SpriteRenderer PlayercharacterRenderer;
    public SpriteRenderer ChronocharacterRenderer;

    public Sprite PlayerSmile;
    public Sprite PlayerEager;
    public Sprite PlayerReflective;
    public Sprite PlayerEmbarassed;

    public Sprite ChronoThinking;
    public Sprite ChronoCheerful;
    public Sprite ChronoSad;
    public Sprite ChronoSmile;
    public SpriteRenderer BlurBG;

    void Start()
    {
        nextButton.gameObject.SetActive(false);

        GameState.ResetHearts();

        if (GameState.hearts <= 0)
            GameState.hearts = 3;

        UpdateHeartsUI();

        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Sino ang tinaguriang pinakadakilang hari ng Babylonia?"
            },
        };

        ShowDialogue();
    }

    private DialogueLine[] HammurabiLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Hammurabi!"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Tama. Siya ang bumuo ng Imperyong Babylonia."
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Ang galing… parang ngayon lang ako nakarinig ng hari na gumawa ng mga batas para sa lahat."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Kung gano’n, mas lalo kang mamamangha sa ipapakita ko sa’yo ngayon. Tara, silipin natin mismo ang kanyang ipinagmamalaking mga batas."
        },
    };
    private DialogueLine[] SargonLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Sargon I?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Si Sargon ay mula sa Akkad. Ibang panahon at lugar iyon."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Pumiling muli."
        }
    };
    private DialogueLine[] AshurbanipalLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Ashurbanipal?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Si Ashurbanipal ay hari ng Assyria, hindi ng Babylonia."
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
            new Answer { text = "Sargon I", isCorrect = false },
            new Answer { text = "Hammurabi", isCorrect = true },
            new Answer { text = "Ashurbanipal", isCorrect = false },
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

        if (dialogueLines == HammurabiLines)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerEager;
                    ChronocharacterRenderer.sprite = ChronoSmile;
                    break;
                case 1:
                    ChronocharacterRenderer.sprite = ChronoCheerful;
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
                    BlurBG.gameObject.SetActive(false);
                    ChronocharacterRenderer.sprite = ChronoThinking;
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    break;
                case 3:
                    ChronocharacterRenderer.sprite = ChronoSmile;
                    PlayercharacterRenderer.sprite = PlayerSmile;
                    break;
            }
        }
        else if (dialogueLines == SargonLines)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    ChronocharacterRenderer.sprite = ChronoThinking;
                    break;
                case 1:
                    ChronocharacterRenderer.sprite = ChronoSad;
                    PlayercharacterRenderer.sprite = PlayerEmbarassed;
                    break;
                case 2:
                    ChronocharacterRenderer.sprite = ChronoThinking;
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    break;
            }
        }
        else if (dialogueLines == AshurbanipalLines)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    ChronocharacterRenderer.sprite = ChronoThinking;
                    break;
                case 1:
                    ChronocharacterRenderer.sprite = ChronoSad;
                    PlayercharacterRenderer.sprite = PlayerEmbarassed;
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

                if (isShowingHammurabiDialogue)
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
            isShowingHammurabiDialogue = true;
            currentDialogueIndex = 0;
            dialogueLines = HammurabiLines;
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
                        SceneManager.LoadScene("BabylonianSceneTwo");
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

            if (selected.text == "Sargon I")
            {
                isShowingSargonDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = SargonLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextSargonDialogue);
            }
            else if (selected.text == "Ashurbanipal")
            {
                isShowingAshurbanipalDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = AshurbanipalLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextAshurbanipalDialogue);
            }
            else
            {
                foreach (Button btn in answerButtons)
                    btn.interactable = true;

                nextButton.gameObject.SetActive(false);
                hasAnswered = false;
            }
        }

        void ShowNextSargonDialogue()
        {
            currentDialogueIndex++;
            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextSargonDialogue);
            }
        }

        void ShowNextAshurbanipalDialogue()
        {
            currentDialogueIndex++;
            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextAshurbanipalDialogue);
            }
        }
    }

    void LoadGameOverScene()
    {
        SceneManager.LoadScene("GameOver");
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("BabylonianSceneTwo");
    }
}    