using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class AkkadianSecondRecallChallenges : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueLine
    {
        public string characterName;
        public string line;
    }

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
    private bool isShowingtagtuyotDialogue = false;
    private bool isShowingpanloobDialogue = false;
    private bool isShowingHurrianDialogue = false;
    public AudioSource finishAudioSource;

    public AudioSource audioSource;
    public AudioClip[] initialClips;
    public AudioClip[] hurrianClips;
    public AudioClip[] tagtuyotClips;
    public AudioClip[] panloobClips;

    public Button ArtifactImageButton;
    public Button ArtifactUseButton;
    public Button ArtifactButton;

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
        GameState.ResetHearts();

        if (GameState.hearts <= 0)
            GameState.hearts = 3;

        UpdateHeartsUI();

        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "SARGON I",
                line = " Ano ang pangunahing dahilan ng pagbagsak ng Akkadian Empire?"
            },
        };

        ShowDialogue();
    }
    public void UseArtifactButton()
    {
        if (PlayerPrefs.GetInt("UseAkkadianArtifactUsed", 0) == 0)
        {
            PlayerPrefs.SetInt("UseAkkadianArtifactUsed", 1);
            PlayerPrefs.Save();

            dialogueLines = new DialogueLine[]
            {
                new DialogueLine
                {
                    characterName = "Hint",
                    line = " Dahil sa pagsalakay ng dalawang hukbo"
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
                        line = " Ano ang pangunahing dahilan ng pagbagsak ng Akkadian Empire?"
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

    private DialogueLine[] HurrianLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Dahil sa pagsalakay ng Amorite at Hurrian"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Tumpak. Sa panahong mahina ang pamahalaan, sinamantala ito ng mga dayuhang mananakop."
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Kung ganoon... lahat ng itinayo ni Sargon, nawala rin sa dulo?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Hindi lahat. May mga naiwan, hindi sa anyo ng palasyo kundi sa alaala ng mga sumunod sa kanya. Pakinggan natin ang kanyang huling pagninilay."
        },

    };
    private DialogueLine[] tagtuyotLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Dahil sa isang matagal na tagtuyot"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Walang matibay na tala tungkol sa tagtuyot bilang pangunahing dahilan." },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Pumiling muli."
        }
    };
    private DialogueLine[] panloobLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Dahil sa kaguluhang panloob"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " May mga kahinaan sa pamahalaan, pero ang panlabas na pagsalakay ang naging mitsa ng tuluyang pagbagsak" },
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
            new Answer { text = "Dahil sa isang matagal na tagtuyot", isCorrect = false },
            new Answer { text = "Dahil sa kaguluhang panloob", isCorrect = false },
            new Answer { text = "Dahil sa pagsalakay ng Amorite at Hurrian", isCorrect = true },
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

            if (dialogueLines == HurrianLines && hurrianClips != null && currentDialogueIndex < hurrianClips.Length)
                clipToPlay = hurrianClips[currentDialogueIndex];
            else if (dialogueLines == tagtuyotLines && tagtuyotClips != null && currentDialogueIndex < tagtuyotClips.Length)
                clipToPlay = tagtuyotClips[currentDialogueIndex];
            else if (dialogueLines == panloobLines && panloobClips != null && currentDialogueIndex < panloobClips.Length)
                clipToPlay = panloobClips[currentDialogueIndex];
            else if (initialClips != null && currentDialogueIndex < initialClips.Length)
                clipToPlay = initialClips[currentDialogueIndex];

            if (clipToPlay != null)
            {
                audioSource.Stop();
                audioSource.clip = clipToPlay;
                audioSource.Play();
            }
        }

        if (dialogueLines == HurrianLines)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerSmile;
                    ChronocharacterRenderer.sprite = ChronoCheerful;
                    
                    if (!challengeCompleted)
                    {
                        challengeCompleted = true;
                        // Overwrite all existing saves to the next scene to prevent going back
                        if (SaveLoadManager.Instance != null)
                        {
                            SaveLoadManager.Instance.OverwriteAllSavesAfterChallenge("AkkadianSceneFive", 0);
                        }
                    }
                    
                    foreach (Button btn in answerButtons)
                    {
                        btn.interactable = false;
                    }
                    break;
                case 1:
                    foreach (Button btn in answerButtons)
                    {
                        btn.gameObject.SetActive(false);
                    }
                    
                    foreach (Image heart in heartImages)
                    {
                        heart.gameObject.SetActive(false);
                    }
                    BlurBG.gameObject.SetActive(false);
                    PlayercharacterRenderer.sprite = PlayerEager;
                    ChronocharacterRenderer.sprite = ChronoSmile;
                    break;
            }
        }
        else if (dialogueLines == tagtuyotLines)
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
        else if (dialogueLines == panloobLines)
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
                btn.interactable = !(dialogueLines == HurrianLines && currentDialogueIndex == 0);
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

                if (isShowingHurrianDialogue) // ✅ Correct branch for right answer
                {
                    nextButton.interactable = false;

                    // Calculate dialogue audio duration
                    float dialogueDelay = 0f;
                    if (audioSource != null && audioSource.clip != null)
                        dialogueDelay = audioSource.clip.length;
                    else
                        dialogueDelay = 2f; // default if no dialogue audio

                    // Play congrats audio AFTER dialogue finishes
                    Invoke(nameof(PlayCongratsAudio), dialogueDelay);

                    // Calculate total delay (dialogue + congrats + buffer)
                    float congratsDelay = 0f;
                    if (finishAudioSource != null && finishAudioSource.clip != null)
                        congratsDelay = finishAudioSource.clip.length;
                    else
                        congratsDelay = 2f; // default if no congrats audio

                    float totalDelay = dialogueDelay + congratsDelay + 1f;
                    Invoke(nameof(LoadNextScene), totalDelay);
                }
                else
                {
                // For wrong answers, keep looping back
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
            isShowingHurrianDialogue = true;
            currentDialogueIndex = 0;
            dialogueLines = HurrianLines;
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

            if (selected.text.Contains("tagtuyot"))
            {
                isShowingtagtuyotDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = tagtuyotLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNexttagtuyotDialogue);
            }
            else if (selected.text.Contains("panloob"))
            {
                isShowingpanloobDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = panloobLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextpanloobDialogue);
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


        void ShowNextHurrianDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextHurrianDialogue);
            }
        }

        void ShowNexttagtuyotDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNexttagtuyotDialogue);
            }
        }

        void ShowNextpanloobDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextpanloobDialogue);
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
        SceneManager.LoadScene("AkkadianSceneFive");
    }

}