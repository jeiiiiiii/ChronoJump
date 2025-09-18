using UnityEngine;
using UnityEngine.SceneManagement;

public class StorySelector : MonoBehaviour
{
    [SerializeField] private int storyIndex; // Set this in inspector (0-5)

    // Reference to the popup panel (assign in inspector)
    [SerializeField] private GameObject actionPopup;

    void Start()
    {
        if (StoryManager.Instance.GetCurrentStory() == null)
        {
            StoryManager.Instance.CreateNewStory();
        }
    }

    public void OnStoryClicked()
    {
        var stories = StoryManager.Instance.stories;

        // ✅ Validate index
        if (storyIndex < 0 || storyIndex >= stories.Count)
        {
            Debug.Log($"❌ Invalid story index {storyIndex}");
            return;
        }

        StoryData story = stories[storyIndex];

        // ✅ Check if this slot has a background (our definition of "story exists")
        bool storyExists = story != null && !string.IsNullOrEmpty(story.backgroundPath);

        if (storyExists)
        {
            // Existing story → show action popup (edit/view/delete)
            ShowActionPopup();
        }
        else
        {
            // Empty slot → create new story
            CreateNewStory();
        }
    }


    void ShowActionPopup()
    {
        if (actionPopup != null)
        {
            actionPopup.SetActive(true);
        }
    }

void CreateNewStory()
{
    // ✅ Remember which slot is being created
    ImageStorage.CurrentStoryIndex = storyIndex;

    var stories = StoryManager.Instance.allStories;

    StoryData newStory = new StoryData();

    // ✅ If slot already exists, overwrite. Otherwise, expand list.
    if (storyIndex < stories.Count)
    {
        stories[storyIndex] = newStory;
    }
    else
    {
        // Fill empty slots if needed
        while (stories.Count <= storyIndex)
            stories.Add(null);

        stories[storyIndex] = newStory;
    }

    // ✅ Set as current story
    StoryManager.Instance.SetCurrentStory(newStory);

    // ✅ Persist immediately
    StoryManager.Instance.SaveStories();

    // ✅ Clear temporary uploads
    ImageStorage.UploadedTexture = null;
    ImageStorage.uploadedTexture1 = null;
    ImageStorage.uploadedTexture2 = null;

    // ✅ Load the creation scene
    SceneManager.LoadScene("CreateNewAddTitleScene");
}


    public void OnSelectStory(int index)
    {
        if (index >= 0 && index < StoryManager.Instance.stories.Count)
        {
            StoryData selected = StoryManager.Instance.stories[index];

            // ✅ Set current story
            StoryManager.Instance.SetCurrentStory(selected);

            // ✅ Restore images if paths exist
            if (!string.IsNullOrEmpty(selected.backgroundPath))
                ImageStorage.UploadedTexture = ImageStorage.LoadImage(selected.backgroundPath);

            if (!string.IsNullOrEmpty(selected.character1Path))
                ImageStorage.uploadedTexture1 = ImageStorage.LoadImage(selected.character1Path);

            if (!string.IsNullOrEmpty(selected.character2Path))
                ImageStorage.uploadedTexture2 = ImageStorage.LoadImage(selected.character2Path);

            // Load the next scene (e.g. AddQuiz, Dialogue, etc.)
            SceneManager.LoadScene("AddQuizScene");
        }
        else
        {
            Debug.LogError("❌ Invalid story index selected!");
        }
    }
}
