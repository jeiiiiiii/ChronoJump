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
    
    [Header("Remember Me Feature")]
    public Toggle rememberMeToggle;

    private FirebaseFirestore _firestore;

    private void Awake()
    {
        _firestore = FirebaseFirestore.DefaultInstance;
    }

    private void Start()
    {
        // Load saved email if Remember Me was enabled
        LoadRememberedEmail();
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
                        
                        // Handle Remember Me for teacher
                        HandleRememberMe(email);
                        
                        feedbackText.text = message;
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
                                    errorMessagePanel.SetActive(true);
                                    feedbackText.text = "Failed to fetch student profile.";
                                    return;
                                }

                                QuerySnapshot snapshot = task.Result;

                                if (snapshot.Count == 0)
                                {
                                    errorMessagePanel.SetActive(true);
                                    feedbackText.text = "No student profile found.";
                                    return;
                                }

                                DocumentSnapshot studDoc = snapshot.Documents.FirstOrDefault();

                                if (studDoc == null || !studDoc.Exists)
                                {
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

                                // Initialize the student's game progress if it doesn't exist
                                if (studentState.GameProgress == null)
                                {
                                    Debug.Log("No GameProgress found for student, creating new one");
                                    studentState.SetGameProgress(new GameProgressModel
                                    {
                                        currentHearts = 3,
                                        unlockedChapters = new List<string> { "CH001" },
                                        unlockedStories = new List<string> { "ST001" },
                                        unlockedAchievements = new List<string>(),
                                        unlockedArtifacts = new List<string>(),
                                        unlockedCivilizations = new List<string> { "Sumerian" },
                                        lastUpdated = Timestamp.GetCurrentTimestamp(),
                                        isRemoved = false
                                    });
                                }

                                feedbackText.text = "Loading game progress...";

                                GameProgressManager.Instance.SetStudentState(studentState, () => {
                                    // This callback runs when GameProgressManager is fully initialized
                                    Debug.Log($"Student {studentState.StudentId} fully initialized, navigating to TitleScreen");

                                    // Handle Remember Me for student
                                    HandleRememberMe(email);

                                    // Clear other students' local data, keep only this student's folder
                                    SaveLoadManager.Instance?.ClearOtherStudentsLocalData(studentState.StudentId);
                                    
                                    // Now it's safe to load the TitleScreen
                                    SceneManager.LoadScene("TitleScreen");
                                });

                            });
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

    // Close the error message panel and clear feedback text
    public void ErrorBackButtonClicked()
    {
        errorMessagePanel.SetActive(false);
        feedbackText.text = string.Empty;
    }

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
}