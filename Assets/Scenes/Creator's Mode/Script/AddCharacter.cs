using UnityEngine;
using UnityEngine.SceneManagement;

public class AddCharacter : MonoBehaviour
{
    public void MainMenu()
    {
        SceneManager.LoadScene("Creator'sModeScene");
    }
    public void Back()
    {
        SceneManager.LoadScene("CreateNewAddTitleScene");
    }
    public void Next()
    {
        SceneManager.LoadScene("CreateNewAddFrameScene");
    }
    public void Try()
    {
        SceneManager.LoadScene("GameScene");
    }
}
