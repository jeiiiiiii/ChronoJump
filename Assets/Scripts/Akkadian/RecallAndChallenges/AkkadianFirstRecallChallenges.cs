using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class AkkadianFirstRecallChallenges : MonoBehaviour
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
    private bool isShowingUrNammuDialogue = false;
    private bool isShowingHammurabiDialogue = false;
    private bool isShowingSargonDialogue = false;
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
                line = " Sino ang pinunong nagtayo ng kauna-unahang imperyo sa daigdig?"
            },
        };

        ShowDialogue();
    }

    private DialogueLine[] SargonLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Sargon I!"
        },
        new DialogueLine
        {
            characterName = " CHRONO",
            line = " Tama! Siya ang simula ng konsepto ng imperyo."
        },
        new DialogueLine
        {
            characterName = " PLAYER",
            line = " Parang hindi biro ang ginawa ni Sargon… Pero paano niya napanatili ang gano’n kalawak na imperyo?"
        },
        new DialogueLine
        {
            characterName = " CHRONO",
            line = " Pero ang pagtatag ng imperyo ay simula pa lang… Ibang hamon ang pagpapanatili nito."
        },
        new DialogueLine
        {
            characterName = " CHRONO",
            line = " Halina't silipin natin kung paano pinalawak ni Sargon ang kanyang pamumuno mula hilaga hanggang katimugan..."
        },

    };
    private DialogueLine[] UrNammuLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = "Si Ur-Nammu ay naging pinuno sa Ur pero mas huli siya. Isipin mo kung sino ang unang nagbuklod sa mga lungsod." },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Pumiling muli."
        }
    };
    private DialogueLine[] HammurabiLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = "Si Hammurabi ay sikat sa kanyang mga batas pero ibang panahon at imperyo na iyon. Baka masyado tayong umabante." },
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
            new Answer { text = "UrNammu", isCorrect = false },
            new Answer { text = "Sargon 1", isCorrect = true },
            new Answer { text = "Hammurabi", isCorrect = false },
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

                if (isShowingSargonDialogue)
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
            isShowingSargonDialogue = true;
            currentDialogueIndex = 0;
            dialogueLines = SargonLines;
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
                        SceneManager.LoadScene("AkkadianSceneThree");
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

            if (selected.text == "UrNammu")
            {
                isShowingUrNammuDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = UrNammuLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextUrNammuDialogue);
            }
            else if (selected.text == "Hammurabi")
            {
                isShowingHammurabiDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = HammurabiLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextHammurabiDialogue);
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


        void ShowNextSargonDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextSargonDialogue);
            }
        }

        void ShowNextUrNammuDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextUrNammuDialogue);
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
    }
    void LoadGameOverScene()
    {
        SceneManager.LoadScene("GameOver");
    }
    
    void LoadNextScene()
    {
        SceneManager.LoadScene("AkkadianSceneThree");
    }

}
