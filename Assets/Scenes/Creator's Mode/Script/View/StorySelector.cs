using UnityEngine;
using UnityEngine.SceneManagement;

public class StorySelector : MonoBehaviour
{
    [SerializeField] private int storyIndex; // Set this in inspector (0-5)
    [SerializeField] private GameObject actionPopup;

    void Start()
    {
        Debug.Log($"🔍 StorySelector {storyIndex} Ready - Total Stories: {StoryManager.Instance.stories.Count}");

        if (StoryManager.Instance.GetCurrentStory() == null)
        {
            Debug.Log("ℹ️ No current story selected - this is OK for empty grid");
        }
        else
        {
            Debug.Log($"ℹ️ Current story: {StoryManager.Instance.GetCurrentStory().storyTitle}");
        }
    }

    public void OnStoryClicked()
    {
        Debug.Log($"Story slot {storyIndex} clicked");

        var stories = StoryManager.Instance.allStories;

        if (storyIndex < 0 || storyIndex > 5)
        {
            Debug.Log($"Invalid story index {storyIndex}");
            return;
        }

        // ✅ FIXED: Ensure list has enough slots
        while (stories.Count <= storyIndex)
        {
            stories.Add(null);
        }

        // Check if there's a saved story in this slot
        StoryData story = stories[storyIndex];
        bool hasSavedStory = story != null && !string.IsNullOrEmpty(story.createdAt);

        Debug.Log($"Story slot {storyIndex}: {(hasSavedStory ? "SAVED STORY EXISTS" : "EMPTY/NOT SAVED")}");

        if (hasSavedStory)
        {
            Debug.Log($"Opening action popup for saved story");

            StoryManager.Instance.currentStory = story;
            ImageStorage.CurrentStoryIndex = storyIndex;

            ShowActionPopup();
        }
        else
        {
            Debug.Log($"Creating new story in slot {storyIndex}");
            CreateNewStory();
        }
    }



    void ShowActionPopup()
    {
        if (actionPopup != null)
        {
            actionPopup.SetActive(true);

            // ✅ Notify StoryActionHandler about which story is selected
            StoryActionHandler actionHandler = actionPopup.GetComponent<StoryActionHandler>();
            if (actionHandler != null)
            {
                actionHandler.OnPopupShown(storyIndex);
            }

            Debug.Log($"📋 Action popup shown for story slot {storyIndex}");
        }
        else
        {
            Debug.LogError("❌ Action popup reference is missing!");
        }
    }


    void CreateNewStory()
    {
        Debug.Log($"Creating new story in slot {storyIndex}");

        ImageStorage.CurrentStoryIndex = storyIndex;
        ImageStorage.UploadedTexture = null;
        ImageStorage.uploadedTexture1 = null;
        ImageStorage.uploadedTexture2 = null;

        // Create temporary story with the slot index
        StoryData newStory = new StoryData
        {
            storyIndex = storyIndex, // This is the KEY - the slot index
            backgroundPath = string.Empty,
            character1Path = string.Empty,
            character2Path = string.Empty,
            storyTitle = string.Empty,
            createdAt = string.Empty,
            storyId = string.Empty
        };

        // Ensure the list has enough slots
        while (StoryManager.Instance.allStories.Count <= storyIndex)
        {
            StoryManager.Instance.allStories.Add(null);
        }

        // Place the story in the correct slot
        StoryManager.Instance.allStories[storyIndex] = newStory;
        StoryManager.Instance.currentStory = newStory;
        StoryManager.Instance.currentStoryIndex = storyIndex;

        Debug.Log($"Current story set to new temporary story for slot {storyIndex}");
        Debug.Log($"Loading CreateNewAddTitleScene for new story creation");

        SceneManager.LoadScene("CreateNewAddTitleScene");
    }

    public void OnSelectStory(int index)
    {
        Debug.Log($"🎯 Selecting story at index {index} for editing");

        if (index >= 0 && index < StoryManager.Instance.stories.Count)
        {
            StoryData selected = StoryManager.Instance.stories[index];

            if (selected == null)
            {
                Debug.LogError($"❌ Story at index {index} is null!");
                return;
            }

            // Set current story
            StoryManager.Instance.SetCurrentStory(selected);
            Debug.Log($"📖 Current story set to: {selected.storyTitle} (Index: {selected.storyIndex})");

            // Set ImageStorage index
            ImageStorage.CurrentStoryIndex = selected.storyIndex;
            Debug.Log($"📝 ImageStorage.CurrentStoryIndex set to: {selected.storyIndex}");

            // Restore images if paths exist
            if (!string.IsNullOrEmpty(selected.backgroundPath))
            {
                ImageStorage.UploadedTexture = ImageStorage.LoadImage(selected.backgroundPath);
                Debug.Log($"🖼️ Loaded background: {selected.backgroundPath}");
            }
            else
            {
                Debug.Log("ℹ️ No background path found");
            }

            if (!string.IsNullOrEmpty(selected.character1Path))
            {
                ImageStorage.uploadedTexture1 = ImageStorage.LoadImage(selected.character1Path);
                Debug.Log($"🖼️ Loaded character 1: {selected.character1Path}");
            }

            if (!string.IsNullOrEmpty(selected.character2Path))
            {
                ImageStorage.uploadedTexture2 = ImageStorage.LoadImage(selected.character2Path);
                Debug.Log($"🖼️ Loaded character 2: {selected.character2Path}");
            }

            // Load the next scene
            Debug.Log($"🎬 Loading AddQuizScene for story editing");
            SceneManager.LoadScene("AddQuizScene");
        }
        else
        {
            Debug.LogError($"❌ Invalid story index selected: {index} (total stories: {StoryManager.Instance.stories.Count})");
        }
    }

    [ContextMenu("Debug This Story Slot")]
    public void DebugThisStorySlot()
    {
        Debug.Log($"🔍 STORY SLOT {storyIndex} DEBUG:");
        Debug.Log($"🔍 Total Stories: {StoryManager.Instance.stories.Count}");

        if (storyIndex < StoryManager.Instance.stories.Count)
        {
            var story = StoryManager.Instance.stories[storyIndex];
            Debug.Log($"🔍 Story at slot {storyIndex}: {(story != null ? story.storyTitle : "NULL")}");
            Debug.Log($"🔍 Background Path: {(story != null ? story.backgroundPath : "N/A")}");
            Debug.Log($"🔍 Story Index: {(story != null ? story.storyIndex.ToString() : "N/A")}");
        }
        else
        {
            Debug.Log($"🔍 Slot {storyIndex} is beyond current stories count");
        }
    }
    [ContextMenu("Debug Current Story State")]
    public void DebugCurrentStoryState()
    {
        Debug.Log("🔍 === CURRENT STORY STATE DEBUG ===");
        Debug.Log($"ImageStorage.CurrentStoryIndex: {ImageStorage.CurrentStoryIndex}");
        Debug.Log($"StoryManager.currentStory: {StoryManager.Instance.currentStory != null}");
        Debug.Log($"StoryManager.currentStoryIndex: {StoryManager.Instance.currentStoryIndex}");

        if (StoryManager.Instance.currentStory != null)
        {
            Debug.Log($"Current Story Title: {StoryManager.Instance.currentStory.storyTitle}");
            Debug.Log($"Current Story Index: {StoryManager.Instance.currentStory.storyIndex}");
        }

        Debug.Log($"Total stories in list: {StoryManager.Instance.allStories.Count}");
        Debug.Log("🔍 === END DEBUG ===");
    }

}