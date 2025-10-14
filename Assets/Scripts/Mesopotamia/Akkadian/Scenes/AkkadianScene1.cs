using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class AkkadianScene1 : MonoBehaviour
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
    public SpriteRenderer PlayerFulldrawnSprite;
    public SpriteRenderer ChronoFulldrawnSprite;

    public GameObject AchievementUnlockedRenderer;
    public Sprite PlayerFulldrawn;
    public Sprite PlayerCurious;
    public Sprite PlayerEager;
    public Sprite PlayerSmile;

    public SpriteRenderer SargoncharacterRenderer;
    public Sprite SargonCommand;

    public SpriteRenderer ChronocharacterRenderer;
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
            {   characterName = "PLAYER",
                line = " Sandali… ibang lugar ‘to. Parang mas... masigla pero mas istrikto."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Tama ang pakiramdam mo. Nandito tayo sa lungsod ng Akkad, hilagang bahagi ng Mesopotamia. Isang lungsod na hindi lang ordinaryo. Ito ang puso ng kauna-unahang imperyo sa daigdig"
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Imperyo? Ibig sabihin… maraming bayan ang pinagsama?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " At ang gumawa nito ay si Sargon. Isang dakilang pinuno mula sa hindi maharlikang lahi pero binago ang kasaysayan."
            },
            new DialogueLine
            {
                characterName = "SARGON I",
                line = " Ako si Sargon ng Akkad. Hindi ako ipinanganak na hari pero pinili akong manguna."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Siya ba talaga si…?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Oo. Si Sargon I. Pinuno ng Akkadian Empire. Magmula sa simpleng pinuno ng hukbo, naging tagapagtatag ng imperyo."
            },
            new DialogueLine
            {
                characterName = "SARGON I",
                line = " Halina, samahan ninyo ako. Ipakikita ko kung paano nagsimula ang lahat."
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
            if (StudentPrefs.HasKey("AkkadianSceneOne_DialogueIndex"))
            {
                currentDialogueIndex = StudentPrefs.GetInt("AkkadianSceneOne_DialogueIndex");
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
        AchievementUnlockedRenderer.SetActive(false);
        SargoncharacterRenderer.enabled = false;
        PlayerFulldrawnSprite.enabled = false;
        ChronoFulldrawnSprite.enabled = false;

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
                PlayerFulldrawnSprite.enabled = true;
                PlayercharacterRenderer.enabled = false;
                ChronocharacterRenderer.enabled = false;
                break;
            case 1:
                ChronoFulldrawnSprite.enabled = true;
                PlayercharacterRenderer.enabled = true;
                PlayercharacterRenderer.sprite = PlayerSmile;
                break;
            case 2:
                PlayercharacterRenderer.sprite = PlayerCurious;
                ChronocharacterRenderer.enabled = true;
                break;
            case 3:
                ChronocharacterRenderer.sprite = ChronoCheerful;
                break;
            case 4:
                PlayerAchievementManager.UnlockAchievement("Rise");
                SargoncharacterRenderer.enabled = true;
                AchievementUnlockedRenderer.SetActive(true);
                break;
            case 5:
                ChronocharacterRenderer.sprite = ChronoCheerful;
                PlayercharacterRenderer.sprite = PlayerEager;
                AchievementUnlockedRenderer.SetActive(false);
                break;
            case 6:
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 7:
                ChronocharacterRenderer.sprite = SargonCommand;
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
            SceneManager.LoadScene("AkkadianSceneTwo");
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
        StudentPrefs.SetInt("AkkadianSceneOne_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("LastScene", "AkkadianSceneOne");
        StudentPrefs.DeleteKey("AccessMode");    
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();

        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SetCurrentGameState("AkkadianSceneOne", currentDialogueIndex);

        SceneManager.LoadScene("SaveAndLoadScene");
    }

    void SaveCurrentProgress()
    {
        StudentPrefs.SetInt("AkkadianSceneOne_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("CurrentScene", "AkkadianSceneOne");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();
    }

    public void GoToSettings()
    {
        // Save current progress
        SaveCurrentProgress();

        // Mark that we're coming from a story scene
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("LastScene", "AkkadianSceneOne");
        StudentPrefs.Save();

        Debug.Log("Going to Settings from AkkadianSceneOne");
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
