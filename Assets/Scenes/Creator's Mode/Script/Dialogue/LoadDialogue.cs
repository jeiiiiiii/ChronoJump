using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DialoguePlayer : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public Button nextButton;

    private int currentIndex = 0;
    private List<DialogueLine> dialogues;

    void Start()
    {
        dialogues = DialogueStorage.dialogues;
        if (dialogues.Count > 0)
        {
            ShowDialogue(0);
        }

        nextButton.onClick.AddListener(NextDialogue);
    }

    void ShowDialogue(int index)
    {
        // Combine character name and dialogue text in one field
        dialogueText.text = $"{dialogues[index].characterName}: {dialogues[index].dialogueText}";
    }

    void NextDialogue()
    {
        currentIndex++;
        if (currentIndex < dialogues.Count)
        {
            ShowDialogue(currentIndex);
        }
        else
        {
            dialogueText.text = "End of Dialogue!";
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(QuizTime);
        }
    }

    public void QuizTime()
    {
        SceneManager.LoadScene("QuizTime");
    }
}
