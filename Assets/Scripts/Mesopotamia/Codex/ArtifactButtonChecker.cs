// FIXED: ArtifactButtonChecker.cs - Use consistent checking
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ArtifactButtonChecker : MonoBehaviour
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
        else if (currentScene.Contains("Harappa"))
        {
            if (StudentPrefs.GetInt("UseHarappaArtifactUsed", 0) == 1)
            {
                gameObject.SetActive(false);
            }
        }
        else if (currentScene.Contains("Sining"))
        {
            if (StudentPrefs.GetInt("UseSiningArtifactUsed", 0) == 1)
            {
                gameObject.SetActive(false);
            }
        }
        else if (currentScene.Contains("Kingdom"))
        {
            if (StudentPrefs.GetInt("UseKingdomArtifactUsed", 0) == 1)
            {
                gameObject.SetActive(false);
            }
        }
        else if (currentScene.Contains("Nile"))
        {
            if (StudentPrefs.GetInt("UseNileArtifactUsed", 0) == 1)
            {
                gameObject.SetActive(false);
            }
        }
        else if (currentScene.Contains("HuangHe"))
        {
            if (StudentPrefs.GetInt("UseHuangHeArtifactUsed", 0) == 1)
            {
                gameObject.SetActive(false);
            }
        }
        else if (currentScene.Contains("Shang"))
        {
            if (StudentPrefs.GetInt("UseShangArtifactUsed", 0) == 1)
            {
                gameObject.SetActive(false);
            }
        }
    }
}