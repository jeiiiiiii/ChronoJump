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
    public GameObject studentOverviewPage;

    [Header("Loading Prefabs")]
    public GameObject classListLoadingSpinnerPrefab;
    public GameObject teacherInfoLoadingSpinnerPrefab;

    [Header("Loading Parent Transforms")]
    public Transform classListLoadingParent;
    public Transform teacherInfoLoadingParent;

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
    public GameObject deleteStudentButton;
    public Button landingRefreshButton; // ADD THIS - refresh button on landing page

    [Header("Leaderboard Controls")]
    public GameObject viewAllLeaderboardButton;

    [Header("Full Page Controls")]
    public Button fullProgressRefreshButton; // ADD THIS - refresh button on full progress page

    [Header("Others")]
    public GameObject newClassButton;
    public GameObject publishedStoriesButton;

    [Header("Dashboard Title")]
    public TextMeshProUGUI dashboardTitleText; // Assign in Inspector

    // Runtime spinner instances
    private GameObject _classListSpinnerInstance;
    private GameObject _teacherInfoSpinnerInstance;

    // Add this new method to update the title
    public void UpdateDashboardTitle(string title)
    {
        if (dashboardTitleText != null)
        {
            dashboardTitleText.text = title;
            Debug.Log($"📝 Dashboard title updated to: {title}");
        }
        else
        {
            Debug.LogWarning("Dashboard title text reference is missing!");
        }
    }

    private void Start()
    {
        CreateLoadingSpinners();
        SetLoadingState(true);
    }

    private void CreateLoadingSpinners()
    {
        // Create class list spinner
        if (classListLoadingSpinnerPrefab != null && classListLoadingParent != null)
        {
            _classListSpinnerInstance = Instantiate(classListLoadingSpinnerPrefab, classListLoadingParent);
            _classListSpinnerInstance.name = "ClassListLoadingSpinner";

            RectTransform rect = _classListSpinnerInstance.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
            }

            _classListSpinnerInstance.SetActive(false);
        }

        // Create teacher info spinner
        if (teacherInfoLoadingSpinnerPrefab != null && teacherInfoLoadingParent != null)
        {
            _teacherInfoSpinnerInstance = Instantiate(teacherInfoLoadingSpinnerPrefab, teacherInfoLoadingParent);
            _teacherInfoSpinnerInstance.name = "TeacherInfoLoadingSpinner";

            RectTransform rect = _teacherInfoSpinnerInstance.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
            }

            _teacherInfoSpinnerInstance.SetActive(false);
        }

        Debug.Log("✅ Loading spinners created from prefabs");
    }

    public void SetLoadingState(bool isLoading)
    {
        if (_classListSpinnerInstance != null)
        {
            _classListSpinnerInstance.SetActive(isLoading);
        }

        if (_teacherInfoSpinnerInstance != null)
        {
            _teacherInfoSpinnerInstance.SetActive(isLoading);
        }

        SetDashboardInteractable(!isLoading);
        Debug.Log($"🔄 TeacherDashboardView Loading: {(isLoading ? "SHOWING" : "HIDING")}");
    }

    public void SetPartialLoadingState(bool classListLoading, bool teacherInfoLoading)
    {
        if (_classListSpinnerInstance != null)
        {
            _classListSpinnerInstance.SetActive(classListLoading);
        }

        if (_teacherInfoSpinnerInstance != null)
        {
            _teacherInfoSpinnerInstance.SetActive(teacherInfoLoading);
        }
    }

    private void OnDestroy()
    {
        if (_classListSpinnerInstance != null)
        {
            Destroy(_classListSpinnerInstance);
        }
        if (_teacherInfoSpinnerInstance != null)
        {
            Destroy(_teacherInfoSpinnerInstance);
        }
    }

    public void ShowLandingPage()
    {
        SetActivePanel(landingPage);

        // ✅ Show the page immediately, just disable interaction during loading
        landingPageCanvasGroup.alpha = 1f;           // MAKE VISIBLE
        landingPageCanvasGroup.interactable = false; // But not interactive yet
        landingPageCanvasGroup.blocksRaycasts = false;

        Debug.Log("✅ Landing page shown (visible but non-interactive)");
    }


    public void ShowEmptyLandingPage()
    {
        SetActivePanel(emptyLandingPage);

        // ✅ Also make empty page visible immediately
        var emptyCanvasGroup = emptyLandingPage.GetComponent<CanvasGroup>();
        if (emptyCanvasGroup != null)
        {
            emptyCanvasGroup.alpha = 1f;
            emptyCanvasGroup.interactable = false;
            emptyCanvasGroup.blocksRaycasts = false;
        }
    }


    // Update the SetDashboardInteractable method:
    public void SetDashboardInteractable(bool interactable)
    {
        if (landingPageCanvasGroup != null)
        {
            // ✅ Keep alpha at 1f, only change interactivity
            landingPageCanvasGroup.alpha = 1f;  // ALWAYS VISIBLE
            landingPageCanvasGroup.interactable = interactable;
            landingPageCanvasGroup.blocksRaycasts = interactable;
        }

        // ✅ Also handle empty landing page
        if (emptyLandingPage != null)
        {
            var emptyCanvasGroup = emptyLandingPage.GetComponent<CanvasGroup>();
            if (emptyCanvasGroup != null)
            {
                emptyCanvasGroup.alpha = 1f;
                emptyCanvasGroup.interactable = interactable;
                emptyCanvasGroup.blocksRaycasts = interactable;
            }
        }

        Debug.Log($"🔄 Dashboard interactable: {interactable}");
    }


    public void ShowStudentProgressPage()
    {
        SetActivePanel(studentProgressPage);
        SetViewAllProgressButtonVisible(false);
        SetDeleteStudentButtonVisible(false);
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

        if (teacherProfileIcon != null)
        {
            if (profileIcon != null)
            {
                teacherProfileIcon.sprite = profileIcon;
            }
            else
            {
                teacherProfileIcon.sprite = GetIconForTitle(teacherName);
            }
        }

        // Hide teacher info loading when data is populated
        if (_teacherInfoSpinnerInstance != null)
        {
            _teacherInfoSpinnerInstance.SetActive(false);
        }
    }

    private Sprite GetIconForTitle(string teacherName)
    {
        if (string.IsNullOrEmpty(teacherName))
            return null;

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
                return mrIcon;
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

        // Hide class list loading when class is selected
        if (_classListSpinnerInstance != null)
        {
            _classListSpinnerInstance.SetActive(false);
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

    public void SetDeleteStudentButtonVisible(bool visible)
    {
        if (deleteStudentButton != null)
            deleteStudentButton.SetActive(visible);
        
    }

// NEW: Show student overview page
public void ShowStudentOverviewPage()
{
    Debug.Log("🎯 TEACHER DASHBOARD VIEW: ShowStudentOverviewPage called");
    if (studentOverviewPage != null)
    {
        SetActivePanel(studentOverviewPage);
        Debug.Log("✅ Overview page activated via SetActivePanel");
    }
    else
    {
        Debug.LogError("❌ studentOverviewPage is NULL in ShowStudentOverviewPage!");
    }
}

// NEW: Hide all pages (for overview page management)
public void HideAllPages()
{
    Debug.Log("📁 TEACHER DASHBOARD VIEW: HideAllPages called");
    
    landingPage.SetActive(false);
    emptyLandingPage.SetActive(false);
    studentProgressPage.SetActive(false);
    leaderboardPage.SetActive(false);
    createNewClassPanel.SetActive(false);
    
    // NEW: Also hide overview page
    if (studentOverviewPage != null)
    {
        Debug.Log("👁️ Hiding studentOverviewPage");
        studentOverviewPage.SetActive(false);
    }
    else
    {
        Debug.LogError("❌ studentOverviewPage is NULL in HideAllPages!");
    }
}

    private void SetActivePanel(GameObject activePanel)
    {
        Debug.Log($"📄 Setting active panel: {activePanel?.name}");

        landingPage.SetActive(activePanel == landingPage);
        emptyLandingPage.SetActive(activePanel == emptyLandingPage);
        studentProgressPage.SetActive(activePanel == studentProgressPage);
        leaderboardPage.SetActive(activePanel == leaderboardPage);
        createNewClassPanel.SetActive(false);

        // Handle overview page
        if (studentOverviewPage != null)
        {
            studentOverviewPage.SetActive(activePanel == studentOverviewPage);
        }

        // ✅ NEW: Update dashboard title based on active panel
        if (activePanel == studentProgressPage)
        {
            UpdateDashboardTitle("Student Progress");
        }
        else if (activePanel == leaderboardPage)
        {
            UpdateDashboardTitle("Student Leaderboards");
        }
        else if (activePanel == studentOverviewPage)
        {
            UpdateDashboardTitle("Student Overview");
        }
        else if (activePanel == landingPage || activePanel == emptyLandingPage)
        {
            UpdateDashboardTitle("Teacher's Dashboard");
        }

        if (newClassButton != null)
        {
            bool shouldShowNewClass = (activePanel == landingPage || activePanel == emptyLandingPage);
            newClassButton.SetActive(shouldShowNewClass);
        }

        // ✅ NEW: LANDING PAGE REFRESH BUTTON
        if (landingRefreshButton != null)
        {
            landingRefreshButton.gameObject.SetActive(activePanel == landingPage);
        }

        // ✅ NEW: FULL PROGRESS PAGE REFRESH BUTTON
        if (fullProgressRefreshButton != null)
        {
            fullProgressRefreshButton.gameObject.SetActive(activePanel == studentProgressPage);
        }

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

    // NEW: Method to enable/disable landing refresh button
    public void SetLandingRefreshButtonEnabled(bool enabled)
    {
        if (landingRefreshButton != null)
        {
            landingRefreshButton.interactable = enabled;
            Debug.Log($"🔘 Landing refresh button: {(enabled ? "ENABLED" : "DISABLED")}");
        }
    }

    // NEW: Method to enable/disable full progress refresh button
    public void SetFullProgressRefreshButtonEnabled(bool enabled)
    {
        if (fullProgressRefreshButton != null)
        {
            fullProgressRefreshButton.interactable = enabled;
            Debug.Log($"🔘 Full progress refresh button: {(enabled ? "ENABLED" : "DISABLED")}");
        }
    }
}