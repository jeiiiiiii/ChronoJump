using UnityEngine;
using UnityEngine.SceneManagement;


public class IndusCodexScene : MonoBehaviour
{
    public void Back()
    {
        SceneManager.LoadScene("IndusStoryCoordinate");
    }
    public void AchievementsScene()
    {
        SceneManager.LoadScene("IndusAchievementsScene");
    }
    public void Artifacts()
    {
        SceneManager.LoadScene("IndusArtifactsScene");
    }
    public void Characters()
    {
        SceneManager.LoadScene("IndusCharacterCoordinate");
    }
    public void Story()
    {
        SceneManager.LoadScene("IndusStoryCoordinate");
    }

}
