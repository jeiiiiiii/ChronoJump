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
        // CRITICAL FIX: Proper singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"📍 SceneNavigationManager created. Initial scene: {SceneManager.GetActiveScene().name}");
            
            // Don't set PreviousSceneName here - wait for actual navigation
        }
        else
        {
            Debug.Log($"🗑️ Destroying duplicate SceneNavigationManager from scene: {SceneManager.GetActiveScene().name}");
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"📍 Scene loaded: {scene.name}, Previous: {PreviousSceneName}");
    }

    // === NAVIGATION METHODS ===

    public void GoToCreatorMode()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene != creatorModeSceneName)
        {
            PreviousSceneName = currentScene;
            Debug.Log($"📝 Previous scene set to: {PreviousSceneName}");
        }
        Debug.Log($"🔄 Navigating to Creator Mode");
        SceneManager.LoadScene(creatorModeSceneName);
    }

    public void GoToTeacherDashboard()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene != teacherDashboardSceneName)
        {
            PreviousSceneName = currentScene;
            Debug.Log($"📝 Previous scene set to: {PreviousSceneName}");
        }
        Debug.Log($"🔄 Navigating to Teacher Dashboard");
        SceneManager.LoadScene(teacherDashboardSceneName);
    }

    public void GoToStoryPublish()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene != storyPublishSceneName)
        {
            PreviousSceneName = currentScene;
            Debug.Log($"📝 Previous scene set to: {PreviousSceneName}");
        }
        Debug.Log($"🔄 Navigating to Story Publish");
        SceneManager.LoadScene(storyPublishSceneName);
    }

    public void GoToTitleScreen()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene != titleScreenSceneName)
        {
            PreviousSceneName = currentScene;
            Debug.Log($"📝 Previous scene set to: {PreviousSceneName}");
        }
        Debug.Log($"🔄 Navigating to Title Screen");
        SceneManager.LoadScene(titleScreenSceneName);
    }

    // === BACK NAVIGATION ===

    public void GoBack()
    {
        if (string.IsNullOrEmpty(PreviousSceneName) || PreviousSceneName == SceneManager.GetActiveScene().name)
        {
            Debug.LogWarning($"⚠️ No valid previous scene. Current: {SceneManager.GetActiveScene().name}, Going to Title Screen");
            GoToTitleScreen();
            return;
        }

        Debug.Log($"🔙 Going back to: {PreviousSceneName}");
        
        // Store the previous scene before loading
        string targetScene = PreviousSceneName;
        
        // Clear previous scene to prevent loops
        PreviousSceneName = null;
        
        // Load the target scene
        SceneManager.LoadScene(targetScene);
    }

    // === MANUAL OVERRIDE ===

    public void SetPreviousSceneManually(string sceneName)
    {
        if (sceneName != SceneManager.GetActiveScene().name)
        {
            PreviousSceneName = sceneName;
            Debug.Log($"📝 Manually set previous scene to: {PreviousSceneName}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Cannot set previous scene to current scene: {sceneName}");
        }
    }

    [ContextMenu("Debug Navigation State")]
    public void DebugNavigationState()
    {
        Debug.Log($"🔍 NAVIGATION STATE: Current={SceneManager.GetActiveScene().name}, Previous={PreviousSceneName}");
    }
}