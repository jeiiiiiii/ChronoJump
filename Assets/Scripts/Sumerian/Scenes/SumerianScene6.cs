using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class SumerianScene6 : MonoBehaviour
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
    public Sprite PlayerEager;
    public Sprite PlayerReflective;
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoSmile;
    public Sprite ChronoThinking;

    public Sprite EnkiPokerface;
    public Sprite EnkiStern;

    void Start()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Tila may ipinapahayag na mahalagang kautusan sa lugar na ito... Halika, lumapit tayo."
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Nariyan pala kayo. Mainam at dumating kayo ngayon. Itinatanghal dito ang mga batas na iniakda ni Haring Ur-Nammu , ang mga tuntuning gumagabay sa aming pamayanan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Ang laki ng sulat... parang ukit na masinsin. Para saan po ang mga ito?"
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Bawat linya ay may dalang layunin , upang itaguyod ang katarungan. Sa mga panahong may alitan, pagkakautang, o pagkukulang, hindi sapat ang galit o tsismis. Kailangang may sinusunod na batayan."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " At dito ipinamalas ng mga Sumerian ang isa sa pinakamahalagang ambag nila: ang pagsusulat ng mga batas sa isang sistema. Hindi ito basta utos , ito'y mga alituntunin na nagtatanggol sa karapatan ng mahina at mapagbigay ng parusa sa mapang-abuso."
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Ang pagkakaroon ng malinaw na batas ay nagsilbing haligi ng aming kaayusan. Kahit alipin, kahit babae , may karapatang pinoprotektahan ng mga alituntuning ito."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Ibig sabihin po... may paraan kayo para lutasin ang problema ng makatarungan?"
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Ganyan nga. Dahil sa mga batas na ito, naiiwasan ang kaguluhan. May proseso. May timbang ang salita ng bawat isa."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ngayon, gusto kong malaman... sa dami ng layunin ng batas, ano sa tingin mo ang pinakapuso nito?"
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
            case 1:
                ChronocharacterRenderer.sprite = EnkiPokerface;
                break;
            case 2:
                PlayercharacterRenderer.sprite = PlayerEager;
                break;
            case 3:
                ChronocharacterRenderer.sprite = EnkiStern;
                break;
            case 4:
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 5:
                ChronocharacterRenderer.sprite = EnkiPokerface;
                break;
            case 6:
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 7:
                ChronocharacterRenderer.sprite = EnkiStern;
                break;
            case 8:
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
        }
    }


    void ShowNextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("SumerianFifthRecallChallenges");
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


