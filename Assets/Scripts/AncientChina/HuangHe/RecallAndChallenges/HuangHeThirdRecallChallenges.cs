using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class HuangHeThirdRecallChallenges : MonoBehaviour
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
    public Sprite PlayerCurious;
    public Sprite PlayerEmabarrassed;
    public Sprite ChronoThinking;
    public Sprite ChronoSmile;
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
    private bool isShowingAlpsDialogue = false;
    private bool isShowingSaharaDialogue = false;
    private bool isShowingHimalayasDialogue = false;
    public AudioSource finishAudioSource;

    public Button ArtifactImageButton;
    public Button ArtifactUseButton;
    public Button ArtifactButton;

    public AudioSource audioSource;
    public AudioClip[] dialogueClips;
    public AudioClip[] HimalayasClips;
    public AudioClip[] AlpsClips;
    public AudioClip[] SaharaClips;

    void Start()
    {
        if (PlayerAchievementManager.IsAchievementUnlocked("Jar"))
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
            Debug.Log("Achievement 'Jar' is not unlocked yet. Button functionality disabled.");
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
                line = " Ano-ano ang mga natural barriers na nagprotekta sa China mula sa mga mananakop?"
            },
        };

        ShowDialogue();
    }
        public void UseArtifactButton()
    {
        ArtifactButton.onClick.AddListener(() =>
        {
            StudentPrefs.SetInt("UseHuangHeArtifactUsed", 1);
            StudentPrefs.Save();
            
            GameState.hearts++;
            UpdateHeartsUI();

            ArtifactButton.gameObject.SetActive(false);
            ArtifactImageButton.gameObject.SetActive(false);

        });
    }

    private DialogueLine[] HimalayasLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Himalayas, Taklamakan, Tibet, at Gobi!"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Tumpak. Ang mga natural barriers na ito ang nagbigay kay China ng proteksyon, at isolation."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Ang proteksyon ay minsan nagiging kulungan. Ang isolation ay minsan nagiging pagmamataas. Tandaan mo yan."
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Parang double-edged sword... blessing at curse."
        },
        new DialogueLine
        {
            characterName = "PLAYER ",
            line = " Panahon na para bumalik."
        },
        new DialogueLine
        {
            characterName = "CHRONO ",
            line = " Pero tandaan mo, ang Yellow River ay hindi tumigil. Hanggang ngayon, umaagos pa rin ito, nagdudulot pa rin ng luha... at pag-asa."
        },

    };
    private DialogueLine[] AlpsLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Ah... Mediterranean Sea at Alps?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Yan ay sa Europe. Ang China ay napapaligiran ng bundok, disyerto, at plateau, hindi dagat."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Pumiling muli."
        }
    };
    private DialogueLine[] SaharaLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Atlantic Ocean at Sahara ba?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Hindi. Yan ay sa Africa at Americas. Ang China ay nasa East Asia, napapaligiran ng ibang uri ng hadlang."
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
            new Answer { text = "Mediterranean Sea at Alps", isCorrect = false },
            new Answer { text = "Atlantic Ocean at Sahara", isCorrect = false },
            new Answer { text = "Himalayas, Taklamakan, Tibet, Gobi", isCorrect = true },
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

            if (dialogueLines == HimalayasLines && HimalayasClips != null && currentDialogueIndex < HimalayasClips.Length)
                clipToPlay = HimalayasClips[currentDialogueIndex];
            else if (dialogueLines == AlpsLines && AlpsClips != null && currentDialogueIndex < AlpsClips.Length)
                clipToPlay = AlpsClips[currentDialogueIndex];
            else if (dialogueLines == SaharaLines && SaharaClips != null && currentDialogueIndex < SaharaClips.Length)
                clipToPlay = SaharaClips[currentDialogueIndex];
            else if (dialogueClips != null && currentDialogueIndex < dialogueClips.Length)
                clipToPlay = dialogueClips[currentDialogueIndex];

            if (clipToPlay != null)
            {
                audioSource.Stop();
                audioSource.clip = clipToPlay;
                audioSource.Play();
            }
        }

        if (dialogueLines == HimalayasLines)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerSmile;
                    ChronocharacterRenderer.sprite = ChronoThinking;

                    if (!challengeCompleted)
                    {
                        challengeCompleted = true;
                        // Overwrite all existing saves to the next scene to prevent going back
                        if (SaveLoadManager.Instance != null)
                        {
                            SaveLoadManager.Instance.OverwriteAllSavesAfterChallenge("HuangHeSceneFive", 0);
                        }
                    }

                    foreach (Button btn in answerButtons)
                    {
                        btn.interactable = false;
                    }
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
                    BlurBG.gameObject.SetActive(false);
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    ChronocharacterRenderer.sprite = ChronoThinking;
                    break;
                case 3:
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    ChronocharacterRenderer.sprite = ChronoSmile;
                    break;
                case 4:
                    PlayercharacterRenderer.sprite = PlayerEager;
                    break;
                case 5:
                    PlayercharacterRenderer.sprite = PlayerEager;
                    ChronocharacterRenderer.sprite = ChronoSmile;
                    break;

            }
        }
        else if (dialogueLines == AlpsLines)
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
        else if (dialogueLines == SaharaLines)
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
                    PlayercharacterRenderer.sprite = PlayerSmile;
                    ChronocharacterRenderer.sprite = ChronoSmile;
                    break;
            }
        }

        if (currentDialogueIndex == 0)
        {
            SetAnswers();
            foreach (Button btn in answerButtons)
            {
                btn.interactable = !(dialogueLines == HimalayasLines && currentDialogueIndex == 0);
                btn.gameObject.SetActive(true);
            }

            // Only hide the Next button for the initial/main dialogue set.
            // If we're displaying one of the special dialogues (Himalayas/Alps/Sahara) that also start at index 0,
            // leave visibility control to the callers who set up those dialogues.
            bool isSpecialDialogue = (dialogueLines == HimalayasLines || dialogueLines == AlpsLines || dialogueLines == SaharaLines);
            if (!isSpecialDialogue)
                nextButton.gameObject.SetActive(false);
        }
        else
        {
            if (currentDialogueIndex == dialogueLines.Length - 1)
            {
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();


                if (isShowingHimalayasDialogue) // ✅ Correct flag for this script
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
            isShowingHimalayasDialogue = true;
            currentDialogueIndex = 0;
            dialogueLines = HimalayasLines;
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

            if (selected.text == "Mediterranean Sea at Alps")
            {
                isShowingAlpsDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = AlpsLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextAlpsDialogue);
            }
            else if (selected.text == "Atlantic Ocean at Sahara")
            {
                isShowingSaharaDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = SaharaLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextSaharaDialogue);
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

        void ShowNextAlpsDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextAlpsDialogue);
            }
        }

        void ShowNextSaharaDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextSaharaDialogue);
            }
        }
    }
    void LoadGameOverScene()
    {
        SceneManager.LoadScene("HuangHeGameOver");
    }

    void PlayCongratsAudio()
    {
        if (finishAudioSource != null)
            finishAudioSource.Play();
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("HuangHeSceneFive");
    }
}