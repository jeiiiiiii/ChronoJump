using UnityEngine;
using UnityEngine.SceneManagement;

public class LandingPage : MonoBehaviour
{
    public void CreateNew()
    {
        SceneManager.LoadScene("CreateNewAddTitleScene");
    }
    public void CreatedStories()
    {
        SceneManager.LoadScene("ViewCreatedStoriesScene");
    }
}
