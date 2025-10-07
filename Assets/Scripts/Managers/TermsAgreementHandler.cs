// using UnityEngine;
// using UnityEngine.UI;

// public class TermsAgreementHandler : MonoBehaviour
// {
//     [Header("UI References")]
//     public Toggle agreementToggle;
//     public GameObject termsPopup;
//     public Button agreeButton;
//     public ScrollRect scrollRect; // 👈 Add this

//     private bool hasShownPopup = false;
//     private bool hasReachedBottom = false;

//     void Start()
//     {
//         termsPopup.SetActive(false);
//         agreementToggle.onValueChanged.AddListener(OnToggleChanged);
//         agreeButton.onClick.AddListener(OnAgreeClicked);

//         // Initially disable the Agree button
//         agreeButton.interactable = false;

//         // Listen to scroll changes
//         scrollRect.onValueChanged.AddListener(OnScrollChanged);
//     }

//     void OnToggleChanged(bool isOn)
//     {
//         if (isOn && !hasShownPopup)
//         {
//             termsPopup.SetActive(true);
//             agreementToggle.isOn = false;
//         }
//     }

//     void OnScrollChanged(Vector2 scrollPos)
//     {
//         // scrollRect.verticalNormalizedPosition = 1 when top, 0 when bottom
//         if (scrollRect.verticalNormalizedPosition <= 0.01f && !hasReachedBottom)
//         {
//             hasReachedBottom = true;
//             agreeButton.interactable = true; // Enable button
//         }
//     }

//     void OnAgreeClicked()
//     {
//         hasShownPopup = true;
//         termsPopup.SetActive(false);
//         agreementToggle.isOn = true;
//     }
// }
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
    private bool isAgreeProcess = false; // 🔹 Flag to detect system-initiated toggle change

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
        // Ignore event if we're programmatically changing toggle during the agree process
        if (isAgreeProcess)
            return;

        // Always show popup when user tries to check or uncheck
        if (isOn || !isOn)
        {
            // Reset scroll + disable agree button
            scrollRect.verticalNormalizedPosition = 1f;
            agreeButton.interactable = false;
            hasReachedBottom = false;

            // Open popup and uncheck toggle until they agree
            termsPopup.SetActive(true);
            agreementToggle.isOn = false;
        }
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

        // Temporarily mark this as system-driven
        isAgreeProcess = true;
        agreementToggle.isOn = true;
        isAgreeProcess = false;
    }
}
