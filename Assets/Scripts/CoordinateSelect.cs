using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles coordinate/map selection screen navigation and UI feedback.
/// Delegates actual civilization loading to CivilizationLoader.
/// </summary>
public class CoordinateSelect : MonoBehaviour
{
    [Header("UI Feedback")]
    [SerializeField] private GameObject lockedMessagePanel;
    [SerializeField] private Text lockedMessageText;
    [SerializeField] private float messageDisplayTime = 3f;

    [Header("Civilization Loader")]
    [SerializeField] private CivilizationLoader civilizationLoader;

    private void Start()
    {
        // Find CivilizationLoader if not assigned
        if (civilizationLoader == null)
        {
            civilizationLoader = FindFirstObjectByType<CivilizationLoader>();
        }
    }

    #region Scene Navigation
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
    #endregion

    #region Civilization Loading with UI Feedback
    public void LoadSumerian()
    {
        if (CanLoadWithFeedback("Sumerian"))
        {
            ClearProgressForNewGame();
            civilizationLoader.LoadSumerian();
        }
    }

    public void LoadAkkadian()
    {
        if (CanLoadWithFeedback("Akkadian"))
        {
            ClearProgressForNewGame();
            civilizationLoader.LoadAkkadian();
        }
    }

    public void LoadAssyrian()
    {
        if (CanLoadWithFeedback("Assyrian"))
        {
            ClearProgressForNewGame();
            civilizationLoader.LoadAssyrian();
        }
    }

    public void LoadBabylonian()
    {
        if (CanLoadWithFeedback("Babylonian"))
        {
            ClearProgressForNewGame();
            civilizationLoader.LoadBabylonian();
        }
    }

    public void LoadHarappa()
    {
        if (CanLoadWithFeedback("Harappa"))
        {
            ClearProgressForNewGame();
            civilizationLoader.LoadHarappa();
        }
    }
    public void LoadSining()
    {
        if (CanLoadWithFeedback("Sining"))
        {
            ClearProgressForNewGame();
            civilizationLoader.LoadSining();
        }
    }
    public void LoadHuangHe()
    {
        if (CanLoadWithFeedback("HuangHe"))
        {
            ClearProgressForNewGame();
            civilizationLoader.LoadHuangHe();
        }
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Checks if civilization can be loaded and provides UI feedback if not.
    /// </summary>
    private bool CanLoadWithFeedback(string civName)
    {
        // Check if GameProgressManager is available
        if (GameProgressManager.Instance == null || GameProgressManager.Instance.CurrentStudentState == null)
        {
            ShowErrorMessage("Please log in first before accessing civilizations.");
            return false;
        }

        // Use CivilizationLoader's method to check availability
        if (civilizationLoader == null || !civilizationLoader.CanLoadCivilization(civName))
        {
            ShowCivilizationLockedMessage(civName);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Shows UI feedback when a civilization is locked.
    /// </summary>
    private void ShowCivilizationLockedMessage(string civName)
    {
        string message = $"Complete previous civilizations to unlock {civName}!";
        ShowMessage(message);
    }

    /// <summary>
    /// Shows error messages for system issues.
    /// </summary>
    private void ShowErrorMessage(string message)
    {
        ShowMessage(message);
    }

    /// <summary>
    /// Generic method to show messages in UI.
    /// </summary>
    private void ShowMessage(string message)
    {
        if (lockedMessagePanel != null)
        {
            lockedMessagePanel.SetActive(true);
            
            if (lockedMessageText != null)
            {
                lockedMessageText.text = message;
            }
            
            // Auto-hide after specified time
            CancelInvoke(nameof(HideMessage));
            Invoke(nameof(HideMessage), messageDisplayTime);
        }
        
        Debug.Log($"UI Message: {message}");
    }

    /// <summary>
    /// Hides the message panel.
    /// </summary>
    private void HideMessage()
    {
        if (lockedMessagePanel != null)
        {
            lockedMessagePanel.SetActive(false);
        }
    }

    /// <summary>
    /// Clears dialogue progress when starting a new game.
    /// This is CoordinateSelect's specific responsibility.
    /// </summary>
    private void ClearProgressForNewGame()
    {
        // Clear dialogue progress for all scenes using StudentPrefs
        string[] sceneKeys = {
            "SumerianSceneOne_DialogueIndex", "SumerianSceneTwo_DialogueIndex",
            "SumerianSceneThree_DialogueIndex", "SumerianSceneFour_DialogueIndex",
            "SumerianSceneFive_DialogueIndex", "SumerianSceneSix_DialogueIndex",
            "SumerianSceneSeven_DialogueIndex",
            "AkkadianSceneOne_DialogueIndex", "AkkadianSceneTwo_DialogueIndex",
            "AkkadianSceneThree_DialogueIndex", "AkkadianSceneFour_DialogueIndex",
            "AkkadianSceneFive_DialogueIndex", "AkkadianSceneSix_DialogueIndex",
            "BabylonianSceneOne_DialogueIndex", "BabylonianSceneTwo_DialogueIndex",
            "BabylonianSceneThree_DialogueIndex", "BabylonianSceneFour_DialogueIndex",
            "BabylonianSceneFive_DialogueIndex", "BabylonianSceneSix_DialogueIndex",
            "BabylonianSceneSeven_DialogueIndex",
            "AssyrianSceneOne_DialogueIndex", "AssyrianSceneTwo_DialogueIndex",
            "AssyrianSceneThree_DialogueIndex", "AssyrianSceneFour_DialogueIndex",
            "AssyrianSceneFive_DialogueIndex",
            "HarappaSceneOne_DialogueIndex", "HarappaSceneTwo_DialogueIndex",
            "HarappaSceneThree_DialogueIndex", "HarappaSceneFour_DialogueIndex",
            "HarappaSceneFive_DialogueIndex","SiningSceneOne_DialogueIndex",
            "SiningSceneTwo_DialogueIndex", "SiningSceneThree_DialogueIndex",
            "SiningSceneFour_DialogueIndex", "SiningSceneFive_DialogueIndex",
            "HuangheSceneOne_DialogueIndex", "HuangheSceneTwo_DialogueIndex",
            "HuangheSceneThree_DialogueIndex", "HuangheSceneFour_DialogueIndex",
            "HuangheSceneFive_DialogueIndex"
        };

        foreach (string key in sceneKeys)
        {
            StudentPrefs.DeleteKey(key);
        }

        // Clear scene navigation data using StudentPrefs
        StudentPrefs.DeleteKey("CurrentScene");
        StudentPrefs.DeleteKey("LastScene");
        StudentPrefs.DeleteKey("SaveTimestamp");
        StudentPrefs.DeleteKey("LoadedDialogueIndex");
        StudentPrefs.DeleteKey("LoadedFromSave");
        
        // Mark this as a new game start
        StudentPrefs.SetString("GameMode", "NewGame");
        StudentPrefs.Save();
        
        Debug.Log("Starting new game - all dialogue progress cleared from StudentPrefs");
    }
    #endregion
}