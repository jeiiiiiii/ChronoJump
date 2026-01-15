using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionManager : MonoBehaviour
{
    public void BacktoMainMenu()
    {
        // Check if we came from a story scene (in-game settings button)
        string saveSource = StudentPrefs.GetString("SaveSource", "");
        string lastScene = StudentPrefs.GetString("LastScene", "");

        if (saveSource == "StoryScene" && !string.IsNullOrEmpty(lastScene))
        {
            // We came from a story scene, go back to that scene
            Debug.Log($"Returning to story scene: {lastScene}");

            // Clear the save source flag since we're going back
            StudentPrefs.DeleteKey("SaveSource");
            StudentPrefs.Save();

            SceneManager.LoadScene(lastScene);
        }
        else
        {
            // Default behavior - go to title screen (when accessed from title screen)
            Debug.Log("Returning to Title Screen");
            SceneManager.LoadScene("TitleScreen");
        }
    }

    // Optional: Add a method to explicitly go to title screen if needed
    public void GoToTitleScreen()
    {
        // Clear any story scene flags
        StudentPrefs.DeleteKey("SaveSource");
        StudentPrefs.Save();

        SceneManager.LoadScene("TitleScreen");
    }
}