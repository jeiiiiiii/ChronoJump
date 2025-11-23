using UnityEngine;
using UnityEngine.SceneManagement;


public class ChinaCodexScene : MonoBehaviour
{
    public void Back()
    {
        SceneManager.LoadScene("HuangHeCoordinateSelect");
    }
    public void AchievementsScene()
    {
        SceneManager.LoadScene("ChinaAchievementsScene");
    }
    public void Artifacts()
    {
        SceneManager.LoadScene("ChinaArtifactsScene");
    }
    public void Characters()
    {
        SceneManager.LoadScene("ChinaCharacterCoordinate");
    }
    public void Story()
    {
        SceneManager.LoadScene("ChinaStoryCoordinate");
    }

}
