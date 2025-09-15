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
        // Check if story exists or is empty
        bool storyExists = ImageStorage.HasBackground[storyIndex];

        if (storyExists)
        {
            // Show popup with Edit/View/Delete options
            ShowActionPopup();
        }
        else
        {
            // Story is empty, directly create new story
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
        // Set which story we're creating
        ImageStorage.CurrentStoryIndex = storyIndex;

        // Clear any temporary data
        ImageStorage.UploadedTexture = null;

        // Load the creation scene
        SceneManager.LoadScene("CreateNewAddTitleScene");
    }
    
    public void OnSelectStory(int index)
{
    if (index >= 0 && index < StoryManager.Instance.stories.Count)
    {
        StoryData selected = StoryManager.Instance.stories[index];

        // ✅ Set current story
        StoryManager.Instance.SetCurrentStory(selected);

        // Load the next scene (e.g. AddQuiz, Dialogue, etc.)
        SceneManager.LoadScene("AddQuizScene"); 
    }
    else
    {
        Debug.LogError("❌ Invalid story index selected!");
    }
}

}