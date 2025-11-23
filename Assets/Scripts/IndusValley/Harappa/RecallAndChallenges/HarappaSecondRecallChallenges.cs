using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class HarappaSecondRecallChallenges : MonoBehaviour
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
    private bool isShowingtabletsDialogue = false;
    private bool isShowingtaoDialogue = false;
    private bool isShowingWalangDialogue = false;
    public AudioSource finishAudioSource;

    public Button ArtifactImageButton;
    public Button ArtifactUseButton;
    public Button ArtifactButton;

    public AudioSource audioSource;
    public AudioClip[] dialogueClips;
    public AudioClip[] WalangClips;
    public AudioClip[] tabletsClips;
    public AudioClip[] taoClips;

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

    private DialogueLine[] WalangLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Walang key o Rosetta Stone na natagpuan!"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Tumpak. Kaya hanggang ngayon, ang kanilang wika ay nananatiling misteryo."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Ang wika ay boses ng isang sibilisasyon. Kapag nawala ito, parang naging bulong na lang ang kanilang kuwento."
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Pero kahit hindi natin mabasa... nandito pa rin sila. Naririnig pa rin natin ang kanilang tahimik na sigaw."
        },

    };
    private DialogueLine[] tabletsLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Ano... walang natirang clay tablets ba?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " May libu-libong seals na natagpuan. Ang problema ay hindi natin alam kung paano ito basahin."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Pumiling muli."
        }
    };
    private DialogueLine[] taoLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Nilimutan na ng lahat ng tao... siguro?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Hindi lang nilimutan, talagang nawala ang susi sa pagbasa nito. Walang koneksyon sa ibang wika."
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
            new Answer { text = "Walang key o Rosetta Stone na natagpuan ", isCorrect = true },
            new Answer { text = "Walang natirang clay tablets", isCorrect = false },
            new Answer { text = "Nilimutan na ng lahat ng tao", isCorrect = false },
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

            if (dialogueLines == WalangLines && WalangClips != null && currentDialogueIndex < WalangClips.Length)
                clipToPlay = WalangClips[currentDialogueIndex];
            else if (dialogueLines == tabletsLines && tabletsClips != null && currentDialogueIndex < tabletsClips.Length)
                clipToPlay = tabletsClips[currentDialogueIndex];
            else if (dialogueLines == taoLines && taoClips != null && currentDialogueIndex < taoClips.Length)
                clipToPlay = taoClips[currentDialogueIndex];
            else if (dialogueClips != null && currentDialogueIndex < dialogueClips.Length)
                clipToPlay = dialogueClips[currentDialogueIndex];

            if (clipToPlay != null)
            {
                audioSource.Stop();
                audioSource.clip = clipToPlay;
                audioSource.Play();
            }
        }

        if (dialogueLines == WalangLines)
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
                            SaveLoadManager.Instance.OverwriteAllSavesAfterChallenge("HarappaSceneThree", 0);
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
        else if (dialogueLines == tabletsLines)
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
        else if (dialogueLines == taoLines)
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
                btn.interactable = !(dialogueLines == WalangLines && currentDialogueIndex == 0);
                btn.gameObject.SetActive(true);
            }

            // Only hide the Next button for the initial/main dialogue set.
            // If we're displaying one of the special dialogues (Walang/tablets/tao) that also start at index 0,
            // leave visibility control to the callers who set up those dialogues.
            bool isSpecialDialogue = (dialogueLines == WalangLines || dialogueLines == tabletsLines || dialogueLines == taoLines);
            if (!isSpecialDialogue)
                nextButton.gameObject.SetActive(false);
        }
        else
        {
            if (currentDialogueIndex == dialogueLines.Length - 1)
            {
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();


                if (isShowingWalangDialogue) // ✅ Correct flag for this script
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
            isShowingWalangDialogue = true;
            currentDialogueIndex = 0;
            dialogueLines = WalangLines;
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

            if (selected.text == "Walang natirang clay tablets")
            {
                isShowingtabletsDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = tabletsLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNexttabletsDialogue);
            }
            else if (selected.text == "Nilimutan na ng lahat ng tao")
            {
                isShowingtaoDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = taoLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNexttaoDialogue);
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

        void ShowNexttabletsDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNexttabletsDialogue);
            }
        }

        void ShowNexttaoDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNexttaoDialogue);
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
        SceneManager.LoadScene("HarappaSceneThree");
    }
}