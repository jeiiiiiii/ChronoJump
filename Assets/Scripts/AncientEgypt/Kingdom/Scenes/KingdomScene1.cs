using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class KingdomScene1 : MonoBehaviour
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
    public SpriteRenderer KhufuFulldrawnSprite;

    // public GameObject AchievementUnlockedRenderer;
    public Sprite PlayerReflective;
    public Sprite PlayerCurious;
    public Sprite PlayerEager;
    public Sprite PlayerEmabarrassed;
    public Sprite KhufuProud;
    public Sprite KhufuStern;

    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    public Sprite ChronoSmile;
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
                line = " Ano... ano 'to?! Ang laki! Paano nila ito ginawa?! Walang crane, walang trucks, paano?!"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Maligayang pagdating sa Lumang Kaharian ng Egypt, 2613 hanggang 2181 BCE. Ito ang 'Age of the Pyramids', ang panahon kung kailan itinayo ang pinakadakilang monuments sa kasaysayan ng sangkatauhan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = "Lumang Kaharian? Ibig sabihin may Gitna at Bago rin?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Tama. Ang Egypt ay may tatlong golden ages, Lumang Kaharian, Gitnang Kaharian, at Bagong Kaharian. Bawat isa ay may sariling character, sariling achievement. At ngayon, nasa unang yugto tayo, ang panahon ng mga piramide."
            },
            new DialogueLine
            {
                characterName = "KHUFU",
                line = " Sino kayong mga mortal na yan? Nakatingin sa aking dakilang gawa? Ako si Khufu, pharaoh ng Egypt, anak ng sun-god Ra! Ang piramideng ito ay magiging aking tahanan sa afterlife, ang aking eternal monument!"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Dakilang Pharaoh Khufu, kami'y mga tagapagmasid ng kasaysayan. Ipinapakita ko sa aking kasama ang inyong pamana, ang Great Pyramid, isa sa Seven Wonders of the Ancient World."
            },
            new DialogueLine
            {
                characterName = "KHUFU",
                line = " Kung gayon, tingnan ninyo. Mahigit dalawang milyon na limestone blocks, bawat isa ay average na 2.5 tons. Libu-libong workers, hindi slaves, kundi paid laborers at skilled craftsmen. Tatagal ito ng 20 years para matapos."
            },
            new DialogueLine
            {
                characterName = "KHUFU",
                line = " Ako ay god-king. Ako ay Ra sa lupa. Ang aking buhay ay sagrado, at ang aking kamatayan ay simula ng aking tunay na buhay sa afterlife. Kaya dapat ang aking libingan ay walang kapantay."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Pero sir... ang dami ng resources na ginastos dito. Paano naman ang mga tao? Hindi ba sila nahirapan?"
            },
            new DialogueLine
            {
                characterName = "KHUFU",
                line = " Ang mga tao ay nabuhay upang maglingkod sa pharaoh. Ako ang nagbibigay ng order, ang nagdadala ng ma'at, balance, truth, at justice. Walang Egypt kung walang pharaoh. Walang prosperity kung walang mga piramide."
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
            if (StudentPrefs.HasKey("KingdomSceneOne_DialogueIndex"))
            {
                currentDialogueIndex = StudentPrefs.GetInt("KingdomSceneOne_DialogueIndex");
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
        KhufuFulldrawnSprite.enabled = false;

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
                KhufuFulldrawnSprite.enabled = true;
                PlayercharacterRenderer.sprite = PlayerEmabarrassed;
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 5:
                PlayercharacterRenderer.sprite = PlayerCurious;
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 6:
                PlayercharacterRenderer.sprite = PlayerEager;
                ChronocharacterRenderer.sprite = KhufuProud;
                break;
            case 7:
                PlayercharacterRenderer.sprite = PlayerCurious;
                ChronocharacterRenderer.sprite = KhufuProud;
                break;
            case 8:
                PlayercharacterRenderer.sprite = PlayerReflective;
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 9:
                PlayercharacterRenderer.sprite = PlayerEager;
                ChronocharacterRenderer.sprite = KhufuStern;
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
            SceneManager.LoadScene("KingdomFirstRecallChallenges");
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
        StudentPrefs.SetInt("KingdomSceneOne_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("LastScene", "KingdomSceneOne");
        StudentPrefs.DeleteKey("AccessMode");    
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();

        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SetCurrentGameState("KingdomSceneOne", currentDialogueIndex);

        SceneManager.LoadScene("SaveAndLoadScene");
    }

    void SaveCurrentProgress()
    {
        StudentPrefs.SetInt("KingdomSceneOne_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("CurrentScene", "KingdomSceneOne");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();
    }

    public void GoToSettings()
    {
        // Save current progress
        SaveCurrentProgress();

        // Mark that we're coming from a story scene
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("LastScene", "KingdomSceneOne");
        StudentPrefs.Save();

        Debug.Log("Going to Settings from KingdomSceneOne");
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
