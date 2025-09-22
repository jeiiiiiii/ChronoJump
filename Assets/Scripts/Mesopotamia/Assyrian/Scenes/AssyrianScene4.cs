using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class AssyrianScene4 : MonoBehaviour
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
    public Sprite PlayerSmile;
    public Sprite PlayerEmbarrassed;

    [Header("Sprites - Chrono & Ashurbanipal")]
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    public Sprite ChronoSad;
    public Sprite AshurbanipalSomber;

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
                line = " Ngunit hindi lahat ng imperyo ay walang hanggan. Noong 612 BCE, ang mga dating kaaway ang Chaldean, Medes, at Persian ay nagkaisa upang gibain ang Assyria."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ang pag-aalsa ay pinangunahan ng mga Chaldean na dating sakop ng imperyo, at ngayo’y naghihiganti para sa mga dekadang pang-aabuso."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Chaldean... Medes... Persian. Mga karaniwang pinaghaharian, ngayo'y magkakampi."
            },
            new DialogueLine
            {
                characterName = "ASHURBANIPAL",
                line = " Kakaibang ironya. Sa paghahangad kong ipunin ang kaalaman, hindi ko nailigtas ang Nineveh sa apoy."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Gumuho ang siyudad. Sunog, dugo, at pagbagsak ng isang dating makapangyarihan. Ang takot na ginamit upang maghari ay binalik ng galit na hindi na mapipigil."
            }
        };
    }

    // === Load Dialogue Index Logic ===
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
            if (StudentPrefs.HasKey("AssyrianSceneFour_DialogueIndex"))
            {
                currentDialogueIndex = StudentPrefs.GetInt("AssyrianSceneFour_DialogueIndex");
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
            case 2:
                PlayercharacterRenderer.sprite = PlayerEmbarrassed;
                break;
            case 3:
                ChronocharacterRenderer.sprite = AshurbanipalSomber;
                break;
            case 4:
                ChronocharacterRenderer.sprite = ChronoSad;
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
            SceneManager.LoadScene("AssyrianThirdRecallChallenges");
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
        StudentPrefs.SetInt("AssyrianSceneFour_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("LastScene", "AssyrianSceneFour");

        StudentPrefs.DeleteKey("AccessMode");
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();

        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SetCurrentGameState("AssyrianSceneFour", currentDialogueIndex);

        Debug.Log($"Going to save menu with dialogue index: {currentDialogueIndex}");
        SceneManager.LoadScene("SaveAndLoadScene");
    }

    // === Save Current Progress Only ===
    void SaveCurrentProgress()
    {
        StudentPrefs.SetInt("AssyrianSceneFour_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("CurrentScene", "AssyrianSceneFour");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();
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
