using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class BabylonianScene4 : MonoBehaviour
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
    public Sprite PlayerEager;
    public Sprite HammurabiProud;
    public Sprite HammurabiExplaining;
    public Sprite HammurabiWise;
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    public Sprite ChronoCheerful;
    public Sprite ChronoSmile;
    void Start()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Chrono… ano itong sinusulat nila sa mga luwad na ito?"
            },
            new DialogueLine
            {
                characterName = "CHRONO ",
                line = " Iyan ang panitikan ng mga taga-Babylonia. Sa panahon ni Hammurabi, umunlad hindi lang ang batas kundi pati sining at panulat."
            },
            new DialogueLine
            {
                characterName = "HAMMURABI",
                line = " Ang mga salitang ito ang magdadala ng aming kwento sa susunod na henerasyon. Isa sa mga pinakaimportanteng isinulat ay ang Epikong Gilgamesh."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Epiko? Parang kwento tungkol sa bayani?"
            },
            new DialogueLine // need to shorten
            {
                characterName = "HAMMURABI",
                line = " Si Gilgamesh ay isang maalamat na hari na naglakbay para hanapin ang lihim ng walang kamatayan. Sa pamamagitan ng kanyang kwento, natututo ang aming bayan tungkol sa tapang, pagkakaibigan, at pagtanggap sa katotohanan ng buhay."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " At hindi lang ‘yan. Isinulat din nila ang Enuma Elish, isang kuwento tungkol sa paglikha ng mundo at kapangyarihan ng diyos na si Marduk."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Grabe… akala ko digmaan lang ang mahalaga. Pero dito, pati mga kwento, pinahahalagahan."
            },
            new DialogueLine
            {
                characterName = "HAMMURABI",
                line = " Ang isang imperyo ay hindi lamang nasusukat sa laki ng nasasakupan, kundi sa lalim ng alaala at karunungang naiiwan."
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
                ChronocharacterRenderer.sprite = ChronoCheerful;
                break;
            case 2:
                PlayercharacterRenderer.sprite = PlayerSmile;
                ChronocharacterRenderer.sprite = HammurabiProud;
                break;
            case 3:
                PlayercharacterRenderer.sprite = PlayerEager;
                break;
            case 4:
                ChronocharacterRenderer.sprite = HammurabiExplaining;
                break;
            case 5:
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 6:
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 7:
                ChronocharacterRenderer.sprite = HammurabiWise;
                break;
        }

    }
    void ShowNextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("BabylonianThirdRecallChallenges");
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


