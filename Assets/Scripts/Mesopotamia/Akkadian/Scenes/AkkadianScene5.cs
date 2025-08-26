using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class AkkadianScene5 : MonoBehaviour
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
    public Sprite SargonNuetral;
    public Sprite SargonWise;
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    public Sprite ChronoSmile;
    void Start()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Matapos bumagsak ang Dinastiyang Ur dahil sa pagsalakay ng mga Amorite at Hurrian, hindi nagtagal ang katahimikan sa Mesopotamia"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Sa halip na pagkakaisa, nagtunggalian ang mga lungsod gaya ng Isin at Larsa para sa kapangyarihan sa timog."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Kaya pala parang paulit-ulit lang. Pagkatapos ng isang pamahalaan, babalik na naman sa tunggalian."
            },
            new DialogueLine
            {
                characterName = "SARGON I",
                line = " Walang pinunong perpekto. Ngunit ang aral ay ito: ang pamumuno ay hindi tungkol sa takot kundi sa layunin."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ang mga imperyo ay bumabagsak, pero ang alaala ng kanilang ambag tulad ng pagkakabuo ng unang imperyo ay nananatili."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Hindi ko akalaing si Sargon pala ang nagsimula ng lahat."
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
                ChronocharacterRenderer.sprite = ChronoThinking;
                PlayercharacterRenderer.sprite = PlayerSmile;
                break;
            case 1:
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 2:
                ChronocharacterRenderer.sprite = SargonWise;
                break;
            case 3:
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 4:
                PlayercharacterRenderer.sprite = PlayerEager;
                break;
        }

    }
    void ShowNextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("AkkadianThirdRecallChallenges");
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


