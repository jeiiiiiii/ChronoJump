using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;


public class AssyrianScene1 : MonoBehaviour
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

    public SpriteRenderer TiglathFulldrawnSprite;
    public Sprite TiglathAssertive;
    public Sprite TiglathPrideful;
    public Sprite TiglathCold;
    public SpriteRenderer PlayercharacterRenderer; // chibi
    public Sprite PlayerCurious;
    public Sprite PlayerEager;
    public Sprite PlayerSmile;
    public Sprite PlayerReflective;
    public Sprite PlayerEmbarassed;
    public SpriteRenderer ChronocharacterRenderer; // chibi
    public Sprite ChronoThinking;
    public Sprite ChronoSmile;

    void Start()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {   characterName = "PLAYER",
                line = " Saan na naman ako napunta? Ang taas ng mga gusali... parang may banta ng digmaan kahit saan."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Maligayang pagdating sa Nineveh, kabisera ng imperyong Assyrian. Panahon ito ng muling pagkakaisa ng Mesopotamia, hindi sa bisa ng kasunduan, kundi sa pamamagitan ng takot, sandata, at pamumuno."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Nineveh? Hindi ba ito ang kilalang lungsod na malapit sa Tigris?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Tumpak. Ang Assyria ay nagsimulang isang tribo sa bulubunduking rehiyon hilaga ng Babylon mula sa mga lambak ng Tigris hanggang sa mataas na kabundukan ng Armenia."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Noon ay mga mangingisda at mangangalakal, ngayo’y mga pinuno ng takot."
            },
            new DialogueLine
            {
                characterName = "TIGLATH-PILISER I",
                line = " Sino kayo? Mga tagalabas? Ang Nineveh ay hindi lugar para sa mga mahihina. Kami ay isinilang sa digmaan, at lumalaki sa pamumuno."//5
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Haring Tiglath-Pileser I, kami’y mga tagapagmasid ng kasaysayan. Ipinapakita ko sa aking kasama ang iyong dakilang pamana, ang pagbubuo ng imperyo, at ang mahigpit mong panuntunan."
            },
            new DialogueLine//7
            {
                characterName = "TIGLATH-PILISER I",
                line = " Kung gayon, saksihan ninyo kung paano ko pinagbuklod ang mga lupain mula silangan hanggang kanluran. Sinupil namin ang mga Hittite, at naabot ng aming mga hukbo ang baybayin ng Mediterranean."
            },
            new DialogueLine
            {
                characterName = "TIGLATH-PILISER I",
                line = " Nagpadala kami ng mga ekspedisyong militar upang sakupin ang mga rutang pangkalakalan. Walang tumutol. Lahat ay yumuko."
            },
            new DialogueLine
            {
                characterName = "TIGLATH-PILISER I",
                line = " Ang mga natalo ay nagbigay ng tributopilak, ginto, hayop, at minsan pati mga anak nilang lalaki."
            },
            new DialogueLine//10
            {
                characterName = "PLAYER",
                line = " Lahat ba ay sumang-ayon? O napilitan lang?"
            },
            new DialogueLine//11
            {
                characterName = "TIGLATH-PILISER I",
                line = " Ang kapayapaan ay hindi regalo. Ito'y kinukuha sa pamamagitan ng takot. Ang mga lungsod na lumaban ay sinunog. Ang mga bihag ay pinako sa poste sa harap ng kanilang paderisang babala sa mga susunod."
            }
        };

        ShowDialogue();
        nextButton.onClick.AddListener(ShowNextDialogue);
        backButton.onClick.AddListener(ShowPreviousDialogue);
    }

    void ShowDialogue()
    {
        TiglathFulldrawnSprite.enabled = false;
        DialogueLine line = dialogueLines[currentDialogueIndex];
        dialogueText.text = $"<b>{line.characterName}</b>: {line.line}";

        switch (currentDialogueIndex)
        {
            case 0:
                PlayercharacterRenderer.enabled = PlayerCurious;
                ChronocharacterRenderer.enabled = ChronoSmile;
                break;
            case 1:
                ChronocharacterRenderer.enabled = ChronoThinking;
                break;
            case 2:
                PlayercharacterRenderer.sprite = PlayerCurious;
                ChronocharacterRenderer.enabled = ChronoSmile;
                break;
            case 3:
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 5:
                PlayercharacterRenderer.sprite = PlayerEmbarassed;
                TiglathFulldrawnSprite.enabled = true;
                break;
            case 6:
                ChronocharacterRenderer.sprite = ChronoSmile;
                PlayercharacterRenderer.sprite = PlayerCurious;
                break;
            case 7:
                ChronocharacterRenderer.sprite = TiglathPrideful;
                break;
            case 8:
                PlayercharacterRenderer.sprite = PlayerReflective;
                ChronocharacterRenderer.sprite = TiglathCold;
                break;
            case 9:
                ChronocharacterRenderer.sprite = TiglathPrideful;
                break;
            case 10:
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 11:
                ChronocharacterRenderer.sprite = TiglathCold;
                break;
        }
    }
    void ShowNextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("AssyrianFirstRecallChallenges");
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


