using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class Warbelt : MonoBehaviour
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

    public Image[] heartImages;
    private bool isShowingsiningDialogue = false;
    private bool isShowingkalabanDialogue = false;
    private bool isShowingpananakopDialogue = false;
    public AudioSource finishAudioSource;

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
    public SpriteRenderer BlurBG;
    public GameObject AchievementUnlockedRenderer;
    public Button ArtifactImageButton;
    public Button ArtifactUseButton;
    public Button ArtifactButton;

    private bool artifactPowerActivated = false;


    void Start()
    {
        if (ArtifactImageButton != null)
        {
            ArtifactImageButton.onClick.AddListener(() =>
            {
                ArtifactUseButton.gameObject.SetActive(!ArtifactUseButton.gameObject.activeSelf);
                ArtifactImageButton.gameObject.SetActive(false);
            });
        }

        // if (StudentPrefs.GetInt("UsePowerArtifactUsed", 0) == 1)
        // {
        //     ArtifactButton.gameObject.SetActive(false);
        //     ArtifactImageButton.gameObject.SetActive(false);
        // }
        // else
        // {
        //     UseArtifactButton();
        // }

        AchievementUnlockedRenderer.SetActive(false);
        nextButton.gameObject.SetActive(false);

        GameState.ResetHearts();

        if (GameState.hearts <= 0)
            GameState.hearts = 3;

        UpdateHeartsUI();

        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ano ang pangunahing paraan na ginamit ni Tiglath-Pileser I upang mapalawak ang imperyong Assyrian?"
            },
        };

        ShowDialogue();
    }

    private DialogueLine[] pananakopLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Ekspedisyong militar at pananakop... ang kanyang paraan ng pagpapalawak."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Tumpak. Sa kanyang pamumuno, ginamit niya ang disiplina at takot bilang sandata upang kontrolin ang mga ruta ng kalakalan at mga lungsod sa paligid."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Ang kapayapaang itinayo sa takot ay maaaring maging matibay—ngunit bihirang tumagal. Tandaan mo ‘yan habang naglalakbay tayo paatras sa panahon."
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Parang hindi tunay na kapayapaan ang sinasabi niya... kundi tahimik na takot."
        },
    };
    private DialogueLine[] siningLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Hmm... baka pagpapalaganap ng sining?" },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Hindi ito ang naging pokus ni Tiglath-Pileser. Bagama’t may mga anyo ng sining sa Assyria, ang kanyang pangunahing paraan ng pagpapalawak ay sa pamamagitan ng digmaan, pananakop, at matinding disiplina sa militar."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Pumiling muli."
        }
    };
    private DialogueLine[] kalabanLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Siguro... pagpapakumbaba sa harap ng kalaban??"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Hindi. Ang kapayapaan sa ilalim ni Tiglath-Pileser ay hindi hiningi — ito’y kinuha gamit ang lakas at pangamba."
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
            new Answer { text = "Pagpapalaganap ng sining", isCorrect = false },
            new Answer { text = "Ekspedisyong militar at pananakop", isCorrect = true },
            new Answer { text = "Pagpapakumbaba sa harap ng kalaban", isCorrect = false },
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

        if (dialogueLines == pananakopLines)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerEager;
                    ChronocharacterRenderer.sprite = ChronoSmile;
                    foreach (Button btn in answerButtons)
                    {
                        btn.interactable = false;
                    }
                    break;
                case 1:
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
                    if (GameState.hearts == 3)
                    {
                        PlayerAchievementManager.UnlockAchievement("Fear");
                        AchievementUnlockedRenderer.SetActive(true);
                    }
                    BlurBG.gameObject.SetActive(false);
                    ChronocharacterRenderer.sprite = ChronoThinking;
                    break;
                case 3:
                    ChronocharacterRenderer.sprite = ChronoSmile;
                    PlayercharacterRenderer.sprite = PlayerSmile;
                    break;
            }
        }
        else if (dialogueLines == siningLines)
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
        else if (dialogueLines == kalabanLines)
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
                btn.interactable = !(dialogueLines == pananakopLines && currentDialogueIndex == 0);
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

                if (isShowingpananakopDialogue)
                {
                    if (finishAudioSource != null)
                        finishAudioSource.Play();
                    nextButton.interactable = false;
                    Invoke(nameof(LoadNextScene), 2f);
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
            isShowingpananakopDialogue = true;
            currentDialogueIndex = 0;
            dialogueLines = pananakopLines;
            ShowDialogue();

            nextButton.gameObject.SetActive(true);
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() =>
            {
                currentDialogueIndex++;
                if (currentDialogueIndex < dialogueLines.Length - 1)
                {
                    ShowDialogue();
                }
                else
                {
                    ShowDialogue();
                    nextButton.onClick.RemoveAllListeners();
                    nextButton.onClick.AddListener(() =>
                    {
                        SceneManager.LoadScene("AssyrianSceneTwo");
                    });
                }
            });
        }
        else
        {
            // if (artifactPowerActivated)
            //     {
            //         ConsumeArtifactPower();
            //     }
            // else
            //     {
            //         GameState.hearts--;
            //         UpdateHeartsUI();
            //     }

            if (GameState.hearts <= 0)
            {
                dialogueText.text = "<b>CHRONO</b>: Uliting muli. Wala ka nang natitirang puso.";
                nextButton.gameObject.SetActive(false);
                foreach (Button btn in answerButtons)
                    btn.interactable = false;
                Invoke(nameof(LoadGameOverScene), 2f);
                return;
            }

            if (selected.text == "Pagpapalaganap ng sining")
            {
                isShowingsiningDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = siningLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextsiningDialogue);
            }
            else if (selected.text == "Pagpapakumbaba sa harap ng kalaban")
            {
                isShowingkalabanDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = kalabanLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextkalabanDialogue);
            }
            else
            {
                foreach (Button btn in answerButtons)
                    btn.interactable = true;

                nextButton.gameObject.SetActive(false);
                hasAnswered = false;
            }
        }

        void ShowNextsiningDialogue()
        {
            currentDialogueIndex++;
            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextsiningDialogue);
            }
        }

        void ShowNextkalabanDialogue()
        {
            currentDialogueIndex++;
            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextkalabanDialogue);
            }
        }
    }

    // public void UseArtifactButton()
    // {
    //     ArtifactButton.onClick.AddListener(() =>
    //     {
    //         artifactPowerActivated = true;
            
    //         ArtifactButton.interactable = false;
    //         ArtifactImageButton.interactable = false;

    //     });
    // }

    // void ConsumeArtifactPower()
    // {
    //     StudentPrefs.SetInt("UsePowerArtifactUsed", 1);
    //     StudentPrefs.Save();
        
    //     ArtifactButton.gameObject.SetActive(false);
    //     ArtifactImageButton.gameObject.SetActive(false);
        
    //     artifactPowerActivated = false;
        
    //     Debug.Log("Artifact power consumed! Protected from heart loss.");
    // }

    void LoadGameOverScene()
    {
        SceneManager.LoadScene("GameOver");
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("AssyrianSceneTwo");
    }
}
