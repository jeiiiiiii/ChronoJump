using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class SiningSecondRecallChallenges : MonoBehaviour
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
    private bool isShowinghariDialogue = false;
    private bool isShowingpaintingsDialogue = false;
    private bool isShowingCarnelianDialogue = false;
    public AudioSource finishAudioSource;

    public Button ArtifactImageButton;
    public Button ArtifactUseButton;
    public Button ArtifactButton;
    public Animator chronoAnimator;
    public Animator playerAnimator;
    public AudioSource audioSource;
    public AudioClip[] dialogueClips;
    public AudioClip[] CarnelianClips;
    public AudioClip[] hariClips;
    public AudioClip[] paintingsClips;

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
                line = "  Ano ang ipinagmamalaki ng Indus Valley sa kanilang craft at sining"
            },
        };

        ShowDialogue();
    }
    public void UseArtifactButton()
    {
        if (StudentPrefs.GetInt("UseSiningArtifactUsed", 0) == 0)
        {
            StudentPrefs.SetInt("UseSiningArtifactUsed", 1);
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
                        line = "  Ano ang ipinagmamalaki ng Indus Valley sa kanilang craft at sining?"
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

    private DialogueLine[] CarnelianLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Carnelian beads at advanced pottery! Ang ganda ng kanilang craftsmanship."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Magaling! Ang kanilang sining ay simple pero perpekto, yan ang tunay na husay."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Ang sining ay hindi kailangang maging malaki upang maging mahalaga. Minsan, ang pinakamaganda ay nasa detalye."
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Mas nakakamangha pala kapag maliit pero perpekto."
        },

    };
    private DialogueLine[] hariLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Ano... mga estatwa ng mga hari ba?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Walang malalaking estatwa ng mga hari sa Indus Valley. Ang kanilang sining ay mas praktikal, beads, pottery, at everyday objects."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Pumiling muli."
        }
    };
    private DialogueLine[] paintingsLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Malalaking mural paintings... siguro?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Hindi. Walang natagpuang malalaking paintings. Ang kanilang sining ay maliit, detailed, at portable."
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
            new Answer { text = "Carnelian beads at advanced pottery", isCorrect = true },
            new Answer { text = "Mga estatwa ng mga hari", isCorrect = false },
            new Answer { text = "Malalaking mural paintings", isCorrect = false },
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

            if (dialogueLines == CarnelianLines && CarnelianClips != null && currentDialogueIndex < CarnelianClips.Length)
                clipToPlay = CarnelianClips[currentDialogueIndex];
            else if (dialogueLines == hariLines && hariClips != null && currentDialogueIndex < hariClips.Length)
                clipToPlay = hariClips[currentDialogueIndex];
            else if (dialogueLines == paintingsLines && paintingsClips != null && currentDialogueIndex < paintingsClips.Length)
                clipToPlay = paintingsClips[currentDialogueIndex];
            else if (dialogueClips != null && currentDialogueIndex < dialogueClips.Length)
                clipToPlay = dialogueClips[currentDialogueIndex];

            if (clipToPlay != null)
            {
                audioSource.Stop();
                audioSource.clip = clipToPlay;
                audioSource.Play();
            }
        }

        if (dialogueLines == CarnelianLines)
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
                            SaveLoadManager.Instance.OverwriteAllSavesAfterChallenge("SiningSceneTwo", 0);
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
        else if (dialogueLines == hariLines)
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
        else if (dialogueLines == paintingsLines)
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
                btn.interactable = !(dialogueLines == CarnelianLines && currentDialogueIndex == 0);
                btn.gameObject.SetActive(true);
            }

            // Only hide the Next button for the initial/main dialogue set.
            // If we're displaying one of the special dialogues (Carnelian/hari/paintings) that also start at index 0,
            // leave visibility control to the callers who set up those dialogues.
            bool isSpecialDialogue = (dialogueLines == CarnelianLines || dialogueLines == hariLines || dialogueLines == paintingsLines);
            if (!isSpecialDialogue)
                nextButton.gameObject.SetActive(false);
        }
        else
        {
            if (currentDialogueIndex == dialogueLines.Length - 1)
            {
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();


                if (isShowingCarnelianDialogue) // ✅ Correct flag for this script
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
            isShowingCarnelianDialogue = true;
            currentDialogueIndex = 0;
            dialogueLines = CarnelianLines;
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

            if (selected.text == "Mga estatwa ng mga hari")
            {
                isShowinghariDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = hariLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNexthariDialogue);
            }
            else if (selected.text == "Malalaking mural paintings")
            {
                isShowingpaintingsDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = paintingsLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextpaintingsDialogue);
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

        void ShowNexthariDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNexthariDialogue);
            }
        }

        void ShowNextpaintingsDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextpaintingsDialogue);
            }
        }
    }
    void LoadGameOverScene()
    {
        SceneManager.LoadScene("SiningGameOver");
    }

    void PlayCongratsAudio()
    {
        if (finishAudioSource != null)
            finishAudioSource.Play();
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("SiningSceneThree");
    }
}