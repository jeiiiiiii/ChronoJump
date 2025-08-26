using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class AssyrianScene3 : MonoBehaviour
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
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    public Sprite ChronoSad;
    public Sprite ChronoSmile;
    public Sprite AshurbanipalStern;
    void Start()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Kilala rin ang Assyria sa pag-unlad ng imprastruktura. Naglatag sila ng maayos na kalsada upang mapabilis ang komunikasyon at kontrolin ang kanilang malawak na teritoryo."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Mayroon ding serbisyong postal na ipinapadala sa mga malalayong lungsod."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Kahit sa kalupitan, may kaayusan pala..."
            },
            new DialogueLine
            {
                characterName = "ASHURBANIPAL",
                line = " Ang galit ay nararapat lamang kung ito'y ginamit upang protektahan ang kaayusan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Paano naman ang mga taong pinahirapan, ang mga lungsod na sinunog?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ang Assyria ay simbulo ng kapangyarihan at babala ng kung ano ang mangyayari kapag inabuso ito."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ngunit ang bawat imperyong itinayo sa takot… may katapusan."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " May parating?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Oo. Ang mga galit, ang mga sugatan, at ang mga nakaligtas. Magkakaisa sila… upang durugin ang haliging ito."
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
            case 2:
                PlayercharacterRenderer.sprite = PlayerReflective;
                break;
            case 3:
                ChronocharacterRenderer.sprite = AshurbanipalStern;
                break;
            case 4:
                PlayercharacterRenderer.sprite = PlayerCurious;
                break;
            case 5:
                ChronocharacterRenderer.sprite = ChronoSad;
                break;
            case 6:
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 7:
                PlayercharacterRenderer.sprite = PlayerCurious;
                break;
            case 8:
                ChronocharacterRenderer.sprite = ChronoSad;
                break;
        }

    }
    void ShowNextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("AssyrianSceneFour");
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


