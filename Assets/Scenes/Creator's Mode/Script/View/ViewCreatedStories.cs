using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ViewCreatedStoriesScene : MonoBehaviour
{
    [System.Serializable]
    public class StorySlot
    {
        public RawImage backgroundImage; // For grid/slots
    }

    [SerializeField] private StorySlot[] storySlots = new StorySlot[6];
    [SerializeField] private GameObject storyActionPopup;
    [SerializeField] private Image currentBackgroundImage;
    
    // Button images for each story slot (buttonStory1.png to buttonStory6.png)
    [SerializeField] private Texture2D[] storyButtonImages = new Texture2D[6];
    
    // Default background (div (22).png)
    [SerializeField] private Texture2D defaultButtonBackground;

    void Start()
    {
        UpdateAllStoryBackgrounds();

        if (storyActionPopup != null)
        {
            storyActionPopup.SetActive(false);
        }
    }

    void UpdateAllStoryBackgrounds()
    {
        for (int i = 0; i < storySlots.Length; i++)
        {
            UpdateStorySlot(i);
        }
    }

    void UpdateStorySlot(int index)
    {
        if (index < 0 || index >= storySlots.Length) return;

        var slot = storySlots[index];
        Texture2D backgroundToUse = null;
        
        // Check if a story exists at this index
        bool storyExists = StoryManager.Instance != null && 
                          StoryManager.Instance.allStories != null && 
                          index < StoryManager.Instance.allStories.Count &&
                          StoryManager.Instance.allStories[index] != null;

        if (storyExists)
        {
            var story = StoryManager.Instance.allStories[index];
            
            // Priority 1: Check if story has a custom uploaded background
            if (!string.IsNullOrEmpty(story.backgroundPath) && System.IO.File.Exists(story.backgroundPath))
            {
                backgroundToUse = ImageStorage.LoadImage(story.backgroundPath);
                Debug.Log($"[Slot {index}] Using custom uploaded background");
            }
            // Priority 2: Use indexed button image (buttonStory1-6)
            else if (storyButtonImages != null && index < storyButtonImages.Length && storyButtonImages[index] != null)
            {
                backgroundToUse = storyButtonImages[index];
                Debug.Log($"[Slot {index}] Using indexed button image: buttonStory{index + 1}");
            }
            // Priority 3: Fallback to default background
            else
            {
                backgroundToUse = defaultButtonBackground;
                Debug.Log($"[Slot {index}] Using default background (fallback)");
            }
        }
        else
        {
            // No story exists - always use default background
            backgroundToUse = defaultButtonBackground;
            Debug.Log($"[Slot {index}] Empty slot - using default background");
        }
        
        // Apply the selected background
        if (backgroundToUse != null)
        {
            slot.backgroundImage.texture = backgroundToUse;
            slot.backgroundImage.gameObject.SetActive(true);

            // Update aspect ratio
            AspectRatioFitter fitter = slot.backgroundImage.GetComponent<AspectRatioFitter>();
            if (fitter != null)
            {
                fitter.aspectRatio = (float)backgroundToUse.width / backgroundToUse.height;
            }
        }
        else
        {
            // If no background is available at all, hide the slot
            slot.backgroundImage.gameObject.SetActive(false);
            Debug.LogWarning($"[Slot {index}] No background available - hiding slot");
        }
    }

    public void RefreshBackgrounds()
    {
        UpdateAllStoryBackgrounds();
    }

    void OnEnable()
    {
        UpdateAllStoryBackgrounds();
    }

    // Method to handle background deletion for a specific story
    public void DeleteStoryBackground(int storyIndex)
    {
        if (storyIndex >= 0 && storyIndex < StoryManager.Instance.allStories.Count)
        {
            var story = StoryManager.Instance.allStories[storyIndex];
            if (story != null)
            {
                // Clear the background path
                story.backgroundPath = string.Empty;
                
                // Save the changes
                StoryManager.Instance.SaveStories();
                
                // Update the visual - will revert to default background
                UpdateStorySlot(storyIndex);
                
                Debug.Log($"🗑️ Deleted custom background for story {storyIndex} - reverted to default");
            }
        }
    }

    // Method to check if a story has a custom background
    public bool HasCustomBackground(int storyIndex)
    {
        if (storyIndex >= 0 && storyIndex < StoryManager.Instance.allStories.Count)
        {
            var story = StoryManager.Instance.allStories[storyIndex];
            return story != null && !string.IsNullOrEmpty(story.backgroundPath) && System.IO.File.Exists(story.backgroundPath);
        }
        return false;
    }

    // Method to get the current background type for debugging
    public string GetBackgroundType(int storyIndex)
    {
        if (storyIndex >= 0 && storyIndex < StoryManager.Instance.allStories.Count)
        {
            var story = StoryManager.Instance.allStories[storyIndex];
            if (story == null) return "No Story (Default Background)";
            
            if (!string.IsNullOrEmpty(story.backgroundPath) && System.IO.File.Exists(story.backgroundPath))
                return "Custom Uploaded Background";
            else if (storyIndex < storyButtonImages.Length && storyButtonImages[storyIndex] != null)
                return $"Indexed Button Image (buttonStory{storyIndex + 1})";
            else
                return "Default Background (div 22)";
        }
        return "Empty Slot (Default Background)";
    }

    [ContextMenu("Debug All Slot Backgrounds")]
    public void DebugAllSlotBackgrounds()
    {
        Debug.Log("🔍 === BACKGROUND STATUS FOR ALL SLOTS ===");
        for (int i = 0; i < storySlots.Length; i++)
        {
            Debug.Log($"Slot {i}: {GetBackgroundType(i)}");
        }
        Debug.Log("🔍 === END DEBUG ===");
    }

    public void OnEditStory(int storyIndex)
    {
        if (storyIndex >= 0 && storyIndex < StoryManager.Instance.allStories.Count)
        {
            StoryManager.Instance.currentStory = StoryManager.Instance.allStories[storyIndex];
            SceneManager.LoadScene("Creator'sModeScene");
        }
    }

    public void OnViewStory(int storyIndex)
    {
        if (storyIndex >= 0 && storyIndex < StoryManager.Instance.allStories.Count)
        {
            StoryManager.Instance.currentStory = StoryManager.Instance.allStories[storyIndex];
            SceneManager.LoadScene("GameScene");
        }
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Creator'sModeScene");
    }
}