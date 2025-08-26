using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class AkkadianScene3 : MonoBehaviour
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
    public Sprite SargonWise;
    public Sprite SargonExplaining;
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    void Start()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "SARGON I",
                line = " Hindi lang lakas ang sandata ng isang imperyo. Kailangan din ng sistemang gumagalaw kahit ako’y wala."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Ibig sabihin… may opisyal, sundalo, tagapamahala?"
            },
            new DialogueLine
            {
                characterName = "SARGON I",
                line = " Tama. Nagtalaga ako ng mga gobernador sa bawat bahagi ng imperyo. Lahat ay may tungkulin. Lahat may kaayusan."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Dahil sa pamahalaang ito, nakontrol ni Sargon ang buong rehiyon mula hilaga hanggang katimugan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Pero anong nangyari pagkatapos ni Sargon?"
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
                ChronocharacterRenderer.sprite = SargonWise;
                PlayercharacterRenderer.sprite = PlayerSmile;
                break;
            case 1:
                PlayercharacterRenderer.sprite = PlayerCurious;
                break;
            case 2:
                ChronocharacterRenderer.sprite = SargonExplaining;
                break;
            case 3:
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 4:
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
        }

    }
    void ShowNextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("AkkadianSceneFour");
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


