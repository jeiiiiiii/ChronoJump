using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class BabylonianScene4 : MonoBehaviour
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
    public Sprite PlayerReflective;
    public Sprite PlayerEager;

    [Header("Sprites - Hammurabi")]
    public Sprite HammurabiProud;
    public Sprite HammurabiExplaining;
    public Sprite HammurabiWise;

    [Header("Sprites - Chrono")]
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
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
                characterName = "PLAYER",
                line = " Chrono… ano itong sinusulat nila sa mga luwad na ito?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Iyan ang panitikan ng mga taga-Babylonia. Sa panahon ni Hammurabi, umunlad hindi lang ang batas kundi pati sining at panulat."
            },
            new DialogueLine
            {
                characterName = "HAMMURABI",
                line = " Ang mga salitang ito ang magdadala ng aming kwento sa susunod na henerasyon. Isa sa mga pinakaimportanteng isinulat ay ang Epikong Gilgamesh."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Epiko? Parang kwento tungkol sa bayani?"
            },
            new DialogueLine
            {
                characterName = "HAMMURABI",
                line = " Si Gilgamesh ay isang maalamat na hari na naglakbay para hanapin ang lihim ng walang kamatayan. Sa pamamagitan ng kanyang kwento, natututo ang aming bayan tungkol sa tapang, pagkakaibigan, at pagtanggap sa katotohanan ng buhay."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " At hindi lang ‘yan. Isinulat din nila ang Enuma Elish, isang kuwento tungkol sa paglikha ng mundo at kapangyarihan ng diyos na si Marduk."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Grabe… akala ko digmaan lang ang mahalaga. Pero dito, pati mga kwento, pinahahalagahan."
            },
            new DialogueLine
            {
                characterName = "HAMMURABI",
                line = " Ang isang imperyo ay hindi lamang nasusukat sa laki ng nasasakupan, kundi sa lalim ng alaala at karunungang naiiwan."
            },
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
            if (StudentPrefs.HasKey("BabylonianSceneFour_DialogueIndex"))
            {
                currentDialogueIndex = StudentPrefs.GetInt("BabylonianSceneFour_DialogueIndex");
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
                PlayercharacterRenderer.sprite = PlayerCurious;
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 1:
                ChronocharacterRenderer.sprite = ChronoCheerful;
                break;
            case 2:
                PlayercharacterRenderer.sprite = PlayerSmile;
                ChronocharacterRenderer.sprite = HammurabiProud;
                break;
            case 3:
                PlayercharacterRenderer.sprite = PlayerEager;
                break;
            case 4:
                ChronocharacterRenderer.sprite = HammurabiExplaining;
                break;
            case 5:
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 6:
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 7:
                ChronocharacterRenderer.sprite = HammurabiWise;
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
            SceneManager.LoadScene("BabylonianThirdRecallChallenges");
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
        StudentPrefs.SetInt("BabylonianSceneFour_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("LastScene", "BabylonianSceneFour");

        StudentPrefs.DeleteKey("AccessMode");
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();

        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SetCurrentGameState("BabylonianSceneFour", currentDialogueIndex);

        Debug.Log($"Going to save menu with dialogue index: {currentDialogueIndex}");
        SceneManager.LoadScene("SaveAndLoadScene");
    }

    // === Save Current Progress Only ===
    void SaveCurrentProgress()
    {
        StudentPrefs.SetInt("BabylonianSceneFour_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("CurrentScene", "BabylonianSceneFour");
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
