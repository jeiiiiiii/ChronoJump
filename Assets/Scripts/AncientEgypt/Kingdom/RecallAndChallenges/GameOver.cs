using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

public class GameOverKingdom : MonoBehaviour
{
    public void TryAgain()
    {
        SceneManager.LoadScene("KingdomSceneOne");
    }

        public void MainMenu()
    {
        SceneManager.LoadScene("ChapterSelect");
    }

}
