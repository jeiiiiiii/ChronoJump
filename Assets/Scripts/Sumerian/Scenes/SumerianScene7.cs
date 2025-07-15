using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class SumerianScene7 : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueLine
    {
        public string characterName;
        public string line;
    }

    [SerializeField] public TextMeshProUGUI dialogueText;
    [SerializeField] public Button nextButton;
    public Button backButton;

    public int currentDialogueIndex = 0;

    public DialogueLine[] dialogueLines;

    public SpriteRenderer PlayercharacterRenderer;
    public Sprite PlayerSmile;
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoSmile;

    void Start()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Parang panaginip... pero ang totoo, ang dami kong natutunan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Hindi lang basta luwad... ito'y alaala ng kabihasnang nagbigay daan sa mundo ngayon."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Galing mo! Handa ka na sa susunod na paglalakbay , kapag bukas na ang bagong daan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Simula pa lang pala ‘yon…"
            },
        };

        ShowDialogue();
        nextButton.onClick.AddListener(ShowNextDialogue);
        backButton.onClick.AddListener(ShowPreviousDialogue);
    }

    void ShowDialogue()
    {
        DialogueLine line = dialogueLines[currentDialogueIndex];
        dialogueText.text = $"<b>{line.characterName}</b>: {line.line}";

        switch (currentDialogueIndex)
        {
            case 0:
                ChronocharacterRenderer.sprite = ChronoSmile;
                PlayercharacterRenderer.sprite = PlayerSmile;
                break;
        }
    }


    void ShowNextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("QuizTime");
            nextButton.interactable = false;
            return;
        }

        ShowDialogue();
    }
    void ShowPreviousDialogue()
    {
        if (currentDialogueIndex > 0)
        {
            currentDialogueIndex--;
            ShowDialogue();
        }
    }
    public void Home()
    {
        SceneManager.LoadScene("TitleScreen");
    }
}


