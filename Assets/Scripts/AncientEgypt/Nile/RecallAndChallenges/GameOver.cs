using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

public class GameOverNile : MonoBehaviour
{
    public void TryAgain()
    {
        SceneManager.LoadScene("NileSceneOne");
    }

        public void MainMenu()
    {
        SceneManager.LoadScene("ChapterSelect");
    }

}
