using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class SiningScene1 : MonoBehaviour
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
    public SpriteRenderer SindhuFulldrawnSprite;

    public Sprite PlayerReflective;
    public Sprite PlayerCurious;
    public Sprite PlayerEager;
    public Sprite PlayerEmabarrassed;
    public Sprite SindhuProud;
    public Sprite SindhuWise;
    public Sprite SindhuCalm;

    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    public Sprite ChronoSmile;
    public Sprite ChronoCheerful;
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
            {   characterName = "PLAYER",
                line = " Ano 'to? Parang swimming pool... pero ang luma ng hitsura. Ang ganda ng pagkakagawa!"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Maligayang pagdating sa Great Bath ng Mohenjo-daro. Isa ito sa pinakamahalagang estruktura ng Indus Valley Civilization, hindi lang para sa pagligo, kundi para sa ritwal ng kalinisan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = "Great Bath? Paano nila ito ginawa? Walang modernong tools noon, diba?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Tumpak. Ang Great Bath ay may dimensions na 12 meters haba at 7 meters laki. Gawa ito sa standardized bricks na sobrang tibay. Ang bawat brick ay may exact na sukat, lahat ay perpekto."
            },
            new DialogueLine
            {
                characterName = "SINDHU",
                line = " Sino kayo? Mga tagapag-aral ng aming sining? Ang Great Bath na ito ay bunga ng libu-libong oras ng pagpaplano at pag-iisip. Bawat brick, bawat hagdan, bawat drainage, lahat ay may layunin."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Master Sindhu, kami'y mga tagapagmasid ng kasaysayan. Ipinapakita ko sa aking kasama ang iyong dakilang gawa, ang teknolohiya na lampas sa inyong panahon."
            },
            new DialogueLine
            {
                characterName = "SINDHU",
                line = " Kung gayon, tingnan ninyo kung paano namin ito ginawa. Ang bawat brick ay sinukat nang eksakto, 9 pulgada haba, 4.5 pulgada lapad, 2.25 pulgada taas. Walang brick na mali ang sukat"
            },
            new DialogueLine
            {
                characterName = "SINDHU",
                line = " Ang drainage system ay dumadaloy pababa, patungo sa main canal. Kahit maulan o tuyo, walang baha. Ang tubig ay umaagos nang natural, walang bomba, walang makina."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Pero bakit kailangan ng ganito kalaking paliguan? Para lang ba sa pagligo?"
            },
            new DialogueLine
            {
                characterName = "SINDHU",
                line = " Ang kalinisan ay sagrado. Ang tubig ay nag-lilinis hindi lang ng katawan, kundi ng kaluluwa. Ito ay ritwal, hindi simpleng pagligo."
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
            if (StudentPrefs.HasKey("SiningSceneOne_DialogueIndex"))
            {
                currentDialogueIndex = StudentPrefs.GetInt("SiningSceneOne_DialogueIndex");
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
        SindhuFulldrawnSprite.enabled = false;

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
                    chronoAnimator.Play("Chrono_Cheerful", 0, 0f);
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
                    chronoAnimator.Play("Chrono_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Reflective", 0, 0f);
                break;
            case 4:
                SindhuFulldrawnSprite.enabled = true;
                if (NPCFulldrawnAnimator != null)
                    NPCFulldrawnAnimator.Play("Sindhu_FullDrawn", 0, 0f);
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
                    chronoAnimator.Play("Sindhu_Proud", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Eager", 0, 0f);
                break;
            case 7:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Sindhu_Wise", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Reflective", 0, 0f);
                break;
            case 8:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Sindhu_Idle", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Talking", 0, 0f);
                break;
            case 9:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Sindhu_Proud", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Curious", 0, 0f);
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
            SceneManager.LoadScene("SiningFirstRecallChallenges");
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
        StudentPrefs.SetInt("SiningSceneOne_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("LastScene", "SiningSceneOne");
        StudentPrefs.DeleteKey("AccessMode");    
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();

        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SetCurrentGameState("SiningSceneOne", currentDialogueIndex);

        SceneManager.LoadScene("SaveAndLoadScene");
    }

    void SaveCurrentProgress()
    {
        StudentPrefs.SetInt("SiningSceneOne_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("CurrentScene", "SiningSceneOne");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();
    }

    public void GoToSettings()
    {
        // Save current progress
        SaveCurrentProgress();

        // Mark that we're coming from a story scene
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("LastScene", "SiningSceneOne");
        StudentPrefs.Save();

        Debug.Log("Going to Settings from SiningSceneOne");
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
