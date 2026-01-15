using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateQuizScene : MonoBehaviour
{
    public void MainMenu()
    {
        SceneManager.LoadScene("Creator'sModeScene");
    }
    public void Next()
    {
        SceneManager.LoadScene("ReviewQuestionsScene");
    }
    public void BackToDialogue()
    {
        SceneManager.LoadScene("ReviewDialogueScene");
    }
    
}
