using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;


public class SumerianScene1 : MonoBehaviour
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
    public SpriteRenderer PlayerFulldrawnSprite;
    public SpriteRenderer ChronoFulldrawnSprite;
    public Sprite PlayerFulldrawn;
    public Sprite PlayerCurious;
    public Sprite PlayerEager;
    public Sprite PlayerSmile;
    public Sprite PlayerReflective;
    public Sprite PlayerEmbarassed;

    public SpriteRenderer EnkicharacterRenderer;
    public Sprite EnkiKind;
    public Sprite EnkiPokerface;
    public Sprite EnkiStern;
    public Sprite EnkiTesting;
    public SpriteRenderer ChronocharacterRenderer;

    public Sprite ChronoCheerful;
    public Sprite ChronoSad;
    public Sprite ChronoSmile;
    public Sprite ChronoThinking;

    public AudioSource audioSource;
    public AudioClip[] dialogueClips;


    void Start()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {   characterName = "PLAYER",
                line = " Ano ‘to…? Ang daming putik... Teka, hindi ito 'yung kwarto ko!"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Huwag kang mag-alala. Nandito tayo ngayon sa Uruk. Isa sa mga unang lungsod sa kasaysayan. Kabihasnang Sumerian , mga tatlong libong taon bago pa ipinanganak si Kristo (3300 BCE)."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Parang disyerto, pero may mga pananim... at ang taas ng gusaling 'yon!"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ziggurat ang tawag diyan. Sentro ng lungsod. Hindi sila umaasa sa ulan, kaya may mga kanal at imbakan ng tubig sila. Lahat may gamit, lahat may layunin."
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Ano’t may mga mukhang banyaga sa gitna ng Uruk? Hmm… hindi kayo taga-rito."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " A-ah... eh, opo. Ngayon lang po ako napadpad dito."
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Huwag kang matakot. Kami’y hindi sarado sa panauhin. Ako si Enki, isang eskriba. Tagapag-ingat ng kaalaman ng aming bayan."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Isa siya sa mga tagasulat ng ating kasaysayan. Sa panahon nila, ang pagsulat ay sining, hindi biro."
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Nakatala sa putik ang mga mahahalagang pangyayari, kung ilang sako ng butil ang naani, kung anong batas ang pinairal, o maging ang mga dasal ng mga tao."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Gamit nila ang stylus sa luwad. Tinatawag ang sistemang ito na cuneiform, mga hugis-pakong simbolo na may kahulugan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Kaya pala parang may mga ukit sa luwad..."
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Mukhang may interes ka. Halika’t sumama sa akin. Ipapakita ko sa’yo kung paanong ang mga simpleng marka ay nagiging salamin ng buong lungsod."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ihanda mo ang isip mo. Hindi lang ito basta pagsusulat, ito ang puso ng kabihasnan."
            },
        };
        ShowDialogue();
        nextButton.onClick.AddListener(ShowNextDialogue);
        backButton.onClick.AddListener(ShowPreviousDialogue);
    }

    void ShowDialogue()
    {
        EnkicharacterRenderer.enabled = false;
        PlayerFulldrawnSprite.enabled = false;
        ChronoFulldrawnSprite.enabled = false;
        DialogueLine line = dialogueLines[currentDialogueIndex];
        dialogueText.text = $"<b>{line.characterName}</b>: {line.line}";

        if (audioSource != null && dialogueClips != null && currentDialogueIndex < dialogueClips.Length)
        {
            audioSource.clip = dialogueClips[currentDialogueIndex];
            audioSource.Play();
        }

        switch (currentDialogueIndex)
        {
            case 0:
                PlayerFulldrawnSprite.enabled = true;
                PlayercharacterRenderer.enabled = false;
                ChronocharacterRenderer.enabled = false;
                break;
            case 1:
                ChronoFulldrawnSprite.enabled = true;
                PlayercharacterRenderer.enabled = true;
                PlayercharacterRenderer.sprite = PlayerEager;
                break;
            case 2:
                PlayercharacterRenderer.sprite = PlayerEager;
                ChronocharacterRenderer.enabled = true;
                break;
            case 3:
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 4:
                EnkicharacterRenderer.enabled = true;
                break;
            case 5:
                ChronocharacterRenderer.sprite = EnkiPokerface;
                PlayercharacterRenderer.sprite = PlayerSmile;
                break;
            case 6:
                ChronocharacterRenderer.sprite = EnkiKind;
                break;
            case 7:
                ChronocharacterRenderer.sprite = ChronoCheerful;
                break;
            case 8:
                ChronocharacterRenderer.sprite = EnkiPokerface;
                break;
            case 9:
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 11:
                PlayercharacterRenderer.sprite = PlayerReflective;
                ChronocharacterRenderer.sprite = EnkiKind;
                break;
            case 12:
                PlayercharacterRenderer.sprite = PlayerSmile;
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
        }
    }
    void ShowNextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("SumerianSceneTwo");
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


