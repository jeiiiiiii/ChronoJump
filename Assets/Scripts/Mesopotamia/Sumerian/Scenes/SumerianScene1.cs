using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class SumerianScene1 : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueLine
    {
        public string characterName;
        public string line;
    }

    [SerializeField] public TextMeshProUGUI dialogueText;
    [SerializeField] public Button nextButton;
    public SpriteRenderer PastBG;
    public SpriteRenderer NewBG;
    public Button backButton;
    
    [Header("UI Buttons")]
    public Button saveButton;
    public Button homeButton;

    public int currentDialogueIndex = 0;
    public DialogueLine[] dialogueLines;

    public SpriteRenderer PlayercharacterRenderer;
    public SpriteRenderer PlayerFulldrawnSprite;
    public SpriteRenderer ChronoFulldrawnSprite;
    public Sprite PlayerFulldrawn;
    public Sprite PlayerCurious;
    public Sprite PlayerEager;
    public Sprite PlayerSmile;
    public Sprite PlayerReflective;
    public Sprite PlayerEmbarassed;

    public SpriteRenderer EnkicharacterRenderer;
    public Sprite EnkiKind;
    public Sprite EnkiPokerface;
    public Sprite EnkiStern;
    public Sprite EnkiTesting;
    public SpriteRenderer ChronocharacterRenderer;

    public Sprite ChronoCheerful;
    public Sprite ChronoSad;
    public Sprite ChronoSmile;
    public Sprite ChronoThinking;

    // ======= VOICE NARRATION ADDITION #1 =======
    // Add these two variables for voice narration
    public AudioSource audioSource;
    public AudioClip[] dialogueClips;
    // ==========================================

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

        // Load saved dialogue index - THIS IS THE KEY FIX
        LoadDialogueIndex();

        SetupButtons();
        ShowDialogue();
    }

    void InitializeDialogueLines()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {   characterName = "PLAYER",
                line = " Ano 'to…? Ang daming putik... Teka, hindi ito 'yung kwarto ko!"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Huwag kang mag-alala. Nandito tayo ngayon sa Uruk. Isa sa mga unang lungsod sa kasaysayan. Kabihasnang Sumerian , mga tatlong libong taon bago pa ipinanganak si Kristo (3300 BCE)."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Parang disyerto, pero may mga pananim... at ang taas ng gusaling 'yon!"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ziggurat ang tawag diyan. Sentro ng lungsod. Hindi sila umaasa sa ulan, kaya may mga kanal at imbakan ng tubig sila. Lahat may gamit, lahat may layunin."
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Ano't may mga mukhang banyaga sa gitna ng Uruk? Hmm… hindi kayo taga-rito."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " A-ah... eh, opo. Ngayon lang po ako napadpad dito."
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Huwag kang matakot. Kami'y hindi sarado sa panauhin. Ako si Enki, isang eskriba. Tagapag-ingat ng kaalaman ng aming bayan."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Isa siya sa mga tagasulat ng ating kasaysayan. Sa panahon nila, ang pagsulat ay sining, hindi biro."
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Nakatala sa putik ang mga mahahalagang pangyayari, kung ilang sako ng butil ang naani, kung anong batas ang pinairal, o maging ang mga dasal ng mga tao."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Gamit nila ang stylus sa luwad. Tinatawag ang sistemang ito na cuneiform, mga hugis-pakong simbolo na may kahulugan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Kaya pala parang may mga ukit sa luwad..."
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Mukhang may interes ka. Halika't sumama sa akin. Ipapakita ko sa'yo kung paanong ang mga simpleng marka ay nagiging salamin ng buong lungsod."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ihanda mo ang isip mo. Hindi lang ito basta pagsusulat, ito ang puso ng kabihasnan."
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
            if (StudentPrefs.HasKey("SumerianSceneOne_DialogueIndex"))
            {
                currentDialogueIndex = StudentPrefs.GetInt("SumerianSceneOne_DialogueIndex");
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
        EnkicharacterRenderer.enabled = false;
        PlayerFulldrawnSprite.enabled = false;
        ChronoFulldrawnSprite.enabled = false;
        PastBG.enabled = true;
        NewBG.enabled = false;
        
        DialogueLine line = dialogueLines[currentDialogueIndex];
        dialogueText.text = $"<b>{line.characterName}</b>: {line.line}";

        // ======= VOICE NARRATION ADDITION #2 =======
        // This is the CORE voice narration code - plays audio for current dialogue
        if (audioSource != null && dialogueClips != null && currentDialogueIndex < dialogueClips.Length)
        {
            audioSource.clip = dialogueClips[currentDialogueIndex];
            audioSource.Play();
        }
        // ==========================================

        // Your existing sprite logic
        switch (currentDialogueIndex)
        {
            case 0:
                PlayerFulldrawnSprite.enabled = true;
                PlayercharacterRenderer.enabled = false;
                ChronocharacterRenderer.enabled = false;
                break;
            case 1:
                ChronoFulldrawnSprite.enabled = true;
                PlayercharacterRenderer.enabled = true;
                PlayercharacterRenderer.sprite = PlayerEager;
                break;
            case 2:
                PlayercharacterRenderer.sprite = PlayerEager;
                ChronocharacterRenderer.enabled = true;
                PastBG.enabled = false;
                NewBG.enabled = true;
                break;
            case 3:
                ChronocharacterRenderer.sprite = ChronoSmile;
                PastBG.enabled = true;
                NewBG.enabled = false;
                break;
            case 4:
                EnkicharacterRenderer.enabled = true;
                break;
            case 5:
                ChronocharacterRenderer.sprite = EnkiPokerface;
                PlayercharacterRenderer.sprite = PlayerSmile;
                break;
            case 6:
                ChronocharacterRenderer.sprite = EnkiKind;
                break;
            case 7:
                ChronocharacterRenderer.sprite = ChronoCheerful;
                break;
            case 8:
                ChronocharacterRenderer.sprite = EnkiPokerface;
                break;
            case 9:
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 11:
                PlayercharacterRenderer.sprite = PlayerReflective;
                ChronocharacterRenderer.sprite = EnkiKind;
                break;
            case 12:
                PlayercharacterRenderer.sprite = PlayerSmile;
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
        }
    }

    void ShowNextDialogue()
    {
        currentDialogueIndex++;
        SaveCurrentProgress();

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("SumerianSceneTwo");
            nextButton.interactable = false;
            return;
        }
        ShowDialogue(); // This calls ShowDialogue() which now includes voice narration
    }

    void ShowPreviousDialogue()
    {
        if (currentDialogueIndex > 0)
        {
            currentDialogueIndex--;
            SaveCurrentProgress();
            ShowDialogue(); // This calls ShowDialogue() which now includes voice narration
        }
    }

    public void SaveAndLoad()
    {
        // Save the current dialogue index and scene info
        StudentPrefs.SetInt("SumerianSceneOne_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("LastScene", "SumerianSceneOne");
        
        // NEW: Clear LoadOnly mode and set story scene access
        StudentPrefs.DeleteKey("AccessMode"); // Clear any previous LoadOnly restriction
        StudentPrefs.SetString("SaveSource", "StoryScene"); // Mark that we came from a story scene
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();
        
        // Also directly set the state in SaveLoadManager if it exists
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SetCurrentGameState("SumerianSceneOne", currentDialogueIndex);
        }
        
        Debug.Log($"Going to save menu with dialogue index: {currentDialogueIndex} from story scene - save buttons will be enabled");
        SceneManager.LoadScene("SaveAndLoadScene");
    }

    void SaveCurrentProgress()
    {
        // Save current progress for scene continuity
        StudentPrefs.SetInt("SumerianSceneOne_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("CurrentScene", "SumerianSceneOne");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();
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