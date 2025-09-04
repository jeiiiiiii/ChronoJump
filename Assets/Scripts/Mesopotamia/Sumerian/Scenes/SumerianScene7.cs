using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class SumerianScene7 : MonoBehaviour
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
    public Sprite PlayerSmile;
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoSmile;
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

        // Load saved dialogue index - same logic pattern as Scene 1
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
                characterName = "PLAYER",
                line = " Parang panaginip... pero ang totoo, ang dami kong natutunan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Hindi lang basta luwad... ito'y alaala ng kabihasnang nagbigay daan sa mundo ngayon."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Galing mo! Handa ka na sa susunod na paglalakbay , kapag bukas na ang bagong daan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Simula pa lang pala ‘yon…"
            },
        };
    }

    // Replace the LoadDialogueIndex() method in SumerianScene7.cs with this:
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
            if (PlayerPrefs.HasKey("SumerianSceneSeven_DialogueIndex"))
            {
                currentDialogueIndex = PlayerPrefs.GetInt("SumerianSceneSeven_DialogueIndex");
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
                ChronocharacterRenderer.sprite = ChronoSmile;
                PlayercharacterRenderer.sprite = PlayerSmile;
                break;
        }
    }

    void ShowNextDialogue()
    {
        currentDialogueIndex++;
        SaveCurrentProgress();

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("SumerianQuizTime");
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
        PlayerPrefs.SetInt("SumerianSceneSeven_DialogueIndex", currentDialogueIndex);
        PlayerPrefs.SetString("LastScene", "SumerianSceneSeven");
        
        // NEW: Clear LoadOnly mode and set story scene access
        PlayerPrefs.DeleteKey("AccessMode"); // Clear any previous LoadOnly restriction
        PlayerPrefs.SetString("SaveSource", "StoryScene"); // Mark that we came from a story scene
        PlayerPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        PlayerPrefs.Save();
        
        // Also directly set the state in SaveLoadManager if it exists
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SetCurrentGameState("SumerianSceneSeven", currentDialogueIndex);
        }
        
        Debug.Log($"Going to save menu with dialogue index: {currentDialogueIndex} from story scene - save buttons will be enabled");
        SceneManager.LoadScene("SaveAndLoadScene");
    }

    void SaveCurrentProgress()
    {
        // Save current progress for scene continuity
        PlayerPrefs.SetInt("SumerianSceneSeven_DialogueIndex", currentDialogueIndex);
        PlayerPrefs.SetString("CurrentScene", "SumerianSceneSeven");
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
