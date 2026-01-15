using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationDialog : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Button confirmButton;
    public Button cancelButton;
    public GameObject dialogPanel;

    private System.Action _onConfirm;
    private System.Action _onCancel;

    private void Start()
    {
        confirmButton.onClick.AddListener(OnConfirmClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
        
        // Hide dialog initially
        if (dialogPanel != null)
            dialogPanel.SetActive(false);
    }

    public void ShowDialog(string title, string message, System.Action onConfirm, System.Action onCancel = null)
    {
        titleText.text = title;
        messageText.text = message;
        _onConfirm = onConfirm;
        _onCancel = onCancel;
        
        if (dialogPanel != null)
            dialogPanel.SetActive(true);
    }

    private void OnConfirmClicked()
    {
        _onConfirm?.Invoke();
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
