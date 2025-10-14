using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions; // <- required for ContinueWithOnMainThread
using System.Linq;        // <- required for FirstOrDefault
using System.Collections.Generic;

public class LoginManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public TextMeshProUGUI feedbackText;
    public Toggle isTeacherToggle;
    public GameObject errorMessagePanel;
    public Button backButton;
    public Button loginButton; // Reference to the login button

    [Header("Password Visibility")]
    public Button showPasswordButton;   // 👁 When holding, show password
    public Button hidePasswordButton;   // 👁 When released, hide password



    [Header("Remember Me Feature")]
    public Toggle rememberMeToggle;

    [Header("Loading Spinner")]
    public GameObject loadingSpinner; // Reference to the spinner prefab/GameObject
    public TextMeshProUGUI loginButtonText; // Reference to the button's text component

    private FirebaseFirestore _firestore;
    private string originalLoginButtonText = "Login"; // Store original button text

    private void Awake()
    {
        _firestore = FirebaseFirestore.DefaultInstance;

        // Store the original button text
        if (loginButtonText != null)
        {
            originalLoginButtonText = loginButtonText.text;
        }
    }

    private void Start()
    {
        // Load saved email if Remember Me was enabled
        LoadRememberedEmail();

        // Ensure spinner is initially hidden
        if (loadingSpinner != null)
        {
            loadingSpinner.SetActive(false);
        }
        // 👇 Make sure the password starts hidden
        if (passwordField != null)
        {
            passwordField.contentType = TMP_InputField.ContentType.Password;
            passwordField.ForceLabelUpdate();
        }

        // 👁 Default icon states
        if (showPasswordButton != null)
            showPasswordButton.gameObject.SetActive(true);

        if (hidePasswordButton != null)
            hidePasswordButton.gameObject.SetActive(false);

    }

    // Navigate to the registration scene
    public void RegisterButtonClicked()
    {
        SceneManager.LoadScene("Register");
    }

    // Navigate back to the landing page and reset error messages
    public void BackButtonClicked()
    {
        SceneManager.LoadScene("LandingPage");
        errorMessagePanel.SetActive(false);
        feedbackText.text = string.Empty;
    }

    // Validate input fields and attempt to log in the user, handling role verification and errors    
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

        // Show loading state
        SetLoadingState(true);
        feedbackText.text = "Logging in...";

        FirebaseManager.Instance.SignIn(email, password, (success, message) =>
        {
            if (!success)
            {
                SetLoadingState(false);
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
                        SetLoadingState(false);
                        errorMessagePanel.SetActive(true);
                        feedbackText.text = "No user data found.";
                        return;
                    }

                    bool isTeacher = userData.role.ToLower() == "teacher";

                    if (isTeacherToggle.isOn && isTeacher)
                    {
                        Debug.Log($"User {userData.displayName} logged in as teacher.");

                        PlayerPrefs.SetString("UserRole", "teacher");
                        PlayerPrefs.Save();

                        // Handle Remember Me for teacher
                        HandleRememberMe(email);

                        feedbackText.text = message;

                        // ✅ FORCE FRESH STORIES LOAD FOR TEACHER
                        if (StoryManager.Instance != null)
                        {
                            // Force a fresh load of stories for the new teacher
                            StoryManager.Instance.LoadStories();
                            Debug.Log($"✅ Fresh stories loaded for teacher: {userData.userId}");
                        }

                        // Keep loading state until scene loads
                        SceneManager.LoadScene("TitleScreen");
                    }
                    else if (!isTeacherToggle.isOn && !isTeacher)
                    {
                        Debug.Log($"User {userData.displayName} logged in as student.");
                        feedbackText.text = "Loading student profile...";

                        _firestore.Collection("students")
                            .WhereEqualTo("userId", userData.userId)
                            .Limit(1)
                            .GetSnapshotAsync()
                            .ContinueWithOnMainThread(task =>
                            {
                                if (task.IsFaulted)
                                {
                                    SetLoadingState(false);
                                    errorMessagePanel.SetActive(true);
                                    feedbackText.text = "Failed to fetch student profile.";
                                    return;
                                }

                                QuerySnapshot snapshot = task.Result;

                                if (snapshot.Count == 0)
                                {
                                    SetLoadingState(false);
                                    errorMessagePanel.SetActive(true);
                                    feedbackText.text = "No student profile found.";
                                    return;
                                }

                                DocumentSnapshot studDoc = snapshot.Documents.FirstOrDefault();

                                if (studDoc == null || !studDoc.Exists)
                                {
                                    SetLoadingState(false);
                                    errorMessagePanel.SetActive(true);
                                    feedbackText.text = "No student profile found.";
                                    return;
                                }

                                StudentModel studentModel = studDoc.ConvertTo<StudentModel>();

                                // Create StudentState with proper initialization
                                var studentState = new StudentState(
                                    studentModel.studId,
                                    studentModel.userId,
                                    studentModel
                                );

                                // Ensure GameProgressManager exists
                                if (GameProgressManager.Instance == null)
                                {
                                    GameObject gameManagerGO = new GameObject("GameProgressManager");
                                    gameManagerGO.AddComponent<GameProgressManager>();
                                    DontDestroyOnLoad(gameManagerGO);

                                    Debug.Log("Created GameProgressManager instance for student login");
                                }

                                feedbackText.text = "Loading game progress...";

                                GameProgressManager.Instance.SetStudentState(studentState, () =>
                                {
                                    // Add debug logs to verify data
                                    if (GameProgressManager.Instance.CurrentStudentState != null)
                                    {
                                        var progress = GameProgressManager.Instance.CurrentStudentState.Progress;
                                        Debug.Log($"🎯 Student Progress After Initialization:");
                                        Debug.Log($"   - OverallScore: {progress.overallScore}");
                                        Debug.Log($"   - SuccessRate: {progress.successRate}");
                                    }
    
                                    // This callback runs when GameProgressManager is fully initialized
                                    Debug.Log($"Student {studentState.StudentId} fully initialized, navigating to TitleScreen");

                                    PlayerPrefs.SetString("UserRole", "student");
                                    PlayerPrefs.Save();

                                    // Handle Remember Me for student
                                    HandleRememberMe(email);
                                    
                                    // Now it's safe to load the TitleScreen
                                    SceneManager.LoadScene("TitleScreen");
                                });


                            });
                    }
                    else if (isTeacherToggle.isOn && !isTeacher)
                    {
                        SetLoadingState(false);
                        errorMessagePanel.SetActive(true);
                        feedbackText.text = "Role mismatch: Please uncheck the Educator toggle.";
                    }
                    else
                    {
                        SetLoadingState(false);
                        errorMessagePanel.SetActive(true);
                        feedbackText.text = "Role mismatch: Please check the Educator toggle.";
                    }
                });
            });
        });
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
        // Enable/disable the login button
        if (loginButton != null)
        {
            loginButton.interactable = !isLoading;
        }

        // Show/hide the loading spinner
        if (loadingSpinner != null)
        {
            loadingSpinner.SetActive(isLoading);
        }

        // Update button text
        if (loginButtonText != null)
        {
            loginButtonText.text = isLoading ? "Logging in..." : originalLoginButtonText;
        }
    }

    #endregion

    #region Remember Me Functionality

    private void LoadRememberedEmail()
    {
        // Check if Remember Me was enabled and we have a saved email
        if (PlayerPrefs.HasKey("RememberMe") && PlayerPrefs.GetInt("RememberMe") == 1)
        {
            string savedEmail = PlayerPrefs.GetString("SavedEmail");

            if (!string.IsNullOrEmpty(savedEmail))
            {
                emailField.text = savedEmail;
                rememberMeToggle.isOn = true;

                // Set focus to password field for better UX
                passwordField.Select();
                passwordField.ActivateInputField();
            }
            else
            {
                // Clear invalid saved data
                ClearRememberedEmail();
            }
        }
        else
        {
            // Ensure toggle is off if no remembered email
            rememberMeToggle.isOn = false;
        }
    }

    private void HandleRememberMe(string email)
    {
        if (rememberMeToggle.isOn)
        {
            SaveRememberedEmail(email);
        }
        else
        {
            ClearRememberedEmail();
        }
    }

    private void SaveRememberedEmail(string email)
    {
        PlayerPrefs.SetInt("RememberMe", 1);
        PlayerPrefs.SetString("SavedEmail", email);
        PlayerPrefs.Save();

        Debug.Log("Email saved for Remember Me feature");
    }

    private void ClearRememberedEmail()
    {
        PlayerPrefs.DeleteKey("RememberMe");
        PlayerPrefs.DeleteKey("SavedEmail");
        PlayerPrefs.Save();

        Debug.Log("Remember Me data cleared");
    }

    // Public method to clear remembered email (can be called from a "Clear" button)
    public void ClearRememberedCredentials()
    {
        ClearRememberedEmail();
        emailField.text = string.Empty;
        passwordField.text = string.Empty;
        rememberMeToggle.isOn = false;

        feedbackText.text = "Remembered credentials cleared.";
        Debug.Log("All remembered credentials cleared");
    }

    // Optional: Call this when the Remember Me toggle is changed manually
    public void OnRememberMeToggleChanged()
    {
        // If user unchecks Remember Me, clear the saved email immediately
        if (!rememberMeToggle.isOn)
        {
            // Only clear if we currently have a remembered email loaded
            if (PlayerPrefs.HasKey("RememberMe") && PlayerPrefs.GetInt("RememberMe") == 1)
            {
                // But don't clear the email field, just the saved preference
                PlayerPrefs.DeleteKey("RememberMe");
                PlayerPrefs.Save();
            }
        }
    }
    #endregion

    #region Password Visibility

    public void OnShowPasswordPressed()
    {
        passwordField.contentType = TMP_InputField.ContentType.Standard; // show text
        passwordField.ForceLabelUpdate();

        showPasswordButton.gameObject.SetActive(false);
        hidePasswordButton.gameObject.SetActive(true);
    }

    public void OnShowPasswordReleased()
    {
        passwordField.contentType = TMP_InputField.ContentType.Password; // hide text
        passwordField.ForceLabelUpdate();

        showPasswordButton.gameObject.SetActive(true);
        hidePasswordButton.gameObject.SetActive(false);
    }

    #endregion

}