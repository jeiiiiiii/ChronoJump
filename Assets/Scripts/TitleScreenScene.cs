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
    public Button publishedStoriesButton;
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
                // ✅ Ensure teacher data is loaded before initializing StoryManager
                await LoadTeacherData(userData.userId);
                
                newGameButton.gameObject.SetActive(false);
                loadGameButton.gameObject.SetActive(false);
                classroomButton.gameObject.SetActive(false);
                dashboardButton.gameObject.SetActive(true);
                creatormodeButton.gameObject.SetActive(true);
                logoutButton.gameObject.SetActive(true);
                educatorsModeText.gameObject.SetActive(true);
                publishedStoriesButton.gameObject.SetActive(true);

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
                publishedStoriesButton.gameObject.SetActive(false);

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
        if (newGameButton != null)
            newGameButton.gameObject.SetActive(state);
        else
            Debug.LogError("[TitleScreen] newGameButton is NULL!");

        if (loadGameButton != null)
            loadGameButton.gameObject.SetActive(state);
        else
            Debug.LogError("[TitleScreen] loadGameButton is NULL!");

        if (continueButton != null)
            continueButton.gameObject.SetActive(state);
        else
            Debug.LogError("[TitleScreen] continueButton is NULL!");

        if (classroomButton != null)
            classroomButton.gameObject.SetActive(state);
        else
            Debug.LogError("[TitleScreen] classroomButton is NULL!");

        if (dashboardButton != null)
            dashboardButton.gameObject.SetActive(state);
        else
            Debug.LogError("[TitleScreen] dashboardButton is NULL!");

        if (creatormodeButton != null)
            creatormodeButton.gameObject.SetActive(state);
        else
            Debug.LogError("[TitleScreen] creatormodeButton is NULL!");

        if (logoutButton != null)
            logoutButton.gameObject.SetActive(state);
        else
            Debug.LogError("[TitleScreen] logoutButton is NULL!");

        if (educatorsModeText != null)
            educatorsModeText.gameObject.SetActive(state);
        else
            Debug.LogError("[TitleScreen] educatorsModeText is NULL!");
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
        // ✅ Clear StoryManager data on logout
        if (StoryManager.Instance != null)
        {
            StoryManager.Instance.ClearStoriesForNewTeacher();
            StoryManager.Instance.ClearCurrentTeacher();
        }

        SceneManager.LoadScene("Login");
        StudentPrefs.DeleteKey("LastScene");
        StudentPrefs.DeleteKey("SaveSource");
        StudentPrefs.SetString("AccessMode", "LoadOnly");
        StudentPrefs.Save();

        Debug.Log("✅ Teacher data cleared on logout");
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


    public void OnPublishedStoriesClicked()
    {
        Debug.Log("Published Stories button clicked!");
        if (SceneNavigationManager.Instance != null)
            SceneNavigationManager.Instance.GoToStoryPublish();
        else
            SceneManager.LoadScene("StoryPublish");
    }

    private async System.Threading.Tasks.Task LoadTeacherData(string userId)
    {
        var completionSource = new System.Threading.Tasks.TaskCompletionSource<bool>();

        FirebaseManager.Instance.GetTeacherData(userId, teacher =>
        {
            if (teacher != null)
            {
                Debug.Log($"✅ Teacher data loaded in TitleScreen: {teacher.teachFirstName} {teacher.teachLastName} (ID: {teacher.teachId})");

                // Ensure StoryManager uses the correct teachId
                if (StoryManager.Instance != null)
                {
                    StoryManager.Instance.SetCurrentTeacher(teacher.teachId);
                }

                completionSource.SetResult(true);
            }
            else
            {
                Debug.LogError("❌ Failed to load teacher data in TitleScreen");
                completionSource.SetResult(false);
            }
        });

        await completionSource.Task;
    }

 

}