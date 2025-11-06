using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class HuangHeScene1 : MonoBehaviour
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
    public SpriteRenderer DayuFulldrawnSprite;

    // public GameObject AchievementUnlockedRenderer;
    public Sprite PlayerReflective;
    public Sprite PlayerCurious;
    public Sprite PlayerEager;
    public Sprite PlayerSmile;
    public Sprite PlayerEmabarrassed;
    public Sprite DayuStern;
    public Sprite DayuCalm;
    public Sprite Dayuwise;
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
                line = " Saan na naman ako? Ang laki ng ilog na 'to... pero bakit dilaw ang tubig? Parang puno ng lupa."
            },
            new DialogueLine
            {   characterName = "CHRONO",
                line = " Maligayang pagdating sa Yellow River o Huang He, ang dugo ng sinaunang China. Ang ilog na ito ay nagbigay ng buhay, ngunit nagdulot din ng kamatayan. Kaya tinawag itong 'China's Sorrow."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " China's Sorrow? Bakit ganyan ang tawag? Hindi ba dapat blessing ang ilog sa isang sibilisasyon?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Dahil sa loess, ang dilaw na lupa na dinadalang ng ilog mula sa kabundukan. Dumadaloy ito mula sa bulubunduking rehiyon, at kapag umaapaw ang ilog, bumabagsak ang mga pader ng luwad, lumulubog ang mga pananim, nawawala ang mga tahanan."
            },
            new DialogueLine
            {
                characterName = "DA YU",
                line = " Sino kayo? Mga dayuhan? Kung nandito kayo upang magtanong kung bakit kami nananatili sa tabi ng ilog na ito kahit mapanganib, ang sagot ay simple: walang ibang mapupuntahan."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Mang Da Yu, kami'y mga tagapagmasid ng kasaysayan. Ipinapakita ko sa aking kasama kung paano nagsimula ang dakilang sibilisasyong Tsino, dito, sa tabi ng ilog na nagdudulot ng luha at pag-asa."
            },
            new DialogueLine
            {
                characterName = "DA YU",
                line = " Tama ka. Ang ilog na ito ay pumapahid ng lupa sa aming mga bukid, kaya sagana ang ani. Ngunit kapag galit ito, nilulunod nito ang lahat. Minsan blessing, minsan sumpa."
            },
            new DialogueLine
            {
                characterName = "DA YU",
                line = " Tingnan mo ang North China Plain, ang lupain sa pagitan ng Yellow River at Yangtze River. Ito ang pinakamataba, pinakaproduktibong lupain sa buong China. Dito nagsimula ang aming mga ninuno."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Pero kung ganoon ka-dangerous ang ilog, bakit hindi kayo lumipat sa mas safe na lugar?"
            },
            new DialogueLine
            {
                characterName = "DA YU",
                line = " Dahil protektado tayo. Tingnan mo, sa timog, ang Himalayas. Sa kanluran, ang Taklamakan at Tibet. Sa hilaga, ang Gobi desert. Mga natural na pader na hadlang sa mga mananakop. Dito tayo ligtas, at dito tayo nabuhay."
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
            if (StudentPrefs.HasKey("HuangHeSceneOne_DialogueIndex"))
            {
                currentDialogueIndex = StudentPrefs.GetInt("HuangHeSceneOne_DialogueIndex");
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
        DayuFulldrawnSprite.enabled = false;

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
                DayuFulldrawnSprite.enabled = true;
                PlayercharacterRenderer.sprite = PlayerSmile;
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 5:
                PlayercharacterRenderer.sprite = PlayerEager;
                ChronocharacterRenderer.sprite = ChronoCheerful;
                break;
            case 6:
                PlayercharacterRenderer.sprite = PlayerSmile;
                ChronocharacterRenderer.sprite = Dayuwise;
                break;
            case 7:
                PlayercharacterRenderer.sprite = PlayerReflective;
                ChronocharacterRenderer.sprite = Dayuwise;
                break;
            case 8:
                PlayercharacterRenderer.sprite = PlayerEager;
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 9:
                PlayercharacterRenderer.sprite = PlayerReflective;
                ChronocharacterRenderer.sprite = DayuStern;
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
            SceneManager.LoadScene("HuangHeFirstRecallChallenges");
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
        StudentPrefs.SetInt("HuangHeSceneOne_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("LastScene", "HuangHeSceneOne");
        StudentPrefs.DeleteKey("AccessMode");    
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();

        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SetCurrentGameState("HuangHeSceneOne", currentDialogueIndex);

        SceneManager.LoadScene("SaveAndLoadScene");
    }

    void SaveCurrentProgress()
    {
        StudentPrefs.SetInt("HuangHeSceneOne_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("CurrentScene", "HuangHeSceneOne");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();
    }

    public void GoToSettings()
    {
        // Save current progress
        SaveCurrentProgress();

        // Mark that we're coming from a story scene
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("LastScene", "HuangHeSceneOne");
        StudentPrefs.Save();

        Debug.Log("Going to Settings from HuangHeSceneOne");
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
