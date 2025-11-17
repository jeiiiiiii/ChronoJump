using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class ShangScene1 : MonoBehaviour
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
    public SpriteRenderer WudingFulldrawnSprite;

    // public GameObject AchievementUnlockedRenderer;
    public Sprite PlayerReflective;
    public Sprite PlayerCurious;
    public Sprite PlayerEager;
    public Sprite PlayerEmabarrassed;
    public Sprite WudingProud;
    public Sprite WudingStern;
    
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    public Sprite ChronoSmile;
    public Sprite ChronoCheerful;
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
            {   characterName = "PLAYER",
                line = " Saan na naman ako? May mga palasyo na! Hindi tulad ng Indus Valley na pantay-pantay lang lahat. May hierarchy na dito."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Maligayang pagdating sa Anyang, kabisera ng Dinastiyang Shang. Panahon ito ng 1700 hanggang 1027 BCE, ang unang dinastiyang nag-iwan ng nakasulat na kasaysayan sa China."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = "Anyang? Ito ba ang unang lungsod ng China?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Hindi ang unang lungsod, pero ang unang documented capital. Dito natagpuan ang oracle bones, mga buto at turtle shells na may nakaukit na ancient Chinese writing. Ito ang pinakamalapit nating makikitang record ng kanilang buhay."
            },
            new DialogueLine
            {
                characterName = "WU DING",
                line = " Sino kayo? Mga bisita sa aking kaharian? Ako si Wu Ding, ika-21 na hari ng Dinastiyang Shang. Sa ilalim ng aking pamumuno, lumakas ang aming imperyo mula sa sentral na China hanggang sa mga karatig na rehiyon"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Dakilang Hari Wu Ding, kami'y mga tagapagmasid ng kasaysayan. Ipinapakita ko sa aking kasama ang inyong pamana, ang unang dinastiya na nag-iwan ng nakasulat na tala, ang simula ng organized government sa China."
            },
            new DialogueLine
            {
                characterName = "WU DING",
                line = " Kung gayon, tingnan ninyo ang aking lungsod. Ang Anyang ay sentro ng kapangyarihan. Dito kami lumilikha ng bronze vessels para sa mga ritwal, dito kami nakikipag-usap sa mga espiritu ng aming mga ninuno gamit ang oracle bones."
            },
            new DialogueLine
            {
                characterName = "WU DING",
                line = " Hindi tulad ng mga barbarong nasa labas ng aming teritoryo, kami ay may sistema, may nakasulat na wika, may batas, may hierarchy. Ang bawat tao ay may lugar, ang hari sa tuktok, ang mga noble sa gitna, ang mga magsasaka sa ilalim."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Pero sir... paano kayo naging hari? Sino ang nagpasya na kayo ang mamumuno?"
            },
            new DialogueLine
            {
                characterName = "WU DING",
                line = " Ang Mandate of Heaven, ang karapatan na ibinigay ng langit. Ang aming pamilya, ang Shang, ay pinili ng mga diyos upang mamuno. Hangga't maayos ang pamumuno namin, mayroon kaming karapatang maghari."
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
            if (StudentPrefs.HasKey("ShangSceneOne_DialogueIndex"))
            {
                currentDialogueIndex = StudentPrefs.GetInt("ShangSceneOne_DialogueIndex");
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
        WudingFulldrawnSprite.enabled = false;

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
                    chronoAnimator.Play("Chrono_Smiling (Idle)", 0, 0f);
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
                    chronoAnimator.Play("Chrono_Cheerful", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Talking", 0, 0f);
                break;
            case 3:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Curious", 0, 0f);
                break;
            case 4:
                WudingFulldrawnSprite.enabled = true;
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Cheerful", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Smiling", 0, 0f);
                break;
            case 5:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Smiling", 0, 0f);
                break;
            case 6:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Wu Ding - Proud", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Curious", 0, 0f);
                break;
            case 7:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Wu Ding - Wise", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Reflective", 0, 0f);
                break;
            case 8:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Wu Ding - Idle", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Talking", 0, 0f);
                break;
            case 9:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Wu Ding - Stern", 0, 0f);
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
            SceneManager.LoadScene("ShangFirstRecallChallenges");
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
        StudentPrefs.SetInt("ShangSceneOne_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("LastScene", "ShangSceneOne");
        StudentPrefs.DeleteKey("AccessMode");    
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();

        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SetCurrentGameState("ShangSceneOne", currentDialogueIndex);

        SceneManager.LoadScene("SaveAndLoadScene");
    }

    void SaveCurrentProgress()
    {
        StudentPrefs.SetInt("ShangSceneOne_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("CurrentScene", "ShangSceneOne");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();
    }

    public void GoToSettings()
    {
        // Save current progress
        SaveCurrentProgress();

        // Mark that we're coming from a story scene
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("LastScene", "ShangSceneOne");
        StudentPrefs.Save();

        Debug.Log("Going to Settings from ShangSceneOne");
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
