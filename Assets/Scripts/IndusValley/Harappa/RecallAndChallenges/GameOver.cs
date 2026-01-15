using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

public class GameOverHarappa : MonoBehaviour
{
    public void TryAgain()
    {
        SceneManager.LoadScene("HarappaSceneOne");
    }

        public void MainMenu()
    {
        SceneManager.LoadScene("ChapterSelect");
    }

}
