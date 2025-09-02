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

    [Header("Empty State Messages")]
    public TextMeshProUGUI studentProgressEmptyText;
    public TextMeshProUGUI leaderboardEmptyText;

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
    }

    public void ShowLeaderboardPage()
    {
        SetActivePanel(leaderboardPage);
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

    public void SetProgressEmptyMessages(bool isProgressEmtpy)
    {
        if (studentProgressEmptyText != null)
            studentProgressEmptyText.gameObject.SetActive(isProgressEmtpy);
    }

    public void SetLeaderboardEmptyMessages(bool isLeaderboardEmpty)
    {
        if (leaderboardEmptyText != null)
            leaderboardEmptyText.gameObject.SetActive(isLeaderboardEmpty);
    }

    private void SetActivePanel(GameObject activePanel)
    {
        landingPage.SetActive(activePanel == landingPage);
        emptyLandingPage.SetActive(activePanel == emptyLandingPage);
        studentProgressPage.SetActive(activePanel == studentProgressPage);
        leaderboardPage.SetActive(activePanel == leaderboardPage);
        createNewClassPanel.SetActive(false);
    }
}