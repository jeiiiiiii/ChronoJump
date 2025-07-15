using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class SumerianFirstRecallChallenges : MonoBehaviour
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
    private bool isShowingBaybayinDialogue = false;
    private bool isShowingHieroglyphicsDialogue = false;
    private bool isShowingCuneiformDialogue = false;
    public AudioSource finishAudioSource;


    void Start()
    {
        nextButton.gameObject.SetActive(false);

        if (GameState.hearts <= 0)
            GameState.hearts = 3;

        UpdateHeartsUI();

        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "ENKI",
                line = "Ano ang tawag sa uri ng pagsusulat na ito?"
            },
        };

        ShowDialogue();
    }

    private DialogueLine[] CuneiformLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "ENKI",
            line = "Tumpak! Isa kang batang eskriba!"
        },
        new DialogueLine
        {
            characterName = "ENKI",
            line = "Marami ngang tagalabas, ngunit bihira ang mabilis matuto."
        },
        new DialogueLine
        {
            characterName = "ENKI",
            line = "Kung gayon, nararapat lamang na makita mo kung paano ginamit ang kaalamang ito upang labanan ang isa sa pinakamalaking hamon dito sa aming lupain , ang kakulangan sa ulan."
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = "May sagot kayo sa tuyong lupa?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Isang tanong ng tubig... at talino."
        },
        new DialogueLine
        {
            characterName = "ENKI",
            line = "Halina’t dalhin ko kayo sa aming mga kanal. Doon niyo makikita kung paano naging masagana ang buhay sa gitna ng disyerto."
        },
    };
    private DialogueLine[] baybayinLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = "Baybayin...?" },
        new DialogueLine
        {
            characterName = "ENKI",
            line = "Hmm… maganda rin ‘yan, pero hindi sa panahong ito. Ang Baybayin ay matutuklasan pa lamang sa mga isla ng malayo. Isipin mong mas luma pa rito."
        },
        new DialogueLine
        {
            characterName = "ENKI",
            line = "Ito’y ukit sa luwad, at may hugis pantusok. Subukan mong muli."
        },
        new DialogueLine
        {
            characterName = "ENKI",
            line = "Pumiling muli."
        }
    };
    private DialogueLine[] HieroglyphicsLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = "Hieroglyphics...?" },
        new DialogueLine
        {
            characterName = "ENKI",
            line = "Hmm… maganda rin ‘yan, pero hindi sa panahong ito. Ang Hieroglyphics ay matutuklasan pa lamang sa mga isla ng malayo. Isipin mong mas luma pa rito."
        },
        new DialogueLine
        {
            characterName = "ENKI",
            line = "Ito’y ukit sa luwad, at may hugis pantusok. Subukan mong muli."
        },
        new DialogueLine
        {
            characterName = "ENKI",
            line = "Pumiling muli."
        }
    };

    void SetAnswers()
    {
        answers = new Answer[]
        {
            new Answer { text = "Baybayin", isCorrect = false },
            new Answer { text = "Cuneiform", isCorrect = true },
            new Answer { text = "Hieroglyphics", isCorrect = false },
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

        // Only set answers for the first question
        if (currentDialogueIndex == 0)
        {
            SetAnswers();
            foreach (Button btn in answerButtons)
            {
                btn.interactable = true;
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

                if (isShowingCuneiformDialogue)
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
            isShowingCuneiformDialogue = true;
            currentDialogueIndex = 0;
            dialogueLines = CuneiformLines;
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
                        SceneManager.LoadScene("SumerianSceneThree");
                    });
                }
            });
        }
        else
        {
            GameState.hearts--;
            UpdateHeartsUI();

            if (GameState.hearts <= 0)
            {
                dialogueText.text = "<b>ENKI</b>: Uliting muli. Wala ka nang natitirang puso.";
                nextButton.gameObject.SetActive(false);
                foreach (Button btn in answerButtons)
                    btn.interactable = false;
                Invoke(nameof(LoadGameOverScene), 2f);
                return;
            }

            if (selected.text == "Baybayin")
            {
                isShowingBaybayinDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = baybayinLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextBaybayinDialogue);
            }
            else if (selected.text == "Hieroglyphics")
            {
                isShowingHieroglyphicsDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = HieroglyphicsLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextHieroglyphicsDialogue);
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


        void ShowNextCuneiformDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextCuneiformDialogue);
            }
        }

        void ShowNextBaybayinDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextBaybayinDialogue);
            }
        }

        void ShowNextHieroglyphicsDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextHieroglyphicsDialogue);
            }
        }
    }
    void LoadGameOverScene()
    {
        SceneManager.LoadScene("GameOver");
    }
    
    void LoadNextScene()
    {
        SceneManager.LoadScene("SumerianSceneThree");
    }

}
