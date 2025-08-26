using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;


public class AkkadianScene1 : MonoBehaviour
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
    public SpriteRenderer PlayerFulldrawnSprite;
    public SpriteRenderer ChronoFulldrawnSprite;

    public GameObject AchievementUnlockedRenderer;
    public Sprite PlayerFulldrawn;
    public Sprite PlayerCurious;
    public Sprite PlayerEager;
    public Sprite PlayerSmile;

    public SpriteRenderer SargoncharacterRenderer;
    public Sprite SargonCommand;
    
    public SpriteRenderer ChronocharacterRenderer;

    public Sprite ChronoCheerful;
    public Sprite ChronoSmile;

    void Start()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {   characterName = "PLAYER",
                line = " Sandali… iba ‘to. Parang mas... masigla pero mas istrikto."
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Tama ang pakiramdam mo. Nandito tayo sa lungsod ng Akkad, hilagang bahagi ng Mesopotamia. Isang lungsod na hindi lang ordinaryo. Ito ang puso ng kauna-unahang imperyo sa daigdig"
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Imperyo? Ibig sabihin… maraming bayan ang pinagsama?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " At ang gumawa nito ay si Sargon. Isang dakilang pinuno mula sa hindi maharlikang lahi pero binago ang kasaysayan."
            },
            new DialogueLine
            {
                characterName = "SARGON I",
                line = " Ako si Sargon ng Akkad. Hindi ako ipinanganak na hari pero pinili akong manguna."
            },
            new DialogueLine
            {
                characterName = "PLAYER",
                line = " Siya ba talaga si…?"
            },
            new DialogueLine
            {
                characterName = "CHRONO",
                line = " Oo. Si Sargon I. Pinuno ng Akkadian Empire. Magmula sa simpleng pinuno ng hukbo, naging tagapagtatag ng imperyo."
            },
            new DialogueLine
            {
                characterName = "SARGON I",
                line = " Halina, samahan ninyo ako. Ipakikita ko kung paano nagsimula ang lahat."
            },
        };

        ShowDialogue();
        nextButton.onClick.AddListener(ShowNextDialogue);
        backButton.onClick.AddListener(ShowPreviousDialogue);
    }

    void ShowDialogue()
    {
        AchievementUnlockedRenderer.SetActive(false);
        SargoncharacterRenderer.enabled = false;
        PlayerFulldrawnSprite.enabled = false;
        ChronoFulldrawnSprite.enabled = false;
        DialogueLine line = dialogueLines[currentDialogueIndex];
        dialogueText.text = $"<b>{line.characterName}</b>: {line.line}";

        switch (currentDialogueIndex)
        {
            case 0:
                PlayerFulldrawnSprite.enabled = true;
                PlayercharacterRenderer.enabled = false;
                ChronocharacterRenderer.enabled = false;
                break;
            case 1:
                ChronoFulldrawnSprite.enabled = true;
                PlayercharacterRenderer.enabled = true;
                PlayercharacterRenderer.sprite = PlayerSmile;
                break;
            case 2:
                PlayercharacterRenderer.sprite = PlayerCurious;
                ChronocharacterRenderer.enabled = true;
                break;
            case 3:
                ChronocharacterRenderer.sprite = ChronoCheerful;
                break;

            case 4:
                PlayerAchievementManager.UnlockAchievement("Rise");
                SargoncharacterRenderer.enabled = true;
                AchievementUnlockedRenderer.SetActive(true);
                break;

            case 5:
                ChronocharacterRenderer.sprite = ChronoCheerful;
                PlayercharacterRenderer.sprite = PlayerEager;
                AchievementUnlockedRenderer.SetActive(false);
                break;

            case 6:
                ChronocharacterRenderer.sprite = ChronoSmile;
                break;

            case 7:
                ChronocharacterRenderer.sprite = SargonCommand;
                break;
        }

    }
    void ShowNextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene("AkkadianSceneTwo");
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


