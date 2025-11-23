using UnityEngine;
using UnityEngine.SceneManagement;


public class IndusStoryCoordinate : MonoBehaviour
{
    public void Harappa()
    {
        SceneManager.LoadScene("HarappaStoryScene");
    }
    public void Sining()
    {
        SceneManager.LoadScene("SiningStoryScene");
    }
    public void Back()
    {
        SceneManager.LoadScene("IndusCodexScene");
    }
    public void BackStoryCoordinate()
    {
        SceneManager.LoadScene("IndusStoryCoordinate");
    }
}
