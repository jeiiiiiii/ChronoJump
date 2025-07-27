using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class AssyrianSecondRecallChallenges : MonoBehaviour
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
    private bool isShowingHammurabiDialogue = false;
    private bool isShowingNebuchadnezzarDialogue = false;
    private bool isShowingAshurbanipalDialogue = false;
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
                characterName = "CHRONO",
                line = " Sino ang nagpatayo ng kauna-unahang silid-aklatan sa mundo?"
            },
        };

        ShowDialogue();
    }

    private DialogueLine[] AshurbanipalLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Ashurbanipal! Ikaw ang nag-ipon ng karunungan."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Tumpak. Pero huwag kalimutan—hari rin siya ng kalupitan."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Ang karunungan ay pamana. Sa kabila ng kalupitan ng panahon, ang mga salita ay nananatili."
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Mas matatag pa pala ang kaalaman kaysa sa espada."
        },

    };
    private DialogueLine[] HammurabiLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Ano... si Hammurabi ba?" },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Siya'y tagapaglikha ng batas, hindi ng aklatan."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Pumiling muli."
        },

    };
    private DialogueLine[] NebuchadnezzarLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Si Nebuchadnezzar... siguro?" },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Hindi. Siya ang hari ng Babylon, hindi ng aklatan."
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
            new Answer { text = "Hammurabi", isCorrect = false },
            new Answer { text = "Nebuchadnezzar", isCorrect = false },
            new Answer { text = "Ashurbanipal", isCorrect = true },
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

                if (isShowingAshurbanipalDialogue)
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
            isShowingAshurbanipalDialogue = true;
            currentDialogueIndex = 0;
            dialogueLines = AshurbanipalLines;
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
                        SceneManager.LoadScene("AssyrianSceneThree");
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
                dialogueText.text = "<b>CHRONO</b>: Uliting muli. Wala ka nang natitirang puso.";
                nextButton.gameObject.SetActive(false);
                foreach (Button btn in answerButtons)
                    btn.interactable = false;
                Invoke(nameof(LoadGameOverScene), 2f);
                return;
            }

            if (selected.text == "Hammurabi")
            {
                isShowingHammurabiDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = HammurabiLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextHammurabiDialogue);
            }
            else if (selected.text == "Nebuchadnezzar")
            {
                isShowingNebuchadnezzarDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = NebuchadnezzarLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextNebuchadnezzarDialogue);
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


        void ShowNextAshurbanipalDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextAshurbanipalDialogue);
            }
        }

        void ShowNextHammurabiDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextHammurabiDialogue);
            }
        }

        void ShowNextNebuchadnezzarDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextNebuchadnezzarDialogue);
            }
        }
    }
    void LoadGameOverScene()
    {
        SceneManager.LoadScene("GameOver");
    }
    
    void LoadNextScene()
    {
        SceneManager.LoadScene("AssyrianSceneThree");
    }

}
