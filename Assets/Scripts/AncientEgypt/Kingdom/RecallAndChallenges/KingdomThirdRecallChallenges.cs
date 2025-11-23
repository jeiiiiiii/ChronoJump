using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class KingdomThirdRecallChallenges : MonoBehaviour
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
    private bool isShowingbuildingDialogue = false;
    private bool isShowingbansaDialogue = false;
    private bool isShowingimperyoDialogue = false;
    public AudioSource finishAudioSource;

    public Button ArtifactImageButton;
    public Button ArtifactUseButton;
    public Button ArtifactButton;

    public AudioSource audioSource;
    public AudioClip[] dialogueClips;
    public AudioClip[] imperyoClips;
    public AudioClip[] buildingClips;
    public AudioClip[] bansaClips;
        private bool artifactPowerActivated = false;

    void Start()
    {
        if (PlayerAchievementManager.IsAchievementUnlocked("Papyrus"))
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
            Debug.Log("Achievement 'Papyrus' is not unlocked yet. Button functionality disabled.");
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
                line = " Ano ang \"Mandate of Heaven\" at bakit ito mahalaga sa Chinese dynasties?"
            },
        };

        ShowDialogue();
    }
        public void UseArtifactButton()
    {
        ArtifactButton.onClick.AddListener(() =>
        {
            artifactPowerActivated = true;
            
            ArtifactButton.interactable = false;
            ArtifactImageButton.interactable = false;

        });
    }

    void ConsumeArtifactPower()
    {
        StudentPrefs.SetInt("UsePowerArtifactUsed", 1);
        StudentPrefs.Save();
        
        ArtifactButton.gameObject.SetActive(false);
        ArtifactImageButton.gameObject.SetActive(false);
        
        artifactPowerActivated = false;
        
        Debug.Log("Artifact power consumed! Protected from heart loss.");
    }

    private DialogueLine[] imperyoLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Expansionist at militaristic, naging imperyo!"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Tumpak. Ang Bagong Kaharian ay naging aggressive power. Sumakop sila ng neighboring territories at naging international superpower."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Ang tatlong kingdoms ay tatlong lessons: Lumang Kaharian, ang glory may presyo. Gitnang Kaharian, ang service ay mas important kaysa glory. Bagong Kaharian, ang power ay temporary."
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Parang... rise, fall, rise, fall... yan ang cycle ng lahat ng civilizations..."
        },
        new DialogueLine
        {
            characterName = "PLAYER ",
            line = " Panahon na para bumalik."
        },
        new DialogueLine
        {
            characterName = "CHRONO ",
            line = " Pero tandaan mo, ang Egypt ay tumagal ng 3,000 years. Tatlong golden ages, maraming intermediate periods. Pero ang legacy ay nananatili, pyramids, hieroglyphics, mummies, mythology."
        },

    };
    private DialogueLine[] buildingLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Ah... focus sa pyramid building?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Yan ay sa Lumang Kaharian. Sa Bagong Kaharian, ang focus ay military expansion at wealth accumulation."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Pumiling muli."
        }
    };
    private DialogueLine[] bansaLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Complete isolation mula sa ibang bansa ba?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Baliktad. Ang Bagong Kaharian ay peak ng international relations, may trade, diplomacy, at warfare sa buong region."
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
            new Answer { text = "Focus sa pyramid building", isCorrect = false },
            new Answer { text = "Complete isolation mula sa ibang bansa", isCorrect = false },
            new Answer { text = "Expansionist at militaristic, naging imperyo ", isCorrect = true },
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

            if (dialogueLines == imperyoLines && imperyoClips != null && currentDialogueIndex < imperyoClips.Length)
                clipToPlay = imperyoClips[currentDialogueIndex];
            else if (dialogueLines == buildingLines && buildingClips != null && currentDialogueIndex < buildingClips.Length)
                clipToPlay = buildingClips[currentDialogueIndex];
            else if (dialogueLines == bansaLines && bansaClips != null && currentDialogueIndex < bansaClips.Length)
                clipToPlay = bansaClips[currentDialogueIndex];
            else if (dialogueClips != null && currentDialogueIndex < dialogueClips.Length)
                clipToPlay = dialogueClips[currentDialogueIndex];

            if (clipToPlay != null)
            {
                audioSource.Stop();
                audioSource.clip = clipToPlay;
                audioSource.Play();
            }
        }

        if (dialogueLines == imperyoLines)
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
                            SaveLoadManager.Instance.OverwriteAllSavesAfterChallenge("KingdomSceneTwo", 0);
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
        else if (dialogueLines == buildingLines)
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
        else if (dialogueLines == bansaLines)
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
                btn.interactable = !(dialogueLines == imperyoLines && currentDialogueIndex == 0);
                btn.gameObject.SetActive(true);
            }

            // Only hide the Next button for the initial/main dialogue set.
            // If we're displaying one of the special dialogues (imperyo/building/bansa) that also start at index 0,
            // leave visibility control to the callers who set up those dialogues.
            bool isSpecialDialogue = (dialogueLines == imperyoLines || dialogueLines == buildingLines || dialogueLines == bansaLines);
            if (!isSpecialDialogue)
                nextButton.gameObject.SetActive(false);
        }
        else
        {
            if (currentDialogueIndex == dialogueLines.Length - 1)
            {
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();


                if (isShowingimperyoDialogue) // ✅ Correct flag for this script
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
            isShowingimperyoDialogue = true;
            currentDialogueIndex = 0;
            dialogueLines = imperyoLines;
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
            if (artifactPowerActivated)
                {
                    ConsumeArtifactPower();
                }
            else
                {
                    GameState.hearts--;
                    UpdateHeartsUI();
                }

            if (GameState.hearts <= 0)
            {
                dialogueText.text = "<b>CHRONO</b>: Uliting muli. Wala ka nang natitirang puso.";
                nextButton.gameObject.SetActive(false);
                foreach (Button btn in answerButtons)
                    btn.interactable = false;
                Invoke(nameof(LoadGameOverScene), 2f);
                return;
            }

            if (selected.text == "Focus sa pyramid building")
            {
                isShowingbuildingDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = buildingLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextbuildingDialogue);
            }
            else if (selected.text == "Complete isolation mula sa ibang bansa")
            {
                isShowingbansaDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = bansaLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextbansaDialogue);
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

        void ShowNextbuildingDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextbuildingDialogue);
            }
        }

        void ShowNextbansaDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextbansaDialogue);
            }
        }
    }
    void LoadGameOverScene()
    {
        SceneManager.LoadScene("KingdomGameOver");
    }

    void PlayCongratsAudio()
    {
        if (finishAudioSource != null)
            finishAudioSource.Play();
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("KingdomSceneFive");
    }
}