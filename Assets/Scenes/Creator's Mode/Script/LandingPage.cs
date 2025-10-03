using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LandingPage : MonoBehaviour
{
    [Header("Button References")]
    public Button firstButton;
    public Button secondButton;
    
    [Header("Image to Move")]
    public RectTransform movingImage;
    
    [Header("Positions")]
    private Vector2 firstButtonPosition = new Vector2(-504, -78);
    private Vector2 secondButtonPosition = new Vector2(439, -78);

    void Start()
    {
        // Set initial position to first button
        if (movingImage != null)
        {
            movingImage.anchoredPosition = firstButtonPosition;
        }
        
        // Add hover listeners to buttons
        AddHoverListener(firstButton, firstButtonPosition);
        AddHoverListener(secondButton, secondButtonPosition);
    }

    void AddHoverListener(Button button, Vector2 position)
    {
        if (button == null) return;
        
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        // Create pointer enter event
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => { OnButtonHover(position); });
        trigger.triggers.Add(entry);
    }

    void OnButtonHover(Vector2 position)
    {
        if (movingImage != null)
        {
            movingImage.anchoredPosition = position;
        }
    }

    public void CreateNew()
    {
        SceneManager.LoadScene("ViewCreatedStoriesScene");
    }
    
    public void CreatedStories()
    {
        SceneManager.LoadScene("ViewCreatedStoriesScene");
    }
    
    public void MainMenu()
    {
        SceneManager.LoadScene("TitleScreen");
    }
}