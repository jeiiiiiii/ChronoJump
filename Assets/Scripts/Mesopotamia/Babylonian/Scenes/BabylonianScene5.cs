using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class BabylonianScene5 : MonoBehaviour
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
    public Sprite PlayerReflective;
    public Sprite PlayerEager;
    public Sprite PlayerSmile;
    public Sprite PlayerCurious;
    public Sprite PlayerEmbarrased;
    public Sprite HammurabiReverent;
    public Sprite HammurabiExplaining;
    public Sprite HammurabiWise;
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    public Sprite ChronoSmile;
    public Sprite ChronoCheerful;
    public Sprite ChronoSad;
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
            line = " Chrono... sino kaya itong nililok nila? Parang sobrang mahalaga sa kanila."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Isa siya sa pinakakilalang nilalang ng mga taga-Babylonia. Dito nakasentro ang kanilang pananampalataya."
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " May mga nag-aalay pa ng pagkain… para ba ito sa isang tao? O sa isang espiritu?"
        },
        new DialogueLine
        {
            characterName = "HAMMURABI",
            line = " Hindi siya karaniwang nilalang. Siya si Marduk — ang diyos na nagbigay ng lakas at direksyon sa aming pamahalaan."
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Diyos?! Siya ang sentro ng paniniwala ninyo?"
        },
        new DialogueLine
        {
            characterName = "HAMMURABI",
            line = " Oo. Sa panahon ng pagsasama-sama ng mga lungsod sa ilalim ng Babylonia, kailangan naming kilalanin ang isang diyos na magbubuklod sa mga tao."
        },
        new DialogueLine
        {
            characterName = "HAMMURABI",
            line = " Pinili naming itaas si Marduk bilang pangunahing diyos. Siya ang sagisag ng kaayusan at tagumpay." // 5
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Sa mga sinaunang kwento, tinalo ni Marduk ang mga puwersa ng kaguluhan at nilikha ang mundo mula sa kaguluhan. Isa sa mga akdang isinulat noon ay Enuma Elish, na naglalarawan ng kanyang kapangyarihan."
        },
        new DialogueLine
        {
            characterName = "PLAYER",
            line = " Ah, kaya pala parang may respeto at takot ang mga tao sa paligid… Hindi lang ito estatwa. Para sa kanila, ito ang kinatawan ng lakas na higit pa sa tao."
        },
        new DialogueLine
        {
            characterName = "HAMMURABI",
            line = " Walang pamahalaan kung walang gabay. At walang gabay kung walang pananampalataya. Sa tulong ni Marduk, naitaguyod ko ang pagkakaisa sa buong Babylonia."
        },
        new DialogueLine
        {
            characterName = "CHRONO",
            line = " Ang pananampalataya nila kay Marduk ay naging isang mahalagang bahagi ng kultura at pagkakakilanlan ng imperyo. Hindi lang ito tungkol sa paniniwala — ito rin ay patunay ng pagkakabuklod ng mga tao."
         },
        new DialogueLine
         {
            characterName = "PLAYER ",
            line = " Ang galing… iba talaga kapag may paniniwalang pinanghahawakan. Parang mas nagiging buo ang pagkatao ng isang bayan."
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
            if (StudentPrefs.HasKey("BabylonianSceneFive_DialogueIndex"))
            {
                currentDialogueIndex = StudentPrefs.GetInt("BabylonianSceneFive_DialogueIndex");
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
                    playerAnimator.Play("Player_Curious", 0, 0f);
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
                    playerAnimator.Play("Player_Reflective", 0, 0f);
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
                    chronoAnimator.Play("Hammurabi_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Curious", 0, 0f);
                break;
            case 7:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Curious", 0, 0f);
                break;
            case 8:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Smiling (Idle)", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Talking", 0, 0f);
                break;
            case 9:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Hammurabi_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Reflective", 0, 0f);
                break;
            case 10:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Talking", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Curious", 0, 0f);
                break;
            case 11:
                if (chronoAnimator != null)
                    chronoAnimator.Play("Chrono_Cheerful", 0, 0f);
                if (playerAnimator != null)
                    playerAnimator.Play("Player_Talking", 0, 0f);
                break;
        }
    }

    void ShowNextDialogue()
    {
        currentDialogueIndex++;
        SaveCurrentProgress();

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("BabylonianFourthRecallChallenges");
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
        StudentPrefs.SetInt("BabylonianSceneFive_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("LastScene", "BabylonianSceneFive");

        StudentPrefs.DeleteKey("AccessMode");
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();

        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SetCurrentGameState("BabylonianSceneFive", currentDialogueIndex);

        Debug.Log($"Going to save menu with dialogue index: {currentDialogueIndex}");
        SceneManager.LoadScene("SaveAndLoadScene");
    }

    // === Save Current Progress Only ===
    void SaveCurrentProgress()
    {
        StudentPrefs.SetInt("BabylonianSceneFive_DialogueIndex", currentDialogueIndex);
        StudentPrefs.SetString("CurrentScene", "BabylonianSceneFive");
        StudentPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        StudentPrefs.Save();
    }

    public void GoToSettings()
    {
        // Save current progress
        SaveCurrentProgress();

        // Mark that we're coming from a story scene
        StudentPrefs.SetString("SaveSource", "StoryScene");
        StudentPrefs.SetString("LastScene", "BabylonianSceneFive");
        StudentPrefs.Save();

        Debug.Log("Going to Settings from BabylonianSceneFive");
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
