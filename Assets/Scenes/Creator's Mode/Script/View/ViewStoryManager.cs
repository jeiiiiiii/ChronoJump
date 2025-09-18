using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ViewStoriesBackgroundManager : MonoBehaviour
{
    [System.Serializable]
    public class StorySlot
    {
        public RawImage backgroundImage;   // For grid/slots
    }

    [SerializeField] private StorySlot[] storySlots = new StorySlot[6];
    [SerializeField] private GameObject storyActionPopup;
    [SerializeField] private Image currentBackgroundImage;  // ✅ assign in Inspector

    void Start()
    {
        UpdateAllStoryBackgrounds();

        // Make sure popup is initially hidden
        if (storyActionPopup != null)
        {
            storyActionPopup.SetActive(false);
        }

        // ✅ SAFETY CHECKS
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

        // ✅ Pull the story from StoryManager instead of ImageStorage
        var stories = StoryManager.Instance.allStories;
        if (index >= stories.Count)
        {
            slot.backgroundImage.gameObject.SetActive(false);
            return;
        }

        StoryData story = stories[index];
        if (story != null && !string.IsNullOrEmpty(story.backgroundPath))
        {
            Texture2D background = StoryManager.Instance.LoadBackground(story.backgroundPath);
            if (background != null)
            {
                slot.backgroundImage.texture = background;
                slot.backgroundImage.gameObject.SetActive(true);

                // ✅ Maintain aspect ratio
                AspectRatioFitter fitter = slot.backgroundImage.GetComponent<AspectRatioFitter>();
                if (fitter != null)
                {
                    fitter.aspectRatio = (float)background.width / background.height;
                }
            }
        }
        else
        {
            slot.backgroundImage.gameObject.SetActive(false);
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

    public void OnEditStory(int storyIndex)
    {
        var stories = StoryManager.Instance.allStories;

        if (storyIndex >= 0 && storyIndex < stories.Count)
        {
            StoryData story = stories[storyIndex];

            if (story != null)
            {
                // ✅ Set the current story
                StoryManager.Instance.currentStory = story;

                // ✅ Remember which slot is being edited
                ImageStorage.CurrentStoryIndex = storyIndex;

                // ✅ Save to persistence so it survives reload
                StoryManager.Instance.SaveStories();

                // ✅ Load your editing scene
                UnityEngine.SceneManagement.SceneManager.LoadScene("CreateNewAddTitleScene");
            }
            else
            {
                Debug.LogWarning($"❌ Tried to edit story at slot {storyIndex}, but it's empty.");
            }
        }
        else
        {
            Debug.LogError($"❌ Invalid story index: {storyIndex}");
        }
    }


    public void OnViewStory(int storyIndex)
    {
        if (storyIndex >= 0 && storyIndex < StoryManager.Instance.allStories.Count)
        {
            StoryManager.Instance.currentStory = StoryManager.Instance.allStories[storyIndex];

            // ✅ update preview before leaving
            UpdatePreviewBackground(StoryManager.Instance.currentStory);

            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }
    
    private void UpdatePreviewBackground(StoryData story)
    {
        if (story == null || string.IsNullOrEmpty(story.backgroundPath)) return;

        Texture2D tex = StoryManager.Instance.LoadBackground(story.backgroundPath);
        if (tex != null && currentBackgroundImage != null)
        {
            currentBackgroundImage.sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f)
            );
        }
    }

    public bool StoryExistsAt(int index)
    {
        var stories = StoryManager.Instance.stories; // <-- adjust to .allStories if needed

        if (index < 0 || index >= stories.Count)
            return false;

        StoryData story = stories[index];
        return story != null && !string.IsNullOrEmpty(story.backgroundPath);
    }

}
