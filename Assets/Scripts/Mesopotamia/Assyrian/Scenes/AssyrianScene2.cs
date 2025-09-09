using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class AssyrianScene2 : MonoBehaviour
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

    [Header("Sprites - Player")]
    public SpriteRenderer PlayercharacterRenderer;
    public Sprite PlayerCurious;
    public Sprite PlayerSmile;
    public Sprite PlayerEmbarrassed;
    public Sprite PlayerReflective;

    [Header("Sprites - Ashurbanipal")]
    public SpriteRenderer AshurbanipalFulldrawnSprite;
    public Sprite AshurbanipalWise;

    [Header("Sprites - Chrono")]
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    public Sprite ChronoSmile;
    public Sprite ChronoCheerful;
    

    [Header("Achievements")]
    public GameObject AchievementUnlockedRenderer;

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

        // Load saved progress
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
                line = " Ngunit hindi puro dahas ang sumunod na hari. Si Ashurbanipal, namuno noong 668 hanggang 627 BCE, ay kilala bilang hari ng kaalaman."
            },
            new DialogueLine
            {
                characterName = "ASHURBANIPAL",
                line = " Ako si Ashurbanipal. Hari ng mundo at tagapangalaga ng karunungan. Ang silid-aklatang ito sa Nineveh ay tahanan ng mahigit 200,000 tabletang luwad — mga kasaysayan, batas, medisina, at panalangin."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Ikaw ang hari... pero hindi ka mukhang mandirigma."
            },
            new DialogueLine
            {
                characterName = "ASHURBANIPAL",
                line = " Ako'y parehong espada at stylus. Hindi lang pananakop, kundi pangangalaga ng kultura ang pamana ko."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ang aklatan ni Ashurbanipal ang kauna-unahang silid-aklatan sa kasaysayan ng mundo. Isa ito sa mga dahilan kung bakit kahit sa kalupitan ng Assyria, may natirang anyo ng sibilisasyon."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Ito pala ang pundasyon ng mga aklatan ngayon..."
            },
        };
    }

    // === Load Dialogue Index Logic ===
    void LoadDialogueIndex()
    {
        if (PlayerPrefs.GetString("GameMode", "") == "NewGame")
        {
            currentDialogueIndex = 0;
            PlayerPrefs.DeleteKey("GameMode");
            Debug.Log("New game started - dialogue index reset to 0");
            return;
        }

        if (PlayerPrefs.GetString("LoadedFromSave", "false") == "true")
        {
            if (PlayerPrefs.HasKey("LoadedDialogueIndex"))
            {
                currentDialogueIndex = PlayerPrefs.GetInt("LoadedDialogueIndex");
                PlayerPrefs.DeleteKey("LoadedDialogueIndex");
                Debug.Log($"Loaded from save file at dialogue index: {currentDialogueIndex}");
            }

            PlayerPrefs.SetString("LoadedFromSave", "false");
        }
        else
        {
            if (PlayerPrefs.HasKey("AssyrianSceneTwo_DialogueIndex"))
            {
                currentDialogueIndex = PlayerPrefs.GetInt("AssyrianSceneTwo_DialogueIndex");
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

    // === Setup Buttons ===
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

    // === Show Dialogue ===
    void ShowDialogue()
    {
        AchievementUnlockedRenderer.SetActive(false);
        AshurbanipalFulldrawnSprite.enabled = false;

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
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 1:
                AshurbanipalFulldrawnSprite.enabled = true;
                PlayercharacterRenderer.sprite = PlayerEmbarrassed;
                ChronocharacterRenderer.sprite = ChronoCheerful;
                break;
            case 2:
                PlayercharacterRenderer.sprite = PlayerCurious;
                break;
            case 3:
                ChronocharacterRenderer.sprite = AshurbanipalWise;
                break;
            case 4:
                AchievementUnlockedRenderer.SetActive(true);
                PlayerAchievementManager.UnlockAchievement("Guardian");
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 5:
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
        }
    }

    // === Next Dialogue ===
    void ShowNextDialogue()
    {
        currentDialogueIndex++;
        SaveCurrentProgress();

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("AssyrianSecondRecallChallenges");
            nextButton.interactable = false;
            return;
        }
        ShowDialogue();
    }

    // === Previous Dialogue ===
    void ShowPreviousDialogue()
    {
        if (currentDialogueIndex > 0)
        {
            currentDialogueIndex--;
            SaveCurrentProgress();
            ShowDialogue();
        }
    }

    // === Save & Load ===
    public void SaveAndLoad()
    {
        PlayerPrefs.SetInt("AssyrianSceneTwo_DialogueIndex", currentDialogueIndex);
        PlayerPrefs.SetString("LastScene", "AssyrianSceneTwo");

        PlayerPrefs.DeleteKey("AccessMode");
        PlayerPrefs.SetString("SaveSource", "StoryScene");
        PlayerPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        PlayerPrefs.Save();

        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SetCurrentGameState("AssyrianSceneTwo", currentDialogueIndex);

        Debug.Log($"Going to save menu with dialogue index: {currentDialogueIndex}");
        SceneManager.LoadScene("SaveAndLoadScene");
    }

    // === Save Current Progress Only ===
    void SaveCurrentProgress()
    {
        PlayerPrefs.SetInt("AssyrianSceneTwo_DialogueIndex", currentDialogueIndex);
        PlayerPrefs.SetString("CurrentScene", "AssyrianSceneTwo");
        PlayerPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        PlayerPrefs.Save();
    }

    // === Go Home ===
    public void Home()
    {
        SaveCurrentProgress();
        SceneManager.LoadScene("TitleScreen");
    }

    // === Manual Save ===
    public void ManualSave()
    {
        SaveCurrentProgress();
        Debug.Log($"Manual save completed at dialogue {currentDialogueIndex}");
    }
}
