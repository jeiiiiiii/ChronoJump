using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class SumerianThirdRecallChallenges : MonoBehaviour
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
    public bool challengeCompleted = false;

    public Image[] heartImages;
    private bool isShowingOligarkiyaDialogue = false;
    private bool isShowingMonarkiyaDialogue = false;
    private bool isShowingTheocracyDialogue = false;
    public AudioSource finishAudioSource;

    public AudioSource audioSource;
    public AudioClip[] dialogueClips;      // default
    public AudioClip[] theocracyClips;     // Theocracy dialogue
    public AudioClip[] oligarkiyaClips;    // Oligarkiya dialogue
    public AudioClip[] monarkiyaClips;     // Monarkiya dialogue

    public SpriteRenderer PlayercharacterRenderer;
    public SpriteRenderer ChronocharacterRenderer;

    public Sprite PlayerSmile;
    public Sprite PlayerReflective;
    public Sprite PlayerEager;
    public Sprite PlayerEmbarassed;
    public Sprite ChronoCheerful;
    public Sprite ChronoSad;
    public Sprite ChronoThinking;
    public Sprite PatesiFormal;
    public Sprite PatesiWise;
    public Sprite PatesiDisappointed;
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
                characterName = "PATESI NINURTA",
                line = " Dayuhan, sa iyong nakita at narinig, masasabi mo ba kung anong uri ng pamahalaan ang mayroon kami sa Uruk?"
            },
        };

        ShowDialogue();
    }
    private DialogueLine[] Theocracy = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = " PLAYER",
            line = " Theocracy...?"
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Tumpak! Talagang lumalalim na ang pagkaunawa mo sa kabihasnan ng Sumer."
        },
        new DialogueLine
        {
            characterName = "PATESI NINURTA",
            line = " Karapat-dapat kang dalhin sa susunod na bahagi ng aming lungsod. Halina at ipapakita ko sa inyo ang iba pa naming kaayusan."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Ngayon ay panahon namang masdan ang kanilang mga imbensyon at kalakalan... Tara?"
        },
    };
    private DialogueLine[] Oligarkiya = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = "Oligarkiya...?" },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Hindi. Sa ganitong kabihasnan, ang kapangyarihan ay nagmumula sa paniniwala sa mga diyos. Subukan mong alalahanin kung sino ang tunay na may hawak ng pamahalaan dito."
        },
        new DialogueLine
        {
            characterName = "PATESI NINURTA",
            line = " Ang pamahalaan ay hindi lamang tungkol sa kapangyarihan ng tao, kundi sa pananampalatayang nakaangkla sa aming mga diyos."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Pumiling muli!"
        }
    };
    private DialogueLine[] Monarkiya = new DialogueLine[]
    {
        new DialogueLine
        {
            characterName = "PLAYER",
            line = "Monarkiya…?" },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = "Hindi. Sa ganitong kabihasnan, ang kapangyarihan ay nagmumula sa paniniwala sa mga diyos. Subukan mong alalahanin kung sino ang tunay na may hawak ng pamahalaan dito."
        },
        new DialogueLine
        {
            characterName = "PATESI NINURTA",
            line = " Ang pamahalaan ay hindi lamang tungkol sa kapangyarihan ng tao, kundi sa pananampalatayang nakaangkla sa aming mga diyos."
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
            new Answer { text = "Theocracy", isCorrect = true },
            new Answer { text = "Oligarkiya", isCorrect = false },
            new Answer { text = "Monarkiya", isCorrect = false },
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

            if (dialogueLines == Theocracy && theocracyClips != null && currentDialogueIndex < theocracyClips.Length)
                clipToPlay = theocracyClips[currentDialogueIndex];
            else if (dialogueLines == Oligarkiya && oligarkiyaClips != null && currentDialogueIndex < oligarkiyaClips.Length)
                clipToPlay = oligarkiyaClips[currentDialogueIndex];
            else if (dialogueLines == Monarkiya && monarkiyaClips != null && currentDialogueIndex < monarkiyaClips.Length)
                clipToPlay = monarkiyaClips[currentDialogueIndex];
            else if (dialogueClips != null && currentDialogueIndex < dialogueClips.Length)
                clipToPlay = dialogueClips[currentDialogueIndex];

            if (clipToPlay != null)
            {
                audioSource.Stop();
                audioSource.clip = clipToPlay;
                audioSource.Play();
            }
        }

    if (dialogueLines == Theocracy)
        {
            switch (currentDialogueIndex)
            {
                case 0:
                    if (chronoAnimator != null)
                        chronoAnimator.Play("Chrono_Smiling (Idle)", 0, 0f);
                    if (playerAnimator != null)
                        playerAnimator.Play("Player_Talking", 0, 0f);

                    if (!challengeCompleted)
                    {
                        challengeCompleted = true;
                        // Overwrite all existing saves to the next scene to prevent going back
                        if (SaveLoadManager.Instance != null)
                        {
                            SaveLoadManager.Instance.OverwriteAllSavesAfterChallenge("SumerianSceneFive", 0);
                        }
                    }

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
                        chronoAnimator.Play("Patesi_Explaining", 0, 0f);
                    if (playerAnimator != null)
                        playerAnimator.Play("Player_Curious", 0, 0f);
                    break;
                case 3:
                    if (chronoAnimator != null)
                        chronoAnimator.Play("Chrono_Talking", 0, 0f);
                    if (playerAnimator != null)
                        playerAnimator.Play("Player_Eager", 0, 0f);
                    break;
            }
        }
        else if (dialogueLines == Oligarkiya)
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
                        chronoAnimator.Play("Patesi_Explaining", 0, 0f);
                    if (playerAnimator != null)
                        playerAnimator.Play("Player_Curious", 0, 0f);
                    break;
                case 3:
                    if (chronoAnimator != null)
                        chronoAnimator.Play("Chrono_Talking", 0, 0f);
                    if (playerAnimator != null)
                        playerAnimator.Play("Player_Reflective", 0, 0f);
                    break;
            }
        }
        else if (dialogueLines == Monarkiya)
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
                        chronoAnimator.Play("Patesi_Explaining", 0, 0f);
                    if (playerAnimator != null)
                        playerAnimator.Play("Player_Curious", 0, 0f);
                    break;
                case 3:
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
                        chronoAnimator.Play("Patesi_Explaining", 0, 0f);
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
                btn.interactable = !(dialogueLines == Theocracy && currentDialogueIndex == 0);
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

                if (isShowingTheocracyDialogue) // ✅ correct answer flow
                {
                    nextButton.interactable = false;

                    // 1. Dialogue audio delay
                    float dialogueDelay = 0f;
                    if (audioSource != null && audioSource.clip != null)
                        dialogueDelay = audioSource.clip.length;
                    else
                        dialogueDelay = 2f;

                    // 2. Play congrats after dialogue
                    Invoke(nameof(PlayCongratsAudio), dialogueDelay);

                    // 3. Congrats delay
                    float congratsDelay = 0f;
                    if (finishAudioSource != null && finishAudioSource.clip != null)
                        congratsDelay = finishAudioSource.clip.length;
                    else
                        congratsDelay = 2f;

                    // 4. Load next scene after total delay
                    float totalDelay = dialogueDelay + congratsDelay + 1f;
                    Invoke(nameof(LoadNextScene), totalDelay);
                }
                else
                {
                    // Wrong answers → cycle back through dialogue
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

        if (selected.isCorrect)
        {
            isShowingTheocracyDialogue = true;
            currentDialogueIndex = 0;
            dialogueLines = Theocracy;
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
                        SceneManager.LoadScene("SumerianSceneFive");
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

            if (selected.text == "Oligarkiya")
            {
                isShowingOligarkiyaDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = Oligarkiya;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextOligarkiyaDialogue);
            }
            else if (selected.text == "Monarkiya")
            {
                isShowingMonarkiyaDialogue = true;
                currentDialogueIndex = 0;
                dialogueLines = Monarkiya;
                ShowDialogue();

                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextMonarkiyaDialogue);
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

        void ShowNextTheocracyDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextTheocracyDialogue);
            }
        }

        void ShowNextOligarkiyaDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextOligarkiyaDialogue);
            }
        }

        void ShowNextMonarkiyaDialogue()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex <= dialogueLines.Length - 1)
            {
                ShowDialogue();
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ShowNextMonarkiyaDialogue);
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
        SceneManager.LoadScene("SumerianSceneFive");
    }
}
