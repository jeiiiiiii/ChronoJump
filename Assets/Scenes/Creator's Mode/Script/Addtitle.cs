using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class AddTitle : MonoBehaviour
{
    public TMP_InputField titleInputField;
    public TMP_InputField descriptionInputField;

    void Start()
    {
        var story = StoryManager.Instance.currentStory;
        if (story != null)
        {
            if (!string.IsNullOrEmpty(story.storyTitle))
                titleInputField.text = story.storyTitle;

            if (!string.IsNullOrEmpty(story.storyDescription))
                descriptionInputField.text = story.storyDescription;
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