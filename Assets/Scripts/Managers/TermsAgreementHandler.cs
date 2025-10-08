using UnityEngine;
using UnityEngine.UI;

public class TermsAgreementHandler : MonoBehaviour
{
    [Header("UI References")]
    public Toggle agreementToggle;
    public GameObject termsPopup;
    public Button agreeButton;
    public ScrollRect scrollRect;

    private bool hasReachedBottom = false;
    private bool isAgreeProcess = false; // Flag to detect system-initiated toggle change

    void Start()
    {
        termsPopup.SetActive(false);
        agreementToggle.onValueChanged.AddListener(OnToggleChanged);
        agreeButton.onClick.AddListener(OnAgreeClicked);

        agreeButton.interactable = false;
        scrollRect.onValueChanged.AddListener(OnScrollChanged);
    }

    void OnToggleChanged(bool isOn)
    {
        // Ignore programmatic toggle changes
        if (isAgreeProcess)
            return;

        // Only show popup when user tries to CHECK the toggle
        if (isOn)
        {
            // Reset scroll + disable agree button
            scrollRect.verticalNormalizedPosition = 1f;
            agreeButton.interactable = false;
            hasReachedBottom = false;

            // Open popup and uncheck toggle until they fully agree
            termsPopup.SetActive(true);
            agreementToggle.isOn = false;
        }
        // No popup when unchecking
    }

    void OnScrollChanged(Vector2 scrollPos)
    {
        // Enable button when scrolled to bottom
        if (scrollRect.verticalNormalizedPosition <= 0.01f && !hasReachedBottom)
        {
            hasReachedBottom = true;
            agreeButton.interactable = true;
        }
    }

    void OnAgreeClicked()
    {
        // Close popup and set toggle to checked
        termsPopup.SetActive(false);

        // Temporarily mark this as system-driven to prevent re-trigger
        isAgreeProcess = true;
        agreementToggle.isOn = true;
        isAgreeProcess = false;
    }
}
