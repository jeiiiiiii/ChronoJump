using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class NileScene2 : MonoBehaviour
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
    public SpriteRenderer MeritFulldrawnSprite;

    // public GameObject AchievementUnlockedRenderer;
    public Sprite PlayerReflective;
    public Sprite PlayerCurious;
    public Sprite PlayerEager;
    public Sprite PlayerSmile;
    public Sprite ImhotepWise;
    public Sprite MeritHopeful;

    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    public Sprite ChronoSmile;
    public Animator chronoAnimator;
    public Animator playerAnimator;
    public Animator NPCFulldrawnAnimator;
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
            {   characterName = "CHRONO",
                line = " Ngunit hindi lang basta hinihintay ng mga Egyptian ang pagbaha. Tingnan mo, may mga canals, may mga basins, may sistema. Ito ang simula ng irrigation engineering."
            },
            new DialogueLine
            {   characterName = "MERIT",
                line = " Ako si Merit! Nagtatanim ako ng wheat at barley dito sa lambak. Kapag bumaha ang Nile, naghihintay kami. Kapag humupa, nagtatanim kami. Kapag tag-init, nag-aani kami. Yan ang cycle ng buhay namin."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Paano kayo nakakasurvive? Paano kung may taon na kulang ang tubig?"
            },
            new DialogueLine
            {
                characterName = "MERIT",
                line = " Nagdasal kami kay Hapi, ang diyos ng Nile. Nag-aalay kami ng offerings. At importante, nag-iimbak kami ng grain sa mga granaries. Kapag may surplus, itinago namin para sa lean years."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ito ang dahilan kung bakit ang Egypt ay naging stable. May surplus production sila, ibig sabihin, hindi lahat ay farmers. May scribes, craftsmen, priests, soldiers. Lahat ay possible dahil sa Nile."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Ibig sabihin... dahil sa ilog, nagkaroon sila ng specialized jobs?"
            },
            new DialogueLine
            {
                characterName = "IMHOTEP",
                line = " Tama. Kapag may sapat na pagkain, may panahon para sa iba pang gawain. Ako ay scribe, nagsusulat gamit ang hieroglyphics. May iba na architects, builders, artists. Lahat ay dahil sa security na dulot ng Nile."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Pero may problema rin. Tingnan mo ang paligid, sa magkabilang gilid ng Nile, disyerto. Walang buhay, walang tubig. Ang buong Egypt ay nakakulong sa makitid na strip ng lupa sa tabi ng ilog."
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
            if (StudentPrefs.HasKey("NileSceneTwo_DialogueIndex"))
            {
                currentDialogueIndex = StudentPrefs.GetInt("NileSceneTwo_DialogueIndex");
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
        MeritFulldrawnSprite.enabled = false;

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
                    chronoAnimator.Play("Chrono_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Reflective", 0, 0f);
                break;
            case 1:
                MeritFulldrawnSprite.enabled = true;
                if (NPCFulldrawnAnimator != null)
                    NPCFulldrawnAnimator.Play("Merit_FullDrawn", 0, 0f);
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Cheerful", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Smiling", 0, 0f);
                break;
            case 2:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Merit - Idle", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Talking", 0, 0f);
                break;
            case 3:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Merit - Hopeful", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Eager", 0, 0f);
                break;
            case 4:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Curious", 0, 0f);
                break;
            case 5:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Thinking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Talking", 0, 0f);
                break;
            case 6:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Imhotep - Calm", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Eager", 0, 0f);
                break;
            case 7:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Reflective", 0, 0f);
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
            SceneManager.LoadScene("NileSecondRecallChallenges");
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
        StudentPrefs.SetInt("NileSceneTwo_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("LastScene", "NileSceneTwo");
        StudentPrefs.DeleteKey("AccessMode");    
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();

        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SetCurrentGameState("NileSceneTwo", currentDialogueIndex);

        SceneManager.LoadScene("SaveAndLoadScene");
    }

    void SaveCurrentProgress()
    {
        StudentPrefs.SetInt("NileSceneTwo_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("CurrentScene", "NileSceneTwo");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();
    }

    public void GoToSettings()
    {
        // Save current progress
        SaveCurrentProgress();

        // Mark that we're coming from a story scene
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("LastScene", "NileSceneTwo");
        StudentPrefs.Save();

        Debug.Log("Going to Settings from NileSceneTwo");
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
