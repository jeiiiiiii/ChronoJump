using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class KingdomSecondRecallChallenges : MonoBehaviour
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
    private bool isShowingauthoritarianDialogue = false;
    private bool isShowingroleDialogue = false;
    private bool isShowingNagingDialogue = false;
    public AudioSource finishAudioSource;

    public Button ArtifactImageButton;
    public Button ArtifactUseButton;
    public Button ArtifactButton;
    public Animator chronoAnimator;
    public Animator playerAnimator;
    public AudioSource audioSource;
    public AudioClip[] dialogueClips;
    public AudioClip[] NagingClips;
    public AudioClip[] authoritarianClips;
    public AudioClip[] roleClips;

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
                line = "   Sino si Fu Hao at bakit siya mahalaga sa Dinastiyang Kingdom?"
            },
        };

        ShowDialogue();
    }
    public void UseArtifactButton()
    {
        if (StudentPrefs.GetInt("UseKingdomArtifactUsed", 0) == 0)
        {
            StudentPrefs.SetInt("UseKingdomArtifactUsed", 1);
            StudentPrefs.Save();

            dialogueLines = new DialogueLine[]
            {
                new DialogueLine
                {
                    characterName = "Hint",
                    line = "Ang sagot ay isang authoritarian na nagsimula sa Akkad at naging kauna-unahang emperor sa kasaysayan. Ang kanyang pangalan ay nagsisimula sa letrang 'S'."
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
                        line = "   Sino si Fu Hao at bakit siya mahalaga sa Dinastiyang Kingdom?"
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

    private DialogueLine[] NagingLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Naging 'shepherd of the people' na nag-focus sa welfare ng bayan!"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Magaling! Ang Gitnang Kaharian ay mas humanitarian. Focus sa irrigation, trade, justice, hindi lang sa personal glory ng pharaoh."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Ang true leadership ay hindi tungkol sa glory, ito ay tungkol sa sacrifice para sa iyong tao."
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Mas inspiring ang leadership style na 'to..."
        },

    };
    private DialogueLine[] authoritarianLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Ano... naging mas powerful at authoritarian ba?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Actually, baliktad. Naging mas compassionate ang leadership style. Ang focus ay nasa service, hindi domination."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Pumiling muli."
        }
    };
    private DialogueLine[] roleLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Nawala ang kanilang religious role... siguro?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Hindi. Nananatili pa rin silang religious leader. Ang nagbago lang ay ang emphasis, from divine ruler to servant leader."
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
            new Answer { text = "Naging \"shepherd of the people\" na nag-focus sa welfare ng bayan", isCorrect = true },
            new Answer { text = "Naging mas powerful at authoritarian", isCorrect = false },
            new Answer { text = "Nawala ang kanilang religious role", isCorrect = false },
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

            if (dialogueLines == NagingLines && NagingClips != null && currentDialogueIndex < NagingClips.Length)
                clipToPlay = NagingClips[currentDialogueIndex];
            else if (dialogueLines == authoritarianLines && authoritarianClips != null && currentDialogueIndex < authoritarianClips.Length)
                clipToPlay = authoritarianClips[currentDialogueIndex];
            else if (dialogueLines == roleLines && roleClips != null && currentDialogueIndex < roleClips.Length)
                clipToPlay = roleClips[currentDialogueIndex];
            else if (dialogueClips != null && currentDialogueIndex < dialogueClips.Length)
                clipToPlay = dialogueClips[currentDialogueIndex];

            if (clipToPlay != null)
            {
                audioSource.Stop();
                audioSource.clip = clipToPlay;
                audioSource.Play();
            }
        }

        if (dialogueLines == NagingLines)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    if (chronoAnimator != null)
                        chronoAnimator.Play("Chrono_Smiling (Idle)", 0, 0f);
                    if (playerAnimator != null)
                        playerAnimator.Play("Player_Talking", 0, 0f);

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
                    if (chronoAnimator != null)
                        chronoAnimator.Play("Chrono_Talking", 0, 0f);
                    if (playerAnimator != null)
                        playerAnimator.Play("Player_Eager", 0, 0f);
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
                    if (chronoAnimator != null)
                        chronoAnimator.Play("Chrono_Talking", 0, 0f);
                    if (playerAnimator != null)
                        playerAnimator.Play("Player_Reflective", 0, 0f);
                    break;
                case 3:
                    if (chronoAnimator != null)
                        chronoAnimator.Play("Chrono_Cheerful", 0, 0f);
                    if (playerAnimator != null)
                        playerAnimator.Play("Player_Talking", 0, 0f);
                    break;

            }
        }
        else if (dialogueLines == authoritarianLines)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    if (chronoAnimator != null)
                        chronoAnimator.Play("Chrono_Sad", 0, 0f);
                    if (playerAnimator != null)
                        playerAnimator.Play("Player_Talking", 0, 0f);
                    break;
                case 1:
                    if (chronoAnimator != null)
                        chronoAnimator.Play("Chrono_Talking", 0, 0f);
                    if (playerAnimator != null)
                        playerAnimator.Play("Player_Embarassed", 0, 0f);
                    break;
                case 2:
                    if (chronoAnimator != null)
                        chronoAnimator.Play("Chrono_Talking", 0, 0f);
                    if (playerAnimator != null)
                        playerAnimator.Play("Player_Reflective", 0, 0f);
                    break;
            }
        }
        else if (dialogueLines == roleLines)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    if (chronoAnimator != null)
                        chronoAnimator.Play("Chrono_Sad", 0, 0f);
                    if (playerAnimator != null)
                        playerAnimator.Play("Player_Talking", 0, 0f);
                    break;
                case 1:
                    if (chronoAnimator != null)
                        chronoAnimator.Play("Chrono_Talking", 0, 0f);
                    if (playerAnimator != null)
                        playerAnimator.Play("Player_Embarassed", 0, 0f);
                    break;
                case 2:
                    if (chronoAnimator != null)
                        chronoAnimator.Play("Chrono_Talking", 0, 0f);
                    if (playerAnimator != null)
                        playerAnimator.Play("Player_Reflective", 0, 0f);
                    break;
            }
        }
        else
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    if (chronoAnimator != null)
                        chronoAnimator.Play("Chrono_Talking", 0, 0f);
                    if (playerAnimator != null)
                        playerAnimator.Play("Player_Reflective", 0, 0f);
                    break;
            }
        }

        if (currentDialogueIndex == 0)
        {
            SetAnswers();
            foreach (Button btn in answerButtons)
            {
                btn.interactable = !(dialogueLines == NagingLines && currentDialogueIndex == 0);
                btn.gameObject.SetActive(true);
            }

            // Only hide the Next button for the initial/main dialogue set.
            // If we're displaying one of the special dialogues (Naging/authoritarian/role) that also start at index 0,
            // leave visibility control to the callers who set up those dialogues.
            bool isSpecialDialogue = (dialogueLines == NagingLines || dialogueLines == authoritarianLines || dialogueLines == roleLines);
            if (!isSpecialDialogue)
                nextButton.gameObject.SetActive(false);
        }
        else
        {
            if (currentDialogueIndex == dialogueLines.Length - 1)
            {
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();


                if (isShowingNagingDialogue) // ✅ Correct flag for this script
                {
                    nextButton.interactable = false;

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
            isShowingNagingDialogue = true;
            currentDialogueIndex = 0;
            dialogueLines = NagingLines;
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

            if (selected.text == "Naging mas powerful at authoritarian")
            {
                isShowingauthoritarianDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = authoritarianLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextauthoritarianDialogue);
            }
            else if (selected.text == "Nawala ang kanilang religious role")
            {
                isShowingroleDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = roleLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextroleDialogue);
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

        void ShowNextauthoritarianDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextauthoritarianDialogue);
            }
        }

        void ShowNextroleDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextroleDialogue);
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
        SceneManager.LoadScene("KingdomSceneThree");
    }
}