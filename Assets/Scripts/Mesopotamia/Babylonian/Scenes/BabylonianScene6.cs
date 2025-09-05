using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class BabylonianScene6 : MonoBehaviour
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

    public SpriteRenderer PlayercharacterRenderer;
    public Sprite PlayerReflective;
    public Sprite PlayerEager;
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoSad;
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
                line = " Chrono… parang may hindi magandang nangyayari. Parang hindi na ganito kasigla kanina."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Tama ang kutob mo. Pagkatapos mamatay ni Hammurabi, unti-unting humina ang pamahalaan ng Babylonia."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Bakit? Hindi ba’t maayos ang pagkakatatag ng imperyo?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Oo, pero kahit matatag ang pamahalaan noong nabubuhay si Hammurabi, mahirap itong mapanatili kung wala na ang matalinong namumuno."
            },
            new DialogueLine
            {
                characterName = "CHRONO ",
                line = " Pagkatapos ng kanyang pamumuno, sinalakay ng mga Hittite ang Babylonia. Isa silang makapangyarihang grupo mula sa Anatolia. Dahil sa huminang pamahalaan, naging madali para sa kanila ang pananakop."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Ang galing ni Hammurabi pero kahit ganoon, hindi pa rin iyon sapat para manatili ang imperyo…"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Walang imperyo ang panghabambuhay. Pero ang iniwang pamana gaya ng Kodigo ni Hammurabi ay hindi mawawala sa kasaysayan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Mukhang oras na para bumalik tayo."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Oo. Marami pa tayong matututunan sa susunod na paglalakbay."
            }
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
            if (PlayerPrefs.HasKey("BabylonianSceneSix_DialogueIndex"))
            {
                currentDialogueIndex = PlayerPrefs.GetInt("BabylonianSceneSix_DialogueIndex");
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
                ChronocharacterRenderer.sprite = ChronoSmile;
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 1:
                ChronocharacterRenderer.sprite = ChronoSad;
                break;
            case 2:
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 3:
                ChronocharacterRenderer.sprite = ChronoThinking;
                PlayercharacterRenderer.sprite = PlayerEager;
                break;
            case 4:
                ChronocharacterRenderer.sprite = ChronoSad;
                break;
            case 5:
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 6:
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 7:
                PlayercharacterRenderer.sprite = PlayerEager;
                break;
            case 8:
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
        }
    }

    void ShowNextDialogue()
    {
        currentDialogueIndex++;
        SaveCurrentProgress();

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("BabylonianSceneSeven");
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

    // === Save & Load ===
    public void SaveAndLoad()
    {
        PlayerPrefs.SetInt("BabylonianSceneSix_DialogueIndex", currentDialogueIndex);
        PlayerPrefs.SetString("LastScene", "BabylonianSceneSix");

        PlayerPrefs.DeleteKey("AccessMode");
        PlayerPrefs.SetString("SaveSource", "StoryScene");
        PlayerPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        PlayerPrefs.Save();

        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SetCurrentGameState("BabylonianSceneSix", currentDialogueIndex);

        Debug.Log($"Going to save menu with dialogue index: {currentDialogueIndex}");
        SceneManager.LoadScene("SaveAndLoadScene");
    }

    // === Save Current Progress Only ===
    void SaveCurrentProgress()
    {
        PlayerPrefs.SetInt("BabylonianSceneSix_DialogueIndex", currentDialogueIndex);
        PlayerPrefs.SetString("CurrentScene", "BabylonianSceneSix");
        PlayerPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        PlayerPrefs.Save();
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
