using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EgyptStoryNavigator : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject storyTextObject;    // Your TextMeshPro GameObject
    [SerializeField] private Button nextButton;            // Your right arrow button
    [SerializeField] private Button backButton;            // Your left arrow button (optional)
    
    [Header("Story Data")]
    [SerializeField] private StoryData[] stories;
    
    private TextMeshProUGUI storyText;
    private int currentIndex = 0;
    
    [System.Serializable]
    public class StoryData
    {
        public string storyTitle;
        [TextArea(8, 15)]
        public string storyContent;
    }
    
    void Start()
    {
        // Get TextMeshPro component from the assigned GameObject
        storyText = storyTextObject.GetComponent<TextMeshProUGUI>();
        
        // Validate component
        if (storyText == null)
        {
            Debug.LogError("TextMeshProUGUI not found on Story Text GameObject!");
            return;
        }
        
        // Setup button click events
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(ShowNext);
            Debug.Log("Next button connected successfully");
        }
        else
        {
            Debug.LogWarning("Next button is not assigned!");
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(ShowPrevious);
            Debug.Log("Back button connected successfully");
        }
        else
        {
            Debug.LogWarning("Back button is not assigned!");
        }
        
        // Validate story data
        if (stories.Length == 0)
        {
            Debug.LogWarning("No story data assigned!");
            return;
        }
        
        // Show first story
        ShowCurrentStory();
    }
    
    public void ShowNext()
    {
        Debug.Log("Next button clicked! Current index: " + currentIndex);
        currentIndex = (currentIndex + 1) % stories.Length;
        ShowCurrentStory();
    }
    
    public void ShowPrevious()
    {
        Debug.Log("Previous button clicked! Current index: " + currentIndex);

        currentIndex = (currentIndex - 1 + stories.Length) % stories.Length;
        ShowCurrentStory();
    }
    
    public void ShowStory(int index)
    {
        if (index >= 0 && index < stories.Length)
        {
            currentIndex = index;
            ShowCurrentStory();
        }
    }
    
    private void ShowCurrentStory()
    {
        // Check if we have valid data
        if (stories.Length == 0)
        {
            Debug.LogWarning("No stories assigned!");
            return;
        }
        
        StoryData currentStory = stories[currentIndex];
        
        // Update story text - you can format it however you want
        if (storyText != null)
        {
            // Option 1: Show title and content together
            storyText.text = $"<size=120%><b>{currentStory.storyTitle}</b></size>\n\n{currentStory.storyContent}";
            
        }
        
        Debug.Log($"Showing story: {currentStory.storyTitle} ({currentIndex + 1}/{stories.Length})");
    }
    
    public int CurrentIndex => currentIndex;
    public int TotalStories => stories.Length;
    public string CurrentStoryTitle => stories.Length > 0 ? stories[currentIndex].storyTitle : "None";
    
    public void JumpToFirstStory()
    {
        currentIndex = 0;
        ShowCurrentStory();
    }
    
    public void JumpToLastStory()
    {
        currentIndex = stories.Length - 1;
        ShowCurrentStory();
    }
    
    public bool IsFirstStory()
    {
        return currentIndex == 0;
    }
    
    public bool IsLastStory()
    {
        return currentIndex == stories.Length - 1;
    }
}