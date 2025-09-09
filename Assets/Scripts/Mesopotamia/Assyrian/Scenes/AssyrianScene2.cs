using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class AssyrianScene2 : MonoBehaviour
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
    public GameObject AchievementUnlockedRenderer;
    public Sprite PlayerCurious;
    public Sprite PlayerSmile;
    public Sprite PlayerEmabrrassed;
    public Sprite PlayerReflective;
    public Sprite SargonNuetral;
    public SpriteRenderer AshurbanipalFulldrawnSprite;
    public Sprite AshurbanipalWise;
    public SpriteRenderer ChronocharacterRenderer;
    public Sprite ChronoThinking;
    public Sprite ChronoSmile;
    public Sprite ChronoCheerful;
    void Start()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ngunit hindi puro dahas ang sumunod na hari. Si Ashurbanipal, namuno noong 668 hanggang 627 BCE, ay kilala bilang hari ng kaalaman."
            },
            new DialogueLine
            {
                characterName = "ASHURBANIPAL",
                line = " Ako si Ashurbanipal. Hari ng mundo at tagapangalaga ng karunungan. Ang silid-aklatang ito sa Nineveh ay tahanan ng mahigit 200,000 tabletang luwad mga kasaysayan, batas, medisina, at panalangin."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Ikaw ang hari... pero hindi ka mukhang mandirigma."
            },
            new DialogueLine
            {
                characterName = "ASHURBANIPAL",
                line = " Ako'y parehong espada at stylus. Hindi lang pananakop, kundi pangangalaga ng kultura ang pamana ko."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Ang aklatan ni Ashurbanipal ang kauna-unahang silid-aklatan sa kasaysayan ng mundo. Isa ito sa mga dahilan kung bakit kahit sa kalupitan ng Assyria, may natirang anyo ng sibilisasyon."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Ito pala ang pundasyon ng mga aklatan ngayon..."
            },
        };

        ShowDialogue();
        nextButton.onClick.AddListener(ShowNextDialogue);
        backButton.onClick.AddListener(ShowPreviousDialogue);
    }

    void ShowDialogue()
    {
        AchievementUnlockedRenderer.SetActive(false);
        AshurbanipalFulldrawnSprite.enabled = false;
        DialogueLine line = dialogueLines[currentDialogueIndex];
        dialogueText.text = $"<b>{line.characterName}</b>: {line.line}";

        switch (currentDialogueIndex)
        {
            case 0:
                PlayercharacterRenderer.sprite = PlayerSmile;
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;
            case 1:
                AshurbanipalFulldrawnSprite.enabled = true;
                PlayercharacterRenderer.sprite = PlayerEmabrrassed;
                ChronocharacterRenderer.sprite = ChronoCheerful;
                break;
            case 2:
                PlayercharacterRenderer.sprite = PlayerCurious;
                break;
            case 3:
                ChronocharacterRenderer.sprite = AshurbanipalWise;
                break;
            case 4:
                AchievementUnlockedRenderer.SetActive(true);
                PlayerAchievementManager.UnlockAchievement("Guardian");
                ChronocharacterRenderer.sprite = ChronoThinking;
                break;
            case 5:
                PlayercharacterRenderer.sprite = PlayerReflective;
                
                break;
        }

    }
    void ShowNextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("AssyrianSecondRecallChallenges");
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


