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
        SceneManager.LoadScene("CreateNewAddFrameScene");
    }
    public void Next()
    {
        SceneManager.LoadScene("ReviewDialogueScene");
    }
}
