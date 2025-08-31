using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateClassManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField classNameInputField;
    public TMP_Dropdown classCodeOptionDropdown;
    public GameObject createClassPanel;
    public GameObject errorMessagePanel;
    public TextMeshProUGUI feedbackText;

    // Method to handle class creation logic
    public void CreateClass()
    {
        Debug.Log("CreateClass method called.");
        string className = classNameInputField.text.Trim();
        string classCodeOption = classCodeOptionDropdown.options[classCodeOptionDropdown.value].text;

        if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(classCodeOption))
        {
            // Call FirebaseManager to create the class
            FirebaseManager.Instance.CreateClass(className, classCodeOption, (success, message) =>
            {
                if (success)
                {
                    feedbackText.text = "Class created successfully!";
                    createClassPanel.SetActive(false);
                    classNameInputField.text = "";
                    classCodeOptionDropdown.value = 0;
                    SceneManager.LoadScene("TeacherDashboard");
                }
                else
                {
                    errorMessagePanel.SetActive(true);
                    feedbackText.text = !string.IsNullOrEmpty(message) ? message : "Failed to create class. Please try again.";
                }
            });
        }
        else
        {
            errorMessagePanel.SetActive(true);
            feedbackText.text = "Class name cannot be empty!";
        }
    }

    // Method to handle cancel button click
    public void CancelButtonClicked()
    {
        createClassPanel.SetActive(false);
        classNameInputField.text = "";
        classCodeOptionDropdown.value = 0;
    }

    // Method to handle error message panel back button click
    public void ErrorBackButtonClicked()
    {
        errorMessagePanel.SetActive(false);
        feedbackText.text = string.Empty;
    }
}