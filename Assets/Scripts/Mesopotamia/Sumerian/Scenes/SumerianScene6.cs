using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class SumerianScene6 : MonoBehaviour
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
    public Sprite PlayerSmile;
    public Sprite PlayerEager;
    public Sprite PlayerReflective;
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoSmile;
    public Sprite ChronoThinking;

    public Sprite EnkiPokerface;
    public Sprite EnkiStern;
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
            {
                characterName = "CHRONO",
                line = " Tila may ipinapahayag na mahalagang kautusan sa lugar na ito... Halika, lumapit tayo."
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Nariyan pala kayo. Mainam at dumating kayo ngayon. Itinatanghal dito ang mga batas na iniakda ni Haring Ur-Nammu , ang mga tuntuning gumagabay sa aming pamayanan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Ang laki ng sulat... parang ukit na masinsin. Para saan po ang mga ito?"
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Bawat linya ay may dalang layunin , upang itaguyod ang katarungan. Sa mga panahong may alitan, pagkakautang, o pagkukulang, hindi sapat ang galit o tsismis. Kailangang may sinusunod na batayan."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " At dito ipinamalas ng mga Sumerian ang isa sa pinakamahalagang ambag nila: ang pagsusulat ng mga batas sa isang sistema. Hindi ito basta utos , ito'y mga alituntunin na nagtatanggol sa karapatan ng mahina at mapagbigay ng parusa sa mapang-abuso."
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Ang pagkakaroon ng malinaw na batas ay nagsilbing haligi ng aming kaayusan. Kahit alipin, kahit babae , may karapatang pinoprotektahan ng mga alituntuning ito."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Ibig sabihin po... may paraan kayo para lutasin ang problema ng makatarungan?"
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Ganyan nga. Dahil sa mga batas na ito, naiiwasan ang kaguluhan. May proseso. May timbang ang salita ng bawat isa."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ngayon, gusto kong malaman... sa dami ng layunin ng batas, ano sa tingin mo ang pinakapuso nito?"
            },
        };
    }

    void LoadDialogueIndex()
    {
        // Check if this is a new game
        if (StudentPrefs.GetString("GameMode", "") == "NewGame")
        {
            currentDialogueIndex = 0;
            StudentPrefs.DeleteKey("GameMode");
            Debug.Log("New game started - dialogue index reset to 0");
            return;
        }

        // Check if loading from a save
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
            if (StudentPrefs.HasKey("SumerianSceneSix_DialogueIndex"))
            {
                currentDialogueIndex = StudentPrefs.GetInt("SumerianSceneSix_DialogueIndex");
                Debug.Log($"Continuing from previous session at dialogue index: {currentDialogueIndex}");
            }
            else
            {
                currentDialogueIndex = 0;
                Debug.Log("Starting from beginning");
            }
        }

        // Ensure index stays within bounds
        if (currentDialogueIndex >= dialogueLines.Length)
            currentDialogueIndex = dialogueLines.Length - 1;
        if (currentDialogueIndex < 0)
            currentDialogueIndex = 0;
    }

    void SetupButtons()
    {
        // Next and Back
        if (nextButton != null)
            nextButton.onClick.AddListener(ShowNextDialogue);

        if (backButton != null)
            backButton.onClick.AddListener(ShowPreviousDialogue);

        // Save Button
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(SaveAndLoad);
        }
        else
        {
            GameObject saveButtonObj = GameObject.Find("SaveBT");
            if (saveButtonObj != null)
            {
                Button foundSaveButton = saveButtonObj.GetComponent<Button>();
                if (foundSaveButton != null)
                {
                    foundSaveButton.onClick.AddListener(SaveAndLoad);
                    Debug.Log("Save button found and connected!");
                }
            }
        }

        // Home Button
        if (homeButton != null)
        {
            homeButton.onClick.AddListener(Home);
        }
        else
        {
            GameObject homeButtonObj = GameObject.Find("HomeBt");
            if (homeButtonObj != null)
            {
                Button foundHomeButton = homeButtonObj.GetComponent<Button>();
                if (foundHomeButton != null)
                {
                    foundHomeButton.onClick.AddListener(Home);
                    Debug.Log("Home button found and connected!");
                }
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
                PlayercharacterRenderer.sprite = PlayerSmile;
                break;
            case 1:
                ChronocharacterRenderer.sprite = EnkiPokerface;
                break;
            case 2:
                PlayercharacterRenderer.sprite = PlayerEager;
                break;
            case 3:
                ChronocharacterRenderer.sprite = EnkiStern;
                break;
            case 4:
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 5:
                ChronocharacterRenderer.sprite = EnkiPokerface;
                break;
            case 6:
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 7:
                ChronocharacterRenderer.sprite = EnkiStern;
                break;
            case 8:
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
        }
    }

    void ShowNextDialogue()
    {
        currentDialogueIndex++;
        SaveCurrentProgress();

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("SumerianFifthRecallChallenges");
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

    void SaveCurrentProgress()
    {
        StudentPrefs.SetInt("SumerianSceneSix_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("CurrentScene", "SumerianSceneSix");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();
    }

    public void SaveAndLoad()
    {
        StudentPrefs.SetInt("SumerianSceneSix_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("LastScene", "SumerianSceneSix");

        StudentPrefs.DeleteKey("AccessMode");
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();

        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SetCurrentGameState("SumerianSceneSix", currentDialogueIndex);
        }

        Debug.Log($"Going to save menu with dialogue index: {currentDialogueIndex} from SumerianSceneSix");
        SceneManager.LoadScene("SaveAndLoadScene");
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
