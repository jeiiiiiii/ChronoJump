using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class SumerianSecondRecallChallenges : MonoBehaviour
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
    private bool isShowingDisyertoDialogue = false;
    private bool isShowingImbakNaUlanDialogue = false;
    private bool isShowingTigrisAndEuphratesDialogue = false;

    public AudioSource finishAudioSource;

    // 🔹 ADDED - Audio for narration
    public AudioSource audioSource;
    public AudioClip[] dialogueClips;           // for default dialogue
    public AudioClip[] tigrisClips;             // for Tigris and Euphrates dialogue
    public AudioClip[] disyertoClips;           // for Disyerto dialogue
    public AudioClip[] imbakNaUlanClips;        // for Imbak na Ulan dialogue

    public SpriteRenderer PlayercharacterRenderer;
    public SpriteRenderer ChronocharacterRenderer;

    public Sprite PlayerSmile;
    public Sprite PlayerReflective;
    public Sprite PlayerCurious;
    public Sprite PlayerEager;
    public Sprite PlayerEmbarassed;
    public Sprite ChronoCheerful;
    public Sprite ChronoSad;
    public Sprite ChronoSmile;
    public Sprite ZulNeutral;
    public Sprite ZulFriendly;
    public SpriteRenderer BlurBG;
    public Animator chronoAnimator;
    public Animator playerAnimator;

    void Start()
    {
        nextButton.gameObject.SetActive(false);
        UpdateHeartsUI();

        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "ZUL",
                line = "Ano, alam mo ba kung saan talaga nanggagaling ang tubig na dinadala sa mga kanal na ito?"
            },
        };

        ShowDialogue();
    }

    private DialogueLine[] TigrisAndEuphrates = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Tigris at Euphrates!"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Ayos! Talagang nakikinig ka. Iyan ang puso ng buhay sa Mesopotamia."
        },
        new DialogueLine
        {
            characterName = "ZUL",
            line = "Marunong kang makinig, bata. Maraming batang sumasama sa sakahan ang hindi pa rin alam 'yan."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Ngayong naunawaan mo na ang kahalagahan ng irigasyon, may isa pa akong gustong ipakita. Halina’t magtungo tayo sa harap ng templo. Doon mo makikilala ang pinunong-pari na si Ninurta."
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = "Pinunong-pari?"
        },
        new DialogueLine
        {
            characterName = "ZUL",
            line = "Ay, si Patesi Ninurta , ang tagapamagitan namin sa mga diyos. Siya rin ang nagpapatakbo sa mga gawain sa lungsod."
        },
    };
    private DialogueLine[] Disyerto = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = "Disyerto...?" },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Hindi sapat ang ulan dito. At ang disyerto ay lalong hindi mapagkakatiwalaan. Tandaan mo , ang mga ilog ang bumuhay sa kabihasnang ito."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Pumiling muli!"
        },
    };
    private DialogueLine[] ImbakNaUlan = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = "Imbak na ulan...?" },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Hindi sapat ang ulan dito. At ang disyerto ay lalong hindi mapagkakatiwalaan. Tandaan mo , ang mga ilog ang bumuhay sa kabihasnang ito."
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
            new Answer { text = "Disyerto", isCorrect = false },
            new Answer { text = "Tigris at Euphrates", isCorrect = true },
            new Answer { text = "Imbak na ulan", isCorrect = false },
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

        // 🔹 ADDED - Play narration for current line
        if (audioSource != null)
        {
            AudioClip clipToPlay = null;

            if (dialogueLines == TigrisAndEuphrates && tigrisClips != null && currentDialogueIndex < tigrisClips.Length)
                clipToPlay = tigrisClips[currentDialogueIndex];
            else if (dialogueLines == Disyerto && disyertoClips != null && currentDialogueIndex < disyertoClips.Length)
                clipToPlay = disyertoClips[currentDialogueIndex];
            else if (dialogueLines == ImbakNaUlan && imbakNaUlanClips != null && currentDialogueIndex < imbakNaUlanClips.Length)
                clipToPlay = imbakNaUlanClips[currentDialogueIndex];
            else if (dialogueClips != null && currentDialogueIndex < dialogueClips.Length)
                clipToPlay = dialogueClips[currentDialogueIndex];

            if (clipToPlay != null)
            {
                audioSource.Stop();
                audioSource.clip = clipToPlay;
                audioSource.Play();
            }
        }

        // 🔹 (the rest of ShowDialogue stays the same below...)
        
    if (dialogueLines == TigrisAndEuphrates)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    if (chronoAnimator != null)
                        chronoAnimator.Play("Chrono_Smiling (Idle)", 0, 0f);
                    if (playerAnimator != null)
                        playerAnimator.Play("Player_Talking", 0, 0f);
                    foreach (Button btn in answerButtons)
                    {
                        btn.interactable = false;
                    }
                    break;
                case 1:
                    if (chronoAnimator != null)
                        chronoAnimator.Play("Chrono_Talking", 0, 0f);
                    if (playerAnimator != null)
                        playerAnimator.Play("Player_Smiling", 0, 0f);
                    break;
                case 2:
                    if (chronoAnimator != null)
                        chronoAnimator.Play("Zul_Talking", 0, 0f);
                    if (playerAnimator != null)
                        playerAnimator.Play("Player_Eager", 0, 0f);
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
                    if (chronoAnimator != null)
                        chronoAnimator.Play("Chrono_Talking", 0, 0f);
                    if (playerAnimator != null)
                        playerAnimator.Play("Player_Curious", 0, 0f);
                    break;
                case 4:
                    if (chronoAnimator != null)
                        chronoAnimator.Play("Chrono_Thinking", 0, 0f);
                    if (playerAnimator != null)
                        playerAnimator.Play("Player_Talking", 0, 0f);
                    break;
                case 5:
                    if (chronoAnimator != null)
                        chronoAnimator.Play("Zul_Talking", 0, 0f);
                    if (playerAnimator != null)
                        playerAnimator.Play("Player_Reflective", 0, 0f);
                    break;
            }
        }
        else if (dialogueLines == Disyerto)
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
        else if (dialogueLines == ImbakNaUlan)
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
                        chronoAnimator.Play("Zul_Talking", 0, 0f);
                    if (playerAnimator != null)
                        playerAnimator.Play("Player_Reflective", 0, 0f);
                    break;
            }
        }
        // Only set answers for the first question
        if (currentDialogueIndex == 0)
        {
            SetAnswers();
            foreach (Button btn in answerButtons)
            {
                btn.interactable = !(dialogueLines == TigrisAndEuphrates && currentDialogueIndex == 0);
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

            if (isShowingTigrisAndEuphratesDialogue) // Use your correct dialogue flag
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
                // For wrong answers, keep original logic
                nextButton.onClick.AddListener(() =>
            {
                if (finishAudioSource != null)
                    finishAudioSource.Play();
                    nextButton.interactable = false;
                    Invoke(nameof(LoadNextScene), 2f);
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
        if (selected.isCorrect)
        {
            isShowingTigrisAndEuphratesDialogue = true;
            currentDialogueIndex = 0;
            dialogueLines = TigrisAndEuphrates;
            ShowDialogue();

            // Simple setup - let ShowDialogue() handle the final scene logic
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
                dialogueText.text = "<b>ENKI</b>: Uliting muli. Wala ka nang natitirang puso.";
                nextButton.gameObject.SetActive(false);
                foreach (Button btn in answerButtons)
                    btn.interactable = false;
                Invoke(nameof(LoadGameOverScene), 2f);
                return;
            }

            if (selected.text == "Disyerto")
            {
                isShowingDisyertoDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = Disyerto;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextDisyertoDialogue);
            }
            else if (selected.text == "Imbak na ulan")
            {
                isShowingImbakNaUlanDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = ImbakNaUlan;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextImbakNaUlanDialogue);
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

        void ShowNextDisyertoDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextDisyertoDialogue);
            }
        }

        void ShowNextImbakNaUlanDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextImbakNaUlanDialogue);
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
        SceneManager.LoadScene("SumerianSceneFour");
    }
}