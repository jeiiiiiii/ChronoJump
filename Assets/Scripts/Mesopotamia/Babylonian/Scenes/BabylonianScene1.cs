using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;


public class BabylonianScene1 : MonoBehaviour
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

    public SpriteRenderer PlayercharacterRenderer; // chibi
    public SpriteRenderer ChronocharacterRenderer; // chibi
    public SpriteRenderer HammurabiFulldrawnSprite;
    public Sprite PlayerCurious;
    public Sprite PlayerEager;
    public Sprite PlayerSmile;
    public Sprite PlayerReflective;
    public Sprite PlayerEmbarassed;

    public Sprite ChronoCheerful;
    public Sprite ChronoSmile;

    void Start()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {   characterName = "PLAYER",
                line = " Nasaan na naman tayo, Chrono? Parang ibang lugar ito… mas tahimik, pero maayos."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Nakarating tayo sa Babylonia. Isa itong imperyo na itinatag ng mga Amorite, isang grupo ng mga taong galing sa kanlurang bahagi ng Asya."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Babylonia? Hindi ko pa naririnig ‘yan. Ano’ng meron dito?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Dito mo makikilala ang isa sa mga pinakakilalang hari sa sinaunang daigdig. Si Hammurabi. Siya ang nag-utos na buuin ang isang hanay ng batas para sa mga tao."
            },
            new DialogueLine
            {
                characterName = "HAMMURABI",
                line = " Maligayang pagdating sa Babylonia. Ako si Hammurabi, ang tagapamahala ng imperyong ito. Halina at samahan ninyo ako."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Kayo po ba ang gumawa ng mga sinaunang batas?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Oo, siya nga. Tinagurian siyang pinakadakilang hari ng Babylonia dahil sa kanyang matalinong pamamahala."
            },
        };

        ShowDialogue();
        nextButton.onClick.AddListener(ShowNextDialogue);
        backButton.onClick.AddListener(ShowPreviousDialogue);
    }

    void ShowDialogue()
    {
        HammurabiFulldrawnSprite.enabled = false;
        DialogueLine line = dialogueLines[currentDialogueIndex];
        dialogueText.text = $"<b>{line.characterName}</b>: {line.line}";

        switch (currentDialogueIndex)
        {
            case 0:
                PlayercharacterRenderer.enabled = PlayerCurious;
                ChronocharacterRenderer.enabled = ChronoSmile;
                break;
            case 1:
                ChronocharacterRenderer.enabled = ChronoCheerful;
                break;
            case 2:
                PlayercharacterRenderer.sprite = PlayerEager;
                break;
            case 3:
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;

            case 4:
                PlayercharacterRenderer.sprite = PlayerCurious;
                HammurabiFulldrawnSprite.enabled = true;
                break;

            case 5:
                HammurabiFulldrawnSprite.enabled = true;
                ChronocharacterRenderer.sprite = ChronoCheerful;
                PlayercharacterRenderer.sprite = PlayerEmbarassed;
                break;

            case 6:
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
        }

    }
    void ShowNextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("BabylonianFirstRecallChallenges");
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


