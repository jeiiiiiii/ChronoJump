using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public TextMeshProUGUI feedbackText;
    public Toggle isTeacherToggle;
    public GameObject errorMessagePanel;
    public Button backButton;
    public void RegisterButtonClicked()
    {
        SceneManager.LoadScene("Register");
    }

    public void LoginButtonClicked()
    {
        string email = emailField.text.Trim();
        string password = passwordField.text.Trim();

        if (!email.Contains("@") || !email.Contains("."))
        {
            errorMessagePanel.SetActive(true);
            feedbackText.text = "Please enter a valid email address.";
            return;
        } else if (string.IsNullOrEmpty(email))
        {
            errorMessagePanel.SetActive(true);
            feedbackText.text = "Email cannot be empty.";
            return;
        } else if (string.IsNullOrEmpty(password))
        {
            errorMessagePanel.SetActive(true);
            feedbackText.text = "Password cannot be empty.";
            return;
        } else if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            errorMessagePanel.SetActive(true);
            feedbackText.text = "Please enter both email and password.";
            return;
        }
        feedbackText.text = "Logging in...";

        FirebaseManager.Instance.SignIn(email, password, (success, message) =>
        {
            if (!success)
            {
                errorMessagePanel.SetActive(true);
                feedbackText.text = message;
                return;
            }
            FirebaseManager.Instance.GetUserData(userData =>
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    if (userData == null)
                    {
                        errorMessagePanel.SetActive(true);
                        feedbackText.text = "No user data found.";
                        return;
                    }

                    bool isTeacher = userData.role.ToLower() == "teacher";

                    if (isTeacherToggle.isOn && isTeacher)
                    {
                        feedbackText.text = message;
                        SceneManager.LoadScene("TeacherDashboard");
                    }
                    else if (!isTeacherToggle.isOn && !isTeacher)
                    {
                        feedbackText.text = message;
                        SceneManager.LoadScene("TitleScreen");
                    }
                    else if (isTeacherToggle.isOn && !isTeacher)
                    {
                        errorMessagePanel.SetActive(true);
                        feedbackText.text = "Role mismatch: Please uncheck the Educator toggle.";
                    }
                    else
                    {
                        errorMessagePanel.SetActive(true);
                        feedbackText.text = "Role mismatch: Please check the Educator toggle.";
                    }
                });
            });
        });
    }

    public void BackButtonClicked()
    {
        errorMessagePanel.SetActive(false);
        feedbackText.text = string.Empty;
    }
}



