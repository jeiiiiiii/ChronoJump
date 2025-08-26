using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class AssyrianThirdRecallChallenges : MonoBehaviour
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
    private bool isShowingEgyptianDialogue = false;
    private bool isShowingRomansDialogue = false;
    private bool isShowingPersianDialogue = false;
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
    public Button ArtifactImageButton;
    public Button ArtifactUseButton;
    public Button ArtifactButton;

    private bool artifactPowerActivated = false;

    void Start()
    {
        if (PlayerAchievementManager.IsAchievementUnlocked("Stone"))
        {
            if (ArtifactImageButton != null)
            {
                ArtifactImageButton.onClick.AddListener(() =>
                {
                    ArtifactUseButton.gameObject.SetActive(!ArtifactUseButton.gameObject.activeInHierarchy);
                    ArtifactImageButton.gameObject.SetActive(false);
                });
            }
            
            if (ArtifactUseButton != null)
            {
                ArtifactUseButton.onClick.AddListener(UseArtifactButton);
            }
        }
        else
        {
            if (ArtifactImageButton != null)
            {
                ArtifactImageButton.gameObject.SetActive(false);
            }
            Debug.Log("Achievement 'Stone' is not unlocked yet. Button functionality disabled.");
        }

        nextButton.gameObject.SetActive(false);

        if (GameState.hearts <= 0)
            GameState.hearts = 3;

        UpdateHeartsUI();

        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Sino-sino ang nagtulungan upang pabagsakin ang Assyria?"
            },
        };

        ShowDialogue();
    }

    private DialogueLine[] PersianLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Chaldean, Medes, at Persian ang nagtulungan."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Magaling. Tanda ng isang matalas na isipan."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Ang mga pinaghaharian ay minsang nagkaisa—at ang dambuhala ay bumagsak."
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Panahon na para bumalik."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Pero huwag kang mag-alala... marami pa tayong kwento sa hinaharap."
        },
    };

    private DialogueLine[] EgyptianLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Sumerian, Akkadian… at Egyptian ba"
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Hindi!. May mga mas aktibong nagtulungan noon."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Pumiling muli."
        }
    };

    private DialogueLine[] RomansLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Ah… Babylonians, Greeks, at Romans?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Hmm... Mukhang kailangan pa nating balikan ang bahaging iyon."
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
            new Answer { text = "Chaldean, Medes, Persian", isCorrect = true },
            new Answer { text = "Babylonians, Greeks, Romans", isCorrect = false },
            new Answer { text = "Sumerian, Akkadian, Egyptian", isCorrect = false },
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

        if (dialogueLines == PersianLines)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerEager;
                    ChronocharacterRenderer.sprite = ChronoSmile;
                    foreach (Button btn in answerButtons)
                    {
                        btn.interactable = false;
                    }
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
                    break;
                case 3:
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    break;
                case 4:
                    ChronocharacterRenderer.sprite = ChronoSmile;
                    PlayercharacterRenderer.sprite = PlayerSmile;
                    break;
            }
        }
        else if (dialogueLines == EgyptianLines)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    ChronocharacterRenderer.sprite = ChronoThinking;
                    break;
                case 1:
                    PlayercharacterRenderer.sprite = PlayerEmbarassed;
                    ChronocharacterRenderer.sprite = ChronoSad;
                    break;
                case 2:
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    ChronocharacterRenderer.sprite = ChronoThinking;
                    break;
            }
        }
        else if (dialogueLines == RomansLines)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    ChronocharacterRenderer.sprite = ChronoThinking;
                    break;
                case 1:
                    PlayercharacterRenderer.sprite = PlayerEmbarassed;
                    ChronocharacterRenderer.sprite = ChronoSad;
                    break;
                case 2:
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    ChronocharacterRenderer.sprite = ChronoThinking;
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
                btn.interactable = !(dialogueLines == PersianLines && currentDialogueIndex == 0);
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

                if (isShowingPersianDialogue)
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
            isShowingPersianDialogue = true;
            currentDialogueIndex = 0;
            dialogueLines = PersianLines;
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
                        SceneManager.LoadScene("AssyrianSceneFive");
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

            if (selected.text == "Sumerian, Akkadian, Egyptian")
            {
                isShowingEgyptianDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = EgyptianLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextEgyptianDialogue);
            }
            else if (selected.text == "Babylonians, Greeks, Romans")
            {
                isShowingRomansDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = RomansLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextRomansDialogue);
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

        void ShowNextPersianDialogue()
        {
            currentDialogueIndex++;
            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextPersianDialogue);
            }
        }

        void ShowNextEgyptianDialogue()
        {
            currentDialogueIndex++;
            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextEgyptianDialogue);
            }
        }

        void ShowNextRomansDialogue()
        {
            currentDialogueIndex++;
            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextRomansDialogue);
            }
        }
    }
    public void UseArtifactButton()
    {
        ArtifactButton.onClick.AddListener(() =>
        {
            PlayerPrefs.SetInt("UseAssyrianArtifactUsed", 1);
            PlayerPrefs.Save();
            
            GameState.hearts++;
            UpdateHeartsUI();

            ArtifactButton.gameObject.SetActive(false);
            ArtifactImageButton.gameObject.SetActive(false);

        });
    }


    void LoadGameOverScene()
    {
        SceneManager.LoadScene("GameOver");
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("AssyrianSceneFive");
    }
}
