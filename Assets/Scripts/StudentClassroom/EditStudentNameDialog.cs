using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditStudentNameDialog : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField studentNameInput;
    public Button saveButton;
    public Button cancelButton;
    public GameObject dialogPanel;
    
    [Header("Error Display")]
    public GameObject errorMessagePanel;
    public TextMeshProUGUI errorText;

    private System.Action<string> _onSave;
    private System.Action _onCancel;
    private bool _isSaving = false;

    private void Start()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(OnSaveClicked);
        
        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked);
        
        // Hide dialog and error initially
        if (dialogPanel != null)
            dialogPanel.SetActive(false);
            
        if (errorMessagePanel != null)
            errorMessagePanel.SetActive(false);
    }

    public void ShowDialog(string currentStudentName, System.Action<string> onSave, System.Action onCancel = null)
    {
        studentNameInput.text = currentStudentName;
        
        _onSave = onSave;
        _onCancel = onCancel;
        
        if (dialogPanel != null)
            dialogPanel.SetActive(true);
            
        // Hide any previous errors
        ClearError();
        
        // Reset loading state
        SetLoadingState(false);
        
        // Focus on input field
        studentNameInput.Select();
        studentNameInput.ActivateInputField();
    }

    private void OnSaveClicked()
    {
        if (_isSaving)
        {
            Debug.LogWarning("⚠️ Already saving, please wait...");
            return;
        }

        string newStudentName = studentNameInput.text.Trim();
        
        // Validate input
        if (string.IsNullOrEmpty(newStudentName))
        {
            ShowError("Name cannot be empty.");
            return;
        }
        
        // Validate minimum length
        if (newStudentName.Length < 2)
        {
            ShowError("Name must be at least 2 characters.");
            return;
        }
        
        // Validate maximum length (50 characters is reasonable for names)
        if (newStudentName.Length > 50)
        {
            ShowError("Name cannot exceed 50 characters.");
            return;
        }

        // Optional: Validate that name contains only letters, spaces, and common name characters
        if (!IsValidName(newStudentName))
        {
            ShowError("Name can only contain letters, spaces, hyphens, and apostrophes.");
            return;
        }
        
        // Show loading state
        SetLoadingState(true);
        
        _onSave?.Invoke(newStudentName);
    }

    private bool IsValidName(string name)
    {
        // Allow letters (any language), spaces, hyphens, apostrophes, and periods
        foreach (char c in name)
        {
            if (!char.IsLetter(c) && c != ' ' && c != '-' && c != '\'' && c != '.')
            {
                return false;
            }
        }
        return true;
    }

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
        
        Debug.LogWarning($"⚠️ Name validation error: {message}");
    }

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

    // Call this when user starts typing to clear errors
    public void OnStudentNameInputChanged()
    {
        ClearError();
    }

    private void OnCancelClicked()
    {
        if (_isSaving)
        {
            Debug.LogWarning("⚠️ Cannot cancel while saving");
            return;
        }

        ClearError();
        _onCancel?.Invoke();
        HideDialog();
    }

    public void HideDialog()
    {
        if (dialogPanel != null)
            dialogPanel.SetActive(false);
            
        ClearError();
        SetLoadingState(false);
    }

    // Call this method from the manager after successful save
    public void OnSaveComplete(bool success, string errorMessage = "")
    {
        SetLoadingState(false);

        if (success)
        {
            Debug.Log("✅ Student name saved successfully");
            HideDialog();
        }
        else
        {
            ShowError(errorMessage ?? "Failed to save name. Please try again.");
        }
    }

    private void SetLoadingState(bool isLoading)
    {
        _isSaving = isLoading;

        if (saveButton != null)
            saveButton.interactable = !isLoading;

        if (cancelButton != null)
            cancelButton.interactable = !isLoading;

        if (studentNameInput != null)
            studentNameInput.interactable = !isLoading;
    }

    private void OnDestroy()
    {
        if (saveButton != null)
            saveButton.onClick.RemoveAllListeners();
        
        if (cancelButton != null)
            cancelButton.onClick.RemoveAllListeners();
    }
}