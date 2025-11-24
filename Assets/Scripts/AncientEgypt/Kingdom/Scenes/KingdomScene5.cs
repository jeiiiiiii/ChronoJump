using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class KingdomScene5 : MonoBehaviour
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
    public GameObject AchievementUnlockedRenderer;
    public Sprite PlayerReflective;
    public Sprite PlayerSmile;
    public SpriteRenderer ChronocharacterRenderer;
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

        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Tatlong panahon, tatlong philosophy. Ang history ay hindi straight line, ito ay cycle ng pag-angat at pagbagsak. Pero ang aral ay nananatili."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ang kingdoms ay lumipas, pero ang kultura ay immortal. Handa ka na sa afterlife, pyramids, mummies, at Book of the Dead."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Paalam, tatlong kingdoms. Salamat sa inyong mga aral."
            },
        };

        LoadDialogueIndex();
        SetupButtons();
        ShowDialogue();
    }

    void LoadDialogueIndex()
    {
        if (StudentPrefs.GetString("GameMode", "") == "NewGame")
        {
            currentDialogueIndex = 0;
            StudentPrefs.DeleteKey("GameMode");
            return;
        }

        if (StudentPrefs.GetString("LoadedFromSave", "false") == "true")
        {
            if (StudentPrefs.HasKey("LoadedDialogueIndex"))
            {
                currentDialogueIndex = StudentPrefs.GetInt("LoadedDialogueIndex");
                StudentPrefs.DeleteKey("LoadedDialogueIndex");
            }
            StudentPrefs.SetString("LoadedFromSave", "false");
        }
        else if (StudentPrefs.HasKey("KingdomSceneFive_DialogueIndex"))
        {
            currentDialogueIndex = StudentPrefs.GetInt("KingdomSceneFive_DialogueIndex");
        }
        else
        {
            currentDialogueIndex = 0;
        }

        if (currentDialogueIndex >= dialogueLines.Length)
            currentDialogueIndex = dialogueLines.Length - 1;
        if (currentDialogueIndex < 0)
            currentDialogueIndex = 0;
    }

    void SetupButtons()
    {
        nextButton.onClick.AddListener(ShowNextDialogue);
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
        AchievementUnlockedRenderer.SetActive(false);
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
                PlayercharacterRenderer.sprite = PlayerReflective;
                ChronocharacterRenderer.sprite = ChronoSad;
                break;
            case 1:
                PlayercharacterRenderer.sprite = PlayerSmile;
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 2:
                PlayerAchievementManager.UnlockAchievement("Civilizations");
                PlayercharacterRenderer.sprite = PlayerSmile;
                ChronocharacterRenderer.sprite = ChronoSmile;
                AchievementUnlockedRenderer.SetActive(true);
                break;
        }
    }

    void ShowNextDialogue()
    {
        currentDialogueIndex++;
        SaveCurrentProgress();

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("KingdomQuizTime");
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
        StudentPrefs.SetInt("KingdomSceneFive_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("LastScene", "KingdomSceneFive");
        StudentPrefs.DeleteKey("AccessMode");
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();

        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SetCurrentGameState("KingdomSceneFive", currentDialogueIndex);

        SceneManager.LoadScene("SaveAndLoadScene");
    }

    void SaveCurrentProgress()
    {
        StudentPrefs.SetInt("KingdomSceneFive_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("CurrentScene", "KingdomSceneFive");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();
    }

    public void GoToSettings()
    {
        // Save current progress
        SaveCurrentProgress();

        // Mark that we're coming from a story scene
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("LastScene", "KingdomSceneFive");
        StudentPrefs.Save();

        Debug.Log("Going to Settings from KingdomSceneFive");
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
