using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class SumerianFifthRecallChallenges : MonoBehaviour
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
    private bool isShowingParusahanAngLahatDialogue = false;
    private bool isShowingBigyangPribilehiyoAngMayayamanDialogue = false;
    private bool isShowingItaguyodAngKatarunganDialogue = false; // true
    public AudioSource finishAudioSource;

    public SpriteRenderer PlayercharacterRenderer;
    public SpriteRenderer ChronocharacterRenderer;
    public GameObject AchievementUnlockedRenderer;

    public Sprite PlayerSmile;
    public Sprite PlayerReflective;
    public Sprite PlayerEager;
    public Sprite PlayerEmbarassed;
    public Sprite ChronoCheerful;
    public Sprite ChronoSad;
    public Sprite ChronoCalculating;
    public Sprite ChronoSmile;
    public Sprite EnkiKind;
    public SpriteRenderer BlurBG;
    
    void Start()
    {
        AchievementUnlockedRenderer.SetActive(false);
        nextButton.gameObject.SetActive(false);
        UpdateHeartsUI();

        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ano ang pangunahing layunin ng mga batas ni Ur-Nammu?"
            },
        };

        ShowDialogue();
    }
    private DialogueLine[] ItaguyodAngKatarungan = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Itaguyod ang katarungan..."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Wasto. Ang batas ay hindi dapat panakot, kundi sandigan. At sa Uruk, ang katarungan ay para sa lahat , mayaman man o alipin."
        },
        new DialogueLine
        {
            characterName = "ENKI",
            line = " Tagalabas, ang layunin mo ay hindi lamang pagmasdan, kundi matuto. Dalhin mo ito... isang sagisag ng aming kabihasnan. Higit sa luwad at bato, ito’y paalala ng aming aral , ang halaga ng katarungan, pagkakaisa, at kaalaman."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Panahon na upang bumalik. Ngunit tandaan mo , ang simula ng kaalaman ay ang pag-unawa sa pinagmulan. Sa mundong ito, nabuo ang maraming ideya na naging pundasyon ng kinabukasan."
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Hindi ko ito malilimutan."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Hanggang sa muling paglalakbay."
        },
    };
    private DialogueLine[] ParusahanAngLahat = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = "Parusahan ang lahat...?" },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Hindi ganoon ang layunin ng batas ni Ur-Nammu. Sa katunayan, pinoprotektahan pa nga nito ang mga mas mahina sa lipunan. Subukang balikan ang sinabi ni Enki... ano raw ang haligi ng kanilang kaayusan?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Pumiling muli!"
        },
    };
    private DialogueLine[] BigyangPribilehiyoAngMayayaman = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Bigyang-pribilehiyo ang mayayaman...?" },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Hindi ganoon ang layunin ng batas ni Ur-Nammu. Sa katunayan, pinoprotektahan pa nga nito ang mga mas mahina sa lipunan. Subukang balikan ang sinabi ni Enki... ano raw ang haligi ng kanilang kaayusan?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Pumiling muli!"
        },
    };

    void SetAnswers()
    {
        answers = new Answer[]
        {
            new Answer { text = "Itaguyod ang katarungan", isCorrect = true },
            new Answer { text = "Parusahan ang lahat", isCorrect = false },
            new Answer { text = "Bigyang-pribilehiyo ang mayayaman", isCorrect = false },
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

    if (dialogueLines == ItaguyodAngKatarungan)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerSmile;
                    ChronocharacterRenderer.sprite = ChronoSmile;
                    foreach (Button btn in answerButtons)
                    {
                        btn.interactable = false;
                    }
                    break;
                case 1:
                    AchievementUnlockedRenderer.SetActive(false);
                    PlayercharacterRenderer.sprite = PlayerEager;
                    ChronocharacterRenderer.sprite = ChronoCheerful;
                    break;
                case 2:
                    PlayercharacterRenderer.sprite = PlayerSmile;
                    ChronocharacterRenderer.sprite = EnkiKind;
                    break;
                case 3:
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
                    ChronocharacterRenderer.sprite = ChronoCheerful;
                    break;
                case 4:
                    PlayercharacterRenderer.sprite = PlayerSmile;
                    break;
                case 5:
                    ChronocharacterRenderer.sprite = ChronoSmile;
                    break;
            }
        }
        else if (dialogueLines == ParusahanAngLahat)
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
            }
        }
        else if (dialogueLines == BigyangPribilehiyoAngMayayaman)
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
            }
        }
        else
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    PlayercharacterRenderer.sprite = PlayerSmile;
                    ChronocharacterRenderer.sprite = ChronoCalculating;
                    break;
            }
        }
        // Only set answers for the first question
        if (currentDialogueIndex == 0)
        {
            SetAnswers();
            foreach (Button btn in answerButtons)
            {
                btn.interactable = !(dialogueLines == ItaguyodAngKatarungan && currentDialogueIndex == 0);
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
                    Invoke(nameof(LoadNextScene), 2f);
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
            if (GameState.hearts == 3)
            {
                PlayerAchievementManager.UnlockAchievement("Master");
                AchievementUnlockedRenderer.SetActive(true);
            }
            
            isShowingItaguyodAngKatarunganDialogue = true;
            currentDialogueIndex = 0;
            dialogueLines = ItaguyodAngKatarungan;
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
                        SceneManager.LoadScene("SumerianSceneSeven");
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

            if (selected.text == "Parusahan ang lahat")
            {
                isShowingParusahanAngLahatDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = ParusahanAngLahat;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextParusahanAngLahatDialogue);
            }
            else if (selected.text == "Bigyang-pribilehiyo ang mayayaman")
            {
                isShowingBigyangPribilehiyoAngMayayamanDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = BigyangPribilehiyoAngMayayaman;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextBigyangPribilehiyoAngMayayamanDialogue);
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

        void ShowNextItaguyodAngKatarunganDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextItaguyodAngKatarunganDialogue);
            }
        }

        void ShowNextParusahanAngLahatDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextParusahanAngLahatDialogue);
            }
        }

        void ShowNextBigyangPribilehiyoAngMayayamanDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextBigyangPribilehiyoAngMayayamanDialogue);
            }
        }
    }
    void LoadGameOverScene()
    {
        SceneManager.LoadScene("GameOver");
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("SumerianSceneSeven");
    }
}
