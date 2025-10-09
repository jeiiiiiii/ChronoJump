using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditClassDialog : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField classNameInput;
    public Button saveButton;
    public Button cancelButton;
    public GameObject dialogPanel;
    
    [Header("Error Display")]
    public GameObject errorMessagePanel; // Add this reference
    public TextMeshProUGUI errorText;    // Add this reference

    private System.Action<string> _onSave;
    private System.Action _onCancel;

    private void Start()
    {
        saveButton.onClick.AddListener(OnSaveClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
        
        // Hide dialog and error initially
        if (dialogPanel != null)
            dialogPanel.SetActive(false);
            
        if (errorMessagePanel != null)
            errorMessagePanel.SetActive(false);
    }

    public void ShowDialog(string currentClassName, System.Action<string> onSave, System.Action onCancel = null)
    {
        // FIX: Extract only the actual class name part (remove the "8 - " prefix)
        string displayName = ExtractClassNameOnly(currentClassName);
        classNameInput.text = displayName;
        
        _onSave = onSave;
        _onCancel = onCancel;
        
        if (dialogPanel != null)
            dialogPanel.SetActive(true);
            
        // Hide any previous errors
        if (errorMessagePanel != null)
            errorMessagePanel.SetActive(false);
            
        // Focus on input field
        classNameInput.Select();
    }

    // NEW: Method to extract only the class name part
    private string ExtractClassNameOnly(string fullClassName)
    {
        if (string.IsNullOrEmpty(fullClassName))
            return fullClassName;

        // If the format is "8 - ClassName", we want to extract "ClassName"
        // Split by " - " and take the last part
        string[] parts = fullClassName.Split(new string[] { " - " }, System.StringSplitOptions.None);
        
        if (parts.Length >= 2)
        {
            // Return everything after the first " - "
            return parts[1];
        }
        
        // If no " - " is found, return the original string
        return fullClassName;
    }

    private void OnSaveClicked()
    {
        string newClassName = classNameInput.text.Trim();
        
        if (string.IsNullOrEmpty(newClassName))
        {
            ShowError("Class name cannot be empty.");
            return;
        }
        
        // NEW: Validate character limit (12 characters)
        if (newClassName.Length > 12)
        {
            ShowError("Class name cannot exceed 12 characters.");
            return;
        }
        
        _onSave?.Invoke(newClassName);
        HideDialog();
    }

    // NEW: Method to show error messages in UI
    private void ShowError(string message)
    {
        if (errorMessagePanel != null)
        {
            errorMessagePanel.SetActive(true);
        }
        
        if (errorText != null)
        {
            errorText.text = message;
        }
        else
        {
            Debug.LogError($"Error message: {message}");
        }
    }

    // NEW: Method to clear errors
    private void ClearError()
    {
        if (errorMessagePanel != null)
        {
            errorMessagePanel.SetActive(false);
        }
        
        if (errorText != null)
        {
            errorText.text = string.Empty;
        }
    }

    // NEW: Clear error when user starts typing
    public void OnClassNameInputChanged()
    {
        ClearError();
    }

    private void OnCancelClicked()
    {
        ClearError();
        _onCancel?.Invoke();
        HideDialog();
    }

    private void HideDialog()
    {
        if (dialogPanel != null)
            dialogPanel.SetActive(false);
            
        ClearError();
    }
}