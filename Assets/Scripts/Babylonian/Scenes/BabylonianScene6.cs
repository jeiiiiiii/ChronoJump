using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class BabylonianScene6 : MonoBehaviour
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
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoSad;
    public Sprite ChronoThinking;
    public Sprite ChronoSmile;

    void Start()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Chrono… parang may hindi magandang nangyayari. Parang hindi na ganito kasigla kanina."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Tama ang kutob mo. Pagkatapos mamatay ni Hammurabi, unti-unting humina ang pamahalaan ng Babylonia."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Bakit? Hindi ba’t maayos ang pagkakatatag ng imperyo?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Oo, pero kahit matatag ang pamahalaan noong nabubuhay si Hammurabi, mahirap itong mapanatili kung wala na ang matalinong namumuno."
            },
            new DialogueLine
            {
                characterName = "CHRONO ",
                line = " Pagkatapos ng kanyang pamumuno, sinalakay ng mga Hittite ang Babylonia. Isa silang makapangyarihang grupo mula sa Anatolia. Dahil sa huminang pamahalaan, naging madali para sa kanila ang pananakop."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Ang galing ni Hammurabi pero kahit ganoon, hindi pa rin iyon sapat para manatili ang imperyo…"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Walang imperyo ang panghabambuhay. Pero ang iniwang pamana gaya ng Kodigo ni Hammurabi ay hindi mawawala sa kasaysayan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Mukhang oras na para bumalik tayo."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Oo. Marami pa tayong matututunan sa susunod na paglalakbay."
            }
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
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 1:
                ChronocharacterRenderer.sprite = ChronoSad;
                break;
            case 2:
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 3:
                ChronocharacterRenderer.sprite = ChronoThinking;
                PlayercharacterRenderer.sprite = PlayerEager;
                break;
            case 4:
                ChronocharacterRenderer.sprite = ChronoSad;
                break;
            case 5:
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 6:
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 7:
                PlayercharacterRenderer.sprite = PlayerEager;
                break;
            case 8:
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
        }
    }

    void ShowNextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("BabylonianSceneSeven");
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


