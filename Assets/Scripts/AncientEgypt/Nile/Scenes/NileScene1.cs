using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class NileScene1 : MonoBehaviour
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
    public SpriteRenderer ImhotepFulldrawnSprite;

    // public GameObject AchievementUnlockedRenderer;
    public Sprite PlayerReflective;
    public Sprite PlayerCurious;
    public Sprite PlayerEager;
    public Sprite PlayerSmile;
    public Sprite ImhotepWise;
    public Sprite ImhotepProud;
    public Sprite ImhotepCalm;
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    public Sprite ChronoSmile;
    public Sprite ChronoSad;
    public Sprite ChronoCheerful;
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
            {   characterName = "PLAYER",
                line = " Saan na naman ako? Ang laki ng ilog na 'to... at bakit parang lahat ng buhay ay nakasentro dito? Walang iba pang settlement malayo sa ilog."
            },
            new DialogueLine
            {   characterName = "CHRONO",
                line = " Maligayang pagdating sa Ilog Nile, ang pinakamahaba't pinakamahalaga sa Africa. Ang Egypt ay 'gift of the Nile', yan ang sinabi ni Herodotus, ang dakilang Greek historian. Walang Nile, walang Egypt."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Gift of the Nile? Bakit ganyan ang tawag? Ano ang special sa ilog na 'to?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Dahil sa annual flooding. Bawat taon, umaapaw ang Nile at nagdadala ng milyun-milyong tonelada ng matabang lupa, ang silt. Kapag humupa ang tubig, naiwan ang tabang lupa na perpekto para sa pagtatanim. Walang ibang sibilisasyon ang may ganitong blessing."
            },
            new DialogueLine
            {
                characterName = "IMHOTEP",
                line = " Sino kayo? Mga dayuhan mula sa disyerto? Kung nandito kayo upang magtanong tungkol sa Nile, makinig kayo. Ang ilog na ito ay hindi simpleng tubig, ito ay buhay, ito ay diyos, ito ay Egypt mismo."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Imhotep, dakilang scribe at engineer, kami'y mga tagapagmasid ng kasaysayan. Ipinapakita ko sa aking kasama kung bakit ang Egypt ay umusbong dito sa lambak ng Nile, at kung paano ang ilog ang naging sentro ng lahat."
            },
            new DialogueLine
            {
                characterName = "IMHOTEP",
                line = " Tama ka. Tingnan mo ang lambak, makitid lang, hindi gaanong malawak. Pero dito nagsimula ang lahat. Ang Nile ay dumadaloy mula sa mataas na Africa, dumadaan sa Egypt, at pumupunta sa Mediterranean Sea. Mahigit 6,600 kilometers ang haba nito."
            },
            new DialogueLine
            {
                characterName = "IMHOTEP",
                line = " Bawat taon, sa panahon ng tag-init, umaapaw ang ilog. Hindi tulad ng Yellow River na nakakasira, ang pagbaha ng Nile ay predictable. Alam namin kung kailan darating, kaya nakakapaghanda kami."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Pero sir... paano kung sobrang taas ng tubig? O paano kung hindi umabot? Hindi ba delikado pa rin?"
            },
            new DialogueLine
            {
                characterName = "IMHOTEP",
                line = " Oo. Kapag kulang ang pagbaha, taggutom. Kapag sobra, wasak ang mga tahanan. Kaya kailangan naming subaybayan, at dyan pumapasok ang aming papel bilang scribes at engineers. Sinusukat namin ang tubig, nirerecord namin ang pattern."
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
            if (StudentPrefs.HasKey("NileSceneOne_DialogueIndex"))
            {
                currentDialogueIndex = StudentPrefs.GetInt("NileSceneOne_DialogueIndex");
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
        // AchievementUnlockedRenderer.SetActive(false);
        ImhotepFulldrawnSprite.enabled = false;

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
                PlayercharacterRenderer.sprite = PlayerSmile;
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 2:
                PlayercharacterRenderer.sprite = PlayerCurious;
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 3:
                PlayercharacterRenderer.sprite = PlayerSmile;
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 4:
                ImhotepFulldrawnSprite.enabled = true;
                PlayercharacterRenderer.sprite = PlayerSmile;
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 5:
                PlayercharacterRenderer.sprite = PlayerEager;
                ChronocharacterRenderer.sprite = ChronoCheerful;
                break;
            case 6:
                PlayercharacterRenderer.sprite = PlayerSmile;
                ChronocharacterRenderer.sprite = ImhotepProud;
                break;
            case 7:
                PlayercharacterRenderer.sprite = PlayerReflective;
                ChronocharacterRenderer.sprite = ImhotepWise;
                break;
            case 8:
                PlayercharacterRenderer.sprite = PlayerEager;
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 9:
                PlayercharacterRenderer.sprite = PlayerReflective;
                ChronocharacterRenderer.sprite = ImhotepCalm;
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
            SceneManager.LoadScene("NileFirstRecallChallenges");
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
        StudentPrefs.SetInt("NileSceneOne_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("LastScene", "NileSceneOne");
        StudentPrefs.DeleteKey("AccessMode");
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();

        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SetCurrentGameState("NileSceneOne", currentDialogueIndex);

        SceneManager.LoadScene("SaveAndLoadScene");
    }

    void SaveCurrentProgress()
    {
        StudentPrefs.SetInt("NileSceneOne_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("CurrentScene", "NileSceneOne");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();
    }

    public void GoToSettings()
    {
        // Save current progress
        SaveCurrentProgress();

        // Mark that we're coming from a story scene
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("LastScene", "NileSceneOne");
        StudentPrefs.Save();

        Debug.Log("Going to Settings from NileSceneOne");
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
