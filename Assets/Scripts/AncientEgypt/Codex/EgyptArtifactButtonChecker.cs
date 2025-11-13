// FIXED: ArtifactButtonChecker.cs - Use consistent checking
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EgyptArtifactButtonChecker : MonoBehaviour
{
    void Start()
    {
        // FIXED: Wait for GameProgressManager to initialize before checking
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.CurrentStudentState != null)
        {
            CheckArtifactUsage();
        }
        else
        {
            // Subscribe to initialization complete event
            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.OnInitializationComplete += CheckArtifactUsage;
            }
            else
            {
                // Fallback: check immediately
                CheckArtifactUsage();
            }
        }
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.OnInitializationComplete -= CheckArtifactUsage;
        }
    }

    private void CheckArtifactUsage()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        
        if (currentScene.Contains("Akkadian"))
        {
            if (StudentPrefs.GetInt("UseAkkadianArtifactUsed", 0) == 1)
            {
                gameObject.SetActive(false);
            }
        }
        else if (currentScene.Contains("Babylonian"))
        {
            if (StudentPrefs.GetInt("UseBabylonianArtifactUsed", 0) == 1)
            {
                gameObject.SetActive(false);
            }
        }
        else if (currentScene.Contains("Assyrian"))
        {
            if (StudentPrefs.GetInt("UseAssyrianArtifactUsed", 0) == 1)
            {
                gameObject.SetActive(false);
            }
        }
    }
}