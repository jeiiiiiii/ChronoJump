using UnityEngine;
using UnityEngine.SceneManagement;

public class GameScene : MonoBehaviour
{
    public void MainMenu()
    {
        string userRole = PlayerPrefs.GetString("UserRole", "student");
        Debug.Log("=== MainMenu Called ===");
        Debug.Log("UserRole from PlayerPrefs: " + userRole);

        // Check if user is a teacher or student and load appropriate scene
        if (IsTeacher())
        {
            Debug.Log("User is Teacher - Loading Creator'sModeScene");
            SceneManager.LoadScene("Creator'sModeScene");
        }
        else
        {
            Debug.Log("User is Student - Loading Classroom");
            SceneManager.LoadScene("Classroom");
        }
    }

    // Helper method to determine if the current user is a teacher
    private bool IsTeacher()
    {
        // Check the user role saved during login
        string userRole = PlayerPrefs.GetString("UserRole", "student");
        return userRole.ToLower() == "teacher";
    }
}