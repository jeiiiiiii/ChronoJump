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
    
    private ClassService _classService;
    private TeacherDashboardManager _dashboardController;
    
    public void Initialize(ClassService classService, TeacherDashboardManager dashboardController)
    {
        _classService = classService;
        _dashboardController = dashboardController;
    }
    
    public void CreateClass()
    {
        Debug.Log("CreateClass method called.");
        string className = classNameInputField.text.Trim();
        string classCodeOption = classCodeOptionDropdown.options[classCodeOptionDropdown.value].text;
        
        if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(classCodeOption))
        {
            _classService.CreateClass(className, classCodeOption, OnClassCreated);
        }
        else
        {
            ShowError("Class name cannot be empty!");
        }
    }
    
    private void OnClassCreated(bool success, string message)
    {
        if (success)
        {
            feedbackText.text = "Class created successfully!";
            ClosePanel();
            _dashboardController.RefreshDashboard();
        }
        else
        {
            ShowError(!string.IsNullOrEmpty(message) ? message : "Failed to create class. Please try again.");
        }
    }
    
    private void ShowError(string message)
    {
        errorMessagePanel.SetActive(true);
        feedbackText.text = message;
    }
    
    public void CancelButtonClicked()
    {
        ClosePanel();
    }
    
    public void ErrorBackButtonClicked()
    {
        errorMessagePanel.SetActive(false);
        feedbackText.text = string.Empty;
    }
    
    private void ClosePanel()
    {
        createClassPanel.SetActive(false);
        classNameInputField.text = "";
        classCodeOptionDropdown.value = 0;
    }
}