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
    [SerializeField] private Image currentBackgroundImage; // ✅ assign in Inspector

    void Start()
    {
        // Check if a student is playing a specific story
        CheckForStudentStoryPlayback();

        UpdateAllStoryBackgrounds();

        if (storyActionPopup != null)
        {
            storyActionPopup.SetActive(false);
        }

        // Load current story background (existing code)
        if (StoryManager.Instance != null && StoryManager.Instance.currentStory != null)
        {
            string path = StoryManager.Instance.currentStory.backgroundPath;
            if (!string.IsNullOrEmpty(path))
            {
                Texture2D tex = StoryManager.Instance.LoadBackground(path);
                if (tex != null && currentBackgroundImage != null)
                {
                    Sprite bgSprite = Sprite.Create(
                        tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f)
                    );
                    currentBackgroundImage.sprite = bgSprite;
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠ No current story set, skipping background preview.");
        }
    }

    private void CheckForStudentStoryPlayback()
    {
        bool isStudentPlaying = PlayerPrefs.GetString("IsStudentPlaying", "false") == "true";
        string selectedStoryID = PlayerPrefs.GetString("SelectedStoryID", "");

        if (isStudentPlaying && !string.IsNullOrEmpty(selectedStoryID))
        {
            Debug.Log($"Student is playing story ID: {selectedStoryID}");

            // Clear the student playing flag
            PlayerPrefs.SetString("IsStudentPlaying", "false");
            PlayerPrefs.Save();

            // Find and auto-play the selected story
            AutoPlayStoryByID(selectedStoryID);
        }
    }

    private void AutoPlayStoryByID(string storyID)
    {
        if (StoryManager.Instance == null) return;

        // Find the story with the matching ID
        var story = StoryManager.Instance.allStories.Find(s => s.storyId == storyID);

        if (story != null)
        {
            // Set as current story
            StoryManager.Instance.currentStory = story;

            Debug.Log($"Auto-playing story: {story.storyTitle}");

            // Load the GameScene directly (same as ViewStory)
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.LogError($"Story with ID {storyID} not found!");
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
        if (index < StoryManager.Instance.allStories.Count)
        {
            var story = StoryManager.Instance.allStories[index];
            if (!string.IsNullOrEmpty(story.backgroundPath))
            {
                Texture2D background = StoryManager.Instance.LoadBackground(story.backgroundPath);
                if (background != null)
                {
                    slot.backgroundImage.texture = background;
                    slot.backgroundImage.gameObject.SetActive(true);

                    AspectRatioFitter fitter = slot.backgroundImage.GetComponent<AspectRatioFitter>();
                    if (fitter != null)
                    {
                        fitter.aspectRatio = (float)background.width / background.height;
                    }
                    return;
                }
            }
        }

        // If no background, hide slot
        slot.backgroundImage.gameObject.SetActive(false);
    }

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
}
