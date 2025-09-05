using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class BabylonianScene3 : MonoBehaviour
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
    public Sprite PlayerSmile;
    public Sprite PlayerReflective;
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    public Sprite ChronoCheerful;
    public Sprite ChronoSmile;
    public Sprite HammurabiFirm;
    public Sprite HammurabiWise;

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
                characterName = "PLAYER",
                line = " Ang laki na pala ng sakop ng Babylonia… Akala ko noon maliit lang ito."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Noong una, isa lang itong lungsod. Pero sa pamumuno ni Hammurabi, napasakamay niya ang maraming lugar sa Mesopotamia."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Hindi lang hilaga ang nasakop, pati ang timog. Kabilang na ang Ashur, isa sa mga mahahalagang lungsod noon."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Paano niya nagawa ‘yon? Sa dami ng lungsod-estado, hindi ba’t mahirap pagsamahin ang mga ito?"
            },
            new DialogueLine
            {
                characterName = "HAMMURABI",
                line = " Hindi madali ang magtaguyod ng imperyo. Kailangang timbangin ang lakas ng hukbo at ang katalinuhan sa pamumuno. Gumamit ako ng diplomasya, kasunduan, at sa panahong kinakailangan—digmaan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Hindi lang pala espadang dala mo, kundi pati karunungan."
            },
            new DialogueLine
            {
                characterName = "HAMMURABI",
                line = " Ang tunay na lakas ng isang pinuno ay nasa kakayahang pag-isahin ang kanyang nasasakupan. At iyon ang layunin ng Babylonia."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Dahil dito, kinilala si Hammurabi hindi lamang bilang mandirigma, kundi bilang tagapagtaguyod ng kaayusan at pagkakaisa sa buong rehiyon."
            },
        };
    }

    void LoadDialogueIndex()
    {
    // Check if this is a new game
    if (PlayerPrefs.GetString("GameMode", "") == "NewGame")
    {
        currentDialogueIndex = 0;
        PlayerPrefs.DeleteKey("GameMode");
        Debug.Log("New game started - dialogue index reset to 0");
        return;
    }

    // Check if this is a load operation from save file
    if (PlayerPrefs.GetString("LoadedFromSave", "false") == "true")
    {
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
        if (PlayerPrefs.HasKey("BabylonianSceneThree_DialogueIndex"))
        {
            currentDialogueIndex = PlayerPrefs.GetInt("BabylonianSceneThree_DialogueIndex");
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
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 1:
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 2:
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 3:
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 4:
                ChronocharacterRenderer.sprite = HammurabiFirm;
                PlayercharacterRenderer.sprite = PlayerSmile;
                break;
            case 5:
                PlayercharacterRenderer.sprite = PlayerCurious;
                break;
            case 6:
                ChronocharacterRenderer.sprite = HammurabiFirm;
                break;
            case 7:
                ChronocharacterRenderer.sprite = ChronoCheerful;
                break;
        }
    }

    void ShowNextDialogue()
    {
        currentDialogueIndex++;
        SaveCurrentProgress();

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("BabylonianSceneFour");
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
        PlayerPrefs.SetInt("BabylonianSceneThree_DialogueIndex", currentDialogueIndex);
        PlayerPrefs.SetString("LastScene", "BabylonianSceneThree");
        PlayerPrefs.DeleteKey("AccessMode");
        PlayerPrefs.SetString("SaveSource", "StoryScene");
        PlayerPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        PlayerPrefs.Save();

        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SetCurrentGameState("BabylonianSceneThree", currentDialogueIndex);

        SceneManager.LoadScene("SaveAndLoadScene");
    }

    void SaveCurrentProgress()
    {
        PlayerPrefs.SetInt("BabylonianSceneThree_DialogueIndex", currentDialogueIndex);
        PlayerPrefs.SetString("CurrentScene", "BabylonianSceneThree");
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
