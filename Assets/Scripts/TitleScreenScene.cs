using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class TitleScreen : MonoBehaviour
{
    [Header("UI References")]
    public Button newGameButton;
    public Button loadGameButton;
    public Button optionsButton;
    public Button classroomButton;
    public Button creatermodeButton;
    public Button logoutButton;
    public TextMeshProUGUI educatorsModeText;



    private void Awake()
    {
        FirebaseManager.Instance.GetUserData(userData =>
            {
                Debug.Log(userData.displayName);
                if (userData == null)
                {
                    Debug.LogError("No user data found. Redirecting to Register.");
                    return;
                }
                else if (userData.role.ToLower() == "teacher")
                {
                    newGameButton.gameObject.SetActive(false);
                    loadGameButton.gameObject.SetActive(false);
                    optionsButton.gameObject.SetActive(true);
                    classroomButton.gameObject.SetActive(true);
                    creatermodeButton.gameObject.SetActive(true);
                    logoutButton.gameObject.SetActive(true);
                    educatorsModeText.gameObject.SetActive(true);
                }
                else if (userData.role.ToLower() == "student")
                {
                    newGameButton.gameObject.SetActive(true);
                    loadGameButton.gameObject.SetActive(true);
                    optionsButton.gameObject.SetActive(true);
                    classroomButton.gameObject.SetActive(false);
                    creatermodeButton.gameObject.SetActive(false);
                    logoutButton.gameObject.SetActive(true);
                    educatorsModeText.gameObject.SetActive(false);
                }
                else
                {
                    Debug.LogError("Unknown user role. Redirecting to Register.");
                    SceneManager.LoadScene("Register");
                }
            });
    }
    public void StartGame()
    {
        SceneManager.LoadScene("ChapterSelect");
    }

    public void Settings()
    {
        SceneManager.LoadScene("Settings");
    }
    public void LoadGame()
    {
        SceneManager.LoadScene("SaveAndLoad");
    }

    public void Classroom()
    {
        SceneManager.LoadScene("TeacherDashboard");
    }

    public void CreatorMode()
    {
        SceneManager.LoadScene("CreatorMode");
    }
    
    public void Logout()
    {
        SceneManager.LoadScene("Login");
    }
}

