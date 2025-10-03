using UnityEngine;
using UnityEngine.SceneManagement;

public class StorySelector : MonoBehaviour
{
    [SerializeField] private int storyIndex; // Set this in inspector (0-5)
    [SerializeField] private GameObject actionPopup;

    void Start()
    {
        // ✅ FIXED: Only initialize, don't auto-create stories
        Debug.Log($"🔍 StorySelector {storyIndex} Ready - Total Stories: {StoryManager.Instance.stories.Count}");
        
        // Optional: Debug current state
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
    Debug.Log($"🎯 Story slot {storyIndex} clicked");
    
    var stories = StoryManager.Instance.stories;

    // ✅ FIXED VALIDATION: Allow empty slots to be created
    if (storyIndex < 0 || storyIndex > 5) // 6 slots total (0-5)
    {
        Debug.Log($"❌ Invalid story index {storyIndex}. Must be between 0-5.");
        return;
    }

    // ✅ Check if slot has content
    bool storyExists = false;
    StoryData story = null;
    
    if (storyIndex < stories.Count)
    {
        story = stories[storyIndex];
        // ✅ FIX: Story exists if not null (regardless of background)
        storyExists = story != null;
    }
    
    Debug.Log($"📖 Story slot {storyIndex}: {(storyExists ? "EXISTS" : "EMPTY")} (Total stories: {stories.Count})");
    Debug.Log($"📖 Story details: {(story != null ? $"Title: '{story.storyTitle}', Has BG: {!string.IsNullOrEmpty(story.backgroundPath)}" : "NULL")}");

    if (storyExists)
    {
        // Existing story → show action popup (edit/view/delete)
        Debug.Log($"✏️ Opening action popup for: {story.storyTitle}");
        ShowActionPopup();
    }
    else
    {
        // Empty slot → create new story
        Debug.Log($"🆕 Creating new story in slot {storyIndex}");
        CreateNewStory();
    }
}

    void ShowActionPopup()
    {
        if (actionPopup != null)
        {
            actionPopup.SetActive(true);
            Debug.Log($"📋 Action popup shown for story slot {storyIndex}");
        }
        else
        {
            Debug.LogError("❌ Action popup reference is missing!");
        }
    }

    void CreateNewStory()
{
    Debug.Log($"🚀 Creating new story in slot {storyIndex}");
    
    // ✅ Remember which slot is being created
    ImageStorage.CurrentStoryIndex = storyIndex;
    Debug.Log($"📝 ImageStorage.CurrentStoryIndex set to: {storyIndex}");

    var stories = StoryManager.Instance.allStories;

    // ✅ Create a clean new story object WITH PROPER INDEX
    StoryData newStory = new StoryData
    {
        storyIndex = storyIndex, // ✅ SET THE INDEX
        backgroundPath = string.Empty,
        character1Path = string.Empty,
        character2Path = string.Empty,
        storyTitle = string.Empty // ✅ EMPTY TITLE - let user set it
    };

    Debug.Log($"📝 New story created with ID: {newStory.storyId}, Index: {newStory.storyIndex}");

    // ✅ If slot already exists, overwrite. Otherwise, expand list.
    if (storyIndex < stories.Count)
    {
        stories[storyIndex] = newStory;
        Debug.Log($"📝 Overwrote existing story at index {storyIndex}");
    }
    else
    {
        // Fill empty slots if needed
        while (stories.Count <= storyIndex)
        {
            stories.Add(null);
            Debug.Log($"📝 Added null placeholder at index {stories.Count - 1}");
        }

        stories[storyIndex] = newStory;
        Debug.Log($"📝 Added new story at index {storyIndex}");
    }

    // ✅ Set as current story
    StoryManager.Instance.SetCurrentStory(newStory);
    Debug.Log($"📖 Current story set to new story at index {storyIndex}");

    // ✅ REMOVED: StoryManager.Instance.SaveStories(); - Don't save to Firestore yet!
    Debug.Log($"ℹ️ Story created but NOT saved to Firestore - waiting for explicit save");

    // ✅ Clear ALL temporary uploads so old images don't leak into new story
    ImageStorage.UploadedTexture = null;   // background
    ImageStorage.uploadedTexture1 = null;  // character 1
    ImageStorage.uploadedTexture2 = null;  // character 2
    Debug.Log($"🔄 Cleared temporary image uploads");

    // ✅ Load the creation scene
    Debug.Log($"🎬 Loading CreateNewAddTitleScene for new story creation");
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

            // ✅ Set current story
            StoryManager.Instance.SetCurrentStory(selected);
            Debug.Log($"📖 Current story set to: {selected.storyTitle} (Index: {selected.storyIndex})");

            // ✅ Set ImageStorage index
            ImageStorage.CurrentStoryIndex = selected.storyIndex;
            Debug.Log($"📝 ImageStorage.CurrentStoryIndex set to: {selected.storyIndex}");

            // ✅ Restore images if paths exist
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

    // ✅ NEW: Debug method to check current state
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
}