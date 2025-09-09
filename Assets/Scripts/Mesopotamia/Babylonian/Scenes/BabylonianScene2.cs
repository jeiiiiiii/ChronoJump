using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class BabylonianScene2 : MonoBehaviour
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
    public Sprite PlayerEager;
    public Sprite PlayerReflective;
    public Sprite HammurabiExplaning;
    public Sprite HammurabiInformative;
    public Sprite HammurabiFirm;
    public Sprite HammurabiWise;
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    public Sprite ChronoSmile;
    void Start()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Chrono, ano ito? Bakit may mga sulat sa bato?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Iyan ang Kodigo ni Hammurabi. Isa ito sa pinakamatandang koleksyon ng mga batas sa kasaysayan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Kodigo ni Hammurabi? Siya ba ang gumawa ng lahat ng ‘yan?."
            },
            new DialogueLine
            {
                characterName = "HAMMURABI",
                line = " Oo, ako ang nagpagawa ng Kodigo. Mayroong 282 na batas na isinulat sa batong iyan. Ang layunin nito ay mapanatili ang kaayusan sa Babylonia."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Ano po’ng klaseng mga batas ang nakasulat d’yan?"
            },
            new DialogueLine
            {
                characterName = "HAMMURABI",
                line = " May mga batas tungkol sa pag-aari, negosyo, pamilya, serbisyong propesyonal, at parusa para sa masama ang ginawa."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Sa madaling salita, hindi lang ito para sa mga mayayaman o makapangyarihan. Ginawa ito para mapangalagaan ang karapatan ng lahat."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Paano po ang parusa kung may nagkamali?"
            },
            new DialogueLine
            {
                characterName = "HAMMURABI",
                line = " Kung sinaktan mo ang iba, may kapalit na parusa. Tinatawag itong prinsipyo ng mata sa mata, ngipin sa ngipin o Lex Talionis . Ibig sabihin, ang ginawa mo sa iba ay maaaring mangyari rin sa iyo."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Mahalaga ito noong panahon nila para matakot ang tao sa paggawa ng masama. Pero mahalaga rin na may sistema para sa hustisya."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Grabe… ang dami palang saklaw ng mga batas noon. Akala ko noon lang nagkaroon ng legal system."
            },
            new DialogueLine
            {
                characterName = "HAMMURABI",
                line = " Ang isang lipunan ay hindi lalago kung walang batas. Ang Kodigo na ito ay ginawa hindi lang para magparusa, kundi para ituro kung ano ang tama."
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
                PlayercharacterRenderer.sprite = PlayerCurious;
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 1:
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 2:
                PlayercharacterRenderer.sprite = PlayerEager;
                break;
            case 3:
                ChronocharacterRenderer.sprite = HammurabiExplaning;
                break;
            case 4:
                PlayercharacterRenderer.sprite = PlayerCurious;
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 5:
                ChronocharacterRenderer.sprite = HammurabiExplaning;
                break;
            case 6:
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 7:
                PlayercharacterRenderer.sprite = PlayerEager;
                break;
            case 8:
                ChronocharacterRenderer.sprite = HammurabiFirm;
                break;
            case 9:
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 10:
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 11:
                ChronocharacterRenderer.sprite = HammurabiWise;
                break;
        }

    }
    void ShowNextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("BabylonianSecondRecallChallenges");
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


