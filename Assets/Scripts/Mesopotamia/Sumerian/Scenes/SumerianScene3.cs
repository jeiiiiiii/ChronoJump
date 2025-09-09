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
        if (PlayerPrefs.GetString("GameMode", "") == "NewGame")
        {
            currentDialogueIndex = 0;
            PlayerPrefs.DeleteKey("GameMode"); // Clear the flag after use
            Debug.Log("New game started - dialogue index reset to 0");
            return;
        }

        // Check if this is a load operation from save file
        if (PlayerPrefs.GetString("LoadedFromSave", "false") == "true")
        {
            // Load from save file
            if (PlayerPrefs.HasKey("LoadedDialogueIndex"))
            {
                currentDialogueIndex = PlayerPrefs.GetInt("LoadedDialogueIndex");
                PlayerPrefs.DeleteKey("LoadedDialogueIndex");
                Debug.Log($"Loaded from save file at dialogue index: {currentDialogueIndex}");
            }
            
            // Clear the load flag
            PlayerPrefs.SetString("LoadedFromSave", "false");
        }
        else
        {
            // Check for regular scene progression (not from load)
            if (PlayerPrefs.HasKey("SumerianSceneThree_DialogueIndex"))
            {
                currentDialogueIndex = PlayerPrefs.GetInt("SumerianSceneThree_DialogueIndex");
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
                ChronocharacterRenderer.sprite = EnkiStern;
                break;
            case 1:
                ZulcharacterRenderer.enabled = true;
                break;
            case 2:
                ChronocharacterRenderer.sprite = EnkiKind;
                break;
            case 3:
                PlayercharacterRenderer.sprite = ZulFriendly;
                break;
            case 4:
                PlayercharacterRenderer.sprite = PlayerEager;
                break;
            case 5:
                ChronocharacterRenderer.sprite = ZulTired;
                break;
            case 6:
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 7:
                ChronocharacterRenderer.sprite = ZulFrustrated;
                break;
            case 8:
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 10:
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 11:
                ChronocharacterRenderer.sprite = ZulFriendly;
                break;
            case 12:
                ChronocharacterRenderer.sprite = ZulNeutral;
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
        PlayerPrefs.SetInt("SumerianSceneThree_DialogueIndex", currentDialogueIndex);
        PlayerPrefs.SetString("LastScene", "SumerianSceneThree");
        
        // Clear LoadOnly mode and set story scene access
        PlayerPrefs.DeleteKey("AccessMode"); // Clear any previous LoadOnly restriction
        PlayerPrefs.SetString("SaveSource", "StoryScene"); // Mark that we came from a story scene
        PlayerPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        PlayerPrefs.Save();
        
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
        PlayerPrefs.SetInt("SumerianSceneThree_DialogueIndex", currentDialogueIndex);
        PlayerPrefs.SetString("CurrentScene", "SumerianSceneThree");
        PlayerPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        PlayerPrefs.Save();
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