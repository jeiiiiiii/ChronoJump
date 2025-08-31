using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CoordinateSelect : MonoBehaviour
{
    public void BacktoChapterSelect()
    {
        SceneManager.LoadScene("ChapterSelect");
    }

    public void LoadSumerian()
    {
        // Clear any existing progress data to start fresh
        ClearProgressForNewGame();
        
        // Load the starting scene
        SceneManager.LoadScene("SumerianSceneOne");
    }

    private void ClearProgressForNewGame()
    {
        // Clear all dialogue progress for starting fresh
        PlayerPrefs.DeleteKey("SumerianSceneOne_DialogueIndex");
        PlayerPrefs.DeleteKey("SumerianSceneTwo_DialogueIndex");
        PlayerPrefs.DeleteKey("CurrentScene");
        PlayerPrefs.DeleteKey("LastScene");
        PlayerPrefs.DeleteKey("SaveTimestamp");
        
        // Clear any load flags
        PlayerPrefs.DeleteKey("LoadedDialogueIndex");
        PlayerPrefs.DeleteKey("LoadedFromSave");
        
        // Mark this as a new game start
        PlayerPrefs.SetString("GameMode", "NewGame");
        PlayerPrefs.Save();
        
        Debug.Log("Starting new game - all progress cleared");
    }

    // Optional: Add method for other chapters if you have them
    public void LoadAkkadian()
    {
        ClearProgressForNewGame();
        SceneManager.LoadScene("AkkadianScene"); // Replace with actual scene name
    }

    public void LoadAssyrian()
    {
        ClearProgressForNewGame();
        SceneManager.LoadScene("AssyrianScene"); // Replace with actual scene name
    }

    public void LoadBabylonian()
    {
        ClearProgressForNewGame();
        SceneManager.LoadScene("BabylonianScene"); // Replace with actual scene name
    }
}