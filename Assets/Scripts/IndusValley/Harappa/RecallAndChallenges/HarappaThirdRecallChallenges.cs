using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class HarappaThirdRecallChallenges : MonoBehaviour
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
    private bool isShowingAryanDialogue = false;
    private bool isShowinglungsodDialogue = false;
    private bool isShowingPagbabagoDialogue = false;
    public AudioSource finishAudioSource;

    public Button ArtifactImageButton;
    public Button ArtifactUseButton;
    public Button ArtifactButton;

    public AudioSource audioSource;
    public AudioClip[] dialogueClips;
    public AudioClip[] PagbabagoClips;
    public AudioClip[] AryanClips;
    public AudioClip[] lungsodClips;

    void Start()
    {
        if (PlayerAchievementManager.IsAchievementUnlocked("Clay"))
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
            Debug.Log("Achievement 'Clay' is not unlocked yet. Button functionality disabled.");
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
                line = " Bakit hindi pa rin nababasa ang Indus script hanggang ngayon?"
            },
        };

        ShowDialogue();
    }
        public void UseArtifactButton()
    {
        ArtifactButton.onClick.AddListener(() =>
        {
            StudentPrefs.SetInt("UseHarappaArtifactUsed", 1);
            StudentPrefs.Save();

            answerButtons[0].interactable = false;

            ArtifactButton.gameObject.SetActive(false);
            ArtifactImageButton.gameObject.SetActive(false);

        });
    }

    private DialogueLine[] PagbabagoLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Pagbabago ng klima at pagkatuyo ng ilog ang dahilan."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Magaling. Ang kalikasan ay mas malakas pa sa pinakamataas na imperyo."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Ang sibilisasyon ay naglaho, ngunit ang aral ay nananatili. Ang tunay na lakas ay hindi nasa espada, kundi sa dunong at kalikasan."
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Parang nagsara ang isang aklat... ngunit may mga pahina pang naiwan."
        },
        new DialogueLine
        {
            characterName = "PLAYER ",
            line = " Panahon na para bumalik."
        },
        new DialogueLine
        {
            characterName = "CHRONO ",
            line = " Pero huwag kang mag-alala... marami pa tayong kwento sa hinaharap."
        },

    };
    private DialogueLine[] AryanLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Ah... pananakop ng mga Aryan?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Yan ay isang lumang teorya, ngunit ang mas malinaw na dahilan ay ang pagbabago ng kapaligiran."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Pumiling muli."
        }
    };
    private DialogueLine[] lungsodLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Digmaan sa pagitan ng mga lungsod ba?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Hindi. Walang ebidensya ng digmaan. Ang kanilang pagkawala ay tahimik at natural."
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
            new Answer { text = "Pananakop ng mga Aryan", isCorrect = false },
            new Answer { text = "Digmaan sa pagitan ng mga lungsod", isCorrect = false },
            new Answer { text = "Pagbabago ng klima at pagkatuyo ng ilog", isCorrect = true },
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

            if (dialogueLines == PagbabagoLines && PagbabagoClips != null && currentDialogueIndex < PagbabagoClips.Length)
                clipToPlay = PagbabagoClips[currentDialogueIndex];
            else if (dialogueLines == AryanLines && AryanClips != null && currentDialogueIndex < AryanClips.Length)
                clipToPlay = AryanClips[currentDialogueIndex];
            else if (dialogueLines == lungsodLines && lungsodClips != null && currentDialogueIndex < lungsodClips.Length)
                clipToPlay = lungsodClips[currentDialogueIndex];
            else if (dialogueClips != null && currentDialogueIndex < dialogueClips.Length)
                clipToPlay = dialogueClips[currentDialogueIndex];

            if (clipToPlay != null)
            {
                audioSource.Stop();
                audioSource.clip = clipToPlay;
                audioSource.Play();
            }
        }

        if (dialogueLines == PagbabagoLines)
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
                            SaveLoadManager.Instance.OverwriteAllSavesAfterChallenge("HarappaSceneFive", 0);
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
        else if (dialogueLines == AryanLines)
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
        else if (dialogueLines == lungsodLines)
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
                btn.interactable = !(dialogueLines == PagbabagoLines && currentDialogueIndex == 0);
                btn.gameObject.SetActive(true);
            }

            // Only hide the Next button for the initial/main dialogue set.
            // If we're displaying one of the special dialogues (Pagbabago/Aryan/lungsod) that also start at index 0,
            // leave visibility control to the callers who set up those dialogues.
            bool isSpecialDialogue = (dialogueLines == PagbabagoLines || dialogueLines == AryanLines || dialogueLines == lungsodLines);
            if (!isSpecialDialogue)
                nextButton.gameObject.SetActive(false);
        }
        else
        {
            if (currentDialogueIndex == dialogueLines.Length - 1)
            {
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();


                if (isShowingPagbabagoDialogue) // ✅ Correct flag for this script
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
            isShowingPagbabagoDialogue = true;
            currentDialogueIndex = 0;
            dialogueLines = PagbabagoLines;
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

            if (selected.text == "Pananakop ng mga Aryan")
            {
                isShowingAryanDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = AryanLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextAryanDialogue);
            }
            else if (selected.text == "Digmaan sa pagitan ng mga lungsod")
            {
                isShowinglungsodDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = lungsodLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextlungsodDialogue);
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

        void ShowNextAryanDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextAryanDialogue);
            }
        }

        void ShowNextlungsodDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextlungsodDialogue);
            }
        }
    }
    void LoadGameOverScene()
    {
        SceneManager.LoadScene("HarappaGameOver");
    }

    void PlayCongratsAudio()
    {
        if (finishAudioSource != null)
            finishAudioSource.Play();
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("HarappaSceneFive");
    }
}