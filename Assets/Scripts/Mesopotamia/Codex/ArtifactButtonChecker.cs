using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ArtifactButtonChecker : MonoBehaviour
{
    void Start()
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