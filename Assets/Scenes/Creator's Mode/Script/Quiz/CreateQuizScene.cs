using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateQuizScene : MonoBehaviour
{
    public void ReviewQuestions()
    {
        SceneManager.LoadScene("ReviewQuestionsScene");
    }
}
