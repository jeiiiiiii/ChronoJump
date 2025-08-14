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

    public void BackButtonClicked()
    {
        SceneManager.LoadScene("LandingPage");
        errorMessagePanel.SetActive(false);
        feedbackText.text = string.Empty;
    }
    
    public void LoginButtonClicked()
    {
        string email = emailField.text.Trim();
        string password = passwordField.text.Trim();

        if (!PatternManager.IsValidEmail(email))
        {
            errorMessagePanel.SetActive(true);
            feedbackText.text = "Please enter a valid email address.";
            return;
        }
        else if (string.IsNullOrEmpty(email))
        {
            errorMessagePanel.SetActive(true);
            feedbackText.text = "Email cannot be empty.";
            return;
        }
        else if (string.IsNullOrEmpty(password))
        {
            errorMessagePanel.SetActive(true);
            feedbackText.text = "Password cannot be empty.";
            return;
        }
        else if (!PatternManager.IsValidPassword(password))
        {
            errorMessagePanel.SetActive(true);
            feedbackText.text = "Password must be 8-32 characters long, include at least one uppercase letter, one lowercase letter, one number, and one special character.";
            return;
        }
        else if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
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
                UnityDispatcher.RunOnMainThread(() =>
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
                        Debug.Log($"User {userData.displayName} logged in as teacher.");
                        feedbackText.text = message;
                        SceneManager.LoadScene("TitleScreen");
                    }
                    else if (!isTeacherToggle.isOn && !isTeacher)
                    {
                        Debug.Log($"User {userData.displayName} logged in as student.");
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

    public void ErrorBackButtonClicked()
    {
        errorMessagePanel.SetActive(false);
        feedbackText.text = string.Empty;
    }
}



