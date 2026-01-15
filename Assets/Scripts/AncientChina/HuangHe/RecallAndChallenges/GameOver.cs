using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

public class GameOverHuangHe : MonoBehaviour
{
    public void TryAgain()
    {
        SceneManager.LoadScene("HuangHeSceneOne");
    }

        public void MainMenu()
    {
        SceneManager.LoadScene("ChapterSelect");
    }

}
