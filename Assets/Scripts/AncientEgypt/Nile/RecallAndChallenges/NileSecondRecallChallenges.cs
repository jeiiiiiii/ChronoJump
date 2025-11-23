using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class NileSecondRecallChallenges : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueLine
    {
        public string characterName;
        public string line;
    }

    public SpriteRenderer PlayercharacterRenderer;
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite PlayerReflective;
    public Sprite PlayerEager;
    public Sprite PlayerSmile;
    public Sprite PlayerEmabarrassed;
    public Sprite ChronoThinking;
    public Sprite ChronoSmile;
    public Sprite ChronoCheerful;
    public Sprite ChronoSad;
    public SpriteRenderer BlurBG;

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
    private bool challengeCompleted = false;

    public Image[] heartImages;
    private bool isShowingSeaDialogue = false;
    private bool isShowingbundokDialogue = false;
    private bool isShowingMalawakDialogue = false;
    public AudioSource finishAudioSource;

    public Button ArtifactImageButton;
    public Button ArtifactUseButton;
    public Button ArtifactButton;

    public AudioSource audioSource;
    public AudioClip[] dialogueClips;
    public AudioClip[] MalawakClips;
    public AudioClip[] SeaClips;
    public AudioClip[] bundokClips;

    void Start()
    {
        if (PlayerAchievementManager.IsAchievementUnlocked("Tablet"))
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
            Debug.Log("Achievement 'Tablet' is not unlocked yet. Button functionality disabled.");
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
                line = " Ano ang natural barrier na nagprotekta sa Egypt mula sa mga invaders?"
            },
        };

        ShowDialogue();
    }
    public void UseArtifactButton()
    {
        if (StudentPrefs.GetInt("UseNileArtifactUsed", 0) == 0)
        {
            StudentPrefs.SetInt("UseNileArtifactUsed", 1);
            StudentPrefs.Save();

            dialogueLines = new DialogueLine[]
            {
                new DialogueLine
                {
                    characterName = "Hint",
                    line = "Ang sagot ay isang hari na nagsimula sa Akkad at naging kauna-unahang emperor sa kasaysayan. Ang kanyang pangalan ay nagsisimula sa letrang 'S'."
                },
            };

            currentDialogueIndex = 0;
            ShowDialogue();
            nextButton.gameObject.SetActive(true);
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() =>
            {
                dialogueLines = new DialogueLine[]
                {
                    new DialogueLine
                    {
                        characterName = "CHRONO",
                        line = " Ano ang natural barrier na nagprotekta sa Egypt mula sa mga invaders?"
                    },
                };
                
                currentDialogueIndex = 0;
                ShowDialogue();
            });

            ArtifactUseButton.gameObject.SetActive(false);
            ArtifactImageButton.gameObject.SetActive(false);
            
            Debug.Log("Artifact hint used!");
        }
        else
        {
            ArtifactUseButton.gameObject.SetActive(false);
        }
    }

    private DialogueLine[] MalawakLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Malawak na disyerto sa magkabilang panig ng Nile!"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Magaling! Ang Sahara Desert sa kanluran at Arabian Desert sa silangan ay natural na proteksyon. Mahirap tumawid doon, kaya ang Egypt ay relatively isolated."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Ang isolation ay blessing at curse. Protektado ka, pero limited din ang interaction mo sa ibang kultura."
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Parang trade-off... security over diversity..."
        },

    };
    private DialogueLine[] SeaLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Ano... Mediterranean Sea at Red Sea ba?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Ang mga dagat ay naging trade routes, hindi barriers. Ang tunay na proteksyon ay ang disyerto, walang mananakop na dadaan doon."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Pumiling muli."
        }
    };
    private DialogueLine[] bundokLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Mataas na bundok... siguro?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Walang malalaking bundok sa paligid ng Egypt. Ang pangunahing barrier ay ang tuyong disyerto sa magkabilang panig."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Pumiling muli."
        }
    };

    void SetAnswers()
    {
        answers = new Answer[]
        {
            new Answer { text = "Malawak na disyerto sa magkabilang panig ng Nile", isCorrect = true },
            new Answer { text = "Mediterranean Sea at Red Sea", isCorrect = false },
            new Answer { text = "Mataas na bundok", isCorrect = false },
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

        if (audioSource != null)
        {
            AudioClip clipToPlay = null;

            if (dialogueLines == MalawakLines && MalawakClips != null && currentDialogueIndex < MalawakClips.Length)
                clipToPlay = MalawakClips[currentDialogueIndex];
            else if (dialogueLines == SeaLines && SeaClips != null && currentDialogueIndex < SeaClips.Length)
                clipToPlay = SeaClips[currentDialogueIndex];
            else if (dialogueLines == bundokLines && bundokClips != null && currentDialogueIndex < bundokClips.Length)
                clipToPlay = bundokClips[currentDialogueIndex];
            else if (dialogueClips != null && currentDialogueIndex < dialogueClips.Length)
                clipToPlay = dialogueClips[currentDialogueIndex];

            if (clipToPlay != null)
            {
                audioSource.Stop();
                audioSource.clip = clipToPlay;
                audioSource.Play();
            }
        }

        if (dialogueLines == MalawakLines)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerEager;
                    ChronocharacterRenderer.sprite = ChronoSmile;

                    if (!challengeCompleted)
                    {
                        challengeCompleted = true;
                        // Overwrite all existing saves to the next scene to prevent going back
                        if (SaveLoadManager.Instance != null)
                        {
                            SaveLoadManager.Instance.OverwriteAllSavesAfterChallenge("NileSceneThree", 0);
                        }
                    }

                    foreach (Button btn in answerButtons)
                    {
                        btn.interactable = false;
                    }
                    break;
                case 1:
                    PlayercharacterRenderer.sprite = PlayerSmile;
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
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    ChronocharacterRenderer.sprite = ChronoSmile;
                    break;
                case 3:
                    PlayercharacterRenderer.sprite = PlayerEager;
                    break;

            }
        }
        else if (dialogueLines == SeaLines)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerEager;
                    ChronocharacterRenderer.sprite = ChronoSmile;
                    break;
                case 1:
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    ChronocharacterRenderer.sprite = ChronoThinking;
                    break;
                case 2:
                    PlayercharacterRenderer.sprite = PlayerEmabarrassed;
                    ChronocharacterRenderer.sprite = ChronoSad;
                    break;
            }
        }
        else if (dialogueLines == bundokLines)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerEager;
                    ChronocharacterRenderer.sprite = ChronoSmile;
                    break;
                case 1:
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    ChronocharacterRenderer.sprite = ChronoThinking;
                    break;
                case 2:
                    PlayercharacterRenderer.sprite = PlayerEmabarrassed;
                    ChronocharacterRenderer.sprite = ChronoSad;
                    break;
            }
        }
        else
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerEager;
                    ChronocharacterRenderer.sprite = ChronoSmile;
                    break;
            }
        }

        if (currentDialogueIndex == 0)
        {
            SetAnswers();
            foreach (Button btn in answerButtons)
            {
                btn.interactable = !(dialogueLines == MalawakLines && currentDialogueIndex == 0);
                btn.gameObject.SetActive(true);
            }

            // Only hide the Next button for the initial/main dialogue set.
            // If we're displaying one of the special dialogues (Malawak/Sea/bundok) that also start at index 0,
            // leave visibility control to the callers who set up those dialogues.
            bool isSpecialDialogue = (dialogueLines == MalawakLines || dialogueLines == SeaLines || dialogueLines == bundokLines);
            if (!isSpecialDialogue)
                nextButton.gameObject.SetActive(false);
        }
        else
        {
            if (currentDialogueIndex == dialogueLines.Length - 1)
            {
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();


                if (isShowingMalawakDialogue) // ✅ Correct flag for this script
                {
                    nextButton.interactable = false;
                    // Should be change
                    nextButton.interactable = false;
                    Invoke(nameof(LoadNextScene), 2f);

                    // Calculate dialogue audio duration
                    float dialogueDelay = 0f;
                    if (audioSource != null && audioSource.clip != null)
                    {
                        dialogueDelay = audioSource.clip.length;
                    }
                    else
                    {
                        dialogueDelay = 2f; // Default if no dialogue audio
                    }

                    // Play congrats audio AFTER dialogue finishes
                    Invoke(nameof(PlayCongratsAudio), dialogueDelay);

                    // Calculate total delay (dialogue + congrats + buffer)
                    float congratsDelay = 0f;
                    if (finishAudioSource != null && finishAudioSource.clip != null)
                    {
                        congratsDelay = finishAudioSource.clip.length;
                    }
                    else
                    {
                        congratsDelay = 2f; // Default if no congrats audio
                    }

                    float totalDelay = dialogueDelay + congratsDelay + 1f; // Total time + buffer
                    Invoke(nameof(LoadNextScene), totalDelay);
                }
                else
                {
                    // For wrong answers, keep original logic
                    nextButton.onClick.AddListener(() =>
                {
                    if (finishAudioSource != null)
                        finishAudioSource.Play();
                    nextButton.interactable = false;
                    Invoke(nameof(LoadNextScene), 2f);
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
            isShowingMalawakDialogue = true;
            currentDialogueIndex = 0;
            dialogueLines = MalawakLines;
            ShowDialogue();

            nextButton.gameObject.SetActive(true);
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() =>
            {
                currentDialogueIndex++;
                ShowDialogue();
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

            if (selected.text == "Mediterranean Sea at Red Sea")
            {
                isShowingSeaDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = SeaLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextSeaDialogue);
            }
            else if (selected.text == "Mataas na bundok")
            {
                isShowingbundokDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = bundokLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextbundokDialogue);
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

        void ShowNextSeaDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextSeaDialogue);
            }
        }

        void ShowNextbundokDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextbundokDialogue);
            }
        }
    }
    void LoadGameOverScene()
    {
        SceneManager.LoadScene("NileGameOver");
    }

    void PlayCongratsAudio()
    {
        if (finishAudioSource != null)
            finishAudioSource.Play();
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("NileSceneThree");
    }
}