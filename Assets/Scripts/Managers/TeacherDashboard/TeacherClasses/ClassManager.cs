using UnityEngine;

public class ClassManager : MonoBehaviour
{
    [Header("UI References")]
    public ClassListView classListView;
    public ConfirmationDialog confirmationDialog;
    public EditClassDialog editClassDialog;
    
    [Header("Events")]
    public System.Action OnClassDeleted;
    public System.Action OnClassEdited;

    private ClassService classService;

    private void Start()
    {
        // Get the ClassService from FirebaseManager
        if (FirebaseManager.Instance != null)
        {
            classService = FirebaseManager.Instance.ClassService;
        }
        else
        {
            Debug.LogError("FirebaseManager.Instance is null. Make sure FirebaseManager is initialized.");
        }

        // Subscribe to class list view events
        if (classListView != null)
        {
            classListView.OnDeleteClassRequested += HandleDeleteClassRequest;
            classListView.OnEditClassRequested += HandleEditClassRequest;
        }
        else
        {
            Debug.LogWarning("ClassListView reference is missing in ClassManager.");
        }
    }

    private void HandleDeleteClassRequest(string classCode, string className)
{
    if (confirmationDialog != null)
    {
        string message = $"Warning: Permanent Deletion\n\n" +
                         $"You are about to delete '{className}' and all student accounts within it. " +
                         $"Once deleted, this data cannot be recovered.\n\n" +
                         $"Are you certain you want to delete this class?";
        
        confirmationDialog.ShowDialog(
            "Delete Class", 
            message,
            () => DeleteClass(classCode, className)
        );
    }
}


    private void HandleEditClassRequest(string classCode, string className)
    {
        classCode = classCode.Trim();

        // Extract only the section name after the dash
        if (className.Contains("-"))
        {
            // Split into ["8 ", " Sectionname"], then take the second part
            className = className.Split('-')[1].Trim();
        }

        if (editClassDialog != null)
        {
            editClassDialog.ShowDialog(
                className,
                (newClassName) => EditClass(classCode, newClassName)
            );
        }
    }


    private void DeleteClass(string classCode, string className)
    {
        if (classService == null)
        {
            Debug.LogError("ClassService is not available.");
            ShowMessage("Error", "Service unavailable. Please try again.");
            return;
        }

        // Show loading or disable UI here if needed
        Debug.Log($"Starting deletion process for class {className} ({classCode})...");

        classService.DeleteClass(classCode, (success, message) =>
        {
            if (success)
            {
                Debug.Log($"Class {className} deleted successfully.");
                
                // Remove from UI
                if (classListView != null)
                    classListView.RemoveClassFromList(classCode);
                
                // Notify other systems about the deletion
                OnClassDeleted?.Invoke();
                
                ShowMessage("Success", "Class and all associated student data have been permanently deleted.");
            }
            else
            {
                Debug.LogError($"Failed to delete class: {message}");
                ShowMessage("Error", $"Failed to delete class: {message}");
            }
        });
    }

    private void EditClass(string classCode, string newClassName)
    {
        if (classService == null)
        {
            Debug.LogError("ClassService is not available.");
            ShowMessage("Error", "Service unavailable. Please try again.");
            return;
        }

        classService.EditClassName(classCode, newClassName, (success, message) =>
        {
            if (success)
            {
                Debug.Log($"Class name updated successfully to: {newClassName}");
                
                // Refresh the class list view to show updated name
                RefreshClassList();
                
                // Notify other systems about the edit
                OnClassEdited?.Invoke();
                
                ShowMessage("Success", "Class name updated successfully.");
            }
            else
            {
                Debug.LogError($"Failed to update class name: {message}");
                ShowMessage("Error", $"Failed to update class name: {message}");
            }
        });
    }

    private void RefreshClassList()
    {
        // You'll need to implement this method to reload the class list
        // This would typically call your method that fetches and displays classes
        // For example, if you have a method called LoadTeacherClasses:
        // LoadTeacherClasses();
        
        Debug.Log("RefreshClassList called - implement class list refresh logic here");
        
        // You can also trigger any events here if other components need to know about the refresh
        OnClassEdited?.Invoke();
    }

    private void ShowMessage(string title, string message)
    {
        if (confirmationDialog != null)
        {
            confirmationDialog.ShowDialog(title, message, null);
        }
        else
        {
            Debug.Log($"{title}: {message}");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (classListView != null)
        {
            classListView.OnDeleteClassRequested -= HandleDeleteClassRequest;
            classListView.OnEditClassRequested -= HandleEditClassRequest;
        }
    }
}