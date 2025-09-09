using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class SumerianScene2 : MonoBehaviour
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
            {
                characterName = "ENKI",
                line = " Malugod kayong tinatanggap sa lugar kung saan itinatala ang diwa ng aming kabihasnan. Ang bawat pangyayari, ani, kasunduan, batas, lahat ay nakaukit dito."
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Tingnan mo ito. Isang talaan ng mga ani noong nakaraang taon. Kapag kulang ang butil o may nanlinlang sa takal, dito kami bumabalik."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Para bang bawat guhit ay may kwento."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Hindi lang kwento… kundi alaala ng buong lungsod. Kung walang ganitong sistema, magugulo ang pamumuhay ng mga Sumerian."
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Sa halip na tinta, ito ang gamit namin, stylus na yari sa tambo. Ipinipindot ito sa malambot na luwad para makalikha ng mga hugis at simbolo."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Kaya pala mukhang may paekis at pakuwit ang mga guhit…"
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Ang tawag dito sa sistemang ito ay cuneiform. Galing sa salitang nangangahulugang ‘hugis-pantusok’. Bawat marka, may ibig sabihin. Bawat tablet, may silbi."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ang mga batang nais maging eskriba ay pinipiling mabuti. Kinakailangang matiyaga at matalas ang pag-iisip. Kapag nagkamali sa pag-ukit, hindi puwedeng burahin. Kailangang magsimula muli."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Parang hindi madaling matutunan…"
            },
            new DialogueLine
            {
                characterName = "ENKI",
                line = " Ngunit tila ikaw ay may malasakit sa aming sining. Sige nga, kung naalala mo , anong tawag sa uri ng pagsusulat na ito?"
            },
        };

        ShowDialogue();
        nextButton.onClick.AddListener(ShowNextDialogue);
        backButton.onClick.AddListener(ShowPreviousDialogue);
    }

    void ShowDialogue()
    {
        EnkicharacterRenderer.enabled = false;
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
                ChronocharacterRenderer.sprite = EnkiPokerface;
                PlayercharacterRenderer.sprite = PlayerSmile;
                break;
            case 1:
                ChronocharacterRenderer.sprite = EnkiStern;
                PlayercharacterRenderer.sprite = PlayerCurious;
                break;
            case 3:
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 4:
                ChronocharacterRenderer.sprite = EnkiPokerface;
                break;
            case 5:
                PlayercharacterRenderer.sprite = PlayerSmile;
                break;
            case 6:
                ChronocharacterRenderer.sprite = EnkiStern;
                break;
            case 7:
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 8:
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 9:
                ChronocharacterRenderer.sprite = EnkiTesting;
                break;
        }
    }


    void ShowNextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("SumerianFirstRecallChallenges");
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


