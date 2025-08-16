using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CoordinateSelect : MonoBehaviour
{
    public void BacktoChapterSelect()
    {
        SceneManager.LoadScene("ChapterSelect");
    }

    public void LoadSumerian()
    {
        SceneManager.LoadScene("SumerianSceneOne");
    }
    public void LoadChapters()
    {
        SceneManager.LoadScene("ChapterSelect");
    }
    public void LoadCodexScene()
    {
        SceneManager.LoadScene("CodexScene");
    }
    public void LoadAchievementScene()
    {
        SceneManager.LoadScene("AchievementsScene");
    }

}
