using UnityEngine;
using UnityEngine.SceneManagement;

public class AddFrame : MonoBehaviour
{
    public void MainMenu()
    {
        SceneManager.LoadScene("Creator'sModeScene");
    }
    public void Back()
    {
        SceneManager.LoadScene("CreateNewAddCharacterScene");
    }
    public void Next()
    {
        SceneManager.LoadScene("CreateNewAddQuizScene");
    }
}
