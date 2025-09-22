using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class AssyrianScene1 : MonoBehaviour
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

    [Header("Sprites - Tiglath")]
    public SpriteRenderer TiglathFulldrawnSprite;
    public Sprite TiglathAssertive;
    public Sprite TiglathPrideful;
    public Sprite TiglathCold;

    [Header("Sprites - Player")]
    public SpriteRenderer PlayercharacterRenderer;
    public Sprite PlayerCurious;
    public Sprite PlayerEager;
    public Sprite PlayerSmile;
    public Sprite PlayerReflective;
    public Sprite PlayerEmbarassed;

    [Header("Sprites - Chrono")]
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
                line = " Saan na naman ako napunta? Ang taas ng mga gusali... parang may banta ng digmaan kahit saan."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Maligayang pagdating sa Nineveh, kabisera ng imperyong Assyrian. Panahon ito ng muling pagkakaisa ng Mesopotamia, hindi sa bisa ng kasunduan, kundi sa pamamagitan ng takot, sandata, at pamumuno."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Nineveh? Hindi ba ito ang kilalang lungsod na malapit sa Tigris?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Tumpak. Ang Assyria ay nagsimulang isang tribo sa bulubunduking rehiyon hilaga ng Babylon mula sa mga lambak ng Tigris hanggang sa mataas na kabundukan ng Armenia."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Noon ay mga mangingisda at mangangalakal, ngayo’y mga pinuno ng takot."
            },
            new DialogueLine
            {
                characterName = "TIGLATH-PILISER I",
                line = " Sino kayo? Mga tagalabas? Ang Nineveh ay hindi lugar para sa mga mahihina. Kami ay isinilang sa digmaan, at lumalaki sa pamumuno."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Haring Tiglath-Pileser I, kami’y mga tagapagmasid ng kasaysayan. Ipinapakita ko sa aking kasama ang iyong dakilang pamana, ang pagbubuo ng imperyo, at ang mahigpit mong panuntunan."
            },
            new DialogueLine
            {
                characterName = "TIGLATH-PILISER I",
                line = " Kung gayon, saksihan ninyo kung paano ko pinagbuklod ang mga lupain mula silangan hanggang kanluran. Sinupil namin ang mga Hittite, at naabot ng aming mga hukbo ang baybayin ng Mediterranean."
            },
            new DialogueLine
            {
                characterName = "TIGLATH-PILISER I",
                line = " Nagpadala kami ng mga ekspedisyong militar upang sakupin ang mga rutang pangkalakalan. Walang tumutol. Lahat ay yumuko."
            },
            new DialogueLine
            {
                characterName = "TIGLATH-PILISER I",
                line = " Ang mga natalo ay nagbigay ng tributo — pilak, ginto, hayop, at minsan pati mga anak nilang lalaki."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Lahat ba ay sumang-ayon? O napilitan lang?"
            },
            new DialogueLine
            {
                characterName = "TIGLATH-PILISER I",
                line = " Ang kapayapaan ay hindi regalo. Ito'y kinukuha sa pamamagitan ng takot. Ang mga lungsod na lumaban ay sinunog. Ang mga bihag ay pinako sa poste sa harap ng kanilang pader — isang babala sa mga susunod."
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
            if (StudentPrefs.HasKey("AssyrianSceneOne_DialogueIndex"))
            {
                currentDialogueIndex = StudentPrefs.GetInt("AssyrianSceneOne_DialogueIndex");
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
        TiglathFulldrawnSprite.enabled = false;
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
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 2:
                PlayercharacterRenderer.sprite = PlayerCurious;
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 3:
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 5:
                PlayercharacterRenderer.sprite = PlayerEmbarassed;
                TiglathFulldrawnSprite.enabled = true;
                break;
            case 6:
                ChronocharacterRenderer.sprite = ChronoSmile;
                PlayercharacterRenderer.sprite = PlayerCurious;
                break;
            case 7:
                ChronocharacterRenderer.sprite = TiglathPrideful;
                break;
            case 8:
                PlayercharacterRenderer.sprite = PlayerReflective;
                ChronocharacterRenderer.sprite = TiglathCold;
                break;
            case 9:
                ChronocharacterRenderer.sprite = TiglathPrideful;
                break;
            case 10:
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 11:
                ChronocharacterRenderer.sprite = TiglathCold;
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
            SceneManager.LoadScene("AssyrianFirstRecallChallenges");
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
        StudentPrefs.SetInt("AssyrianSceneOne_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("LastScene", "AssyrianSceneOne");

        StudentPrefs.DeleteKey("AccessMode");
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();

        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SetCurrentGameState("AssyrianSceneOne", currentDialogueIndex);

        Debug.Log($"Going to save menu with dialogue index: {currentDialogueIndex}");
        SceneManager.LoadScene("SaveAndLoadScene");
    }

    // === Save Current Progress Only ===
    void SaveCurrentProgress()
    {
        StudentPrefs.SetInt("AssyrianSceneOne_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("CurrentScene", "AssyrianSceneOne");
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
