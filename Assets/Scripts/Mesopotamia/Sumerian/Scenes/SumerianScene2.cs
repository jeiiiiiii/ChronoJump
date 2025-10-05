using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class SumerianScene2 : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueLine
    {
        public string characterName;
        public string line;
    }

    [SerializeField] public TextMeshProUGUI dialogueText;
    [SerializeField] public Button nextButton;
    public GameObject Cuneiform;
    public Button backButton;
    
    [Header("UI Buttons")]
    public Button saveButton;
    public Button homeButton;

    public int currentDialogueIndex = 0;

    public DialogueLine[] dialogueLines;

    public SpriteRenderer PlayercharacterRenderer;
    public Sprite PlayerFulldrawn;
    public Sprite PlayerCurious;
    public Sprite PlayerEager;
    public Sprite PlayerSmile;
    public Sprite PlayerReflective;
    public Sprite PlayerEmbarassed;

    public Sprite EnkiKind;
    public Sprite EnkiPokerface;
    public Sprite EnkiStern;
    public Sprite EnkiTesting;
    public SpriteRenderer ChronocharacterRenderer;

    public Sprite ChronoCheerful;
    public Sprite ChronoSad;
    public Sprite ChronoSmile;
    public Sprite ChronoThinking;

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
                line = " Malugod kayong tinatanggap sa lugar kung saan itinatala ang diwa ng aming kabihasnan. Ang bawat pangyayari, ani, kasunduan, batas, lahat ay nakaukit dito."
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Tingnan mo ito. Isang talaan ng mga ani noong nakaraang taon. Kapag kulang ang butil o may nanlinlang sa takal, dito kami bumabalik."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Para bang bawat guhit ay may kwento."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Hindi lang kwento… kundi alaala ng buong lungsod. Kung walang ganitong sistema, magugulo ang pamumuhay ng mga Sumerian."
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Sa halip na tinta, ito ang gamit namin, stylus na yari sa tambo. Ipinipindot ito sa malambot na luwad para makalikha ng mga hugis at simbolo."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Kaya pala mukhang may paekis at pakuwit ang mga guhit…"
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Ang tawag dito sa sistemang ito ay cuneiform. Galing sa salitang nangangahulugang 'hugis-pantusok'. Bawat marka, may ibig sabihin. Bawat tablet, may silbi."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ang mga batang nais maging eskriba ay pinipiling mabuti. Kinakailangang matiyaga at matalas ang pag-iisip. Kapag nagkamali sa pag-ukit, hindi puwedeng burahin. Kailangang magsimula muli."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Parang hindi madaling matutunan…"
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Ngunit tila ikaw ay may malasakit sa aming sining. Sige nga, kung naalala mo , anong tawag sa uri ng pagsusulat na ito?"
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
            if (StudentPrefs.HasKey("SumerianSceneTwo_DialogueIndex"))
            {
                currentDialogueIndex = StudentPrefs.GetInt("SumerianSceneTwo_DialogueIndex");
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
        Cuneiform.SetActive(false);
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
                ChronocharacterRenderer.sprite = EnkiPokerface;
                PlayercharacterRenderer.sprite = PlayerSmile;
                break;
            case 1:
                ChronocharacterRenderer.sprite = EnkiStern;
                PlayercharacterRenderer.sprite = PlayerCurious;
                break;
            case 3:
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 4:
                ChronocharacterRenderer.sprite = EnkiPokerface;
                break;
            case 5:
                PlayercharacterRenderer.sprite = PlayerSmile;
                break;
            case 6:
                ChronocharacterRenderer.sprite = EnkiStern;
                Cuneiform.SetActive(true);
                break;
            case 7:
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 8:
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 9:
                ChronocharacterRenderer.sprite = EnkiTesting;
                break;
        }
    }

    void ShowNextDialogue()
    {
        currentDialogueIndex++;
        SaveCurrentProgress();

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("SumerianFirstRecallChallenges");
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
        StudentPrefs.SetInt("SumerianSceneTwo_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("LastScene", "SumerianSceneTwo");
        
        // Clear LoadOnly mode and set story scene access
        StudentPrefs.DeleteKey("AccessMode"); // Clear any previous LoadOnly restriction
        StudentPrefs.SetString("SaveSource", "StoryScene"); // Mark that we came from a story scene
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();
        
        // Also directly set the state in SaveLoadManager if it exists
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SetCurrentGameState("SumerianSceneTwo", currentDialogueIndex);
        }
        
        Debug.Log($"Going to save menu with dialogue index: {currentDialogueIndex} from story scene - save buttons will be enabled");
        SceneManager.LoadScene("SaveAndLoadScene");
    }

    void SaveCurrentProgress()
    {
        // Save current progress for scene continuity
        StudentPrefs.SetInt("SumerianSceneTwo_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("CurrentScene", "SumerianSceneTwo");
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