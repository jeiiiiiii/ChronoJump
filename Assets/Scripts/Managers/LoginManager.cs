using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;

public class LoginManager : MonoBehaviour
{
    [Header("UI References")]
    public InputField emailField;
    public InputField passwordField;
    public Button loginButton;
    public Button registerButton;
    public Text feedbackText;

    private FirebaseAuth auth;
    private FirebaseFirestore db;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        loginButton.onClick.AddListener(OnLoginButtonClicked);
        registerButton.onClick.AddListener(OnRegisterButtonClicked);
    }

    private void OnLoginButtonClicked()
    {
        string email = emailField.text.Trim();
        string password = passwordField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            feedbackText.text = "Please enter both email and password.";
            return;
        }

        feedbackText.text = "Logging in...";

        auth.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    feedbackText.text = "Login canceled.";
                    return;
                }
                if (task.IsFaulted)
                {
                    feedbackText.text = "Login failed: " + task.Exception.GetBaseException().Message;
                    return;
                }

                FirebaseUser user = task.Result.User;
                feedbackText.text = "Checking role...";

                // Fetch the user's role from Firestore
                DocumentReference docRef = db.Collection("userAccounts").Document(user.UserId);
                docRef.GetSnapshotAsync().ContinueWithOnMainThread(roleTask =>
                {
                    if (roleTask.IsFaulted || !roleTask.Result.Exists)
                    {
                        feedbackText.text = "User role not found.";
                        return;
                    }

                    string role = roleTask.Result.GetValue<string>("role");

                    // Redirect based on role
                    switch (role)
                    {
                        case "Admin":
                            SceneManager.LoadScene("AdminScene");
                            break;
                        case "Teacher":
                            SceneManager.LoadScene("TeacherDashboard");
                            break;
                        case "Student":
                            SceneManager.LoadScene("TitleScreen");
                            break;
                        default:
                            feedbackText.text = "Unknown role.";
                            break;
                    }
                });
            });
    }

    private void OnRegisterButtonClicked()
    {
        SceneManager.LoadScene("Register");
    }
}
