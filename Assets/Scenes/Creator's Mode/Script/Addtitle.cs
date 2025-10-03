using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class AddTitle : MonoBehaviour
{
    public TMP_InputField titleInputField;
    public TMP_InputField descriptionInputField;

    void Start()
    {
        // ✅ FIXED: Ensure we have a current story
        if (StoryManager.Instance.currentStory == null)
        {
            Debug.LogWarning("⚠️ No current story found, creating a new one...");
            
            // Get the current story index from ImageStorage
            int storyIndex = ImageStorage.CurrentStoryIndex;
            if (storyIndex >= 0)
            {
                // Create a new story for this slot
                StoryData newStory = new StoryData
                {
                    storyIndex = storyIndex,
                    backgroundPath = string.Empty,
                    character1Path = string.Empty,
                    character2Path = string.Empty,
                    storyTitle = string.Empty,
                    createdAt = string.Empty
                };
                
                // Add to stories list
                while (StoryManager.Instance.allStories.Count <= storyIndex)
                {
                    StoryManager.Instance.allStories.Add(null);
                }
                StoryManager.Instance.allStories[storyIndex] = newStory;
                StoryManager.Instance.currentStory = newStory;
                StoryManager.Instance.currentStoryIndex = storyIndex;
                
                Debug.Log($"✅ Created new story at index {storyIndex}");
            }
            else
            {
                Debug.LogError("❌ Cannot create story: No valid story index!");
            }
        }

        var story = StoryManager.Instance.currentStory;
        if (story != null)
        {
            if (!string.IsNullOrEmpty(story.storyTitle))
                titleInputField.text = story.storyTitle;

            if (!string.IsNullOrEmpty(story.storyDescription))
                descriptionInputField.text = story.storyDescription;
                
            Debug.Log($"📖 Editing story: {story.storyTitle} (Index: {story.storyIndex})");
        }
    }

    public void OnNextButton()
    {
        var story = StoryManager.Instance.currentStory;

        if (story == null)
        {
            Debug.LogError("❌ No current story to update!");
            return;
        }

        // ✅ Save entered values
        story.storyTitle = titleInputField.text;
        story.storyDescription = descriptionInputField.text;

        Debug.Log($"✅ Updated Story Title: '{story.storyTitle}', Description: '{story.storyDescription}'");

        // ✅ FIXED: If this is a new story (not in list), add it to the list now
        if (StoryManager.Instance.currentStoryIndex == -1 && ImageStorage.CurrentStoryIndex >= 0)
        {
            int targetIndex = ImageStorage.CurrentStoryIndex;

            // Ensure the list is big enough
            while (StoryManager.Instance.allStories.Count <= targetIndex)
            {
                StoryManager.Instance.allStories.Add(null);
            }

            // Add the story to the list
            StoryManager.Instance.allStories[targetIndex] = story;
            StoryManager.Instance.currentStoryIndex = targetIndex;

            Debug.Log($"✅ Added new story to list at index {targetIndex}");
        }

        // ❌ REMOVED: StoryManager.Instance.SaveStories(); - Don't save yet!
        Debug.Log("ℹ️ Story updated in memory - will save when teacher clicks 'Save & Publish'");

        // Move to the next scene
        SceneManager.LoadScene("CreateNewAddCharacterScene");
    }


    public void MainMenu()
    {
        SceneManager.LoadScene("Creator'sModeScene");
    }

    public void Back()
    {
        SceneManager.LoadScene("ViewCreatedStoriesScene");
    }
}