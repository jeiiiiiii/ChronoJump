using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

public class GameOver : MonoBehaviour
{
    public void TryAgain()
    {
        SceneManager.LoadScene("SumerianSceneOne");
    }

        public void MainMenu()
    {
        SceneManager.LoadScene("ChapterSelect");
    }

}
