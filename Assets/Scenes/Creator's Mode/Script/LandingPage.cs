using UnityEngine;
using UnityEngine.SceneManagement;

public class LandingPage : MonoBehaviour
{
    public void CreateNew()
    {
        SceneManager.LoadScene("ViewCreatedStoriesScene");
    }
    public void CreatedStories()
    {
        SceneManager.LoadScene("ViewCreatedStoriesScene");
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("TitleScreen");
    }
}
