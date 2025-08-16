using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class SumerianScene4 : MonoBehaviour
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
    public Sprite PlayerEager;
    public Sprite PlayerSmile;
    public Sprite PlayerReflective;
    public Sprite PlayerEmbarassed;

    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoCheerful;
    public Sprite ChronoSad;
    public Sprite ChronoSmile;
    public Sprite ChronoThinking;

    public SpriteRenderer PatesicharacterRenderer;
    public Sprite PatesiFormal;
    public Sprite PatesiDisapproval;
    public Sprite PatesiExplaining;


    void Start()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ang mga gusaling ito, na tila hagdan paakyat sa langit, ay tinatawag na ziggurat. Dito isinasagawa ang mga ritwal at alay para sa mga diyos ng Sumer."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Napakataas... at ang daming tao sa paligid. Parang may malaking pagtitipon."
            },
            new DialogueLine
            {
                characterName = "PATESI NINURTA",
                line = " Ako si Ninurta, patesi ng Uruk. Sa basbas ng aming mga diyos, ako ang namumuno sa lungsod , tagapagbantay ng batas at tagapamagitan ng langit at lupa."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Parang... siya ang pari? Pero parang siya rin ang lider?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Tama. Si Ninurta ay isang pinunong-pari. Sa kabihasnan ng Sumerian, hindi hiwalay ang pananampalataya sa pamahalaan. Ang tawag sa ganitong sistema ay theocracy , isang uri ng pamahalaang pinamumunuan ng taong kinikilalang kinatawan ng mga diyos."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Dahil dito, ang batas ay itinuturing na sagrado. Ang utos ng pinuno ay utos din ng diyos. Kaya’t ang mga tao ay may takot at paggalang, hindi lamang sa kanilang pinuno kundi pati sa batas na kanilang sinusunod."
            },
        };

        ShowDialogue();
        nextButton.onClick.AddListener(ShowNextDialogue);
        backButton.onClick.AddListener(ShowPreviousDialogue);
    }

    void ShowDialogue()
    {
        PatesicharacterRenderer.enabled = false;
        DialogueLine line = dialogueLines[currentDialogueIndex];
        dialogueText.text = $"<b>{line.characterName}</b>: {line.line}";

        switch (currentDialogueIndex)
        {
            case 0:
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 1:
                PlayercharacterRenderer.sprite = PlayerCurious;
                break;
            case 2:
                PatesicharacterRenderer.enabled = true;
                break;
            case 3:
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 4:
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 6:
                ChronocharacterRenderer.sprite = PatesiFormal;
                break;
        }
    }


    void ShowNextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("SumerianThirdRecallChallenges");
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


