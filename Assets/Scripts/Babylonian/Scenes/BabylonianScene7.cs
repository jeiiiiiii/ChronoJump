using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class BabylonianScene7 : MonoBehaviour
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
                line = " Napakalayo ng pinanggalingan ng lahat ng ito… mula sa malalaking imperyo, mga hari, at mga kwento ng panitikan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Hindi lang ito tungkol sa kapangyarihan o digmaan... kundi pati ang mga aral at alaala na iniwan nila."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Handa ka na ba sa susunod na paglalakbay? Marami pang kwento ang naghihintay na matuklasan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Oo, handa na ako."
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
            SceneManager.LoadScene("BabylonianQuizTime");
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


