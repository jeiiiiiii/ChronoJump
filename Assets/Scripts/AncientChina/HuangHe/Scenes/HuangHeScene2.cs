using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class HuangHeScene2 : MonoBehaviour
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
    public SpriteRenderer QiFulldrawnSprite;

    // public GameObject AchievementUnlockedRenderer;
    public Sprite PlayerReflective;
    public Sprite PlayerCurious;
    public Sprite PlayerEager;
    public Sprite PlayerSmile;
    public Sprite DayuWise;
    public Sprite QiHopeful;

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
                line = " Ngunit sa kabila ng takot, dito nagsimula ang lahat. Ang North China Plain, sampung porsyento lamang ng kabuoang lupain ng bansa na matatatagpuan sa pagitan ng Huang He at Yangtze River."
            },
            new DialogueLine
            {   characterName = "QI",
                line = " Ako si Qi! Nagtatanim ako ng millet at wheat dito sa kapatagan. Ang lupa ay tabang-taba dahil sa loess na dinala ng ilog. Pero lagi kaming nag-aalala, baka bukas, umapaw na naman ang tubig."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Paano kayo nabubuhay nang ganyan? Laging may takot?"
            },
            new DialogueLine
            {
                characterName = "QI",
                line = " Dahil wala kaming choice. At dahil... naniniwala kami na may solusyon. Kailangan lang nating magtulungan, magtayo ng dikes, kontrolin ang daloy ng tubig, magtrabaho bilang isang komunidad."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ito ang simula ng organized society sa China. Hindi tulad ng Indus na may grid system, o Mesopotamia na may mga hari, ang China ay nag-organisa dahil sa pangangailangan. Survival ang nagturo sa kanila ng disiplina."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Ibig sabihin... ang pagbaha ang nag-unite sa kanila?"
            },
            new DialogueLine
            {
                characterName = "DA YU",
                line = " Tama. Ang isang tao ay hindi kayang pigilan ang ilog. Ngunit ang isang libong tao na nagtutulungan, maaari. Kaya nagsimula ang aming mga pamayanan, ang aming mga batas, ang aming mga pinuno."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Dahil dito, nabuo ang konsepto ng collective work. Ang bawat tao ay may tungkulin, magtanim, magtayo ng dikes, bantayan ang ilog. Walang mayaman, walang mahirap, lahat ay pantay-pantay sa harap ng banta ng kalikasan."
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
            if (StudentPrefs.HasKey("HuangHeSceneTwo_DialogueIndex"))
            {
                currentDialogueIndex = StudentPrefs.GetInt("HuangHeSceneTwo_DialogueIndex");
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
        QiFulldrawnSprite.enabled = false;

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
                QiFulldrawnSprite.enabled = true;
                if (NPCFulldrawnAnimator != null)
                    NPCFulldrawnAnimator.Play("Qi_FullDrawn", 0, 0f);
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Cheerful", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Smiling", 0, 0f);
                break;
            case 2:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Qi_Idle", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Talking", 0, 0f);
                break;
            case 3:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Qi_Confident", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Curious", 0, 0f);
                break;
            case 4:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Reflective", 0, 0f);
                break;
            case 5:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Smiling (Idle)", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Talking", 0, 0f);
                break;
            case 6:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Da Yu_Wise", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Curious", 0, 0f);
                break;
            case 7:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Eager", 0, 0f);
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
            SceneManager.LoadScene("HuangHeSecondRecallChallenges");
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
        StudentPrefs.SetInt("HuangHeSceneTwo_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("LastScene", "HuangHeSceneTwo");
        StudentPrefs.DeleteKey("AccessMode");    
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();

        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SetCurrentGameState("HuangHeSceneTwo", currentDialogueIndex);

        SceneManager.LoadScene("SaveAndLoadScene");
    }

    void SaveCurrentProgress()
    {
        StudentPrefs.SetInt("HuangHeSceneTwo_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("CurrentScene", "HuangHeSceneTwo");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();
    }

    public void GoToSettings()
    {
        // Save current progress
        SaveCurrentProgress();

        // Mark that we're coming from a story scene
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("LastScene", "HuangHeSceneTwo");
        StudentPrefs.Save();

        Debug.Log("Going to Settings from HuangHeSceneTwo");
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
