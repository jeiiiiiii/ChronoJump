using UnityEngine;
using UnityEngine.SceneManagement;


public class CodexScene : MonoBehaviour
{
    public void Back()
    {
        SceneManager.LoadScene("CoordinateSelect");
    }
    public void AchievementsScene()
    {
        SceneManager.LoadScene("AchievementsScene");
    }
    public void Artifacts()
    {
        SceneManager.LoadScene("ArtifactsScene");
    }
}
