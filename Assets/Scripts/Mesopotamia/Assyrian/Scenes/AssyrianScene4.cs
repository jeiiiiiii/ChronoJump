using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class AssyrianScene4 : MonoBehaviour
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
    public Sprite PlayerSmile;
    public Sprite PlayerEmbarrassed;
    public Sprite AshurbanipalSomber;
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    public Sprite ChronoSad;
    void Start()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ngunit hindi lahat ng imperyo ay walang hanggan. Noong 612 BCE, ang mga dating kaaway ang Chaldean, Medes, at Persian ay nagkaisa upang gibain ang Assyria."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ang pag-aalsa ay pinangunahan ng mga Chaldean na dating sakop ng imperyo, at ngayo’y naghihiganti para sa mga dekadang pang-aabuso."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Chaldean... Medes... Persian. Mga karaniwang pinaghaharian, ngayo'y magkakampi."
            },
            new DialogueLine
            {
                characterName = "ASHURBANIPAL",
                line = " Kakaibang ironya. Sa paghahangad kong ipunin ang kaalaman, hindi ko nailigtas ang Nineveh sa apoy."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Gumuho ang siyudad. Sunog, dugo, at pagbagsak ng isang dating makapangyarihan. Ang takot na ginamit upang maghari ay binalik ng galit na hindi na mapipigil."
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
                PlayercharacterRenderer.sprite = PlayerSmile;
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 2:
                PlayercharacterRenderer.sprite = PlayerEmbarrassed;
                break;
            case 3:
                ChronocharacterRenderer.sprite = AshurbanipalSomber;
                break;
            case 4:
                ChronocharacterRenderer.sprite = ChronoSad;
                break;
        }
    }
    void ShowNextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("AssyrianThirdRecallChallenges");
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


