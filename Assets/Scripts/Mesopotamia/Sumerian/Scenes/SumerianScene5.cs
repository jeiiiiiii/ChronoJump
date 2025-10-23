using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class SumerianScene5 : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueLine
    {
        public string characterName;
        public string line;
    }

    [SerializeField] public TextMeshProUGUI dialogueText;
    [SerializeField] public Button nextButton;
    public GameObject Gulong;
    public Button backButton;

    [Header("UI Buttons")]
    public Button saveButton;
    public Button homeButton;

    public Button settingsButton;

    public int currentDialogueIndex = 0;

    public DialogueLine[] dialogueLines;

    public SpriteRenderer PlayercharacterRenderer;
    public Sprite PlayerCurious;
    public Sprite PlayerEager;
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoSmile;
    public Sprite ChronoThinking;
    public Sprite PatesiFormal;
    public Sprite PatesiDisapproval;

    public SpriteRenderer IshmacharacterRenderer;

    public Sprite IshmaAmazed;
    public Sprite IshmaExplaining;
    public Sprite IshmaSmirking;
    public Animator chronoAnimator;
    public Animator playerAnimator;
    public Animator NPCFulldrawnAnimator;
    public AudioSource audioSource;
    public AudioClip[] dialogueClips;

    void Start()
    {
        // Ensure SaveLoadManager exists
        if (SaveLoadManager.Instance == null)
        {
            GameObject saveLoadManager = new GameObject("SaveLoadManager");
            saveLoadManager.AddComponent<SaveLoadManager>();
        }

        // Initialize dialogue lines
        InitializeDialogueLines();

        // Load saved dialogue index
        LoadDialogueIndex();

        SetupButtons();
        ShowDialogue();
    }

    void InitializeDialogueLines()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "ISHMA",
                line = " Aba, Patesi! Magandang araw! Sino ang mga kasama mo?"
            },
            new DialogueLine
            {
                characterName = "PATESI NINURTA",
                line = " Isang tagalabas at ang kaniyang gabay. Ipinapasyal namin sila upang maunawaan ang puso ng aming kabihasnan."
            },
            new DialogueLine
            {
                characterName = "ISHMA",
                line = " Ako si Ishma, isa sa mga mangangalakal dito. Halina't tumingin-tingin sa paligid. Makikita ninyong kami'y hindi lamang marunong magtanim, kundi pati rin gumawa ng mga bagay na nagpapagaan ng aming pamumuhay."
            },
            new DialogueLine
            {
                characterName = "ISHMA",
                line = " Tulad nitong sasakyang ito. Noon, bitbit namin sa balikat ang mga paninda. Pero nang matutunan naming gamitin ang gulong, napabilis ang lahat , mula sakahan, hanggang sa pagdadala ng produkto sa ilog."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Napakatalino naman ng naka-isip nito."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ang gulong ay isa sa mga pinakakilalang imbensyon ng mga Sumerian. Hindi lamang ito ginagamit sa mga karwahe. Sa paglipas ng panahon, ginamit din ito sa pag-igib ng tubig, pagpapaandar ng mga kasangkapan, at iba pa. Isipin mo , ang simpleng bilog, nagbukas ng daan sa napakaraming posibilidad."
            },
            new DialogueLine
            {
                characterName = "ISHMA",
                line = " Hindi lahat ng kailangan namin ay makukuha dito sa aming lungsod. Kaya naman natutunan naming makipagpalitan ng produkto , butil para sa kahoy, tela kapalit ng metal. Walang salapi. Lahat ay batay sa pangangailangan."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Tawag diyan ay barter. Noon pa man, marunong na ang mga Sumerian makipagkalakalan , hindi lang sa loob ng lungsod kundi pati sa ibang lungsod-estado. Sa pamamagitan nito, nakarating sa kanila ang mga bagong kagamitan, ideya, at karunungan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Kaya pala ang daming tao dito... at parang ang daming produkto mula sa iba’t ibang lugar."
            },
            new DialogueLine
            {
                characterName = "ISHMA",
                line = " Ang palengke ang puso ng aming kabuhayan. Dito mo makikita ang liksi ng talino at tiyaga ng aming mga mamamayan."
            },
            new DialogueLine
            {
                characterName = "PATESI NINURTA",
                line = " Ngayong nakita mo ang aming mga kasangkapan at kalakaran, masasabi mo ba kung alin sa mga ito ang likha ng aming kabihasnan upang mapadali ang paglalakbay sa lupa?"
            },
        };
    }

    // --- LoadDialogueIndex to handle New Game / Load Save / Continue ---
    void LoadDialogueIndex()
    {
        if (StudentPrefs.GetString("GameMode", "") == "NewGame")
        {
            currentDialogueIndex = 0;
            StudentPrefs.DeleteKey("GameMode");
            Debug.Log("New game started - dialogue index reset to 0");
            return;
        }

        if (StudentPrefs.GetString("LoadedFromSave", "false") == "true")
        {
            if (StudentPrefs.HasKey("LoadedDialogueIndex"))
            {
                currentDialogueIndex = StudentPrefs.GetInt("LoadedDialogueIndex");
                StudentPrefs.DeleteKey("LoadedDialogueIndex");
                Debug.Log($"Loaded from save file at dialogue index: {currentDialogueIndex}");
            }
            StudentPrefs.SetString("LoadedFromSave", "false");
        }
        else
        {
            if (StudentPrefs.HasKey("SumerianSceneFive_DialogueIndex"))
            {
                currentDialogueIndex = StudentPrefs.GetInt("SumerianSceneFive_DialogueIndex");
                Debug.Log($"Continuing from previous session at dialogue index: {currentDialogueIndex}");
            }
            else
            {
                currentDialogueIndex = 0;
                Debug.Log("Starting from beginning");
            }
        }

        if (currentDialogueIndex >= dialogueLines.Length)
            currentDialogueIndex = dialogueLines.Length - 1;
        if (currentDialogueIndex < 0)
            currentDialogueIndex = 0;
    }

    void SetupButtons()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(ShowNextDialogue);

        if (backButton != null)
            backButton.onClick.AddListener(ShowPreviousDialogue);

        // Setup save button
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(SaveAndLoad);
        }
        else
        {
            GameObject saveButtonObj = GameObject.Find("SaveBT");
            if (saveButtonObj != null)
            {
                Button foundSaveButton = saveButtonObj.GetComponent<Button>();
                if (foundSaveButton != null)
                {
                    foundSaveButton.onClick.AddListener(SaveAndLoad);
                    Debug.Log("Save button found and connected!");
                }
            }
        }

        // Setup home button
        if (homeButton != null)
        {
            homeButton.onClick.AddListener(Home);
        }
        else
        {
            GameObject homeButtonObj = GameObject.Find("HomeBt");
            if (homeButtonObj != null)
            {
                Button foundHomeButton = homeButtonObj.GetComponent<Button>();
                if (foundHomeButton != null)
                {
                    foundHomeButton.onClick.AddListener(Home);
                    Debug.Log("Home button found and connected!");
                }
            }
        }

        // Setup settings button
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(GoToSettings);
        }
        else
        {
            GameObject settingsButtonObj = GameObject.Find("SettingsBT");
            if (settingsButtonObj != null)
            {
                Button foundSettingsButton = settingsButtonObj.GetComponent<Button>();
                if (foundSettingsButton != null)
                {
                    foundSettingsButton.onClick.AddListener(GoToSettings);
                    Debug.Log("Settings button found and connected!");
                }
            }
        }
    }

    void ShowDialogue()
    {
        IshmacharacterRenderer.enabled = false;
        Gulong.SetActive(false);
        DialogueLine line = dialogueLines[currentDialogueIndex];
        dialogueText.text = $"<b>{line.characterName}</b>: {line.line}";

        if (audioSource != null && dialogueClips != null && currentDialogueIndex < dialogueClips.Length)
        {
            audioSource.clip = dialogueClips[currentDialogueIndex];
            audioSource.Play();
        }

        switch (currentDialogueIndex)
        {
            case 0:
                IshmacharacterRenderer.enabled = true;
                if (NPCFulldrawnAnimator != null)
                    NPCFulldrawnAnimator.Play("Patesi_FullDrawn", 0, 0f);
                if (chronoAnimator != null)
                    chronoAnimator.Play("Patesi_Formal (Idle)", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Curious", 0, 0f);
                break;
            case 1:
                IshmacharacterRenderer.enabled = false;
                if (chronoAnimator != null)
                    chronoAnimator.Play("Patesi_Explaining", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Eager", 0, 0f);
                break;
            case 2:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Ishma_Explaining", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Smiling", 0, 0f);
                break;
            case 3:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Ishma_Explaining", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Reflective", 0, 0f);
                break;
            case 4:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Ishma_Amazed", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Talking", 0, 0f);
                break;
            case 5:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Reflective", 0, 0f);
                Gulong.SetActive(true);
                break;
            case 6:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Ishma_Explaining", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Curious", 0, 0f);
                break;
            case 7:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Eager", 0, 0f);
                break;
            case 8:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Ishma_Smirking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Talking", 0, 0f);
                break;
            case 9:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Ishma_Explaining", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Smiling", 0, 0f);
                break;
            case 10:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Patesi_Explaining", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Reflective", 0, 0f);
                break;
        }
    }

    void ShowNextDialogue()
    {
        currentDialogueIndex++;
        SaveCurrentProgress();

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("SumerianFourthRecallChallenges");
            nextButton.interactable = false;
            return;
        }
        ShowDialogue();
    }

    void ShowPreviousDialogue()
    {
        if (currentDialogueIndex > 0)
        {
            currentDialogueIndex--;
            SaveCurrentProgress();
            ShowDialogue();
        }
    }

    void SaveCurrentProgress()
    {
        StudentPrefs.SetInt("SumerianSceneFive_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("CurrentScene", "SumerianSceneFive");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();
    }

    public void SaveAndLoad()
    {
        StudentPrefs.SetInt("SumerianSceneFive_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("LastScene", "SumerianSceneFive");

        StudentPrefs.DeleteKey("AccessMode");
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();

        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SetCurrentGameState("SumerianSceneFive", currentDialogueIndex);
        }

        Debug.Log($"Going to save menu with dialogue index: {currentDialogueIndex} from SumerianScene5");
        SceneManager.LoadScene("SaveAndLoadScene");
    }

    public void GoToSettings()
    {
        // Save current progress
        SaveCurrentProgress();

        // Mark that we're coming from a story scene
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("LastScene", "SumerianSceneFive");
        StudentPrefs.Save();

        Debug.Log("Going to Settings from SumerianSceneFive");
        SceneManager.LoadScene("Settings");
    }

    public void Home()
    {
        SaveCurrentProgress();
        SceneManager.LoadScene("TitleScreen");
    }

    public void ManualSave()
    {
        SaveCurrentProgress();
        Debug.Log($"Manual save completed at dialogue {currentDialogueIndex}");
    }
}
