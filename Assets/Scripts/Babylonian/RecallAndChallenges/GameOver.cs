using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

public class GameOverBabylonian : MonoBehaviour
{
    public void TryAgain()
    {
        SceneManager.LoadScene("BabylonianSceneOne");
    }

        public void MainMenu()
    {
        SceneManager.LoadScene("ChapterSelect");
    }

}
