using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ReviewDialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentParent;
    public GameObject dialogueItemPrefab;
    public GameObject editDialoguePrefab;

    private GameObject currentEditPanel;

    [Header("Edit Popup References")]
    public GameObject editPopup;
    public TMP_InputField nameInputField;
    public TMP_InputField textInputField;
    public Button saveEditButton;
    public Button cancelEditButton;

    [Header("Delete Popup References")]
    public GameObject deletePopup;
    public Button deleteYesButton;
    public Button deleteNoButton;

    [Header("TTS Generation UI")]
    public Button generateAllAudioButton;
    public GameObject ttsProgressPanel;
    public TextMeshProUGUI progressText;
    public Slider progressBar;
    public TextMeshProUGUI statusText;

    private int pendingDeleteIndex = -1;

    void Start()
    {
        RefreshList();

        if (editPopup != null) editPopup.SetActive(false);
        if (deletePopup != null) deletePopup.SetActive(false);
        if (ttsProgressPanel != null) ttsProgressPanel.SetActive(false);

        if (generateAllAudioButton != null)
        {
            generateAllAudioButton.onClick.AddListener(GenerateAllAudio);
        }
    }

    public void RefreshList()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        var allDialogues = DialogueStorage.GetAllDialogues();
        for (int i = 0; i < allDialogues.Count; i++)
        {
            int index = i;
            DialogueLine line = allDialogues[i];

            GameObject item = Instantiate(dialogueItemPrefab, contentParent);

            TextMeshProUGUI[] texts = item.GetComponentsInChildren<TextMeshProUGUI>();
            
            // Get voice name
            var voice = VoiceLibrary.GetVoiceById(line.selectedVoiceId);
            
            texts[0].text = $"{line.characterName} ({voice.voiceName})";
            
            // Show audio status indicator
            string audioStatus = line.hasAudio ? " 🔊" : " 🔇";
            texts[1].text = line.dialogueText + audioStatus;

            Button[] buttons = item.GetComponentsInChildren<Button>();
            Button editBtn = buttons[0];
            Button deleteBtn = buttons[1];

            deleteBtn.onClick.AddListener(() => OpenDeletePopup(index));
            editBtn.onClick.AddListener(() => OpenEditPopup(index, line));
        }
    }

    public void GenerateAllAudio()
    {
        var dialogues = DialogueStorage.GetAllDialogues();
        
        if (dialogues == null || dialogues.Count == 0)
        {
            ValidationManager.Instance.ShowWarning(
                "No Dialogues",
                "There are no dialogues to generate audio for!"
            );
            return;
        }

        Debug.Log($"🎙️ GenerateAllAudio called for {dialogues.Count} dialogues");
        
        // Log each dialogue status
        for (int i = 0; i < dialogues.Count; i++)
        {
            Debug.Log($"Dialogue {i+1}: '{dialogues[i].characterName}' - HasAudio: {dialogues[i].hasAudio}, Path: {dialogues[i].audioFilePath}");
        }

        if (ttsProgressPanel != null) ttsProgressPanel.SetActive(true);
        if (generateAllAudioButton != null) generateAllAudioButton.interactable = false;

        StartCoroutine(ElevenLabsTTSManager.Instance.GenerateAllTTS(
            dialogues,
            OnTTSProgress,
            OnTTSComplete
        ));
    }

    void OnTTSProgress(int completed, int total, string message)
    {
        float progress = (float)completed / total;
        
        if (progressBar != null)
            progressBar.value = progress;
        
        if (progressText != null)
            progressText.text = $"{completed}/{total} Generated";
        
        if (statusText != null)
            statusText.text = message;

        Debug.Log($"TTS Progress: {message}");
    }

    void OnTTSComplete(bool success)
    {
        if (ttsProgressPanel != null) ttsProgressPanel.SetActive(false);
        if (generateAllAudioButton != null) generateAllAudioButton.interactable = true;

        if (success)
        {
            ValidationManager.Instance.ShowWarning(
                "Audio Generated!",
                "All dialogue audio has been generated successfully!",
                null,
                null
            );
        }
        else
        {
            ValidationManager.Instance.ShowWarning(
                "Generation Issues",
                "Some audio files failed to generate. Check console for details.",
                null,
                null
            );
        }

        RefreshList(); // Refresh to show audio indicators
    }

    public void OpenEditPopup(int index, DialogueLine line)
    {
        editPopup.SetActive(true);
        nameInputField.text = line.characterName;
        textInputField.text = line.dialogueText;

        saveEditButton.onClick.RemoveAllListeners();
        cancelEditButton.onClick.RemoveAllListeners();

        saveEditButton.onClick.AddListener(() =>
        {
            var validation = ValidationManager.Instance.ValidateNameAndDialogueCombined(
                nameInputField.text.Trim(), textInputField.text.Trim()
            );

            if (!validation.isValid)
            {
                ValidationManager.Instance.ShowWarning("Dialogue Validation", validation.message);
                return;
            }

            DialogueStorage.EditDialogue(index, nameInputField.text, textInputField.text);
            
            var dialogues = DialogueStorage.GetAllDialogues();
            if (index < dialogues.Count)
            {
                dialogues[index].hasAudio = false;
                dialogues[index].audioFilePath = "";
            }
            
            editPopup.SetActive(false);
            RefreshList();
        });

        cancelEditButton.onClick.AddListener(() => editPopup.SetActive(false));
    }

    public void OpenDeletePopup(int index)
    {
        pendingDeleteIndex = index;
        deletePopup.SetActive(true);

        deleteYesButton.onClick.RemoveAllListeners();
        deleteNoButton.onClick.RemoveAllListeners();

        deleteYesButton.onClick.AddListener(() =>
        {
            if (pendingDeleteIndex >= 0)
            {
                DialogueStorage.DeleteDialogue(pendingDeleteIndex);
                RefreshList();
            }
            pendingDeleteIndex = -1;
            deletePopup.SetActive(false);

            var remainingDialogues = DialogueStorage.GetAllDialogues();
            if (remainingDialogues == null || remainingDialogues.Count == 0)
                ValidationManager.Instance.ShowWarning(
                    "No Dialogues Left",
                    "You've deleted all dialogues! Please add at least one before proceeding.",
                    () => SceneManager.LoadScene("CreateNewAddDialogueScene"),
                    () => SceneManager.LoadScene("CreateNewAddDialogueScene")
                );
        });

        deleteNoButton.onClick.AddListener(() =>
        {
            pendingDeleteIndex = -1;
            deletePopup.SetActive(false);
        });
    }

    public void SaveEditedDialogue(int index, string newCharacter, string newDialogue)
    {
        DialogueStorage.EditDialogue(index, newCharacter, newDialogue);
        Destroy(currentEditPanel);
        RefreshList();
    }

    public void CancelEdit()
    {
        if (currentEditPanel != null)
            Destroy(currentEditPanel);
    }

    

    public void Next() => SceneManager.LoadScene("CreateNewAddQuizScene");
    public void MainMenu() => SceneManager.LoadScene("Creator'sModeScene");
    public void Back() => SceneManager.LoadScene("CreateNewAddDialogueScene");
}