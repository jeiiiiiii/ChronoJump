using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class AssyrianScene5 : MonoBehaviour
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
    public Sprite PlayerReflective;
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
                line = " Minsan ang kaalaman ay sandata. Ngunit ang takot ay hindi kailanman tumatagal."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ang imperyo ay naglalaho, ngunit ang alaala ay nananatili. Handa ka na sa susunod."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Paalam, Nineveh. Salamat sa aral."
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
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 2:
                PlayercharacterRenderer.sprite = PlayerSmile;
                break;
        }

    }
    void ShowNextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("AssyrianQuizTime");
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


