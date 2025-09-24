using UnityEngine;
using UnityEngine.SceneManagement;

public class LandingPageManager : MonoBehaviour
{
    private void Start()
    {
        // Log where persistent data is stored
        Debug.Log($"[LandingPageManager] Persistent Data Path: {Application.persistentDataPath}");
    }

    public void LoginButtonClicked()
    {
        SceneManager.LoadScene("Login");
    }

    public void RegisterButtonClicked()
    {
        SceneManager.LoadScene("Register");
    }

    public void ExitButtonClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
        Debug.Log("Application exited.");
    }
}
