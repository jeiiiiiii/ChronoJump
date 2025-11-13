using UnityEngine;
using UnityEngine.SceneManagement;


public class EgyptCodexScene : MonoBehaviour
{
    public void Back()
    {
        SceneManager.LoadScene("EgyptStoryCoordinate");
    }
    public void AchievementsScene()
    {
        SceneManager.LoadScene("EgyptAchievementsScene");
    }
    public void Artifacts()
    {
        SceneManager.LoadScene("EgyptArtifactsScene");
    }
    public void Characters()
    {
        SceneManager.LoadScene("EgyptCharacterCoordinate");
    }
    public void Story()
    {
        SceneManager.LoadScene("EgyptStoryCoordinate");
    }

}
