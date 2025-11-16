using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class ViewCreatedStoriesScene : MonoBehaviour
{
    [System.Serializable]
    public class StorySlot
    {
        public RawImage backgroundImage;
        public GameObject slotContainer; // The parent object for this slot
        public GameObject loadingSpinner; // Assign your spinner prefab instance here
    }

    [SerializeField] private StorySlot[] storySlots = new StorySlot[6];
    [SerializeField] private GameObject storyActionPopup;
    [SerializeField] private Image currentBackgroundImage;

    [SerializeField] private Texture2D[] storyButtonImages = new Texture2D[6];
    [SerializeField] private Texture2D defaultButtonBackground;

    [SerializeField] private GameObject loadingSpinnerPrefab; // Assign your prefab in Inspector
    [SerializeField] private Button refreshButton; // ADD THIS LINE

    private bool _storiesLoaded = false;
    private bool _isRefreshing = false; // ADD THIS LINE - track refresh state

    void Start()
    {
        // Initialize loading spinners for each slot
        InitializeSlotSpinners();

        if (storyActionPopup != null)
        {
            storyActionPopup.SetActive(false);
        }

        // NEW: Setup refresh button
        if (refreshButton != null)
        {
            refreshButton.onClick.RemoveAllListeners();
            refreshButton.onClick.AddListener(OnRefreshButtonClicked);
            Debug.Log("✅ Refresh button listener added");
        }

        // Subscribe to StoryManager events
        if (StoryManager.Instance != null)
        {
            StoryManager.Instance.OnStoriesLoadingStarted += OnStoriesLoadingStarted;
            StoryManager.Instance.OnStoriesLoaded += OnStoriesLoaded;
            StoryManager.Instance.OnStoriesLoadFailed += OnStoriesLoadFailed;
        }

        // Start the loading process
        StartCoroutine(InitializeStories());
    }

    private void InitializeSlotSpinners()
    {
        for (int i = 0; i < storySlots.Length; i++)
        {
            var slot = storySlots[i];
            
            // Create spinner instance if slot container exists
            if (slot.slotContainer != null && loadingSpinnerPrefab != null)
            {
                // Instantiate spinner as child of the slot container
                GameObject spinnerInstance = Instantiate(loadingSpinnerPrefab, slot.slotContainer.transform);
                slot.loadingSpinner = spinnerInstance;
                
                // Position it in the center of the slot
                RectTransform spinnerRect = spinnerInstance.GetComponent<RectTransform>();
                if (spinnerRect != null)
                {
                    spinnerRect.anchorMin = new Vector2(0.5f, 0.5f);
                    spinnerRect.anchorMax = new Vector2(0.5f, 0.5f);
                    spinnerRect.pivot = new Vector2(0.5f, 0.5f);
                    spinnerRect.anchoredPosition = Vector2.zero;
                }
                
                // Hide initially
                spinnerInstance.SetActive(false);
                
                Debug.Log($"✅ Created loading spinner for slot {i}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Could not create spinner for slot {i} - missing slotContainer or prefab");
            }
        }
    }

    private IEnumerator InitializeStories()
    {
        // Wait for StoryManager to be ready
        yield return new WaitUntil(() => StoryManager.Instance != null);

        // If stories are already loaded, update UI immediately
        if (StoryManager.Instance.allStories != null && StoryManager.Instance.allStories.Count > 0)
        {
            Debug.Log("📚 Stories already loaded, updating UI immediately");
            UpdateAllStoryBackgrounds();
            SetLoadingState(false);
            _storiesLoaded = true;
        }
        else
        {
            Debug.Log("🔄 Stories not loaded yet, showing loading spinners...");
            SetLoadingState(true);
        }
    }

    private void OnStoriesLoadingStarted()
{
    Debug.Log("🔄 Stories loading started...");
    SetLoadingState(true);
}

private void OnStoriesLoaded(List<StoryData> stories) // RECEIVE the stories list
{
    Debug.Log($"✅ Stories loaded event received: {stories?.Count} stories");
    
    // You can now validate the data before updating UI
    if (stories != null && stories.Count > 0)
    {
        Debug.Log("📚 Valid stories data received");
    }
    else
    {
        Debug.LogWarning("⚠️ Stories list is empty or null");
    }
    
    UpdateAllStoryBackgrounds();
    SetLoadingState(false);
    _storiesLoaded = true;
}

    private void OnStoriesLoadFailed(string error)
    {
        Debug.LogError($"❌ Stories load failed: {error}");
        UpdateAllStoryBackgrounds();
        SetLoadingState(false);
        _storiesLoaded = true;
    }


    private void SetLoadingState(bool isLoading)
    {
        // Show/hide individual slot loading spinners
        for (int i = 0; i < storySlots.Length; i++)
        {
            var slot = storySlots[i];
            
            if (slot.loadingSpinner != null)
            {
                slot.loadingSpinner.SetActive(isLoading);
            }
            
            // Show/hide background image based on loading state
            if (slot.backgroundImage != null)
            {
                slot.backgroundImage.gameObject.SetActive(!isLoading);
            }
        }

        Debug.Log($"🔄 Loading state: {(isLoading ? "SHOWING spinners" : "HIDING spinners")}");
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
            if (!string.IsNullOrEmpty(story.backgroundPath) && ImageStorage.ImageExists(story.backgroundPath))
            {
                backgroundToUse = ImageStorage.LoadImage(story.backgroundPath);
                Debug.Log($"[Slot {index}] Using custom background from relative path: {story.backgroundPath}");
            }
            else if (storyButtonImages != null && index < storyButtonImages.Length && storyButtonImages[index] != null)
            {
                backgroundToUse = storyButtonImages[index];
                Debug.Log($"[Slot {index}] Using indexed button: buttonStory{index + 1}");
            }
            else
            {
                backgroundToUse = defaultButtonBackground;
                Debug.Log($"[Slot {index}] Using default background");
            }
        }
        else
        {
            backgroundToUse = defaultButtonBackground;
            Debug.Log($"[Slot {index}] Empty slot - using default background");
        }

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

    void OnDestroy()
    {
        // Unsubscribe from events
        if (StoryManager.Instance != null)
        {
            StoryManager.Instance.OnStoriesLoadingStarted -= OnStoriesLoadingStarted;
            StoryManager.Instance.OnStoriesLoaded -= OnStoriesLoaded;
            StoryManager.Instance.OnStoriesLoadFailed -= OnStoriesLoadFailed;
        }
        
        // NEW: Clean up refresh button listener
        if (refreshButton != null)
        {
            refreshButton.onClick.RemoveAllListeners();
        }
    }

    // Add manual refresh capability
    void Update()
    {
        // Auto-refresh when stories become available (backup mechanism)
        if (!_storiesLoaded && StoryManager.Instance != null && 
            StoryManager.Instance.allStories != null && 
            StoryManager.Instance.allStories.Count > 0)
        {
            Debug.Log("🔄 Auto-refreshing stories UI (backup)");
            UpdateAllStoryBackgrounds();
            SetLoadingState(false);
            _storiesLoaded = true;
        }
    }

    // Manual refresh for testing
    [ContextMenu("Manual Refresh")]
    public void ManualRefresh()
    {
        Debug.Log("🔄 Manual refresh triggered");
        UpdateAllStoryBackgrounds();
    }

    // Your existing methods remain the same...
    public void RefreshBackgrounds()
    {
        UpdateAllStoryBackgrounds();
    }

    void OnEnable()
    {
        UpdateAllStoryBackgrounds();
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

    private void DebugTeacherContext()
    {
        Debug.Log("🔍 === TEACHER CONTEXT DEBUG ===");

        // Check Firebase user
        if (FirebaseManager.Instance?.CurrentUserData != null)
        {
            Debug.Log($"Firebase User: {FirebaseManager.Instance.CurrentUserData.displayName}");
            Debug.Log($"Firebase Role: {FirebaseManager.Instance.CurrentUserData.role}");
            Debug.Log($"Firebase UserId: {FirebaseManager.Instance.CurrentUser.UserId}");
        }
        else
        {
            Debug.Log("❌ No Firebase user data");
        }

        // Check StoryManager teacher context
        if (StoryManager.Instance != null)
        {
            Debug.Log($"StoryManager Teacher ID: {StoryManager.Instance.GetCurrentTeacherId()}");
            Debug.Log($"StoryManager Stories Count: {StoryManager.Instance.allStories?.Count ?? 0}");
        }
        else
        {
            Debug.Log("❌ StoryManager instance is null");
        }

        Debug.Log("🔍 === END DEBUG ===");
    }

    // NEW: Public method for refresh button
    public void OnRefreshButtonClicked()
    {
        Debug.Log("🔄 Refresh button clicked - reloading stories from Firebase");
        
        if (_isRefreshing)
        {
            Debug.LogWarning("⚠️ Already refreshing, ignoring click");
            return;
        }
        
        if (StoryManager.Instance == null)
        {
            Debug.LogError("❌ StoryManager.Instance is null");
            return;
        }
        
        StartCoroutine(RefreshStories());
    }

    private IEnumerator RefreshStories()
    {
        _isRefreshing = true;
        _storiesLoaded = false;
        
        // Disable refresh button
        if (refreshButton != null)
        {
            refreshButton.interactable = false;
            Debug.Log("🔘 Refresh button DISABLED");
        }
        
        // Show loading spinners
        SetLoadingState(true);
        Debug.Log("⏳ Showing loading spinners...");
        
        // Clear current stories in StoryManager
        if (StoryManager.Instance != null)
        {
            StoryManager.Instance.allStories.Clear();
            Debug.Log("🗑️ Cleared current stories from StoryManager");
        }
        
        // Wait a frame
        yield return null;
        
        // Reload stories from Firestore (CORRECTED METHOD NAME)
        if (StoryManager.Instance != null)
        {
            Debug.Log("📡 Requesting fresh data from Firestore...");
            StoryManager.Instance.LoadStoriesFromFirestore(); // ✅ CORRECTED
        }
        
        // Wait for stories to load (with timeout)
        float timeout = 10f;
        float elapsed = 0f;
        
        while (!_storiesLoaded && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (elapsed >= timeout)
        {
            Debug.LogWarning("⚠️ Story loading timed out after 10 seconds");
            SetLoadingState(false);
        }
        
        // Add small delay to ensure UI is updated
        yield return new WaitForSeconds(0.3f);
        
        // Re-enable refresh button
        if (refreshButton != null)
        {
            refreshButton.interactable = true;
            Debug.Log("🔘 Refresh button ENABLED");
        }
        
        _isRefreshing = false;
        Debug.Log("✅ Refresh complete");
    }

}