// FIXED: AchievementsUnlocked.cs - Better UI checking
using UnityEngine;
using UnityEngine.UI;

public class AchievementsUnlocked : MonoBehaviour
{
    public string achievementName;
    public GameObject lockedImage;
    public GameObject unlockedImage;

    void Start()
    {
        // FIXED: Wait for GameProgressManager to initialize before checking
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.CurrentStudentState != null)
        {
            CheckAndUpdateUI();
        }
        else
        {
            // Subscribe to initialization complete event
            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.OnInitializationComplete += CheckAndUpdateUI;
            }
            else
            {
                // Fallback: check immediately if no GameProgressManager
                CheckAndUpdateUI();
            }
        }
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.OnInitializationComplete -= CheckAndUpdateUI;
        }
    }

    private void CheckAndUpdateUI()
    {
        bool isUnlocked = PlayerAchievementManager.IsAchievementUnlocked(achievementName);

        if (lockedImage != null) lockedImage.SetActive(!isUnlocked);
        if (unlockedImage != null) unlockedImage.SetActive(isUnlocked);
        
        Debug.Log($"Achievement UI updated for {achievementName}: {(isUnlocked ? "unlocked" : "locked")}");
    }
}