using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class SumerianScene3 : MonoBehaviour
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
    public Sprite PlayerEager;
    public Sprite PlayerSmile;
    public Sprite PlayerReflective;
    public Sprite PlayerEmbarassed;

    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoCheerful;
    public Sprite ChronoSad;
    public Sprite ChronoSmile;
    public Sprite ChronoThinking;
    public SpriteRenderer ZulcharacterRenderer;
    public Sprite ZulFriendly;
    public Sprite ZulFrustrated;
    public Sprite ZulNeutral;
    public Sprite ZulTired;
    public Sprite EnkiKind;
    public Sprite EnkiPokerface;
    public Sprite EnkiStern;
    public Sprite EnkiTesting;
    public Animator chronoAnimator;
    public Animator playerAnimator;
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
                characterName = "ENKI",
                line = " Ito na ang huli kong ihahatid sa inyo. Sa taniman na ito, makikilala ninyo si Zul , isa sa mga pinakamagagaling na tagapangalaga ng aming irigasyon."
            },
            new DialogueLine
            {
                characterName = "ZUL",
                line = " Enki! May bisita ka pala."
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Oo, mga bagong mukhang dapat makakita kung paanong pinapakilos ng tubig ang kabihasnan."
            },
            new DialogueLine
            {
                characterName = "ZUL",
                line = " Aba, kung gano'n… halina't samahan ninyo ako. Ituturo ko sa inyo kung paano binubuhay ng ilog ang aming lupa."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Ang lawak ng mga taniman... at ang linis ng daloy ng tubig."
            },
            new DialogueLine
            {
                characterName = "ZUL",
                line = " Ang tubig ay buhay. Dito sa aming lupain, halos walang ulan. Ang mga ilog ang aming sandigan."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ang Mesopotamia ay tinatawag ding 'lupain sa pagitan ng dalawang ilog.' Ang Tigris at Euphrates. Kapag umapaw ang mga ilog na ito, iniiwan nila ang banlik , isang matabang uri ng lupa."
            },
            new DialogueLine
            {
                characterName = "ZUL",
                line = " Iyon ang dahilan kung bakit sagana ang aming ani. Pero hindi ganoon kadali ang lahat. Kailangan ang sipag at talino para maayos ang daloy ng tubig. Kaya kami nagtayo ng mga kanal at imbakan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Pero… paano ninyo napapagana ang ganito kalawak na sistema?"
            },
            new DialogueLine
            {
                characterName = "ZUL",
                line = " Hindi ito gawa ng iisang tao. Lahat kami ay may tungkulin. Ang ilan ay nagtatalaga kung kailan bubuksan ang kanal, ang iba naman ay nagbabantay sa antas ng tubig. May mga panahong kailangang isara ang daluyan kapag sobra na ang agos."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ang sistemang ito ay tinatawag na irigasyon. Isa ito sa pinakamahalagang ambag ng mga Sumerian sa kabihasnan."
            },
            new DialogueLine
            {
                characterName = "ZUL",
                line = " Wala kaming masyadong ulan dito, pero may paraan kami para hindi magkulang sa tubig ang aming mga pananim."
            },
        };
    }

    void LoadDialogueIndex()
    {
        // Check if this is a new game
        if (StudentPrefs.GetString("GameMode", "") == "NewGame")
        {
            currentDialogueIndex = 0;
            StudentPrefs.DeleteKey("GameMode"); // Clear the flag after use
            Debug.Log("New game started - dialogue index reset to 0");
            return;
        }

        // Check if this is a load operation from save file
        if (StudentPrefs.GetString("LoadedFromSave", "false") == "true")
        {
            // Load from save file
            if (StudentPrefs.HasKey("LoadedDialogueIndex"))
            {
                currentDialogueIndex = StudentPrefs.GetInt("LoadedDialogueIndex");
                StudentPrefs.DeleteKey("LoadedDialogueIndex");
                Debug.Log($"Loaded from save file at dialogue index: {currentDialogueIndex}");
            }
            
            // Clear the load flag
            StudentPrefs.SetString("LoadedFromSave", "false");
        }
        else
        {
            // Check for regular scene progression (not from load)
            if (StudentPrefs.HasKey("SumerianSceneThree_DialogueIndex"))
            {
                currentDialogueIndex = StudentPrefs.GetInt("SumerianSceneThree_DialogueIndex");
                Debug.Log($"Continuing from previous session at dialogue index: {currentDialogueIndex}");
            }
            else
            {
                currentDialogueIndex = 0;
                Debug.Log("Starting from beginning");
            }
        }

        // Ensure index is within bounds
        if (currentDialogueIndex >= dialogueLines.Length)
            currentDialogueIndex = dialogueLines.Length - 1;
        if (currentDialogueIndex < 0)
            currentDialogueIndex = 0;
    }

    void SetupButtons()
    {
        // Setup existing buttons
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
            // If save button reference is missing, try to find it by name
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

        // Setup home button if you have one
        if (homeButton != null)
        {
            homeButton.onClick.AddListener(Home);
        }
        else
        {
            // Try to find home button by name
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
        ZulcharacterRenderer.enabled = false;
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
                    chronoAnimator.Play("Enki_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Curious", 0, 0f);
                break;
            case 1:
                ZulcharacterRenderer.enabled = true;
                break;
            case 2:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Enki_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Eager", 0, 0f);
                break;
            case 3:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Zul_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Smiling", 0, 0f);
                break;
            case 4:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Zul_Neutral", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Talking", 0, 0f);
                break;
            case 5:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Zul_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Reflective", 0, 0f);
                break;
            case 6:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Reflective", 0, 0f);
                break;
            case 7:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Zul_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Curious", 0, 0f);
                break;
            case 8:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Thinking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Talking", 0, 0f);
                break;
            case 9:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Zul_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Reflective", 0, 0f);
                break;
            case 10:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Curious", 0, 0f);
                break;
            case 11:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Zul_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Eager", 0, 0f);
                break;
        }
    }

    void ShowNextDialogue()
    {
        currentDialogueIndex++;
        SaveCurrentProgress();

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("SumerianSecondRecallChallenges");
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

    // Save current dialogue index and go to Save/Load scene
    public void SaveAndLoad()
    {
        // Save the current dialogue index and scene info
        StudentPrefs.SetInt("SumerianSceneThree_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("LastScene", "SumerianSceneThree");
        
        // Clear LoadOnly mode and set story scene access
        StudentPrefs.DeleteKey("AccessMode"); // Clear any previous LoadOnly restriction
        StudentPrefs.SetString("SaveSource", "StoryScene"); // Mark that we came from a story scene
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();
        
        // Also directly set the state in SaveLoadManager if it exists
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SetCurrentGameState("SumerianSceneThree", currentDialogueIndex);
        }
        
        Debug.Log($"Going to save menu with dialogue index: {currentDialogueIndex} from story scene - save buttons will be enabled");
        SceneManager.LoadScene("SaveAndLoadScene");
    }

    void SaveCurrentProgress()
    {
        // Save current progress for scene continuity
        StudentPrefs.SetInt("SumerianSceneThree_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("CurrentScene", "SumerianSceneThree");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();
    }

    public void GoToSettings()
    {
        // Save current progress
        SaveCurrentProgress();

        // Mark that we're coming from a story scene
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("LastScene", "SumerianSceneThree");
        StudentPrefs.Save();

        Debug.Log("Going to Settings from SumerianSceneThree");
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