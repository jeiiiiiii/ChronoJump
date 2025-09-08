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

    private System.Action<string> _onSave;
    private System.Action _onCancel;

    private void Start()
    {
        saveButton.onClick.AddListener(OnSaveClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
        
        // Hide dialog initially
        if (dialogPanel != null)
            dialogPanel.SetActive(false);
    }

    public void ShowDialog(string currentClassName, System.Action<string> onSave, System.Action onCancel = null)
    {
        classNameInput.text = currentClassName;
        _onSave = onSave;
        _onCancel = onCancel;
        
        if (dialogPanel != null)
            dialogPanel.SetActive(true);
            
        // Focus on input field
        classNameInput.Select();
    }

    private void OnSaveClicked()
    {
        string newClassName = classNameInput.text.Trim();
        
        if (string.IsNullOrEmpty(newClassName))
        {
            Debug.LogWarning("Class name cannot be empty.");
            return;
        }
        
        _onSave?.Invoke(newClassName);
        HideDialog();
    }

    private void OnCancelClicked()
    {
        _onCancel?.Invoke();
        HideDialog();
    }

    private void HideDialog()
    {
        if (dialogPanel != null)
            dialogPanel.SetActive(false);
    }
}