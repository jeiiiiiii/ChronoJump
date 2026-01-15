using UnityEngine;
using TMPro;

public class AddDialogueUI : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_InputField dialogueInput;

    public void AddDialogue()
    {
        if (!string.IsNullOrEmpty(nameInput.text) && !string.IsNullOrEmpty(dialogueInput.text))
        {
            DialogueStorage.AddDialogue(nameInput.text, dialogueInput.text);

            // Optional: clear fields for next input
            nameInput.text = "";
            dialogueInput.text = "";
        }
    }
}
