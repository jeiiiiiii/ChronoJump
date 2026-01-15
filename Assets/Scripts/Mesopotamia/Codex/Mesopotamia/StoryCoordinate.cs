using UnityEngine;
using UnityEngine.SceneManagement;


public class StoryCoordinate : MonoBehaviour
{
    public void Sumerian()
    {
        SceneManager.LoadScene("SumerianStoryScene");
    }
    public void Akkadian()
    {
        SceneManager.LoadScene("AkkadianStoryScene");
    }
    public void Babylonian()
    {
        SceneManager.LoadScene("BabylonianStoryScene");
    }
    public void Assyrian()
    {
        SceneManager.LoadScene("AssyrianStoryScene");
    }
    public void Back()
    {
        SceneManager.LoadScene("CodexScene");
    }
    public void BackStoryCoordinate()
    {
        SceneManager.LoadScene("StoryCoordinate");
    }
}
