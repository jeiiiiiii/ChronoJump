using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ReviewDialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentParent; // ScrollView Content
    public GameObject dialogueItemPrefab;
    public GameObject editDialoguePrefab; // assign EditDialogueItems prefab in Inspector

    private GameObject currentEditPanel; // track the open editor

    [Header("Edit Popup References")]
    public GameObject editPopup;               // The popup panel prefab
    public TMP_InputField nameInputField;      // Input for character name
    public TMP_InputField textInputField;      // Input for dialogue text
    public Button saveEditButton;              // Save button inside popup
    public Button cancelEditButton;            // Cancel button inside popup

    void Start()
    {
        RefreshList();
    }

    public void RefreshList()
    {
        // Clear old items
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        // Populate with current dialogues
        var allDialogues = DialogueStorage.GetAllDialogues();
        for (int i = 0; i < allDialogues.Count; i++)
{
            int index = i; // local copy for buttons
            DialogueLine line = allDialogues[i];

            GameObject item = Instantiate(dialogueItemPrefab, contentParent);

            // Fill text
            TextMeshProUGUI[] texts = item.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = line.characterName;
            texts[1].text = line.dialogueText;

            // Buttons
            Button[] buttons = item.GetComponentsInChildren<Button>();
            Button editBtn = buttons[0];
            Button deleteBtn = buttons[1];

            deleteBtn.onClick.AddListener(() =>
            {
                DialogueStorage.DeleteDialogue(index);
                RefreshList();
            });

            editBtn.onClick.AddListener(() =>
            {
                OpenEditPopup(index, line);
            });
        }
    }

    public void OpenEditPopup(int index, DialogueLine line)
    {
        editPopup.SetActive(true);

        // Fill inputs with current values
        nameInputField.text = line.characterName;
        textInputField.text = line.dialogueText;

        // Reset listeners to avoid duplicate calls
        saveEditButton.onClick.RemoveAllListeners();
        cancelEditButton.onClick.RemoveAllListeners();

        // Save button
        saveEditButton.onClick.AddListener(() =>
        {
            DialogueStorage.EditDialogue(index, nameInputField.text, textInputField.text);
            editPopup.SetActive(false);
            RefreshList();
        });

        // Cancel button
        cancelEditButton.onClick.AddListener(() =>
        {
            editPopup.SetActive(false);
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

    // Navigation buttons
    public void Next() => SceneManager.LoadScene("CreateNewAddQuizScene");
    public void MainMenu() => SceneManager.LoadScene("Creator'sModeScene");
    public void Back() => SceneManager.LoadScene("CreateNewAddDialogueScene");
}
