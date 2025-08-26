using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class BabylonianScene3 : MonoBehaviour
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
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    public Sprite ChronoCheerful;
    public Sprite ChronoSmile;
    public Sprite HammurabiFirm;
    public Sprite HammurabiWise;
    void Start()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Ang laki na pala ng sakop ng Babylonia… Akala ko noon maliit lang ito."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Noong una, isa lang itong lungsod. Pero sa pamumuno ni Hammurabi, napasakamay niya ang maraming lugar sa Mesopotamia."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Hindi lang hilaga ang nasakop, pati ang timog. Kabilang na ang Ashur, isa sa mga mahahalagang lungsod noon."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Paano niya nagawa ‘yon? Sa dami ng lungsod-estado, hindi ba’t mahirap pagsamahin ang mga ito?"
            },
            new DialogueLine
            {
                characterName = "HAMMURABI",
                line = " Hindi madali ang magtaguyod ng imperyo. Kailangang timbangin ang lakas ng hukbo at ang katalinuhan sa pamumuno. Gumamit ako ng diplomasya, kasunduan, at sa panahong kinakailangan—digmaan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Hindi lang pala espadang dala mo, kundi pati karunungan."
            },
            new DialogueLine
            {
                characterName = "HAMMURABI",
                line = " Ang tunay na lakas ng isang pinuno ay nasa kakayahang pag-isahin ang kanyang nasasakupan. At iyon ang layunin ng Babylonia."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Dahil dito, kinilala si Hammurabi hindi lamang bilang mandirigma, kundi bilang tagapagtaguyod ng kaayusan at pagkakaisa sa buong rehiyon."
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
            case 1:
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 2:
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 3:
                ChronocharacterRenderer.sprite = HammurabiFirm;
                break;
            case 4:
                PlayercharacterRenderer.sprite = PlayerSmile;
                break;
            case 5:
                ChronocharacterRenderer.sprite = HammurabiWise;
                PlayercharacterRenderer.sprite = PlayerCurious;
                break;
            case 6:
                ChronocharacterRenderer.sprite = ChronoCheerful;
                break;
        }

    }
    void ShowNextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("BabylonianSceneFour");
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


