using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class NileScene3 : MonoBehaviour
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
    public Sprite ImhotepCalm;
    public Sprite ImhotepWise;
    public Sprite ImhotepProud;
    public Sprite MeritGrateful;
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    public Sprite ChronoSmile;
    public Sprite ChronoSad;
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
                line = "Ngunit hindi lang pagkain ang kaloob ng Nile. Tingnan mo ang papyrus, ito ang foundation ng Egyptian writing at record-keeping."
            },
            new DialogueLine
            {
                characterName = "IMHOTEP",
                line = " Ang papyrus ay lumalaki sa marshes ng Nile. Kinukuha namin ito, pinapatag, pinapatuyo, at ginagawang papel. Dito namin isinusulat ang aming records, taxes, laws, stories, rituals."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Parang papel natin ngayon?"
            },
            new DialogueLine
            {
                characterName = "IMHOTEP",
                line = " Tama. Walang papyrus, walang nakasulat na kasaysayan. Walang paraan upang mag-record ng information. Ang Egypt ay magiging amnesia, walang alaala ng nakaraan."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " At tingnan mo ang tubig mismo, puno ng isda. Ang Nile perch, catfish, tilapia, lahat ay pagkain. May waterfowl din, geese, ducks. Ang ilog ay hindi lang water source, kundi source ng protein."
            },
            new DialogueLine
            {
                characterName = "MERIT:",
                line = " Tama! Ang aming diet ay wheat bread, beer, vegetables, at isda mula sa Nile. Bihira lang ang karne, reserved lang yan para sa mga rituals at festivals."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Parang lahat ng kailangan nila ay nanggagaling sa ilog..."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Exactly. Transportation din, ang Nile ay dumadaloy patungo sa hilaga. Kaya madali ang travel from south to north gamit ang current. At pabalik gamit ang sail at hangin mula sa Mediterranean."
            },
            new DialogueLine
            {
                characterName = "IMHOTEP",
                line = " Ang ilog ay highway namin. Dinadala namin ang limestone para sa pyramids, granite para sa statues, grain para sa trade, lahat sa pamamagitan ng boats sa Nile."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Pero tandaan, limited ang interaction nila sa labas. Ang kanilang mundo ay umiikot sa Nile. Lahat ng nasa labas ng disyerto ay uncivilized sa kanilang paningin."
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
            if (StudentPrefs.HasKey("NileSceneThree_DialogueIndex"))
            {
                currentDialogueIndex = StudentPrefs.GetInt("NileSceneThree_DialogueIndex");
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
                PlayercharacterRenderer.sprite = PlayerSmile;
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 1:
                PlayercharacterRenderer.sprite = PlayerSmile;
                ChronocharacterRenderer.sprite = ImhotepCalm;
                break;
            case 2:
                PlayercharacterRenderer.sprite = PlayerCurious;
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 3:
                PlayercharacterRenderer.sprite = PlayerReflective;
                ChronocharacterRenderer.sprite = ImhotepWise;
                break;
            case 4:
                PlayercharacterRenderer.sprite = PlayerSmile;
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 5:
                PlayercharacterRenderer.sprite = PlayerReflective;
                ChronocharacterRenderer.sprite = MeritGrateful;
                break;
            case 6:
                PlayercharacterRenderer.sprite = PlayerReflective;
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 7:
                PlayercharacterRenderer.sprite = PlayerCurious;
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 8:
                PlayercharacterRenderer.sprite = PlayerSmile;
                ChronocharacterRenderer.sprite = ImhotepProud;
                break;
            case 9:
                PlayercharacterRenderer.sprite = PlayerReflective;
                ChronocharacterRenderer.sprite = ChronoSad;
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
            SceneManager.LoadScene("NileSceneFour");
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
        StudentPrefs.SetInt("NileSceneThree_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("LastScene", "NileSceneThree");
        StudentPrefs.DeleteKey("AccessMode");
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();

        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SetCurrentGameState("NileSceneThree", currentDialogueIndex);

        SceneManager.LoadScene("SaveAndLoadScene");
    }

    void SaveCurrentProgress()
    {
        StudentPrefs.SetInt("NileSceneThree_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("CurrentScene", "NileSceneThree");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();
    }

    public void GoToSettings()
    {
        // Save current progress
        SaveCurrentProgress();

        // Mark that we're coming from a story scene
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("LastScene", "NileSceneThree");
        StudentPrefs.Save();

        Debug.Log("Going to Settings from NileSceneThree");
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
