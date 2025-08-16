using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

public class GameOverAkkadian : MonoBehaviour
{
    public void TryAgain()
    {
        SceneManager.LoadScene("AkkadianSceneOne");
    }

        public void MainMenu()
    {
        SceneManager.LoadScene("ChapterSelect");
    }

}
