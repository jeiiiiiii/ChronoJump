using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Load the loading scene additively
    /// </summary>
    public async Task Show()
    {
        if (!SceneManager.GetSceneByName("LoadingScene").isLoaded)
        {
            await SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);
        }
    }

    /// <summary>
    /// Unload the loading scene
    /// </summary>
    public async Task Hide()
    {
        if (SceneManager.GetSceneByName("LoadingScene").isLoaded)
        {
            await SceneManager.UnloadSceneAsync("LoadingScene");
        }
    }

    public async Task LoadSceneWithLoading(string targetScene)
{
    // Show spinner scene
    await Show();

    // Load target scene in the background
    var loadOp = SceneManager.LoadSceneAsync(targetScene);
    loadOp.allowSceneActivation = false;

    // Wait until scene is almost ready
    while (loadOp.progress < 0.9f)
    {
        await Task.Yield();
    }

    // Finish load
    loadOp.allowSceneActivation = true;

    // Wait a frame to ensure scene is active
    await Task.Yield();

    // Hide spinner
    await Hide();
}

}
