using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class AkkadianScene4 : MonoBehaviour
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
    public Sprite SargonNuetral;

    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    public Sprite ChronoSad;
    public Sprite ChronoSmile;

    void Start()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Pagkaraan ng maraming taon, sumunod sa kanya ang apo niyang si Naram-Sin. Mahusay din, ngunit sa kanyang pamumuno, humina ang imperyo."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Bakit siya humina? Dahil sa digmaan?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Bumagsak ang imperyo matapos salakayin ng mga Amorite at Hurrian ang Mesopotamia. Sinamantala nila ang paghina ng pamahalaan ng Akkad."
            },
            new DialogueLine
            {
                characterName = "SARGON I",
                line = " Isang imperyo, gaano man kalawak, ay maaaring bumagsak kapag hindi ito naipagtanggol nang maayos."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Pagkatapos ng pagbagsak, panandaliang nabawi ng lungsod ng Ur ang kapangyarihan sa ilalim ng Ikatlong Dinastiya."
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

        PlayercharacterRenderer.sprite = null;
        ChronocharacterRenderer.sprite = null;

        switch (currentDialogueIndex)
        {
            case 0:
                ChronocharacterRenderer.sprite = ChronoSmile;
                PlayercharacterRenderer.sprite = PlayerSmile;
                break;

            case 1:
                ChronocharacterRenderer.sprite = ChronoSmile;
                PlayercharacterRenderer.sprite = PlayerReflective; 
                break;
            case 2:
                ChronocharacterRenderer.sprite = ChronoSad;
                PlayercharacterRenderer.sprite = PlayerCurious;
                break;

            case 3:
                ChronocharacterRenderer.sprite = SargonNuetral;
                PlayercharacterRenderer.sprite = PlayerCurious;
                break;

            case 4:
                ChronocharacterRenderer.sprite = ChronoSmile;
                PlayercharacterRenderer.sprite = PlayerCurious;
                break;
        }
    }

    void ShowNextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("AkkadianSecondRecallChallenges");
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
