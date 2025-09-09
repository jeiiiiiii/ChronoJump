using UnityEngine;
using UnityEngine.SceneManagement;

public class AddTitle : MonoBehaviour
{
    public void MainMenu()
    {
        SceneManager.LoadScene("Creator'sModeScene");
    }
    public void Back()
    {
        SceneManager.LoadScene("Creator'sModeScene");
    }
    public void Next()
    {
        SceneManager.LoadScene("CreateNewAddCharacterScene");
    }
}
