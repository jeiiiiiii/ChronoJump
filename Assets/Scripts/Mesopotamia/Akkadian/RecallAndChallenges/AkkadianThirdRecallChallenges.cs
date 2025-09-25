using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class AkkadianThirdRecallChallenges : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueLine
    {
        public string characterName;
        public string line;
    }

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
    public Sprite SargonThinking;
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
    private bool isShowingBabyloniaDialogue = false;
    private bool isShowingEmpireDialogue = false;
    private bool isShowingrehiyonDialogue = false;
    public AudioSource finishAudioSource;
    public Button ArtifactImageButton;
    public Button ArtifactUseButton;
    public Button ArtifactButton;
    
    public AudioSource audioSource;
    public AudioClip[] initialClips;
    public AudioClip[] rehiyonClips;
    public AudioClip[] BabyloniaClips;
    public AudioClip[] EmpireClips;


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
                characterName = "SARGON I",
                line = " Ano ang nangyari matapos ang panandaliang pagbawi ng kapangyarihan ng lungsod-estado ng Ur?"
            },
        };

        ShowDialogue();
    }
public void UseArtifactButton()
    {
        if (StudentPrefs.GetInt("UseAkkadianArtifactUsed", 0) == 0)
        {
            StudentPrefs.SetInt("UseAkkadianArtifactUsed", 1);
            StudentPrefs.Save();

            dialogueLines = new DialogueLine[]
            {
                new DialogueLine
                {
                    characterName = "Hint",
                    line = " Nagtunggalian ang dalawang hukbo upang masakop ang lungsod-estado ng Ur"
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
                        line = " Ano ang nangyari matapos ang panandaliang pagbawi ng kapangyarihan ng lungsod-estado ng Ur?"
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

    private DialogueLine[] rehiyonLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Nagtunggalian ang Isin at Larsa!"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Tama. Sa halip na pagkakaisa, nauwi sa tunggalian ang mga lungsod sa timog."
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Sana ganoon din ang pamumuno sa panahon namin. May layunin, hindi lang kapangyarihan."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Panahon nang bumalik, ngunit dalhin mo ang aral. Hindi lahat ng bayani ay isinilang sa palasyo."
        },
    };

    private DialogueLine[] BabyloniaLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Sinimulan na ang panahon ng Babylonia"
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Hindi pa ito ang panahon ng Babylonia. Iba pang mga lungsod-estado ang namayani noon sa Mesopotamia."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Pumiling muli."
        }
    };

    private DialogueLine[] EmpireLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Naibalik ang Akkadian Empire"
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Hindi naibalik ang Akkadian Empire. Natapos na ang panahon nito."
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
            new Answer { text = "Nagtunggalian ang Isin at Larsa upang kontrolin ang rehiyon", isCorrect = true },
            new Answer { text = "Naibalik ang Akkadian Empire", isCorrect = false },
            new Answer { text = "Sinimulan na ang panahon ng Babylonia", isCorrect = false },
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
        AchievementUnlockedRenderer.SetActive(false);
        DialogueLine line = dialogueLines[currentDialogueIndex];
        dialogueText.text = $"<b>{line.characterName}</b>: {line.line}";

        if (audioSource != null)
        {
            AudioClip clipToPlay = null;

            if (dialogueLines == rehiyonLines && rehiyonClips != null && currentDialogueIndex < rehiyonClips.Length)
                clipToPlay = rehiyonClips[currentDialogueIndex];
            else if (dialogueLines == BabyloniaLines && BabyloniaClips != null && currentDialogueIndex < BabyloniaClips.Length)
                clipToPlay = BabyloniaClips[currentDialogueIndex];
            else if (dialogueLines == EmpireLines && EmpireClips != null && currentDialogueIndex < EmpireClips.Length)
                clipToPlay = EmpireClips[currentDialogueIndex];
            else if (initialClips != null && currentDialogueIndex < initialClips.Length)
                clipToPlay = initialClips[currentDialogueIndex];

            if (clipToPlay != null)
            {
                audioSource.Stop();
                audioSource.clip = clipToPlay;
                audioSource.Play();
            }
        }

        if (dialogueLines == rehiyonLines)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    AchievementUnlockedRenderer.SetActive(true);
                    PlayercharacterRenderer.sprite = PlayerEager;
                    ChronocharacterRenderer.sprite = ChronoCheerful;
                    
                    if (!challengeCompleted)
                    {
                        challengeCompleted = true;
                        // Overwrite all existing saves to the next scene to prevent going back
                        if (SaveLoadManager.Instance != null)
                        {
                            SaveLoadManager.Instance.OverwriteAllSavesAfterChallenge("AkkadianSceneSix", 0);
                        }
                    }
                    
                    foreach (Button btn in answerButtons)
                    {
                        btn.interactable = false;
                    }
                    break;
                case 1:
                    AchievementUnlockedRenderer.SetActive(false);
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    ChronocharacterRenderer.sprite = ChronoThinking;
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
                    PlayercharacterRenderer.sprite = PlayerSmile;
                    ChronocharacterRenderer.sprite = ChronoSmile;
                    break;
            }
        }
        else if (dialogueLines == BabyloniaLines)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerEmbarassed;
                    ChronocharacterRenderer.sprite = ChronoSad;
                    break;
                case 1:
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    ChronocharacterRenderer.sprite = ChronoThinking;
                    break;
            }
        }
        else if (dialogueLines == EmpireLines)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerEmbarassed;
                    ChronocharacterRenderer.sprite = ChronoSad;
                    break;
                case 1:
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
                    ChronocharacterRenderer.sprite = SargonThinking;
                    break;
            }
        }

        if (currentDialogueIndex == 0)
        {
            SetAnswers();
            foreach (Button btn in answerButtons)
            {
                btn.interactable = !(dialogueLines == rehiyonLines && currentDialogueIndex == 0);
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

                if (isShowingrehiyonDialogue) // ✅ Correct branch
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
        PlayerAchievementManager.UnlockAchievement("Strategist");
        foreach (Button btn in answerButtons)
            btn.interactable = false;

        if (selected.isCorrect)
        {
            isShowingrehiyonDialogue = true;
            currentDialogueIndex = 0;
            dialogueLines = rehiyonLines;
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

            if (selected.text.Contains("Babylonia"))
            {
                isShowingBabyloniaDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = BabyloniaLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextBabyloniaDialogue);
            }
            else if (selected.text.Contains("Empire"))
            {
                isShowingEmpireDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = EmpireLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextEmpireDialogue);
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

        void ShowNextBabyloniaDialogue()
        {
            currentDialogueIndex++;
            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextBabyloniaDialogue);
            }
        }

        void ShowNextEmpireDialogue()
        {
            currentDialogueIndex++;
            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextEmpireDialogue);
            }
        }
    }

    void LoadGameOverScene()
    {
        SceneManager.LoadScene("GameOver");
    }

    void PlayCongratsAudio()
    {
        if (finishAudioSource != null)
            finishAudioSource.Play();
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("AkkadianSceneSix");
    }
}
