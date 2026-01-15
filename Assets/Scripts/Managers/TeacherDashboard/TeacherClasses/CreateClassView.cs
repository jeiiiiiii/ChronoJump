using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CreateClassView : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField classNameInputField;
    public GameObject createClassPanel;
    public GameObject errorMessagePanel;
    public TextMeshProUGUI feedbackText;
    public Button createButton;
    public GameObject createButtonLoadingSpinner;

    [Header("Settings")]
    public int maxClassNameLength = 12;

    // NEW: Flag to prevent multiple clicks
    private bool isCreatingClass = false;

    private void Start()
    {
        // Ensure button is enabled on start
        if (createButton != null)
            createButton.interactable = true;
            
        if (createButtonLoadingSpinner != null)
            createButtonLoadingSpinner.SetActive(false);
            
        // Set character limit programmatically
        if (classNameInputField != null)
        {
            classNameInputField.characterLimit = maxClassNameLength;
        }
    }

    public void CreateClass()
    {
        // NEW: Prevent multiple clicks - check flag first!
        if (isCreatingClass)
        {
            Debug.LogWarning("Class creation already in progress. Please wait.");
            return;
        }

        // Check if FirebaseManager and services are ready
        if (FirebaseManager.Instance?.ClassService == null)
        {
            Debug.LogError("FirebaseManager or ClassService is not ready!");
            ShowError("System not ready. Please try again in a moment.");
            return;
        }
        
        // Get dashboard controller when needed
        var dashboardController = FindFirstObjectByType<TeacherDashboardManager>();
        if (dashboardController == null)
        {
            Debug.LogError("TeacherDashboardManager not found!");
            ShowError("System error occurred!");
            return;
        }
        
        // Check UI references
        if (classNameInputField == null || createButton == null)
        {
            Debug.LogError("UI references are not assigned in Inspector!");
            return;
        }

        string className = classNameInputField.text.Trim();
        string classGradeOption = "8"; // Default to grade 8 level
       
        // Validate class name
        if (string.IsNullOrEmpty(className))
        {
            ShowError("Class name cannot be empty!");
            return;
        }

        if (className.Length > maxClassNameLength)
        {
            ShowError($"Class name cannot exceed {maxClassNameLength} characters!");
            return;
        }

        // NEW: Set creating flag and disable UI
        isCreatingClass = true;
        SetUIInteractable(false);

        // Use ClassService directly from FirebaseManager
        FirebaseManager.Instance.ClassService.CreateClass(className, classGradeOption, (success, message) => {
            // NEW: Always reset the state when operation completes
            StartCoroutine(ResetUIState());

            if (success)
            {
                Debug.Log($"Class created successfully: {message}");
                if (feedbackText != null)
                    feedbackText.text = "Class created successfully!";
                    
                // Clear input field
                if (classNameInputField != null)
                    classNameInputField.text = "";
                    
                ClosePanel();

                // Refresh dashboard after class creation
                if (dashboardController != null)
                {
                    dashboardController.RefreshDashboardAndSelectClass(message);
                }
            }
            else
            {
                string errorMessage = !string.IsNullOrEmpty(message) ? message : "Failed to create class. Please try again.";
                ShowError(errorMessage);
                Debug.LogError($"Class creation failed: {errorMessage}");
            }
        });
    }

    // NEW: Proper method to set UI state
    private void SetUIInteractable(bool interactable)
    {
        if (createButton != null)
        {
            createButton.interactable = interactable;
        }
        
        if (classNameInputField != null)
        {
            classNameInputField.interactable = interactable;
        }
        
        if (createButtonLoadingSpinner != null)
        {
            createButtonLoadingSpinner.SetActive(!interactable);
        }
    }

    // NEW: Coroutine to reset UI state (with small delay to prevent rapid clicking)
    private IEnumerator ResetUIState()
    {
        yield return new WaitForSeconds(0.5f); // Small delay
        isCreatingClass = false;
        SetUIInteractable(true);
    }
   
    private void ShowError(string message)
    {
        if (errorMessagePanel != null)
            errorMessagePanel.SetActive(true);
            
        if (feedbackText != null)
            feedbackText.text = message;
        else
            Debug.LogError($"Cannot show error message: {message}");
    }
   
    public void CancelButtonClicked()
    {
        // NEW: Reset state when canceling
        if (isCreatingClass)
        {
            Debug.LogWarning("Canceling class creation in progress...");
        }
        StartCoroutine(ResetUIState());
        ClosePanel();
    }
   
    public void ErrorBackButtonClicked()
    {
        if (errorMessagePanel != null)
            errorMessagePanel.SetActive(false);
            
        if (feedbackText != null)
            feedbackText.text = string.Empty;
            
        // NEW: Reset state
        StartCoroutine(ResetUIState());
    }
   
    private void ClosePanel()
    {
        if (createClassPanel != null)
            createClassPanel.SetActive(false);
            
        if (classNameInputField != null)
            classNameInputField.text = "";
            
        // NEW: Reset state
        StartCoroutine(ResetUIState());
    }

    // NEW: Also reset when panel is enabled/disabled
    private void OnEnable()
    {
        StartCoroutine(ResetUIState());
    }

    private void OnDisable()
    {
        isCreatingClass = false;
        SetUIInteractable(true);
    }
}
