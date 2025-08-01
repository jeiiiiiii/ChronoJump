using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class SumerianFourthRecallChallenges : MonoBehaviour
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
    private bool isShowingApoyDialogue = false;
    private bool isShowingLayagDialogue = false;
    private bool isShowingGulongDialogue = false;
    public AudioSource finishAudioSource;
    public SpriteRenderer PlayercharacterRenderer;
    public SpriteRenderer ChronocharacterRenderer;

    public Sprite PlayerSmile;
    public Sprite PlayerReflective;
    public Sprite PlayerEager;
    public Sprite PlayerEmbarassed;
    public Sprite ChronoCheerful;
    public Sprite ChronoSad;
    public Sprite PatesiFormal;
    public Sprite IshmaSmirking;
    public SpriteRenderer BlurBG;


    void Start()
    {
        nextButton.gameObject.SetActive(false);
        UpdateHeartsUI();

        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "ISHMA",
                line = " Ano ang isa sa mga imbensyon ng mga Sumerian na nagpabilis sa transportasyon sa lupa?"
            },
        };

        ShowDialogue();
    }
    private DialogueLine[] Gulong = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Gulong! Gawa ng Sumerian"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Tama! Napakatinding ambag ng simpleng hugis-bilog sa kasaysayan ng tao. Saan man makarating ang gulong, may progreso.”"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Ang bawat gulong ay simula ng bagong pag-ikot. Tara, may isa pa akong gustong ipakita , isang mahalagang batayan ng kaayusan sa kanilang lipunan."
        },
    };
    private DialogueLine[] Apoy = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = "Apoy...?" },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Ah, apoy! Mahalaga ito, ngunit hindi ito ang sagot na hinahanap natin. Ang apoy ay ginagamit para sa pagluluto at pag-init, ngunit hindi ito direktang nakakatulong sa transportasyon sa lupa"
        },
        new DialogueLine
        {
            characterName = "ISHMA",
            line = " Isipin mo ang mga bagay na talagang nakapagpabilis sa ating paglalakbay. Ang apoy ay nagbibigay ng init at liwanag, ngunit ano ang talagang nagpapagalaw sa mga karwahe at sasakyan?"
        },
        new DialogueLine
        {
            characterName = "ISHMA",
            line = " Pumiling muli!"
        },
    };
    private DialogueLine[] Layag = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Layag…?" },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Layag ay para sa ilog at dagat. Pero ang tinutukoy natin ay para sa lupa. Balikan mo ang nakita mong karwahe kanina. Ano ang nagpapagalaw sa kanila?"
        },
        new DialogueLine
        {
            characterName = "ISHMA",
            line = " Hindi lahat ng bagay ay tinutulak ng hangin. Ang ilan ay pinaikot ng talino."
        },
        new DialogueLine
        {
            characterName = "ISHMA",
            line = " Pumiling muli!"
        },
    };

    void SetAnswers()
    {
        answers = new Answer[]
        {
            new Answer { text = "Layag", isCorrect = false },
            new Answer { text = "Apoy", isCorrect = false },
            new Answer { text = "Gulong", isCorrect = true },
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

    if (dialogueLines == Gulong)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerSmile;
                    ChronocharacterRenderer.sprite = PatesiFormal;
                    break;
                case 1:
                    PlayercharacterRenderer.sprite = PlayerEager;
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
                    ChronocharacterRenderer.sprite = ChronoCheerful;
                    break;
            }
        }
        else if (dialogueLines == Apoy)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    ChronocharacterRenderer.sprite = ChronoSad;
                    break;
                case 1:
                    PlayercharacterRenderer.sprite = PlayerEmbarassed;
                    break;
                case 2:
                    ChronocharacterRenderer.sprite = IshmaSmirking;
                    break;
                case 3:
                    ChronocharacterRenderer.sprite = PatesiFormal;
                    break;
            }
        }
        else if (dialogueLines == Layag)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerReflective;
                    ChronocharacterRenderer.sprite = ChronoSad;
                    break;
                case 1:
                    PlayercharacterRenderer.sprite = PlayerEmbarassed;
                    break;
                case 2:
                    ChronocharacterRenderer.sprite = IshmaSmirking;
                    break;
                case 3:
                    ChronocharacterRenderer.sprite = PatesiFormal;
                    break;
            }
        }
        else
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerSmile;
                    ChronocharacterRenderer.sprite = PatesiFormal;
                    break;
            }
        }

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
                nextButton.onClick.AddListener(() =>
                {
                    if (finishAudioSource != null)
                        finishAudioSource.Play();
                    nextButton.interactable = false;
                    Invoke(nameof(LoadNextScene), 2f); // 2 seconds delay, adjust as needed
                });
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

        if (selected.isCorrect)
        {
            isShowingGulongDialogue = true;
            currentDialogueIndex = 0;
            dialogueLines = Gulong;
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
                    // Show the last line
                    ShowDialogue();
                    nextButton.onClick.RemoveAllListeners();
                    nextButton.onClick.AddListener(() =>
                    {
                        SceneManager.LoadScene("SumerianSceneSix");
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

            if (selected.text == "Apoy")
            {
                isShowingApoyDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = Apoy;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextApoyDialogue);
            }
            else if (selected.text == "Layag")
            {
                isShowingLayagDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = Layag;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextLayagDialogue);
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

        void ShowNextGulongDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextGulongDialogue);
            }
        }

        void ShowNextApoyDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextApoyDialogue);
            }
        }

        void ShowNextLayagDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextLayagDialogue);
            }
        }
    }
    void LoadGameOverScene()
    {
        SceneManager.LoadScene("GameOver");
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("SumerianSceneSix");
    }

}
