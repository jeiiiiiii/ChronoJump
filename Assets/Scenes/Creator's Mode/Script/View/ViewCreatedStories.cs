using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
        Texture2D background = ImageStorage.GetBackgroundForStory(index);
        bool hasBackground = ImageStorage.HasBackground[index];

        if (hasBackground && background != null)
        {
            slot.backgroundImage.texture = background;
            slot.backgroundImage.gameObject.SetActive(true);

            AspectRatioFitter fitter = slot.backgroundImage.GetComponent<AspectRatioFitter>();
            if (fitter != null)
            {
                fitter.aspectRatio = (float)background.width / background.height;
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
        if (storyIndex >= 0 && storyIndex < StoryManager.Instance.allStories.Count)
        {
            // ✅ Set the currentStory before switching scene
            StoryManager.Instance.currentStory = StoryManager.Instance.allStories[storyIndex];

            // Load the edit scene (replace "EditSceneName" with your scene name)
            UnityEngine.SceneManagement.SceneManager.LoadScene("Creator'sModeScene");
        }
    }

    public void OnViewStory(int storyIndex)
    {
        if (storyIndex >= 0 && storyIndex < StoryManager.Instance.allStories.Count)
        {
            // ✅ Set currentStory for the View scene
            StoryManager.Instance.currentStory = StoryManager.Instance.allStories[storyIndex];
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }
}
