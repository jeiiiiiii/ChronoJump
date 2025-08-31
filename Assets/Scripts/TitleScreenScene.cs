using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    public void StartGame()
    {
        // Clear any previous save state and mark as new game
        PlayerPrefs.DeleteKey("LastScene");
        PlayerPrefs.DeleteKey("SaveSource");
        PlayerPrefs.SetString("GameMode", "NewGame");
        PlayerPrefs.Save();
        
        SceneManager.LoadScene("ChapterSelect");
    }

    public void Settings()
    {
        SceneManager.LoadScene("Settings");
    }
    
    public void LoadGame()
    {
        // NEW: Clear save source to indicate we're accessing for loading only
        PlayerPrefs.DeleteKey("LastScene");
        PlayerPrefs.DeleteKey("SaveSource");
        PlayerPrefs.SetString("AccessMode", "LoadOnly"); // Mark that we're only here to load
        PlayerPrefs.Save();
        
        Debug.Log("Accessing Save/Load scene for loading only - save buttons will be disabled");
        SceneManager.LoadScene("SaveAndLoadScene");
    }
}