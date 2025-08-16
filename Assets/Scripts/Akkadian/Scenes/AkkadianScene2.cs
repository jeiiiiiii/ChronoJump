using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class AkkadianScene2 : MonoBehaviour
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
    public Sprite PlayerCurious;
    public Sprite PlayerSmile;
    public Sprite PlayerReflective;
    public Sprite SargonNuetral;
    public Sprite SargonFirm;
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    void Start()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "SARGON I",
                line = " Dati, kanya-kanya ang mga lungsod. Lahat gustong maging hari. Kaya't nagpasya akong pag-isahin sila sa ilalim ng isang pamumuno."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Paano mo ‘yon nagawa? Hindi ba mahirap kumbinsihin ang mga hari?"
            },
            new DialogueLine
            {
                characterName = "SARGON I",
                line = " Hindi ko sila kinumbinsi. Nilabanan ko sila. Pinag-isa ko sila sa pamamagitan ng husay sa digmaan at pamahalaan."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Kaya pala tinawag kang tagapagtatag ng unang imperyo. Lahat ng lungsod-estado ay naisa-ilalim sa iisang pamahalaan."
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
                ChronocharacterRenderer.sprite = SargonNuetral;
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 1:
                PlayercharacterRenderer.sprite = PlayerCurious;
                break;
            case 2:
                ChronocharacterRenderer.sprite = SargonFirm;
                break;
            case 3:
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
        }

    }
    void ShowNextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("AkkadianFirstRecallChallenges");
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


