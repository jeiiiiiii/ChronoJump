using UnityEngine;
using UnityEngine.SceneManagement;


public class TitleScreen : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("ChapterSelect");
    }

    public void Settings()
    {
        SceneManager.LoadScene("Settings");
    }
}

