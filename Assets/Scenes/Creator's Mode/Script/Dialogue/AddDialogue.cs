using UnityEngine;
using UnityEngine.SceneManagement;

public class AddDialogue : MonoBehaviour
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
        SceneManager.LoadScene("CreateNewAddQuizScene");
    }
}
