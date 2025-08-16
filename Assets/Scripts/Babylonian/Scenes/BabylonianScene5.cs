using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class BabylonianScene5 : MonoBehaviour
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
    public Sprite PlayerReflective;
    public Sprite PlayerEager;
    public Sprite PlayerSmile;
    public Sprite PlayerCurious;
    public Sprite PlayerEmbarrased;
    public Sprite HammurabiReverent;
    public Sprite HammurabiExplaining;
    public Sprite HammurabiWise;
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    public Sprite ChronoSmile;
    public Sprite ChronoCheerful;
    public Sprite ChronoSad;
    void Start()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Chrono... sino kaya itong nililok nila? Parang sobrang mahalaga sa kanila."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Isa siya sa pinakakilalang nilalang ng mga taga-Babylonia. Dito nakasentro ang kanilang pananampalataya."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " May mga nag-aalay pa ng pagkain… para ba ito sa isang tao? O sa isang espiritu?"
            },
            new DialogueLine
            {
                characterName = "HAMMURABI",
                line = " Hindi siya karaniwang nilalang. Siya si Marduk — ang diyos na nagbigay ng lakas at direksyon sa aming pamahalaan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Diyos?! Siya ang sentro ng paniniwala ninyo?"
            },
            new DialogueLine
            {
                characterName = "HAMMURABI",
                line = " Oo. Sa panahon ng pagsasama-sama ng mga lungsod sa ilalim ng Babylonia, kailangan naming kilalanin ang isang diyos na magbubuklod sa mga tao."
            },
            new DialogueLine
            {
                characterName = "HAMMURABI",
                line = " Pinili naming itaas si Marduk bilang pangunahing diyos. Siya ang sagisag ng kaayusan at tagumpay." // 5
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Sa mga sinaunang kwento, tinalo ni Marduk ang mga puwersa ng kaguluhan at nilikha ang mundo mula sa kaguluhan. Isa sa mga akdang isinulat noon ay Enuma Elish, na naglalarawan ng kanyang kapangyarihan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Ah, kaya pala parang may respeto at takot ang mga tao sa paligid… Hindi lang ito estatwa. Para sa kanila, ito ang kinatawan ng lakas na higit pa sa tao."
            },
            new DialogueLine
            {
                characterName = "HAMMURABI",
                line = " Walang pamahalaan kung walang gabay. At walang gabay kung walang pananampalataya. Sa tulong ni Marduk, naitaguyod ko ang pagkakaisa sa buong Babylonia."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ang pananampalataya nila kay Marduk ay naging isang mahalagang bahagi ng kultura at pagkakakilanlan ng imperyo. Hindi lang ito tungkol sa paniniwala — ito rin ay patunay ng pagkakabuklod ng mga tao."
            },
            new DialogueLine
            {
                characterName = "PLAYER ",
                line = " Ang galing… iba talaga kapag may paniniwalang pinanghahawakan. Parang mas nagiging buo ang pagkatao ng isang bayan."
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
                PlayercharacterRenderer.sprite = PlayerCurious;
                break;
            case 1:
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 2:
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 3:
                ChronocharacterRenderer.sprite = HammurabiReverent;
                break;
            case 4:
                PlayercharacterRenderer.sprite = PlayerEmbarrased;
                break;
            case 5:
                ChronocharacterRenderer.sprite = HammurabiExplaining;
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 6:
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 7:
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 8:
                ChronocharacterRenderer.sprite = HammurabiWise;
                break;
            case 9:
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 10:
                PlayercharacterRenderer.sprite = PlayerSmile;
                break;
        }

    }
    void ShowNextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("BabylonianFourthRecallChallenges");
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


