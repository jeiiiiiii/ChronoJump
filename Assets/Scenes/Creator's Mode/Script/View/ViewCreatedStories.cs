using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ViewCreatedStoriesScene : MonoBehaviour
{
    [System.Serializable]
    public class StorySlot
    {
        public RawImage backgroundImage; // Must be RawImage for Texture2D
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
        if (storyActionPopup != null)
        {
            storyActionPopup.SetActive(false);
        }

        // Wait for stories to load, then update backgrounds
        StartCoroutine(WaitForStoriesAndUpdate());
    }

    private System.Collections.IEnumerator WaitForStoriesAndUpdate()
    {
        // Wait a frame for StoryManager to initialize
        yield return null;

        // Wait for Firestore to load (max 10 seconds)
        float timeout = 10f;
        float elapsed = 0f;

        while (StoryManager.Instance == null || StoryManager.Instance.allStories == null)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
            if (elapsed > timeout)
            {
                Debug.LogError("⏰ Timeout waiting for StoryManager to initialize");
                yield break;
            }
        }

        // Wait for stories to actually be loaded
        yield return new WaitForSeconds(1f);

        Debug.Log($"[ViewCreatedStories] Updating backgrounds - Total stories: {StoryManager.Instance.allStories.Count}");

        // Debug what we actually loaded
        DebugFirestoreStories();

        UpdateAllStoryBackgrounds();
    }


    void OnDestroy()
    {
        // Unsubscribe from events if you add the event-based approach
        if (StoryManager.Instance != null)
        {
            // StoryManager.Instance.OnStoriesLoaded -= UpdateAllStoryBackgrounds;
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

    bool hasValidSavedStory = false;
    StoryData story = null;

    if (StoryManager.Instance != null && 
        StoryManager.Instance.allStories != null && 
        index < StoryManager.Instance.allStories.Count)
    {
        story = StoryManager.Instance.allStories[index];
        hasValidSavedStory = story != null && !string.IsNullOrEmpty(story.createdAt);
    }

    Debug.Log($"[Slot {index}] HasValidStory: {hasValidSavedStory}, Story: {story != null}");

    if (hasValidSavedStory && story != null)
    {
        // ✅ UPDATED: Use ImageStorage.ImageExists and ImageStorage.LoadImage with relative paths
        if (!string.IsNullOrEmpty(story.backgroundPath) && ImageStorage.ImageExists(story.backgroundPath))
        {
            backgroundToUse = ImageStorage.LoadImage(story.backgroundPath);
            Debug.Log($"[Slot {index}] Using custom background from relative path: {story.backgroundPath}");
        }
        // Priority 2: Indexed button
        else if (storyButtonImages != null && index < storyButtonImages.Length && storyButtonImages[index] != null)
        {
            backgroundToUse = storyButtonImages[index];
            Debug.Log($"[Slot {index}] Using indexed button: buttonStory{index + 1}");
        }
        // Priority 3: Default
        else
        {
            backgroundToUse = defaultButtonBackground;
            Debug.Log($"[Slot {index}] Using default background");
        }
    }
    else
    {
        // No valid saved story - ALWAYS use default background
        backgroundToUse = defaultButtonBackground;
        Debug.Log($"[Slot {index}] Empty slot - using default background");
    }

    // Apply background
    if (backgroundToUse != null)
    {
        slot.backgroundImage.texture = backgroundToUse;
        slot.backgroundImage.gameObject.SetActive(true);

        AspectRatioFitter fitter = slot.backgroundImage.GetComponent<AspectRatioFitter>();
        if (fitter != null)
        {
            fitter.aspectRatio = (float)backgroundToUse.width / backgroundToUse.height;
        }
    }
    else
    {
        slot.backgroundImage.gameObject.SetActive(false);
        Debug.LogWarning($"[Slot {index}] No background available");
    }
}

// Also update the HasCustomBackground method:
public bool HasCustomBackground(int storyIndex)
{
    if (storyIndex >= 0 && storyIndex < StoryManager.Instance.allStories.Count)
    {
        var story = StoryManager.Instance.allStories[storyIndex];
        return story != null && !string.IsNullOrEmpty(story.backgroundPath) && ImageStorage.ImageExists(story.backgroundPath);
    }
    return false;
}

// Update the GetBackgroundType method:
public string GetBackgroundType(int storyIndex)
{
    if (storyIndex >= 0 && storyIndex < StoryManager.Instance.allStories.Count)
    {
        var story = StoryManager.Instance.allStories[storyIndex];
        if (story == null) return "No Story (Default Background)";

        if (!string.IsNullOrEmpty(story.backgroundPath) && ImageStorage.ImageExists(story.backgroundPath))
            return $"Custom Background (Relative Path: {story.backgroundPath})";
        else if (storyIndex < storyButtonImages.Length && storyButtonImages[storyIndex] != null)
            return $"Indexed Button Image (buttonStory{storyIndex + 1})";
        else
            return "Default Background (div 22)";
    }
    return "Empty Slot (Default Background)";
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

    [ContextMenu("Debug All Slot Backgrounds")]
    public void DebugAllSlotBackgrounds()
    {
        Debug.Log("🔍 === BACKGROUND STATUS FOR ALL SLOTS ===");
        Debug.Log($"StoryManager exists: {StoryManager.Instance != null}");
        Debug.Log($"Total stories in manager: {StoryManager.Instance?.allStories?.Count ?? 0}");

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

    [ContextMenu("Debug Story Validation")]
    public void DebugStoryValidation()
    {
        Debug.Log("🔍 === STORY VALIDATION DEBUG ===");
        for (int i = 0; i < 6; i++)
        {
            bool hasValidStory = false;
            StoryData story = null;

            if (i < StoryManager.Instance.allStories.Count)
            {
                story = StoryManager.Instance.allStories[i];
                hasValidStory = story != null && !string.IsNullOrEmpty(story.createdAt);
            }

            Debug.Log($"Slot {i}: Story={story != null}, CreatedAt={(story != null ? story.createdAt : "NULL")}, Valid={hasValidStory}");
        }
        Debug.Log("🔍 === END VALIDATION DEBUG ===");
    }

    [ContextMenu("Debug Firestore Stories")]
    public void DebugFirestoreStories()
    {
        Debug.Log("🔍 === FIRESTORE STORIES DEBUG ===");
        Debug.Log($"Total stories in allStories: {StoryManager.Instance.allStories?.Count ?? 0}");

        for (int i = 0; i < StoryManager.Instance.allStories.Count; i++)
        {
            var story = StoryManager.Instance.allStories[i];
            if (story != null)
            {
                Debug.Log($"Story {i}: '{story.storyTitle}' | CreatedAt: {story.createdAt} | ID: {story.storyId} | Index: {story.storyIndex}");
                Debug.Log($"  Background: {story.backgroundPath}");
                Debug.Log($"  Character1: {story.character1Path}");
                Debug.Log($"  Character2: {story.character2Path}");
            }
            else
            {
                Debug.Log($"Story {i}: NULL");
            }
        }
        Debug.Log("🔍 === END DEBUG ===");
    }

    [ContextMenu("Debug Story State For All Slots")]
    public void DebugStoryStateForAllSlots()
    {
        Debug.Log("🔍 === STORY STATE FOR ALL SLOTS ===");
        for (int i = 0; i < 6; i++)
        {
            StoryData story = null;
            if (i < StoryManager.Instance.allStories.Count)
            {
                story = StoryManager.Instance.allStories[i];
            }

            Debug.Log($"Slot {i}: " +
                     $"Exists: {story != null}, " +
                     $"CreatedAt: {(story != null ? (string.IsNullOrEmpty(story.createdAt) ? "EMPTY" : story.createdAt) : "NULL")}, " +
                     $"StoryId: {(story != null ? (string.IsNullOrEmpty(story.storyId) ? "EMPTY" : story.storyId) : "NULL")}, " +
                     $"Title: {(story != null ? (string.IsNullOrEmpty(story.storyTitle) ? "EMPTY" : story.storyTitle) : "NULL")}");
        }
        Debug.Log("🔍 === END DEBUG ===");
    }


}