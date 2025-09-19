using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigationManager : MonoBehaviour
{
    public static SceneNavigationManager Instance { get; private set; }

    [Header("Scene Names")]
    [SerializeField] private string creatorModeSceneName = "Creator'sModeScene";
    [SerializeField] private string teacherDashboardSceneName = "TeacherDashboard";
    [SerializeField] private string storyPublishSceneName = "StoryPublish";
    [SerializeField] private string titleScreenSceneName = "TitleScreen";

    private void Awake()
    {
        // Singleton pattern
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
        SceneManager.LoadScene(storyPublishSceneName);
    }

    public void GoToTitleScreen()
    {
        Debug.Log("Navigating to Title Screen...");
        SceneManager.LoadScene(titleScreenSceneName);
    }

    // Method to navigate with custom scene name
    public void NavigateToScene(string sceneName)
    {
        Debug.Log($"Navigating to {sceneName}...");
        SceneManager.LoadScene(sceneName);
    }
}