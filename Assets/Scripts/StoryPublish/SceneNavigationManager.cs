using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigationManager : MonoBehaviour
{
    public static SceneNavigationManager Instance { get; private set; }
    public static string PreviousSceneName { get; private set; }

    [Header("Scene Names")]
    [SerializeField] private string creatorModeSceneName = "Creator'sModeScene";
    [SerializeField] private string teacherDashboardSceneName = "TeacherDashboard";
    [SerializeField] private string storyPublishSceneName = "StoryPublish";
    [SerializeField] private string titleScreenSceneName = "TitleScreen";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GoToCreatorMode()
    {
        Debug.Log("Navigating to Creator Mode...");
        SceneManager.LoadScene(creatorModeSceneName);
    }

    public void GoToTeacherDashboard()
    {
        Debug.Log("Navigating to Teacher Dashboard...");
        SceneManager.LoadScene(teacherDashboardSceneName);
    }

    public void GoToStoryPublish()
    {
        Debug.Log("Navigating to Story Publish...");
        PreviousSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(storyPublishSceneName);
    }

    public void GoToTitleScreen()
    {
        Debug.Log("Navigating to Title Screen...");
        SceneManager.LoadScene(titleScreenSceneName);
    }

    public void NavigateToScene(string sceneName)
    {
        Debug.Log($"Navigating to {sceneName}...");
        SceneManager.LoadScene(sceneName);
    }
}