using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    [Header("UI References")]
    public Button newGameButton;
    public Button loadGameButton;
    public Button continueButton;
    public Button classroomButton;
    public Button dashboardButton;
    public Button creatormodeButton;
    public Button logoutButton;
    public TextMeshProUGUI educatorsModeText;

    private async void Start()
    {
        // Hide all buttons until role is known
        SetAllButtonsActive(false);

        // Show loading scene/spinner
        await LoadingManager.Instance.Show();

        FirebaseManager.Instance.GetUserData(async userData =>
        {
            if (userData == null)
            {
                Debug.LogError("No user data found. Redirecting to Register.");
                await LoadingManager.Instance.Hide();
                SceneManager.LoadScene("Register");
                return;
            }

            Debug.Log($"[TitleScreen] Loaded user: {userData.displayName} ({userData.role})");

            if (userData.role.ToLower() == "teacher")
            {
                newGameButton.gameObject.SetActive(false);
                loadGameButton.gameObject.SetActive(false);
                classroomButton.gameObject.SetActive(false);
                dashboardButton.gameObject.SetActive(true);
                creatormodeButton.gameObject.SetActive(true);
                logoutButton.gameObject.SetActive(true);
                educatorsModeText.gameObject.SetActive(true);

                await LoadingManager.Instance.Hide();
            }
            else if (userData.role.ToLower() == "student")
            {
                newGameButton.gameObject.SetActive(true);
                loadGameButton.gameObject.SetActive(true);
                classroomButton.gameObject.SetActive(true);
                dashboardButton.gameObject.SetActive(false);
                creatormodeButton.gameObject.SetActive(false);
                logoutButton.gameObject.SetActive(true);
                educatorsModeText.gameObject.SetActive(false);

                // Check student progress before showing Continue
                await CheckStudentProgress(userData.userId);
            }
            else
            {
                Debug.LogError("Unknown user role. Redirecting to Register.");
                await LoadingManager.Instance.Hide();
                SceneManager.LoadScene("Register");
            }
        });
    }

    private void SetAllButtonsActive(bool state)
    {
        newGameButton.gameObject.SetActive(state);
        loadGameButton.gameObject.SetActive(state);
        continueButton.gameObject.SetActive(state);
        classroomButton.gameObject.SetActive(state);
        dashboardButton.gameObject.SetActive(state);
        creatormodeButton.gameObject.SetActive(state);
        logoutButton.gameObject.SetActive(state);
        educatorsModeText.gameObject.SetActive(state);
    }

    public void ContinueGame() => SceneManager.LoadScene("ChapterSelect");

    public void NewGame()
    {
        if (GameProgressManager.Instance.CurrentStudentState == null)
        {
            Debug.LogError("No student is loaded in GameProgressManager. Cannot start a new game.");
            return;
        }

        // Invalidate cache before starting new game
        GameProgressManager.Instance.InvalidateProgressCache();
        
        GameProgressManager.Instance.StartNewGame();
        SceneManager.LoadScene("ChapterSelect");
    }

    public void CreatorMode() => SceneManager.LoadScene("Creator'sModeScene");

    public void Logout()
    {
        SceneManager.LoadScene("Login");
        StudentPrefs.DeleteKey("LastScene");
        StudentPrefs.DeleteKey("SaveSource");
        StudentPrefs.SetString("AccessMode", "LoadOnly");
        StudentPrefs.Save();
    }

    public void Classroom() => SceneManager.LoadScene("Classroom");

    public void LoadGame()
    {
        StudentPrefs.SetString("AccessMode", "LoadOnly");
        StudentPrefs.SetString("SaveSource", "TitleScreen"); 
        StudentPrefs.SetString("LastScene", "TitleScreen");  
        StudentPrefs.Save();

        SceneManager.LoadScene("SaveAndLoadScene");
    }


    public void TeacherDashboard() => SceneManager.LoadScene("TeacherDashboard");

    private async System.Threading.Tasks.Task CheckStudentProgress(string userId)
    {
        bool hasProgress = await GameProgressManager.Instance.HasProgressAsync(userId);
        Debug.Log($"[TitleScreen] Checking HasProgress() for student '{userId}': {hasProgress}");

        continueButton.gameObject.SetActive(hasProgress);

        await LoadingManager.Instance.Hide();
    }
}
