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

    [Header("Class Selection Display")]
    public TextMeshProUGUI selectedClassCodeText;
    public TextMeshProUGUI selectedClassNameText;
    public ClipboardButton clipboardButton;

    [Header("Empty State Messages")]
    public TextMeshProUGUI studentProgressEmptyText;
    public TextMeshProUGUI leaderboardEmptyText;

    [Header("Student Progress Controls")]
    public GameObject viewAllProgressButton;

    [Header("Leaderboard Controls")]
    public GameObject viewAllLeaderboardButton; 


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

        if (teacherProfileIcon != null && profileIcon != null)
            teacherProfileIcon.sprite = profileIcon;
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


    private void SetActivePanel(GameObject activePanel)
    {
        landingPage.SetActive(activePanel == landingPage);
        emptyLandingPage.SetActive(activePanel == emptyLandingPage);
        studentProgressPage.SetActive(activePanel == studentProgressPage);
        leaderboardPage.SetActive(activePanel == leaderboardPage);
        createNewClassPanel.SetActive(false);

        if (activePanel == landingPage)
        {
            SetViewAllProgressButtonVisible(true);
            SetViewAllLeaderboardButtonVisible(true); // 🔹 Leaderboard button visible on landing
        }
    }

}
