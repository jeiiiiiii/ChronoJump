using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EditDialogueUI : MonoBehaviour
{
    public TMP_InputField characterInput;
    public TMP_InputField dialogueInput;
    public Button saveButton;
    public Button cancelButton;

    [Header("Voice Selection")]
    public Button selectVoiceButton;
    public TextMeshProUGUI currentVoiceText;
    public GameObject voiceSelectionPanel;

    private ReviewDialogueManager manager;
    private int dialogueIndex;
    private string selectedVoiceId;
    private VoiceSelectionUI voiceSelectionUI;

    public void Setup(ReviewDialogueManager manager, int index, string character, string dialogue, string currentVoiceId)
    {
        this.manager = manager;
        this.dialogueIndex = index;
        this.selectedVoiceId = currentVoiceId;

        characterInput.text = character;
        dialogueInput.text = dialogue;

        saveButton.onClick.AddListener(OnSave);
        cancelButton.onClick.AddListener(OnCancel);

        // ✅ NEW: Setup voice selection
        if (selectVoiceButton != null)
        {
            selectVoiceButton.onClick.AddListener(OpenVoiceSelection);
        }

        if (voiceSelectionPanel != null)
        {
            voiceSelectionUI = voiceSelectionPanel.GetComponent<VoiceSelectionUI>();
            voiceSelectionPanel.SetActive(false);
        }

        UpdateVoiceDisplay();
    }

    void OnSave()
    {
        // ✅ NEW: Pass the selected voice ID when saving
        manager.SaveEditedDialogue(dialogueIndex, characterInput.text, dialogueInput.text, selectedVoiceId);
    }

    void OnCancel()
    {
        manager.CancelEdit();
    }

    // ✅ NEW: Voice selection methods
    void OpenVoiceSelection()
    {
        if (voiceSelectionUI != null)
        {
            voiceSelectionUI.ShowVoiceSelection(selectedVoiceId, OnVoiceSelected);
        }
        else
        {
            Debug.LogError("❌ VoiceSelectionUI component not found on voiceSelectionPanel!");
        }
    }

    void OnVoiceSelected(string voiceId)
    {
        selectedVoiceId = voiceId;
        UpdateVoiceDisplay();

        var voice = VoiceLibrary.GetVoiceById(voiceId);
        Debug.Log($"🎤 Voice selected for editing: {voice.voiceName}");
    }

    void UpdateVoiceDisplay()
    {
        if (currentVoiceText != null)
        {
            var voice = VoiceLibrary.GetVoiceById(selectedVoiceId);
            currentVoiceText.text = $"Voice: {voice.voiceName} ({voice.gender})";
        }
    }
}
