using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AddDialogue : MonoBehaviour
{
    public TMP_InputField characterNameInput;
    public TMP_InputField dialogueInput;
    public TMP_Text characterCountText;
    
    [Header("Voice Selection")]
    public Button selectVoiceButton;
    public TextMeshProUGUI currentVoiceText;
    public GameObject voiceSelectionPanel;
    
    private string selectedVoiceId;
    private VoiceSelectionUI voiceSelectionUI;

    // Expose the currently selected voice to other systems
    public string GetSelectedVoiceId() => selectedVoiceId;

    void Start()
    {
        // ✅ FIXED: Load the last used voice for this story OR use default
        selectedVoiceId = LoadLastUsedVoiceForStory();
        UpdateVoiceDisplay();
        
        Debug.Log($"🎤 AddDialogue initialized with voice: {VoiceLibrary.GetVoiceById(selectedVoiceId).voiceName}");
        
        // Setup voice selection
        if (voiceSelectionPanel != null)
        {
            voiceSelectionUI = voiceSelectionPanel.GetComponent<VoiceSelectionUI>();
            voiceSelectionPanel.SetActive(false);
        }
        
        if (selectVoiceButton != null)
        {
            selectVoiceButton.onClick.AddListener(OpenVoiceSelection);
        }
        
        // Add listeners to update character count
        if (characterNameInput != null)
        {
            characterNameInput.onValueChanged.AddListener(UpdateCharacterCount);
        }
        if (dialogueInput != null)
        {
            dialogueInput.onValueChanged.AddListener(UpdateCharacterCount);
        }
        UpdateCharacterCount("");
    }

    string LoadLastUsedVoiceForStory()
    {
        // ✅ CHANGED: Start with empty voice instead of defaulting to Rachel
        return ""; // Empty string means no voice selected
    }


    // ✅ FIXED: Save the selected voice for this story session
    void OnVoiceSelected(string voiceId)
    {
        selectedVoiceId = voiceId;
        UpdateVoiceDisplay();
        
        // Save for this session
        var story = StoryManager.Instance?.GetCurrentStory();
        string storyId = story?.storyId ?? "default";
        string storageKey = $"LastUsedVoice_{storyId}";
        PlayerPrefs.SetString(storageKey, voiceId);
        PlayerPrefs.Save();
        
        var voice = VoiceLibrary.GetVoiceById(voiceId);
        Debug.Log($"🎤 Voice selected and saved: {voice.voiceName} for story {storyId}");
    }

    public void AddDialogueLine()
    {
        var story = StoryManager.Instance.currentStory;
        if (story == null)
        {
            Debug.LogError("❌ No current story!");
            return;
        }

        string name = characterNameInput.text.Trim();
        string dialogue = dialogueInput.text.Trim();

        var validation = ValidationManager.Instance.ValidateNameAndDialogueCombined(name, dialogue);
        if (!validation.isValid)
        {
            ValidationManager.Instance.ShowWarning(
                "Dialogue Validation",
                validation.message,
                null,
                () =>
                {
                    if (string.IsNullOrEmpty(name))
                        characterNameInput.Select();
                    else if (string.IsNullOrEmpty(dialogue))
                        dialogueInput.Select();
                }
            );
            return;
        }

        // ✅ Add dialogue with selected voice (could be empty)
        var newDialogue = new DialogueLine(name, dialogue, selectedVoiceId);
        story.dialogues.Add(newDialogue);

        // ✅ Only save voice to persistent storage if it's not empty
        if (!string.IsNullOrEmpty(selectedVoiceId))
        {
            int dialogueIndex = story.dialogues.Count - 1;
            VoiceStorageManager.SaveVoiceSelection($"Dialogue_{dialogueIndex}", selectedVoiceId);
        }

        if (string.IsNullOrEmpty(selectedVoiceId))
        {
            Debug.Log($"✅ Added dialogue: {name} - {dialogue} (No Voice Selected)");
        }
        else
        {
            var voice = VoiceLibrary.GetVoiceById(selectedVoiceId);
            Debug.Log($"✅ Added dialogue: {name} - {dialogue} (Voice: {voice.voiceName})");
        }

        // Clear inputs but KEEP the current voice selection (empty)
        characterNameInput.text = "";
        dialogueInput.text = "";
        UpdateCharacterCount("");
    }


    void OpenVoiceSelection()
    {
        if (voiceSelectionUI != null)
        {
            voiceSelectionUI.ShowVoiceSelection(selectedVoiceId, OnVoiceSelected);
        }
    }

    void UpdateVoiceDisplay()
    {
        if (currentVoiceText != null)
        {
            if (string.IsNullOrEmpty(selectedVoiceId))
            {
                currentVoiceText.text = "Voice: Select Voice...";
            }
            else
            {
                var voice = VoiceLibrary.GetVoiceById(selectedVoiceId);
                currentVoiceText.text = $"Voice: {voice.voiceName} ({voice.gender})";
            }
        }
    }


    private void UpdateCharacterCount(string value)
    {
        if (characterCountText != null)
        {
            int nameLength = characterNameInput?.text?.Length ?? 0;
            int dialogueLength = dialogueInput?.text?.Length ?? 0;
            int totalLength = nameLength + dialogueLength;
            int remaining = ValidationManager.Instance.maxNameDialogueCombinedLength - totalLength;

            characterCountText.text = $"Characters: {totalLength}/{ValidationManager.Instance.maxNameDialogueCombinedLength} (Remaining: {remaining})";
            
            if (remaining < 0)
                characterCountText.color = Color.red;
            else if (remaining < 20)
                characterCountText.color = Color.yellow;
            else
                characterCountText.color = Color.white;
        }
    }

    public void Next()
    {
        var story = StoryManager.Instance.currentStory;
        if (story == null)
        {
            Debug.LogError("❌ No current story!");
            return;
        }

        if (!ValidationManager.Instance.HasDialogues(story.dialogues))
        {
            ValidationManager.Instance.ShowWarning(
                "Dialogues Required",
                "You need to add at least one dialogue before proceeding!",
                null,
                () => { }
            );
            return;
        }

        SceneManager.LoadScene("ReviewDialogueScene");
    }
    
    public void MainMenu()
    {
        SceneManager.LoadScene("Creator'sModeScene");
    }
    
    public void Back()
    {
        SceneManager.LoadScene("CreateNewAddFrameScene");
    }
}
