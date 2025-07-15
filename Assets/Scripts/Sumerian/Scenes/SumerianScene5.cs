using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class SumerianScene5 : MonoBehaviour
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
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoSmile;
    public Sprite ChronoThinking;
    public Sprite PatesiFormal;
    public Sprite PatesiDisapproval;

    public SpriteRenderer IshmacharacterRenderer;

    public Sprite IshmaAmazed;
    public Sprite IshmaExplaining;
    public Sprite IshmaSmirking;


    void Start()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "ISHMA",
                line = " Aba, Patesi! Magandang araw! Sino ang mga kasama mo?"
            },
            new DialogueLine
            {
                characterName = "PATESI NINURTA",
                line = " Isang tagalabas at ang kaniyang gabay. Ipinapasyal namin sila upang maunawaan ang puso ng aming kabihasnan."
            },
            new DialogueLine
            {
                characterName = "ISHMA",
                line = " Ako si Ishma, isa sa mga mangangalakal dito. Halina't tumingin-tingin sa paligid. Makikita ninyong kami'y hindi lamang marunong magtanim, kundi pati rin gumawa ng mga bagay na nagpapagaan ng aming pamumuhay."
            },
            new DialogueLine
            {
                characterName = "ISHMA",
                line = " Tulad nitong sasakyang ito. Noon, bitbit namin sa balikat ang mga paninda. Pero nang matutunan naming gamitin ang gulong, napabilis ang lahat , mula sakahan, hanggang sa pagdadala ng produkto sa ilog."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Napakatalino naman ng naka-isip nito."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ang gulong ay isa sa mga pinakakilalang imbensyon ng mga Sumerian. Hindi lamang ito ginagamit sa mga karwahe. Sa paglipas ng panahon, ginamit din ito sa pag-igib ng tubig, pagpapaandar ng mga kasangkapan, at iba pa. Isipin mo , ang simpleng bilog, nagbukas ng daan sa napakaraming posibilidad."
            },
            new DialogueLine
            {
                characterName = "ISHMA",
                line = " Hindi lahat ng kailangan namin ay makukuha dito sa aming lungsod. Kaya naman natutunan naming makipagpalitan ng produkto , butil para sa kahoy, tela kapalit ng metal. Walang salapi. Lahat ay batay sa pangangailangan."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Tawag diyan ay barter. Noon pa man, marunong na ang mga Sumerian makipagkalakalan , hindi lang sa loob ng lungsod kundi pati sa ibang lungsod-estado. Sa pamamagitan nito, nakarating sa kanila ang mga bagong kagamitan, ideya, at karunungan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Kaya pala ang daming tao dito... at parang ang daming produkto mula sa iba’t ibang lugar."
            },
            new DialogueLine
            {
                characterName = "ISHMA",
                line = " Ang palengke ang puso ng aming kabuhayan. Dito mo makikita ang liksi ng talino at tiyaga ng aming mga mamamayan."
            },
            new DialogueLine
            {
                characterName = "PATESI NINURTA",
                line = " Ngayong nakita mo ang aming mga kasangkapan at kalakaran, masasabi mo ba kung alin sa mga ito ang likha ng aming kabihasnan upang mapadali ang paglalakbay sa lupa?"
            },
        };

        ShowDialogue();
        nextButton.onClick.AddListener(ShowNextDialogue);
        backButton.onClick.AddListener(ShowPreviousDialogue);
    }

    void ShowDialogue()
    {
        IshmacharacterRenderer.enabled = false;
        DialogueLine line = dialogueLines[currentDialogueIndex];
        dialogueText.text = $"<b>{line.characterName}</b>: {line.line}";

        switch (currentDialogueIndex)
        {
            case 0:
                IshmacharacterRenderer.enabled = true;
                ChronocharacterRenderer.sprite = PatesiFormal;
                break;
            case 1:
                IshmacharacterRenderer.enabled = false;
                ChronocharacterRenderer.sprite = PatesiFormal;
                break;
            case 2:
                PlayercharacterRenderer.sprite = IshmaExplaining;
                break;
            case 4:
                PlayercharacterRenderer.sprite = PlayerEager;
                break;
            case 5:
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 6:
                ChronocharacterRenderer.sprite = IshmaSmirking;
                break;
            case 7:
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 8:
                PlayercharacterRenderer.sprite = PlayerCurious;
                break;
            case 9:
                ChronocharacterRenderer.sprite = IshmaAmazed;
                break;
            case 10:
                ChronocharacterRenderer.sprite = PatesiFormal;
                break;
        }
    }


    void ShowNextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("SumerianFourthRecallChallenges");
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


