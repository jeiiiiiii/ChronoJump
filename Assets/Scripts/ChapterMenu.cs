using UnityEngine;
using UnityEngine.SceneManagement;

public class ChapterMenu : MonoBehaviour
{
    public void Back()
    {
        SceneManager.LoadScene("TitleSCreen");
    }

    public void BacktoMainMenu()
    {
        SceneManager.LoadScene("TitleSCreen");
    }
    
    public void StartChapter()
    {
        SceneManager.LoadScene("CoordinateSelect");
    }
    public void StartIndus()
    {
        SceneManager.LoadScene("IndusCoordinateSelect");
    }
    public void StartHuangHe()
    {
        SceneManager.LoadScene("HuangHeCoordinateSelect");
    }
}


