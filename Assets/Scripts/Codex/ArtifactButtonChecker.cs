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
            if (PlayerPrefs.GetInt("UseAkkadianArtifactUsed", 0) == 1)
            {
                gameObject.SetActive(false);
            }
        }
        else if (currentScene.Contains("Babylonian"))
        {
            if (PlayerPrefs.GetInt("UseBabylonianArtifactUsed", 0) == 1)
            {
                gameObject.SetActive(false);
            }
        }
        else if (currentScene.Contains("Assyrian"))
        {
            if (PlayerPrefs.GetInt("UseAssyrianArtifactUsed", 0) == 1)
            {
                gameObject.SetActive(false);
            }
        }
    }
}