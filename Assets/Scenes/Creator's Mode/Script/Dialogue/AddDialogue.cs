using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AddDialogue : MonoBehaviour
{
    public TMP_InputField characterNameInput;
    public TMP_InputField dialogueInput;
    public TMP_Text characterCountText; // Optional: Show remaining characters

    void Start()
    {
        // Add listeners to update character count in real-time
        if (characterNameInput != null)
        {
            characterNameInput.onValueChanged.AddListener(UpdateCharacterCount);
        }
        if (dialogueInput != null)
        {
            dialogueInput.onValueChanged.AddListener(UpdateCharacterCount);
        }
        UpdateCharacterCount(""); // Initialize
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

        // Use combined validation instead of individual
        var validation = ValidationManager.Instance.ValidateNameAndDialogueCombined(name, dialogue);
        if (!validation.isValid)
        {
            ValidationManager.Instance.ShowWarning(
                "Dialogue Validation",
                validation.message,
                null,
                () => { 
                    if (string.IsNullOrEmpty(name)) 
                        characterNameInput.Select();
                    else if (string.IsNullOrEmpty(dialogue))
                        dialogueInput.Select();
                }
            );
            return;
        }

        // Add dialogue to story
        var newDialogue = new DialogueLine(name, dialogue);
        story.dialogues.Add(newDialogue);

        // Clear inputs and update count
        characterNameInput.text = "";
        dialogueInput.text = "";
        UpdateCharacterCount("");

        Debug.Log($"✅ Added dialogue: {newDialogue.characterName} - {newDialogue.dialogueText}");
    }

    // NEW: Real-time character count display
    private void UpdateCharacterCount(string value)
    {
        if (characterCountText != null)
        {
            int nameLength = characterNameInput?.text?.Length ?? 0;
            int dialogueLength = dialogueInput?.text?.Length ?? 0;
            int totalLength = nameLength + dialogueLength;
            int remaining = ValidationManager.Instance.maxNameDialogueCombinedLength - totalLength;

            characterCountText.text = $"Characters: {totalLength}/{ValidationManager.Instance.maxNameDialogueCombinedLength} (Remaining: {remaining})";
            
            // Optional: Change color when approaching limit
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

        // Check if there are any dialogues
        if (!ValidationManager.Instance.HasDialogues(story.dialogues))
        {
            ValidationManager.Instance.ShowWarning(
                "Dialogues Required",
                "You need to add at least one dialogue before proceeding!",
                null,
                () => { /* Stay on current scene */ }
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
