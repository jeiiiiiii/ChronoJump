using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple civilization scene loader that focuses only on scene loading.
/// UI management and progress clearing are handled by other classes.
/// </summary>
public class CivilizationLoader : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string sumerianStartScene = "SumerianSceneOne";
    [SerializeField] private string akkadianStartScene = "AkkadianSceneOne";
    [SerializeField] private string babylonianStartScene = "BabylonianSceneOne";
    [SerializeField] private string assyrianStartScene = "AssyrianSceneOne";
    [SerializeField] private string harappaStartScene = "HarappaSceneOne";

    public void LoadSumerian()
    {
        LoadCivilizationScene("Sumerian", sumerianStartScene);
    }

    public void LoadAkkadian()
    {
        LoadCivilizationScene("Akkadian", akkadianStartScene);
    }

    public void LoadBabylonian()
    {
        LoadCivilizationScene("Babylonian", babylonianStartScene);
    }

    public void LoadAssyrian()
    {
        LoadCivilizationScene("Assyrian", assyrianStartScene);
    }

    public void LoadHarappa()
    {
        LoadCivilizationScene("Harappa", harappaStartScene);
    }

    /// <summary>
    /// Core method that handles civilization scene loading with proper checks.
    /// This is the single point of truth for loading civilizations.
    /// </summary>
    private void LoadCivilizationScene(string civName, string sceneName)
    {
        // Check if GameProgressManager is available
        if (GameProgressManager.Instance == null || GameProgressManager.Instance.CurrentStudentState == null)
        {
            Debug.LogError($"Cannot load {civName}: No student logged in or GameProgressManager not available!");
            // Let other systems handle the UI feedback (like CoordinateSelect)
            return;
        }

        // Check if civilization is unlocked
        if (!GameProgressManager.Instance.IsCivilizationUnlocked(civName))
        {
            Debug.LogWarning($"Civilization {civName} is locked for student {GameProgressManager.Instance.CurrentStudentState.StudentId}");
            // Let other systems handle the UI feedback
            return;
        }

        // All checks passed - load the scene
        Debug.Log($"Loading {civName} civilization for student {GameProgressManager.Instance.CurrentStudentState.StudentId}");
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Public method to check if a civilization can be loaded.
    /// Other classes can use this for UI updates without duplicating logic.
    /// </summary>
    public bool CanLoadCivilization(string civName)
    {
        if (GameProgressManager.Instance == null || GameProgressManager.Instance.CurrentStudentState == null)
        {
            return false;
        }

        return GameProgressManager.Instance.IsCivilizationUnlocked(civName);
    }
}