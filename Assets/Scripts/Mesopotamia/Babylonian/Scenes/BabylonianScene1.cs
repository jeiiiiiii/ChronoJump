using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class BabylonianScene1 : MonoBehaviour
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

    public SpriteRenderer PlayercharacterRenderer; // chibi
    public SpriteRenderer ChronocharacterRenderer; // chibi
    public SpriteRenderer HammurabiFulldrawnSprite;

    public Sprite PlayerCurious;
    public Sprite PlayerEager;
    public Sprite PlayerSmile;
    public Sprite PlayerReflective;
    public Sprite PlayerEmbarassed;

    public Sprite ChronoCheerful;
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
            { characterName = "PLAYER",
              line = " Nasaan na naman tayo, Chrono? Parang ibang lugar ito… mas tahimik, pero maayos." },
            new DialogueLine
            { characterName = "CHRONO",
              line = " Nakarating tayo sa Babylonia. Isa itong imperyo na itinatag ng mga Amorite, isang grupo ng mga taong galing sa kanlurang bahagi ng Asya." },
            new DialogueLine
            { characterName = "PLAYER",
              line = " Babylonia? Hindi ko pa naririnig ‘yan. Ano’ng meron dito?" },
            new DialogueLine
            { characterName = "CHRONO",
              line = " Dito mo makikilala ang isa sa mga pinakakilalang hari sa sinaunang daigdig. Si Hammurabi. Siya ang nag-utos na buuin ang isang hanay ng batas para sa mga tao." },
            new DialogueLine
            { characterName = "HAMMURABI",
              line = " Maligayang pagdating sa Babylonia. Ako si Hammurabi, ang tagapamahala ng imperyong ito. Halina at samahan ninyo ako." },
            new DialogueLine
            { characterName = "PLAYER",
              line = " Kayo po ba ang gumawa ng mga sinaunang batas?" },
            new DialogueLine
            { characterName = "CHRONO",
              line = " Oo, siya nga. Tinagurian siyang pinakadakilang hari ng Babylonia dahil sa kanyang matalinong pamamahala." },
        };
    }

    void LoadDialogueIndex()
    {
    // Check if this is a new game
    if (StudentPrefs.GetString("GameMode", "") == "NewGame")
    {
        currentDialogueIndex = 0;
        StudentPrefs.DeleteKey("GameMode");
        Debug.Log("New game started - dialogue index reset to 0");
        return;
    }

    // Check if this is a load operation from save file
    if (StudentPrefs.GetString("LoadedFromSave", "false") == "true")
    {
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
        if (StudentPrefs.HasKey("BabylonianSceneOne_DialogueIndex"))
        {
            currentDialogueIndex = StudentPrefs.GetInt("BabylonianSceneOne_DialogueIndex");
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
        HammurabiFulldrawnSprite.enabled = false;

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
                PlayercharacterRenderer.sprite = PlayerCurious;
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 1:
                ChronocharacterRenderer.sprite = ChronoCheerful;
                break;
            case 2:
                PlayercharacterRenderer.sprite = PlayerEager;
                break;
            case 3:
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 4:
                PlayercharacterRenderer.sprite = PlayerCurious;
                HammurabiFulldrawnSprite.enabled = true;
                break;
            case 5:
                HammurabiFulldrawnSprite.enabled = true;
                ChronocharacterRenderer.sprite = ChronoCheerful;
                PlayercharacterRenderer.sprite = PlayerEmbarassed;
                break;
            case 6:
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
        }
    }

    void ShowNextDialogue()
    {
        currentDialogueIndex++;
        SaveCurrentProgress();

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("BabylonianFirstRecallChallenges");
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
        StudentPrefs.SetInt("BabylonianSceneOne_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("LastScene", "BabylonianSceneOne");
        StudentPrefs.DeleteKey("AccessMode");
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();

        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SetCurrentGameState("BabylonianSceneOne", currentDialogueIndex);

        SceneManager.LoadScene("SaveAndLoadScene");
    }

    void SaveCurrentProgress()
    {
        StudentPrefs.SetInt("BabylonianSceneOne_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("CurrentScene", "BabylonianSceneOne");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();
    }

    public void GoToSettings()
    {
        // Save current progress
        SaveCurrentProgress();

        // Mark that we're coming from a story scene
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("LastScene", "BabylonianSceneOne");
        StudentPrefs.Save();

        Debug.Log("Going to Settings from BabylonianSceneOne");
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
