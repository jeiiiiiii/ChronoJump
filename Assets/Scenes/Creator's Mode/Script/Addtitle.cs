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
            // If no story exists yet, create one
            story = StoryManager.Instance.CreateNewStory(titleInputField.text);
        }

        // ✅ Save entered values
        story.storyTitle = titleInputField.text;
        story.storyDescription = descriptionInputField.text;

        Debug.Log($"✅ Saved Story Title: {story.storyTitle}, Description: {story.storyDescription}");

        // Optionally auto-save stories
        StoryManager.Instance.SaveStories();

        // Move to the next scene
        SceneManager.LoadScene("CreateNewAddCharacterScene");
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("Creator'sModeScene");
    }
    public void Back()
    {
        SceneManager.LoadScene("Creator'sModeScene");
    }

}
