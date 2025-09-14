using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EditDialogueUI : MonoBehaviour
{
    public TMP_InputField characterInput;
    public TMP_InputField dialogueInput;
    public Button saveButton;
    public Button cancelButton;

    private ReviewDialogueManager manager;
    private int dialogueIndex;

    public void Setup(ReviewDialogueManager manager, int index, string character, string dialogue)
    {
        this.manager = manager;
        this.dialogueIndex = index;

        characterInput.text = character;
        dialogueInput.text = dialogue;

        saveButton.onClick.AddListener(OnSave);
        cancelButton.onClick.AddListener(OnCancel);
    }

    void OnSave()
    {
        manager.SaveEditedDialogue(dialogueIndex, characterInput.text, dialogueInput.text);
    }

    void OnCancel()
    {
        manager.CancelEdit();
    }
}
