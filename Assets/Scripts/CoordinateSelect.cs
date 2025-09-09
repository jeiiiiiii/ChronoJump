using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CoordinateSelect : MonoBehaviour
{

    public void LoadChapters()
    {
        SceneManager.LoadScene("ChapterSelect");
    }
    public void LoadCodexScene()
    {
        SceneManager.LoadScene("CodexScene");
    }
    public void LoadAchievementScene()
    {
        SceneManager.LoadScene("AchievementsScene");
    }

    public void LoadSumerian()
    {
        // Clear any existing progress data to start fresh
        ClearProgressForNewGame();
        
        // Load the starting scene
        SceneManager.LoadScene("SumerianSceneOne");
    }

    public void LoadAkkadian()
    {
        ClearProgressForNewGame();
        SceneManager.LoadScene("AkkadianSceneOne"); // Replace with actual scene name
    }

    public void LoadAssyrian()
    {
        ClearProgressForNewGame();
        SceneManager.LoadScene("AssyrianSceneOne"); // Replace with actual scene name
    }

    public void LoadBabylonian()
    {
        ClearProgressForNewGame();
        SceneManager.LoadScene("BabylonianSceneOne"); // Replace with actual scene name
    }

    private void ClearProgressForNewGame()
    {
        // Clear all dialogue progress for starting fresh
        PlayerPrefs.DeleteKey("SumerianSceneOne_DialogueIndex");
        PlayerPrefs.DeleteKey("SumerianSceneTwo_DialogueIndex");
        PlayerPrefs.DeleteKey("SumerianSceneThree_DialogueIndex");
        PlayerPrefs.DeleteKey("SumerianSceneFour_DialogueIndex");
        PlayerPrefs.DeleteKey("SumerianSceneFive_DialogueIndex");
        PlayerPrefs.DeleteKey("SumerianSceneSix_DialogueIndex");
        PlayerPrefs.DeleteKey("SumerianSceneSeven_DialogueIndex");
        PlayerPrefs.DeleteKey("AkkadianSceneOne_DialogueIndex");
        PlayerPrefs.DeleteKey("AkkadianSceneTwo_DialogueIndex");
        PlayerPrefs.DeleteKey("AkkadianSceneThree_DialogueIndex");
        PlayerPrefs.DeleteKey("AkkadianSceneFour_DialogueIndex");
        PlayerPrefs.DeleteKey("AkkadianSceneFive_DialogueIndex");
        PlayerPrefs.DeleteKey("AkkadianSceneSix_DialogueIndex");
        PlayerPrefs.DeleteKey("BabylonianSceneOne_DialogueIndex");
        PlayerPrefs.DeleteKey("BabylonianSceneTwo_DialogueIndex");
        PlayerPrefs.DeleteKey("BabylonianSceneThree_DialogueIndex");
        PlayerPrefs.DeleteKey("BabylonianSceneFour_DialogueIndex");
        PlayerPrefs.DeleteKey("BabylonianSceneFive_DialogueIndex");
        PlayerPrefs.DeleteKey("BabylonianSceneSix_DialogueIndex");
        PlayerPrefs.DeleteKey("BabylonianSceneSeven_DialogueIndex");
        PlayerPrefs.DeleteKey("AssyrianSceneOne_DialogueIndex");
        PlayerPrefs.DeleteKey("AssyrianSceneTwo_DialogueIndex");
        PlayerPrefs.DeleteKey("AssyrianSceneThree_DialogueIndex");
        PlayerPrefs.DeleteKey("AssyrianSceneFour_DialogueIndex");
        PlayerPrefs.DeleteKey("AssyrianSceneFive_DialogueIndex");
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

}