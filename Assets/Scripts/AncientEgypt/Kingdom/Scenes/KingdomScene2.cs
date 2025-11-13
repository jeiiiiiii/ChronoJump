using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class KingdomScene2 : MonoBehaviour
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
    public SpriteRenderer MentuhotepFulldrawnSprite;

    // public GameObject AchievementUnlockedRenderer;
    public Sprite PlayerReflective;
    public Sprite PlayerCurious;
    public Sprite PlayerEager;
    public Sprite PlayerEmabarrassed;
    public Sprite MentuhotepWise;
    public Sprite KhufuCommanding;
    public Sprite KhufuProud;
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    public Sprite ChronoSmile;
    public Sprite ChronoSad;
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
            {   characterName = "CHRONO",
                line = " Ngunit ang glory ay hindi tumagal. Sa pagtatapos ng Lumang Kaharian, ang Egypt ay bumagsak sa chaos. Ang constant pyramid building ay ubos na ang treasury. Ang mga provincial governors ay nag-rebel."
            },
            new DialogueLine
            {
                characterName = "MGA TAO",
                line = " Bakit kami patuloy na nagbabayad ng taxes para sa mga libingan ng pharaoh?! Gutom na kami!"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ito ang tinatawag na First Intermediate Period, 200 years ng civil war, famine, at political instability. Walang central power, walang order. Ang Egypt ay divided."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Kaya... bumagsak sila dahil sa sobrang gastos sa pyramids?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Isa yan sa dahilan. Pero dumating ang pagbabago, ang reunification. At yan ang simula ng Gitnang Kaharian."
            },
            new DialogueLine
            {
                characterName = "MENTUHOTEP",
                line = " Ako si Mentuhotep II. Hindi ako god-king tulad ni Khufu. Ako ay 'shepherd of the people', tagapangalaga ng aking bayan. Ang aking tungkulin ay maglingkod, hindi magpahirap."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Shepherd of the people? Ibig sabihin... nagbago ang role ng pharaoh?"
            },
            new DialogueLine
            {
                characterName = "MENTUHOTEP",
                line = " Tama. Natutunan namin ang lesson ng nakaraan. Ang power ay hindi privilege, ito ay responsibility. Kaya sa halip na pyramids, nagtayo kami ng irrigation canals, nag-expand ng trade, nag-restore ng batas at kaayusan."
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
            if (StudentPrefs.HasKey("KingdomSceneTwo_DialogueIndex"))
            {
                currentDialogueIndex = StudentPrefs.GetInt("KingdomSceneTwo_DialogueIndex");
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
        MentuhotepFulldrawnSprite.enabled = false;

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
                ChronocharacterRenderer.sprite = ChronoSad;
                break;
            case 1:
                PlayercharacterRenderer.sprite = PlayerEager;
                ChronocharacterRenderer.sprite = KhufuProud;
                break;
            case 2:
                PlayercharacterRenderer.sprite = PlayerReflective;
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 3:
                PlayercharacterRenderer.sprite = PlayerCurious;
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 4:
                PlayercharacterRenderer.sprite = PlayerEmabarrassed;
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 5:
                MentuhotepFulldrawnSprite.enabled = true;
                PlayercharacterRenderer.sprite = PlayerReflective;
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 6:
                PlayercharacterRenderer.sprite = PlayerCurious;
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 7:
                PlayercharacterRenderer.sprite = PlayerReflective;
                ChronocharacterRenderer.sprite = MentuhotepWise;
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
            SceneManager.LoadScene("KingdomSecondRecallChallenges");
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
        StudentPrefs.SetInt("KingdomSceneTwo_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("LastScene", "KingdomSceneTwo");
        StudentPrefs.DeleteKey("AccessMode");    
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();

        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SetCurrentGameState("KingdomSceneTwo", currentDialogueIndex);

        SceneManager.LoadScene("SaveAndLoadScene");
    }

    void SaveCurrentProgress()
    {
        StudentPrefs.SetInt("KingdomSceneTwo_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("CurrentScene", "KingdomSceneTwo");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();
    }

    public void GoToSettings()
    {
        // Save current progress
        SaveCurrentProgress();

        // Mark that we're coming from a story scene
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("LastScene", "KingdomSceneTwo");
        StudentPrefs.Save();

        Debug.Log("Going to Settings from KingdomSceneTwo");
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
