using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class BabylonianScene2 : MonoBehaviour
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
    public Sprite PlayerCurious;
    public Sprite PlayerSmile;
    public Sprite PlayerEager;
    public Sprite PlayerReflective;
    public Sprite HammurabiExplaning;
    public Sprite HammurabiInformative;
    public Sprite HammurabiFirm;
    public Sprite HammurabiWise;
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    public Sprite ChronoSmile;
    public Animator chronoAnimator;
    public Animator playerAnimator;
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
                characterName = "PLAYER",
                line = " Chrono, ano ito? Bakit may mga sulat sa bato?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Iyan ang Kodigo ni Hammurabi. Isa ito sa pinakamatandang koleksyon ng mga batas sa kasaysayan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Kodigo ni Hammurabi? Siya ba ang gumawa ng lahat ng ‘yan?."
            },
            new DialogueLine
            {
                characterName = "HAMMURABI",
                line = " Oo, ako ang nagpagawa ng Kodigo. Mayroong 282 na batas na isinulat sa batong iyan. Ang layunin nito ay mapanatili ang kaayusan sa Babylonia."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Ano po’ng klaseng mga batas ang nakasulat d’yan?"
            },
            new DialogueLine
            {
                characterName = "HAMMURABI",
                line = " May mga batas tungkol sa pag-aari, negosyo, pamilya, serbisyong propesyonal, at parusa para sa masama ang ginawa."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Sa madaling salita, hindi lang ito para sa mga mayayaman o makapangyarihan. Ginawa ito para mapangalagaan ang karapatan ng lahat."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Paano po ang parusa kung may nagkamali?"
            },
            new DialogueLine
            {
                characterName = "HAMMURABI",
                line = " Kung sinaktan mo ang iba, may kapalit na parusa. Tinatawag itong prinsipyo ng mata sa mata, ngipin sa ngipin. Ibig sabihin, ang ginawa mo sa iba ay maaaring mangyari rin sa iyo."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Mahalaga ito noong panahon nila para matakot ang tao sa paggawa ng masama. Pero mahalaga rin na may sistema para sa hustisya."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Grabe… ang dami palang saklaw ng mga batas noon. Akala ko noon lang nagkaroon ng legal system."
            },
            new DialogueLine
            {
                characterName = "HAMMURABI",
                line = " Ang isang lipunan ay hindi lalago kung walang batas. Ang Kodigo na ito ay ginawa hindi lang para magparusa, kundi para ituro kung ano ang tama."
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

    // Check if this is a load operation from save file
    if (StudentPrefs.GetString("LoadedFromSave", "false") == "true")
    {
        if (StudentPrefs.HasKey("LoadedDialogueIndex"))
        {
            currentDialogueIndex = StudentPrefs.GetInt("LoadedDialogueIndex");
            StudentPrefs.DeleteKey("LoadedDialogueIndex");
            Debug.Log($"Loaded from save file at dialogue index: {currentDialogueIndex}");
        }

        // Clear the load flag
        StudentPrefs.SetString("LoadedFromSave", "false");
    }
    else
    {
        // Check for regular scene progression (not from load)
        if (StudentPrefs.HasKey("BabylonianSceneTwo_DialogueIndex"))
        {
            currentDialogueIndex = StudentPrefs.GetInt("BabylonianSceneTwo_DialogueIndex");
            Debug.Log($"Continuing from previous session at dialogue index: {currentDialogueIndex}");
        }
        else
        {
            currentDialogueIndex = 0;
            Debug.Log("Starting from beginning");
        }
    }

    // Ensure index is within bounds
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
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Thinking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Talking", 0, 0f);
                break;
            case 1:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Eager", 0, 0f);
                break;
            case 2:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Smiling (Idle)", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Talking", 0, 0f);
                break;
            case 3:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Hammurabi_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Curious", 0, 0f);
                break;
            case 4:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Hammurabi_Reverent", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Talking", 0, 0f);
                break;
            case 5:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Hammurabi_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Reflective", 0, 0f);
                break;
            case 6:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Reflective", 0, 0f);
                break;
            case 7:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Thinking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Talking", 0, 0f);
                break;
            case 8:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Hammurabi_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Curious", 0, 0f);
                break;
            case 9:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Smiling", 0, 0f);
                break;
            case 10:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Smiling (Idle)", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Talking", 0, 0f);
                break;
            case 11:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Hammurabi_Proud", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Eager", 0, 0f);
                break;
        }
    }

    void ShowNextDialogue()
    {
        currentDialogueIndex++;
        SaveCurrentProgress();

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("BabylonianSecondRecallChallenges");
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
        StudentPrefs.SetInt("BabylonianSceneTwo_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("LastScene", "BabylonianSceneTwo");
        StudentPrefs.DeleteKey("AccessMode");
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();

        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SetCurrentGameState("BabylonianSceneTwo", currentDialogueIndex);

        SceneManager.LoadScene("SaveAndLoadScene");
    }

    void SaveCurrentProgress()
    {
        StudentPrefs.SetInt("BabylonianSceneTwo_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("CurrentScene", "BabylonianSceneTwo");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();
    }

    public void GoToSettings()
    {
        // Save current progress
        SaveCurrentProgress();

        // Mark that we're coming from a story scene
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("LastScene", "BabylonianSceneTwo");
        StudentPrefs.Save();

        Debug.Log("Going to Settings from BabylonianSceneTwo");
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
