using UnityEngine;
using UnityEngine.UI;

public class CivilizationButton : MonoBehaviour
{
    public string civName;
    public Button button;
    public GameObject lockedImage;
    public GameObject unlockedImage;
    public GameObject lockedIcon;
    public GameObject unlockedIcon;

    void Start()
    {
        bool isUnlocked = PlayerProgressManager.IsCivilizationUnlocked(civName);

        button.interactable = isUnlocked;

        if (lockedImage != null) lockedImage.SetActive(!isUnlocked);
        if (unlockedImage != null) unlockedImage.SetActive(isUnlocked);
        if (lockedIcon != null) lockedIcon.SetActive(!isUnlocked);
        if (unlockedIcon != null) unlockedIcon.SetActive(isUnlocked);
    }
}
