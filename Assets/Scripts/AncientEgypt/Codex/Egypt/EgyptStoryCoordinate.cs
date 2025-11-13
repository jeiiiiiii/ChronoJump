using UnityEngine;
using UnityEngine.SceneManagement;


public class EgyptStoryCoordinate : MonoBehaviour
{
    public void Kingdom()
    {
        SceneManager.LoadScene("KingdomStoryScene");
    }
    public void Nile()
    {
        SceneManager.LoadScene("NileStoryScene");
    }
    public void Back()
    {
        SceneManager.LoadScene("EgyptCodexScene");
    }
    public void BackStoryCoordinate()
    {
        SceneManager.LoadScene("EgyptStoryCoordinate");
    }
}
