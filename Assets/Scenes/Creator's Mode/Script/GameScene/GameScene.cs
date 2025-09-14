using UnityEngine;
using UnityEngine.SceneManagement;

public class GameScene : MonoBehaviour
{
    public void MainMenu()
    {
        SceneManager.LoadScene("Creator'sModeScene");
    }
}
