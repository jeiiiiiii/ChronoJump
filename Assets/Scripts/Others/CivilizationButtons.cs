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
        UpdateButtonState();
    }

    void OnEnable()
    {
        // Update button state whenever this object becomes active
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        bool isUnlocked = false;
        
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.CurrentStudentState != null)
        {
            // Use the centralized GameProgressManager
            isUnlocked = GameProgressManager.Instance.IsCivilizationUnlocked(civName);
        }
        else
        {
            // Fallback: if no student is logged in, only Sumerian should be unlocked by default
            isUnlocked = (civName == "Sumerian");
            Debug.LogWarning("GameProgressManager not available or no student logged in. Using default unlock state for " + civName);
        }

        // Update UI elements
        button.interactable = isUnlocked;

        if (lockedImage != null) lockedImage.SetActive(!isUnlocked);
        if (unlockedImage != null) unlockedImage.SetActive(isUnlocked);
        if (lockedIcon != null) lockedIcon.SetActive(!isUnlocked);
        if (unlockedIcon != null) unlockedIcon.SetActive(isUnlocked);

        Debug.Log($"Civilization {civName} - Unlocked: {isUnlocked}");
    }

    // Call this method when you want to refresh the button state (e.g., after unlocking something)
    public void RefreshButtonState()
    {
        UpdateButtonState();
    }
}