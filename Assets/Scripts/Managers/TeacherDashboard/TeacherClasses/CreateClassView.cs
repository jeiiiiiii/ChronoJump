using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateClassView : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField classNameInputField;
    public TMP_Dropdown classCodeOptionDropdown;
    public GameObject createClassPanel;
    public GameObject errorMessagePanel;
    public TextMeshProUGUI feedbackText;
   
    public void CreateClass()
    {
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
        if (classNameInputField == null || classCodeOptionDropdown == null)
        {
            Debug.LogError("UI references are not assigned in Inspector!");
            return;
        }

        string className = classNameInputField.text.Trim();
        string classCodeOption = classCodeOptionDropdown.options[classCodeOptionDropdown.value].text;
       
        if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(classCodeOption))
        {
            // Use ClassService directly from FirebaseManager
            FirebaseManager.Instance.ClassService.CreateClass(className, classCodeOption, (success, message) => {
                if (success)
                {
                    if (feedbackText != null)
                        feedbackText.text = "Class created successfully!";
                    ClosePanel();

                    // Refresh dashboard after class creation
                    dashboardController.RefreshDashboardAndSelectClass(message);
                }
                else
                {
                    ShowError(!string.IsNullOrEmpty(message) ? message : "Failed to create class. Please try again.");
                }
            });
        }
        else
        {
            ShowError("Class name cannot be empty!");
        }
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
        ClosePanel();
    }
   
    public void ErrorBackButtonClicked()
    {
        if (errorMessagePanel != null)
            errorMessagePanel.SetActive(false);
            
        if (feedbackText != null)
            feedbackText.text = string.Empty;
    }
   
    private void ClosePanel()
    {
        if (createClassPanel != null)
            createClassPanel.SetActive(false);
            
        if (classNameInputField != null)
            classNameInputField.text = "";
            
        if (classCodeOptionDropdown != null)
            classCodeOptionDropdown.value = 0;
    }
}