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

    [Header("Voice Selection for Editing")]
    public GameObject voiceSelectionPanel;
    public Button editVoiceSelectButton;
    public TextMeshProUGUI editCurrentVoiceText;

    private int pendingDeleteIndex = -1;
    private int currentEditingIndex = -1;
    private string currentEditingVoiceId = "";

    void Start()
    {
        // ✅ CRITICAL: Load voices from persistent storage before refreshing the list
        DialogueStorage.LoadAllVoices();

        RefreshList();

        if (editPopup != null) editPopup.SetActive(false);
        if (deletePopup != null) deletePopup.SetActive(false);
        if (ttsProgressPanel != null) ttsProgressPanel.SetActive(false);

        if (generateAllAudioButton != null)
        {
            generateAllAudioButton.onClick.AddListener(GenerateAllAudio);
        }

        // Debug: Verify voice assignments
        var dialogues = DialogueStorage.GetAllDialogues();
        Debug.Log($"📊 ReviewDialogueManager: Loaded {dialogues.Count} dialogues with voice assignments");
        for (int i = 0; i < dialogues.Count; i++)
        {
            var dialogue = dialogues[i];
            var voice = VoiceLibrary.GetVoiceById(dialogue.selectedVoiceId);
            Debug.Log($"📊 Dialogue {i}: '{dialogue.characterName}' - Voice: {voice.voiceName}");
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

            // ✅ FIXED: Use VoiceLibrary.GetVoiceById which now handles empty voice properly
            var voice = VoiceLibrary.GetVoiceById(line.selectedVoiceId);
            string voiceDisplay = voice.voiceName; // Will be "No Voice" if empty

            texts[0].text = $"{line.characterName} ({voiceDisplay})";

            // ✅ Show audio status indicator (only if voice is selected)
            string audioStatus = "";
            if (!VoiceLibrary.IsNoVoice(line.selectedVoiceId))
            {
                audioStatus = line.hasAudio ? " 🔊" : " 🔇";
            }
            texts[1].text = line.dialogueText + audioStatus;

            Button[] buttons = item.GetComponentsInChildren<Button>();
            Button editBtn = buttons[0];
            Button deleteBtn = buttons[1];

            deleteBtn.onClick.AddListener(() => OpenDeletePopup(index));
            editBtn.onClick.AddListener(() => OpenEditPopup(index, line));
        }
    }

    public void OpenEditPopup(int index, DialogueLine line)
    {
        editPopup.SetActive(true);
        nameInputField.text = line.characterName;
        textInputField.text = line.dialogueText;

        // ✅ NEW: Store current editing info for voice selection
        currentEditingIndex = index;
        currentEditingVoiceId = line.selectedVoiceId;

        // ✅ NEW: Setup voice selection if available
        if (editVoiceSelectButton != null)
        {
            editVoiceSelectButton.onClick.RemoveAllListeners();
            editVoiceSelectButton.onClick.AddListener(OpenEditVoiceSelection);
        }

        UpdateEditVoiceDisplay();

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

            // ✅ Get the original dialogue to compare changes
            var originalDialogue = DialogueStorage.GetAllDialogues()[index];
            bool textChanged = originalDialogue.dialogueText != textInputField.text.Trim();
            bool nameChanged = originalDialogue.characterName != nameInputField.text.Trim();

            // ✅ UPDATED: Update voice if changed
            if (!string.IsNullOrEmpty(currentEditingVoiceId) || VoiceLibrary.IsNoVoice(currentEditingVoiceId))
            {
                DialogueStorage.UpdateDialogueVoice(index, currentEditingVoiceId);
            }

            // Edit dialogue - this will handle audio invalidation
            DialogueStorage.EditDialogue(index, nameInputField.text, textInputField.text);

            Debug.Log($"✏️ Edited dialogue {index}: TextChanged={textChanged}, NameChanged={nameChanged}");

            editPopup.SetActive(false);
            currentEditingIndex = -1;
            RefreshList();
        });


        cancelEditButton.onClick.AddListener(() =>
        {
            editPopup.SetActive(false);
            currentEditingIndex = -1;
        });
    }


    void OpenEditVoiceSelection()
    {
        if (voiceSelectionPanel != null)
        {
            var voiceSelectionUI = voiceSelectionPanel.GetComponent<VoiceSelectionUI>();
            if (voiceSelectionUI != null)
            {
                voiceSelectionUI.ShowVoiceSelection(currentEditingVoiceId, OnEditVoiceSelected);
            }
            else
            {
                Debug.LogError("❌ VoiceSelectionUI component not found on voiceSelectionPanel!");
            }
        }
    }


    void OnEditVoiceSelected(string voiceId)
    {
        currentEditingVoiceId = voiceId;
        UpdateEditVoiceDisplay();

        var voice = VoiceLibrary.GetVoiceById(voiceId);
        Debug.Log($"🎤 Voice selected for editing dialogue {currentEditingIndex}: {voice.voiceName}");
    }

    void UpdateEditVoiceDisplay()
    {
        if (editCurrentVoiceText != null)
        {
            // ✅ FIXED: Handle empty voice properly
            if (VoiceLibrary.IsNoVoice(currentEditingVoiceId))
            {
                editCurrentVoiceText.text = "Voice: No Voice Selected";
            }
            else
            {
                var voice = VoiceLibrary.GetVoiceById(currentEditingVoiceId);
                editCurrentVoiceText.text = $"Voice: {voice.voiceName} ({voice.gender})";
            }
        }
    }

    // ✅ RESTORED: SaveEditedDialogue methods
    public void SaveEditedDialogue(int index, string newCharacter, string newDialogue, string newVoiceId = null)
    {
        // If a new voice ID is provided, update it
        if (!string.IsNullOrEmpty(newVoiceId) || VoiceLibrary.IsNoVoice(newVoiceId))
        {
            DialogueStorage.UpdateDialogueVoice(index, newVoiceId);
        }

        // Update the dialogue text
        DialogueStorage.EditDialogue(index, newCharacter, newDialogue);

        // Refresh the UI
        if (currentEditPanel != null)
            Destroy(currentEditPanel);
        RefreshList();
    }

    public void SaveEditedDialogue(int index, string newCharacter, string newDialogue)
    {
        DialogueStorage.EditDialogue(index, newCharacter, newDialogue);
        Destroy(currentEditPanel);
        RefreshList();
    }

    // ✅ RESTORED: CancelEdit method
    public void CancelEdit()
    {
        if (currentEditPanel != null)
            Destroy(currentEditPanel);
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

        for (int i = 0; i < dialogues.Count; i++)
        {
            Debug.Log($"Dialogue {i + 1}: '{dialogues[i].characterName}' - HasAudio: {dialogues[i].hasAudio}, Path: {dialogues[i].audioFilePath}");
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

        RefreshList();
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

    public void Next()
    {
        var dialogues = DialogueStorage.GetAllDialogues();

        // ✅ SINGLE VALIDATION: Check if any voice-selected dialogues lack audio
        var audioValidation = ValidationManager.Instance.ValidateAudioRegeneration(dialogues);
        if (!audioValidation.isValid)
        {
            ValidationManager.Instance.ShowWarning(
                "Audio Generation Needed",
                audioValidation.message,
                () =>
                {
                    // Generate audio but stay in current scene
                    Debug.Log("🎙️ Starting audio generation...");
                    GenerateAllAudio();
                },
                () =>
                {
                    // Cancel - just close warning
                    Debug.Log("User cancelled audio generation");
                }
            );
            return;
        }

        SceneManager.LoadScene("CreateNewAddQuizScene");
    }




    public void MainMenu() => SceneManager.LoadScene("Creator'sModeScene");
    public void Back() => SceneManager.LoadScene("CreateNewAddDialogueScene");
}
