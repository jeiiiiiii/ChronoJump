using UnityEngine;
using UnityEngine.SceneManagement;

public class StorySelector : MonoBehaviour
{
    [SerializeField] private int storyIndex; // Set this in inspector (0-5)
    
    // Reference to the popup panel (assign in inspector)
    [SerializeField] private GameObject actionPopup;
    
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
}