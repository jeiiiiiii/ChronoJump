using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class HuangHeScene3 : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueLine
    {
        public string characterName;
        public string line;
    }

    [SerializeField] public TextMeshProUGUI dialogueText;
    [SerializeField] public Button nextButton;
    public Button backButton;

    [Header("UI Buttons")]
    public Button saveButton;
    public Button homeButton;

    public Button settingsButton;

    public int currentDialogueIndex = 0;

    public DialogueLine[] dialogueLines;

    public SpriteRenderer PlayercharacterRenderer;
    public Sprite PlayerCurious;
    public Sprite PlayerSmile;
    public Sprite PlayerReflective;
    public Sprite DayuCalm;
    public Sprite DayuWise;
    public Sprite QiWorried;
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    public Sprite ChronoSmile;
    public Sprite ChronoSad;
    public Animator chronoAnimator;
    public Animator playerAnimator;
    public AudioSource audioSource;
    public AudioClip[] dialogueClips;

    void Start()
    {
        if (SaveLoadManager.Instance == null)
        {
            GameObject saveLoadManager = new GameObject("SaveLoadManager");
            saveLoadManager.AddComponent<SaveLoadManager>();
        }

        InitializeDialogueLines();
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
                characterName = "CHRONO",
                line = "Ngunit hindi lang ang ilog ang nag-define sa China. Tingnan mo ang paligid, mga natural na hadlang na naging sanhi kung bakit nag-develop ang China nang hiwalay sa ibang mundo."
            },
            new DialogueLine
            {
                characterName = "DA YU",
                line = " Sa timog, ang Himalayas, ang pinakamataas na bundok sa mundo. Walang makakatawid doon. Sa kanluran, ang Taklamakan at Tibet, mga disyerto at mataas na plateau. Sa hilaga, ang Gobi, tigang na lupain na walang buhay."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Parang natural na fortress..."
            },
            new DialogueLine
            {
                characterName = "DA YU",
                line = " Tama. Kaya ang China ay naging isolated. Walang mananakop mula sa labas, kaya ang banta ay nanggagaling sa loob, o sa kalikasan."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Dahil dito, ang kultura ng China ay nag-develop nang独立 (independently), sariling wika, sariling sistema ng pagsulat, sariling pilosopiya. Hindi tulad ng Mesopotamia na palaging sinasalakay, o Egypt na nakikipag-trade sa Mediterranean."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Ibig sabihin... sila lang talaga? Walang outside influence?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Halos wala. Kaya naman sa paningin ng mga Tsino, ang kanilang lupain ay sentro ng mundo, ang 'Middle Kingdom.' Ang lahat ng nasa labas ay barbaro, hindi sibilisado."
            },
            new DialogueLine
            {
                characterName = "QI",
                line = " Hindi namin kilala ang ibang mundo... kaya sa amin, kami lang ang mundo."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = "Parang... nakakulong sila sa sarili nilang mundo?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Oo. At yan ay magiging blessing at curse sa hinaharap ng China."
            },
        };
    }

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
            if (StudentPrefs.HasKey("HuangHeSceneThree_DialogueIndex"))
            {
                currentDialogueIndex = StudentPrefs.GetInt("HuangHeSceneThree_DialogueIndex");
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

        if (saveButton != null)
            saveButton.onClick.AddListener(SaveAndLoad);
        else
        {
            GameObject saveButtonObj = GameObject.Find("SaveBT");
            if (saveButtonObj != null)
            {
                Button foundSaveButton = saveButtonObj.GetComponent<Button>();
                if (foundSaveButton != null)
                    foundSaveButton.onClick.AddListener(SaveAndLoad);
            }
        }

        if (homeButton != null)
            homeButton.onClick.AddListener(Home);
        else
        {
            GameObject homeButtonObj = GameObject.Find("HomeBt");
            if (homeButtonObj != null)
            {
                Button foundHomeButton = homeButtonObj.GetComponent<Button>();
                if (foundHomeButton != null)
                    foundHomeButton.onClick.AddListener(Home);
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
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Reflective", 0, 0f);
                break;
            case 1:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Da Yu_Wise", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Smiling", 0, 0f);
                break;
            case 2:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Da Yu_Idle", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Talking", 0, 0f);
                break;
            case 3:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Da Yu_Proud", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Curious", 0, 0f);
                break;
            case 4:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Reflective", 0, 0f);
                break;
            case 5:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Thinking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Talking", 0, 0f);
                break;
            case 6:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Reflective", 0, 0f);
                break;
            case 7:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Qi_Worried", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Curious", 0, 0f);
                break;
            case 8:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Qi_Calm", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Curious", 0, 0f);
                break;
            case 9:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Reflective", 0, 0f);
                break;
        }

        SaveCurrentProgress();
    }

    void ShowNextDialogue()
    {
        currentDialogueIndex++;
        SaveCurrentProgress();

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("HuangHeSceneFour");
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

    public void SaveAndLoad()
    {
        StudentPrefs.SetInt("HuangHeSceneThree_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("LastScene", "HuangHeSceneThree");
        StudentPrefs.DeleteKey("AccessMode");
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();

        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SetCurrentGameState("HuangHeSceneThree", currentDialogueIndex);

        SceneManager.LoadScene("SaveAndLoadScene");
    }

    void SaveCurrentProgress()
    {
        StudentPrefs.SetInt("HuangHeSceneThree_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("CurrentScene", "HuangHeSceneThree");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();
    }

    public void GoToSettings()
    {
        // Save current progress
        SaveCurrentProgress();

        // Mark that we're coming from a story scene
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("LastScene", "HuangHeSceneThree");
        StudentPrefs.Save();

        Debug.Log("Going to Settings from HuangHeSceneThree");
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
