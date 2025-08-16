using UnityEngine;
using UnityEngine.UI;

public class AchievementsUnlocked : MonoBehaviour
{
    public string achievementName;
    public GameObject lockedImage;
    public GameObject unlockedImage;


    void Start()
    {
        bool isUnlocked = PlayerAchievementManager.IsAchievementUnlocked(achievementName);

        if (lockedImage != null) lockedImage.SetActive(!isUnlocked);
        if (unlockedImage != null) unlockedImage.SetActive(isUnlocked);
    }
}
