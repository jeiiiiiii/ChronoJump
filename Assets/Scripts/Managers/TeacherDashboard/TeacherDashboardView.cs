using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeacherDashboardView : MonoBehaviour, IDashboardView
{
    [Header("Panels")]
    public GameObject landingPage;
    public GameObject emptyLandingPage;
    public GameObject studentProgressPage;
    public GameObject leaderboardPage;
    public GameObject createNewClassPanel;

    [Header("Dashboard Visibility")]
    public CanvasGroup landingPageCanvasGroup;

    [Header("Teacher Info")]
    public TextMeshProUGUI teacherNameText;
    public Image teacherProfileIcon;
    
    [Header("Title Profile Icons")]
    public Sprite mrIcon;
    public Sprite msIcon;
    public Sprite mrsIcon;
    public Sprite drIcon;

    [Header("Class Selection Display")]
    public TextMeshProUGUI selectedClassCodeText;
    public TextMeshProUGUI selectedClassNameText;
    public ClipboardButton clipboardButton;

    [Header("Empty State Messages")]
    public TextMeshProUGUI studentProgressEmptyText;
    public TextMeshProUGUI leaderboardEmptyText;

    [Header("Student Progress Controls")]
    public GameObject viewAllProgressButton;
    public GameObject deleteStudentButton; // NEW: Delete student button

    [Header("Leaderboard Controls")]
    public GameObject viewAllLeaderboardButton;

    [Header("Others")]
    public GameObject newClassButton;

    public void ShowLandingPage()
    {
        SetActivePanel(landingPage);
        landingPageCanvasGroup.alpha = 0f;
        landingPageCanvasGroup.interactable = false;
        landingPageCanvasGroup.blocksRaycasts = false;
    }

    public void ShowEmptyLandingPage()
    {
        SetActivePanel(emptyLandingPage);
    }

    public void ShowStudentProgressPage()
    {
        SetActivePanel(studentProgressPage);
        SetViewAllProgressButtonVisible(false);
        SetDeleteStudentButtonVisible(false); // NEW: Hide delete button in full view
    }

    public void ShowLeaderboardPage()
    {
        SetActivePanel(leaderboardPage);
        SetViewAllLeaderboardButtonVisible(false);
    }

    public void ShowCreateClassPanel()
    {
        createNewClassPanel.SetActive(true);
    }

    public void UpdateTeacherInfo(string teacherName, Sprite profileIcon)
    {
        if (teacherNameText != null)
            teacherNameText.text = teacherName;

        // Set profile icon based on title if no specific icon is provided
        if (teacherProfileIcon != null)
        {
            if (profileIcon != null)
            {
                teacherProfileIcon.sprite = profileIcon;
            }
            else
            {
                // Extract title from teacherName and set appropriate icon
                teacherProfileIcon.sprite = GetIconForTitle(teacherName);
            }
        }
    }

    private Sprite GetIconForTitle(string teacherName)
    {
        if (string.IsNullOrEmpty(teacherName))
            return null;

        // Extract the title (first word) from the teacher name
        string title = teacherName.Split(' ')[0].ToLower();

        switch (title)
        {
            case "mr":
            case "mr.":
                return mrIcon;
            case "ms":
            case "ms.":
                return msIcon;
            case "mrs":
            case "mrs.":
                return mrsIcon;
            case "dr":
            case "dr.":
                return drIcon;
            default:
                // Return a default icon or null if no match
                return mrIcon; // or return null;
        }
    }

    public void UpdateClassSelection(string classCode, string className)
    {
        if (selectedClassCodeText != null)
            selectedClassCodeText.text = classCode;

        if (selectedClassNameText != null)
            selectedClassNameText.text = className;
        
        if (clipboardButton != null)
            clipboardButton.SetClassCode(classCode);
    }

    public void SetDashboardInteractable(bool interactable)
    {
        if (landingPageCanvasGroup != null)
        {
            landingPageCanvasGroup.alpha = interactable ? 1f : 0f;
            landingPageCanvasGroup.interactable = interactable;
            landingPageCanvasGroup.blocksRaycasts = interactable;
        }
    }

    public void SetProgressEmptyMessages(bool isProgressEmpty)
    {
        if (studentProgressEmptyText != null)
            studentProgressEmptyText.gameObject.SetActive(isProgressEmpty);
    }

    public void SetLeaderboardEmptyMessages(bool isLeaderboardEmpty)
    {
        if (leaderboardEmptyText != null)
            leaderboardEmptyText.gameObject.SetActive(isLeaderboardEmpty);
    }

    public void SetViewAllProgressButtonVisible(bool visible)
    {
        if (viewAllProgressButton != null)
            viewAllProgressButton.SetActive(visible);
    }

    public void SetViewAllLeaderboardButtonVisible(bool visible)
    {
        if (viewAllLeaderboardButton != null)
            viewAllLeaderboardButton.SetActive(visible);
    }

    // NEW: Method to control delete student button visibility
    public void SetDeleteStudentButtonVisible(bool visible)
    {
        if (deleteStudentButton != null)
            deleteStudentButton.SetActive(visible);
    }

    private void SetActivePanel(GameObject activePanel)
    {
        landingPage.SetActive(activePanel == landingPage);
        emptyLandingPage.SetActive(activePanel == emptyLandingPage);
        studentProgressPage.SetActive(activePanel == studentProgressPage);
        leaderboardPage.SetActive(activePanel == leaderboardPage);
        createNewClassPanel.SetActive(false);

        // Control newClassButton visibility
        if (newClassButton != null)
        {
            // Show only on landingPage and emptyLandingPage
            bool shouldShowNewClassButton = (activePanel == landingPage || activePanel == emptyLandingPage);
            newClassButton.SetActive(shouldShowNewClassButton);
        }

        // Handle view-all buttons
        if (activePanel == landingPage)
        {
            SetViewAllProgressButtonVisible(true);
            SetViewAllLeaderboardButtonVisible(true);
            SetDeleteStudentButtonVisible(true); 
        }
        else
        {
            SetViewAllProgressButtonVisible(false);
            SetViewAllLeaderboardButtonVisible(false);
            SetDeleteStudentButtonVisible(false); 
        }
    }
}