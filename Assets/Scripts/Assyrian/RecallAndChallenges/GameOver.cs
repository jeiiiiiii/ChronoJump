using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

public class GameOverAssyrian : MonoBehaviour
{
    public void TryAgain()
    {
        SceneManager.LoadScene("AssyrianSceneOne");
    }

        public void MainMenu()
    {
        SceneManager.LoadScene("ChapterSelect");
    }

}
