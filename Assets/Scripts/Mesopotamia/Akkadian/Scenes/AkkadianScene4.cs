using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class AkkadianScene4 : MonoBehaviour
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
    public Button saveButton;
    public Button homeButton;

    public int currentDialogueIndex = 0;

    public DialogueLine[] dialogueLines;

    public SpriteRenderer PlayercharacterRenderer;
    public Sprite PlayerCurious;
    public Sprite PlayerSmile;
    public Sprite SargonNuetral;
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    public Sprite ChronoSad;
    public Sprite ChronoSmile;

    void Start()
    {
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
                line = " Pagkaraan ng maraming taon, sumunod sa kanya ang apo niyang si Naram-Sin. Mahusay din, ngunit sa kanyang pamumuno, humina ang imperyo."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Bakit siya humina? Dahil sa digmaan?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Bumagsak ang imperyo matapos salakayin ng mga Amorite at Hurrian ang Mesopotamia. Sinamantala nila ang paghina ng pamahalaan ng Akkad."
            },
            new DialogueLine
            {
                characterName = "SARGON I",
                line = " Isang imperyo, gaano man kalawak, ay maaaring bumagsak kapag hindi ito naipagtanggol nang maayos."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Pagkatapos ng pagbagsak, panandaliang nabawi ng lungsod ng Ur ang kapangyarihan sa ilalim ng Ikatlong Dinastiya."
            },
        };
    }

    void LoadDialogueIndex()
    {
        if (PlayerPrefs.GetString("GameMode", "") == "NewGame")
        {
            currentDialogueIndex = 0;
            PlayerPrefs.DeleteKey("GameMode");
            return;
        }

        if (PlayerPrefs.GetString("LoadedFromSave", "false") == "true")
        {
            if (PlayerPrefs.HasKey("LoadedDialogueIndex"))
            {
                currentDialogueIndex = PlayerPrefs.GetInt("LoadedDialogueIndex");
                PlayerPrefs.DeleteKey("LoadedDialogueIndex");
            }
            PlayerPrefs.SetString("LoadedFromSave", "false");
        }
        else
        {
            if (PlayerPrefs.HasKey("AkkadianSceneFour_DialogueIndex"))
            {
                currentDialogueIndex = PlayerPrefs.GetInt("AkkadianSceneFour_DialogueIndex");
            }
            else
            {
                currentDialogueIndex = 0;
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
    }

    void ShowDialogue()
    {
        DialogueLine line = dialogueLines[currentDialogueIndex];
        dialogueText.text = $"<b>{line.characterName}</b>: {line.line}";

        switch (currentDialogueIndex)
        {
            case 0:
                ChronocharacterRenderer.sprite = ChronoSad;
                PlayercharacterRenderer.sprite = PlayerSmile;
                break;
            case 1:
                PlayercharacterRenderer.sprite = PlayerCurious;
                break;
            case 2:
                ChronocharacterRenderer.sprite = ChronoSad;
                break;
            case 3:
                ChronocharacterRenderer.sprite = SargonNuetral;
                break;
            case 4:
                ChronocharacterRenderer.sprite = ChronoSmile;
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
            SceneManager.LoadScene("AkkadianSecondRecallChallenges");
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
        PlayerPrefs.SetInt("AkkadianSceneFour_DialogueIndex", currentDialogueIndex);
        PlayerPrefs.SetString("LastScene", "AkkadianSceneFour");
        PlayerPrefs.DeleteKey("AccessMode");
        PlayerPrefs.SetString("SaveSource", "StoryScene");
        PlayerPrefs.SetString("SaveTimestamp", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        PlayerPrefs.Save();

        if (SaveLoadManager.Instance != null)
            SaveLoadManager.Instance.SetCurrentGameState("AkkadianSceneFour", currentDialogueIndex);

        SceneManager.LoadScene("SaveAndLoadScene");
    }

    void SaveCurrentProgress()
    {
        PlayerPrefs.SetInt("AkkadianSceneFour_DialogueIndex", currentDialogueIndex);
        PlayerPrefs.SetString("CurrentScene", "AkkadianSceneFour");
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
