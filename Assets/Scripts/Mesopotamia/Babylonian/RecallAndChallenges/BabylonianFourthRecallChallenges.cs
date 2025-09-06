using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class BabylonianFourthRecallChallenges : MonoBehaviour
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
    private bool challengeCompleted = false;

    public Image[] heartImages;
    private bool isShowingIshtarDialogue = false;
    private bool isShowingNannaDialogue = false;
    private bool isShowingMardukDialogue = false;
    public AudioSource finishAudioSource;

    public SpriteRenderer PlayercharacterRenderer;
    public SpriteRenderer ChronocharacterRenderer;
    public GameObject AchievementUnlockedRenderer;

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

    public AudioSource audioSource;
    public AudioClip[] dialogueClips;
    public AudioClip[] MardukClips;
    public AudioClip[] IshtarClips;
    public AudioClip[] NannaClips;


    void Start()
    {
        if (PlayerAchievementManager.IsAchievementUnlocked("Sword"))
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
            Debug.Log("Achievement 'Sword' is not unlocked yet. Button functionality disabled.");
        }
        
        AchievementUnlockedRenderer.SetActive(false);
        nextButton.gameObject.SetActive(false);

        if (GameState.hearts <= 0)
            GameState.hearts = 3;

        UpdateHeartsUI();

        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Sino ang pangunahing diyos na kinilala sa Babylonia noong panahon ni Hammurabi?"
            },
        };

        ShowDialogue();
    }

    private DialogueLine[] MardukLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Marduk!"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Tumpak. Siya ang diyos na kinilala bilang tagapagtanggol ng kaayusan sa Babylonia."
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Ang lakas ng pananalig nila… pero paano kapag sinubok ng panahon? Kapag dumating ang mga hindi inaasahan?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Kung handa ka pa… may isa pa akong ipapakita. Isang mahalagang bahagi ng kasaysayan ng Babylonia—ang pagtatapos ng kanilang ginintuang panahon."
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Gusto kong malaman… paano natapos ang lahat."
        },
    };
    private DialogueLine[] IshtarLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Ishtar?"
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Hindi siya ang naging sentro ng pananampalataya noong panahong iyon."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Pumiling muli."
        }
    };
    private DialogueLine[] NannaLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Nanna?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Hindi siya ang diyos na kinilala ng buong imperyo."
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
            new Answer { text = "Nanna", isCorrect = false },
            new Answer { text = "Marduk", isCorrect = true },
            new Answer { text = "Ishtar", isCorrect = false },
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

            if (dialogueLines == MardukLines && MardukClips != null && currentDialogueIndex < MardukClips.Length)
                clipToPlay = MardukClips[currentDialogueIndex];
            else if (dialogueLines == IshtarLines && IshtarClips != null && currentDialogueIndex < IshtarClips.Length)
                clipToPlay = IshtarClips[currentDialogueIndex];
            else if (dialogueLines == NannaLines && NannaClips != null && currentDialogueIndex < NannaClips.Length)
                clipToPlay = NannaClips[currentDialogueIndex];
            else if (dialogueClips != null && currentDialogueIndex < dialogueClips.Length)
                clipToPlay = dialogueClips[currentDialogueIndex];

            if (clipToPlay != null)
            {
                audioSource.Stop();
                audioSource.clip = clipToPlay;
                audioSource.Play();
            }
        }


        if (dialogueLines == MardukLines)
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
                            SaveLoadManager.Instance.OverwriteAllSavesAfterChallenge("BabylonianSceneSix", 0);
                        }
                    }
                    
                    foreach (Button btn in answerButtons)
                    {
                        btn.interactable = false;
                    }
                    break;
                case 1:
                    ChronocharacterRenderer.sprite = ChronoCheerful;
                    break;
                case 2:
                    AchievementUnlockedRenderer.SetActive(false);
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
                case 4:
                    ChronocharacterRenderer.sprite = ChronoSmile;
                    PlayercharacterRenderer.sprite = PlayerSmile;
                    break;
            }
        }
        else if (dialogueLines == IshtarLines)
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
        else if (dialogueLines == NannaLines)
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
                btn.interactable = !(dialogueLines == MardukLines && currentDialogueIndex == 0);
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

                if (isShowingMardukDialogue) // ✅ Correct branch
                {
                    nextButton.interactable = false;

                    // Calculate dialogue audio duration
                    float dialogueDelay = 0f;
                    if (audioSource != null && audioSource.clip != null)
                        dialogueDelay = audioSource.clip.length;
                    else
                        dialogueDelay = 2f;

                    // Play congrats audio AFTER dialogue finishes
                    Invoke(nameof(PlayCongratsAudio), dialogueDelay);

                    // Calculate total delay (dialogue + congrats + buffer)
                    float congratsDelay = 0f;
                    if (finishAudioSource != null && finishAudioSource.clip != null)
                        congratsDelay = finishAudioSource.clip.length;
                    else
                        congratsDelay = 2f;

                    float totalDelay = dialogueDelay + congratsDelay + 1f;
                    Invoke(nameof(LoadNextScene), totalDelay);
                }
                else // ❌ Wrong answers → reset back to question
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
        if (GameState.hearts >= 2)
        {
            PlayerAchievementManager.UnlockAchievement("Keeper");
            AchievementUnlockedRenderer.SetActive(true);
        }
        else
        {
            AchievementUnlockedRenderer.SetActive(false);
        }
        foreach (Button btn in answerButtons)
            btn.interactable = false;


        if (selected.isCorrect)
        {
            isShowingMardukDialogue = true;
            currentDialogueIndex = 0;
            dialogueLines = MardukLines;
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

            AchievementUnlockedRenderer.SetActive(false);

            if (GameState.hearts <= 0)
            {
                dialogueText.text = "<b>CHRONO</b>: Uliting muli. Wala ka nang natitirang puso.";
                nextButton.gameObject.SetActive(false);
                foreach (Button btn in answerButtons)
                    btn.interactable = false;
                Invoke(nameof(LoadGameOverScene), 2f);
                return;
            }

            if (selected.text == "Ishtar")
            {
                isShowingIshtarDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = IshtarLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextIshtarDialogue);
            }
            else if (selected.text == "Nanna")
            {
                isShowingNannaDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = NannaLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextNannaDialogue);
            }
            else
            {
                foreach (Button btn in answerButtons)
                    btn.interactable = true;

                nextButton.gameObject.SetActive(false);
                hasAnswered = false;
            }
        }

        void ShowNextIshtarDialogue()
        {
            currentDialogueIndex++;
            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextIshtarDialogue);
            }
        }

        void ShowNextNannaDialogue()
        {
            currentDialogueIndex++;
            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextNannaDialogue);
            }
        }
    }
    public void UseArtifactButton()
    {
        ArtifactButton.onClick.AddListener(() =>
        {
            // Make sure this key matches exactly
            PlayerPrefs.SetInt("UsePowerArtifactUsed", 1);
            PlayerPrefs.Save();

            answerButtons[0].interactable = false;

            ArtifactButton.gameObject.SetActive(false);
            ArtifactImageButton.gameObject.SetActive(false);

        });
    }

    void LoadGameOverScene()
    {
        SceneManager.LoadScene("GameOver");
    }

    void PlayCongratsAudio()
    {
        if (finishAudioSource != null && finishAudioSource.clip != null)
        {
            finishAudioSource.Play();
        }
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("BabylonianSceneSix");
    }
}