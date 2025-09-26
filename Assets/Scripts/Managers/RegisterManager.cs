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
    public TMP_InputField codeField;
    public Toggle isTeacherToggle;
    public Toggle termsToggle;
    public TextMeshProUGUI feedbackText;
    public GameObject errorMessagePanel;
    public Button loginButton;
    public Button goBackButton;
    public Button errorBackButton;
    public Button registerButton; // Reference to the register button

    [Header("Loading Spinner")]
    public GameObject loadingSpinner; // Reference to the spinner prefab/GameObject
    public TextMeshProUGUI registerButtonText; // Reference to the button's text component

    private string originalRegisterButtonText = "Register"; // Store original button text

    private void Awake()
    {
        // Store the original button text
        if (registerButtonText != null)
        {
            originalRegisterButtonText = registerButtonText.text;
        }
    }

    private void Start()
    {
        // Ensure spinner is initially hidden
        if (loadingSpinner != null)
        {
            loadingSpinner.SetActive(false);
        }
    }

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
        string code = codeField.text.Trim();

        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
        {
            ShowError("Please enter both first and last names.");
            return;
        }
        else if (!PatternManager.IsValidEmail(email))
        {
            ShowError("Please enter a valid email address.");
            return;
        }
        else if (string.IsNullOrEmpty(email))
        {
            ShowError("Email cannot be empty.");
            return;
        }
        else if (string.IsNullOrEmpty(password))
        {
            ShowError("Password cannot be empty.");
            return;
        }
        else if (!PatternManager.IsValidPassword(password))
        {
            ShowError("Password must be 8-32 characters long, include at least one uppercase letter, one lowercase letter, one number, and one special character.");
            return;
        }
        else if (string.IsNullOrEmpty(confirmPassword))
        {
            ShowError("Please confirm your password.");
            return;
        }
        else if (password != confirmPassword)
        {
            ShowError("Passwords do not match.");
            return;
        }
        else if (string.IsNullOrEmpty(code))
        {
            string codeType = isTeacherToggle.isOn ? "teacher code" : "class code";
            ShowError($"Please enter a valid {codeType}.");
            return;
        }
        else if (!termsToggle.isOn)
        {
            ShowError("You must agree to the terms and conditions.");
            return;
        }

        string fullName = $"{firstName} {lastName}";
        bool isTeacher = isTeacherToggle.isOn;
        
        // Show loading state
        SetLoadingState(true);
        feedbackText.text = "Validating code...";

        // First validate the code before proceeding with registration
        if (isTeacher)
        {
            // Validate teacher code
            FirebaseManager.Instance.ValidateTeacherCode(code, (isValid) =>
            {
                if (!isValid)
                {
                    SetLoadingState(false);
                    ShowError("Invalid teacher code. Please contact your administrator.");
                    return;
                }
                
                // Code is valid, proceed with registration
                ProceedWithRegistration(email, password, fullName, isTeacher, code);
            });
        }
        else
        {
            // Validate class code (student)
            FirebaseManager.Instance.ValidateClassCode(code, (isValid) =>
            {
                if (!isValid)
                {
                    SetLoadingState(false);
                    ShowError("Invalid class code. Please check with your teacher.");
                    return;
                }
                
                // Code is valid, proceed with registration
                ProceedWithRegistration(email, password, fullName, isTeacher, code);
            });
        }
    }

    private void ProceedWithRegistration(string email, string password, string fullName, bool isTeacher, string code)
    {
        feedbackText.text = "Registering...";

        FirebaseManager.Instance.SignUp(email, password, fullName, isTeacher, code, (success, message) =>
        {
            if (!success)
            {
                SetLoadingState(false);
                ShowError(message);
                return;
            }

            UnityDispatcher.RunOnMainThread(() =>
            {
                feedbackText.text = message;
                // Keep loading state until scene loads
                SceneManager.LoadScene("Login");
            });
        });
    }

    private void ShowError(string message)
    {
        errorMessagePanel.SetActive(true);
        feedbackText.text = message;
    }
    
    // Close the error message panel and clear feedback text
    public void ErrorBackButtonClicked()
    {
        errorMessagePanel.SetActive(false);
        feedbackText.text = string.Empty;
    }

    #region Loading State Management

    private void SetLoadingState(bool isLoading)
    {
        // Enable/disable the register button
        if (registerButton != null)
        {
            registerButton.interactable = !isLoading;
        }

        // Show/hide the loading spinner
        if (loadingSpinner != null)
        {
            loadingSpinner.SetActive(isLoading);
        }

        // Update button text
        if (registerButtonText != null)
        {
            registerButtonText.text = isLoading ? "Processing..." : originalRegisterButtonText;
        }
    }

    #endregion
}