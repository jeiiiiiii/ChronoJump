using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class BabylonianThirdRecallChallenges : MonoBehaviour
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
    private bool isShowingUrukDialogue = false;
    private bool isShowingAklatDialogue = false;
    private bool isShowingGilgameshDialogue = false;
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
                line = " Alin sa mga sumusunod ang kilalang akdang pampanitikan mula sa panahong Uruk?"
            },
        };

        ShowDialogue();
    }

    private DialogueLine[] GilgameshLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Epikong Gilgamesh!"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Tama. Isa ito sa pinakamatandang epiko sa daigdig. Isinulat ito sa panahon ng Uruk, at nagpapakita ng pakikipagsapalaran ng isang bayani na si Gilgamesh."
        },
        new DialogueLine
        {
            characterName = "HAMMURABI",
            line = " Ang isang imperyo ay hindi lamang nasusukat sa laki ng nasasakupan, kundi sa lalim ng alaala at karunungang naiiwan."
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Kung ganito kalalim ang iniwan nila sa kwento… paano pa kaya sa paniniwala nila?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Halika. Oras na para makita mo kung paano nila pinarangalan ang mga diyos na nagbibigay liwanag sa kanilang buhay."
        },
    };
    private DialogueLine[] UrukLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Awit ng Uruk?"
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Parang hindi ko narinig ‘yang pamagat na ‘yan. Subukan mong balikan ang mga kilalang kwento noon."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Pumiling muli."
        }
    };
    private DialogueLine[] AklatLines = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = "Aklat ni Hammurabi?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Hmm... parang nalito ka. Hindi ‘yan akdang pampanitikan."
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
            new Answer { text = "Epikong Gilgamesh", isCorrect = true },
            new Answer { text = "Aklat ni Hammurabi", isCorrect = false },
            new Answer { text = "Awit ng Uruk", isCorrect = false },
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

        if (dialogueLines == GilgameshLines)
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
        else if (dialogueLines == UrukLines)
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
        else if (dialogueLines == AklatLines)
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
                btn.interactable = !(dialogueLines == GilgameshLines && currentDialogueIndex == 0);
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

                if (isShowingGilgameshDialogue)
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
            isShowingGilgameshDialogue = true;
            currentDialogueIndex = 0;
            dialogueLines = GilgameshLines;
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
                        SceneManager.LoadScene("BabylonianSceneFive");
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

            if (selected.text == "Awit ng Uruk")
            {
                isShowingUrukDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = UrukLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextUrukDialogue);
            }
            else if (selected.text == "Aklat ni Hammurabi")
            {
                isShowingAklatDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = AklatLines;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextAklatDialogue);
            }
            else
            {
                foreach (Button btn in answerButtons)
                    btn.interactable = true;

                nextButton.gameObject.SetActive(false);
                hasAnswered = false;
            }
        }

        void ShowNextUrukDialogue()
        {
            currentDialogueIndex++;
            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextUrukDialogue);
            }
        }

        void ShowNextAklatDialogue()
        {
            currentDialogueIndex++;
            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextAklatDialogue);
            }
        }
    }

    void LoadGameOverScene()
    {
        SceneManager.LoadScene("GameOver");
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("BabylonianSceneFive");
    }
}