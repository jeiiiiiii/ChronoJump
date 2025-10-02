using UnityEngine;
using UnityEngine.SceneManagement;

public class GameScene : MonoBehaviour
{
    public void MainMenu()
    {
        if (IsTeacher())
        {
            SceneManager.LoadScene("Creator'sModeScene");
        }
        else
        {
            SceneManager.LoadScene("Classroom");
        }
    }

    private bool IsTeacher()
    {
        string userRole = PlayerPrefs.GetString("UserRole", "student");
        return userRole.ToLower() == "teacher";
    }
}