using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueItemUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI characterText;
    public TextMeshProUGUI dialogueText;
    public Button editButton;
    public Button deleteButton;

    private ReviewDialogueManager manager;
    private int dialogueIndex;

    public void Setup(ReviewDialogueManager manager, int index, string character, string dialogue)
    {
        this.manager = manager;
        this.dialogueIndex = index;

        characterText.text = character;
        dialogueText.text = dialogue;

        editButton.onClick.AddListener(OnEditClicked);
        deleteButton.onClick.AddListener(OnDeleteClicked);
    }

    void OnEditClicked()
    {
var dialogues = DialogueStorage.GetAllDialogues();
    if (dialogueIndex < 0 || dialogueIndex >= dialogues.Count)
        return;
    manager.OpenEditPopup(dialogueIndex, dialogues[dialogueIndex]);
    }


    void OnDeleteClicked()
    {
var dialogues = DialogueStorage.GetAllDialogues();
    if (dialogueIndex < 0 || dialogueIndex >= dialogues.Count)
        return;
    DialogueStorage.DeleteDialogue(dialogueIndex);
    manager.RefreshList();
    }
}
