using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class HarappaScene1 : MonoBehaviour
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
    public SpriteRenderer DaroFulldrawnSprite;
    public SpriteRenderer MatrikaFulldrawnSprite;

    // public GameObject AchievementUnlockedRenderer;
    public Sprite PlayerReflective;
    public Sprite PlayerCurious;
    public Sprite PlayerEager;
    public Sprite PlayerEmabarrassed;
    public Sprite DaroProud;
    public Sprite DaroCalm;
    public Sprite Darowise;
    public Sprite MatrikaMysterious;
    public Sprite MatrikaSomber;

    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
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
            {   characterName = "PLAYER",
                line = " Saan na naman ako? Ang ayos ng mga gusali... parang gawa ng mga engineer. Pero... wala akong nakikitang palasyo o hari."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Maligayang pagdating sa Mohenjo-daro, isa sa pinakamaunlad na lungsod ng Indus Valley Civilization. Panahon ito ng 2500 BCE—isang lungsod na pinamumunuan hindi ng hari, kundi ng sistema at dunong."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Mohenjo-daro? Hindi ba ito ang lungsod na natuklasan lang noong 1920s"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Tumpak. Ang sibilisasyong ito ay nakabaon sa lupa ng mahigit 4,000 taon. Nagsimula ito sa lambak ng Indus River, mula sa kasalukuyang Pakistan hanggang hilagang India. Ang mga tao rito ay namuhay nang mapayapa—walang hukbo, walang digmaan."
            },
            new DialogueLine
            {
                characterName = "DARO",
                line = " Sino kayo? Mga bisita mula sa malayong lupain? Ang Mohenjo-daro ay bukas sa lahat ng mangangalakal. Kami ay hindi gumagamit ng espada—gumagamit kami ng timbangan."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Mangangalakal na Daro, kami'y mga tagapagmasid ng kasaysayan. Ipinapakita ko sa aking kasama ang inyong dakilang pamana—ang lungsod na walang hari, ang sistema na walang digmaan."
            },
            new DialogueLine
            {
                characterName = "DARO",
                line = " Kung gayon, saksihan ninyo kung paano kami namuhay. Tingnan mo ang aming mga kalsada—lahat ay tuwid, nakaayos na parang grid. Walang gulo, walang kalat. Bawat bahay ay konektado sa aming drainage system."
            },
            new DialogueLine
            {
                characterName = "DARO",
                line = " Kami ay nangangalakal hanggang Mesopotamia—lapis lazuli, beads, cotton textiles. Gumagamit kami ng standardized weights upang maging patas ang palitan. Walang nandadaya, walang nag-aaway."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Pero sino ang namumuno? Walang palasyo, walang hari?."
            },
            new DialogueLine
            {
                characterName = "DARO",
                line = " Ang lungsod mismo ang namumuno. Ang sistema, ang kaayusan, ang paggalang sa isa't isa. Hindi namin kailangan ng takot upang magkaisa—kailangan lang namin ng dunong."
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
            if (StudentPrefs.HasKey("HarappaSceneOne_DialogueIndex"))
            {
                currentDialogueIndex = StudentPrefs.GetInt("HarappaSceneOne_DialogueIndex");
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
        DaroFulldrawnSprite.enabled = false;
        MatrikaFulldrawnSprite.enabled = false;

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
                PlayercharacterRenderer.sprite = PlayerEager;
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 2:
                PlayercharacterRenderer.sprite = PlayerReflective;
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 3:
                PlayercharacterRenderer.sprite = PlayerCurious;
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 4:
                DaroFulldrawnSprite.enabled = true;
                PlayercharacterRenderer.sprite = PlayerEmabarrassed;
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 5:
                PlayercharacterRenderer.sprite = PlayerCurious;
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 6:
                PlayercharacterRenderer.sprite = PlayerEager;
                ChronocharacterRenderer.sprite = DaroProud;
                break;
            case 7:
                PlayercharacterRenderer.sprite = PlayerCurious;
                ChronocharacterRenderer.sprite = DaroProud;
                break;
            case 8:
                PlayercharacterRenderer.sprite = PlayerReflective;
                ChronocharacterRenderer.sprite = Darowise;
                break;
            case 9:
                PlayercharacterRenderer.sprite = PlayerEager;
                ChronocharacterRenderer.sprite = DaroCalm;
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
            SceneManager.LoadScene("HarappaFirstRecallChallenges");
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
        StudentPrefs.SetInt("HarappaSceneOne_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("LastScene", "HarappaSceneOne");
        StudentPrefs.DeleteKey("AccessMode");    
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();

        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SetCurrentGameState("HarappaSceneOne", currentDialogueIndex);

        SceneManager.LoadScene("SaveAndLoadScene");
    }

    void SaveCurrentProgress()
    {
        StudentPrefs.SetInt("HarappaSceneOne_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("CurrentScene", "HarappaSceneOne");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();
    }

    public void GoToSettings()
    {
        // Save current progress
        SaveCurrentProgress();

        // Mark that we're coming from a story scene
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("LastScene", "HarappaSceneOne");
        StudentPrefs.Save();

        Debug.Log("Going to Settings from HarappaSceneOne");
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
