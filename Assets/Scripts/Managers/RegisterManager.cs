using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RegisterManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField firstNameField;
    public TMP_InputField lastNameField;
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public TMP_InputField confirmPasswordField;
    public Toggle isTeacherToggle;
    public Toggle termsToggle;
    public TextMeshProUGUI feedbackText;
    public GameObject errorMessagePanel;
    public Button loginButton;
    public Button goBackButton;
    public Button errorBackButton;

    // Navigate to the login scene
    public void LoginButtonClicked()
    {
        SceneManager.LoadScene("Login");
    }

    // Navigate back to the landing page and reset error messages
    public void BackButtonClicked()
    {
        SceneManager.LoadScene("LandingPage");
        errorMessagePanel.SetActive(false);
        feedbackText.text = string.Empty;
    }
    
    // Validate input fields and attempt to register the user, handling errors and success feedback
    public void RegisterButtonClicked()
    {
        string email = emailField.text.Trim();
        string firstName = firstNameField.text.Trim();
        string lastName = lastNameField.text.Trim();
        string password = passwordField.text.Trim();
        string confirmPassword = confirmPasswordField.text.Trim();

        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
        {
            errorMessagePanel.SetActive(true);
            feedbackText.text = "Please enter both first and last names.";
            return;
        }
        else if (!PatternManager.IsValidEmail(email))
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
        else if (string.IsNullOrEmpty(confirmPassword))
        {
            errorMessagePanel.SetActive(true);
            feedbackText.text = "Please confirm your password.";
            return;
        }
        else if (password != confirmPassword)
        {
            errorMessagePanel.SetActive(true);
            feedbackText.text = "Passwords do not match.";
            return;
        }
        else if (!termsToggle.isOn)
        {
            errorMessagePanel.SetActive(true);
            feedbackText.text = "You must agree to the terms and conditions.";
            return;
        }

        string fullName = $"{firstName} {lastName}";
        bool isTeacher = isTeacherToggle.isOn;
        feedbackText.text = "Registering...";

        FirebaseManager.Instance.SignUp(email, password, fullName, isTeacher, (success, message) =>
        {
            if (!success)
            {
                errorMessagePanel.SetActive(true);
                feedbackText.text = message;
                return;
            }

            UnityDispatcher.RunOnMainThread(() =>
            {
                feedbackText.text = message;
                SceneManager.LoadScene("Login");
            });
        });
    }
    
    // Close the error message panel and clear feedback text
    public void ErrorBackButtonClicked()
    {
        errorMessagePanel.SetActive(false);
        feedbackText.text = string.Empty;
    }
}