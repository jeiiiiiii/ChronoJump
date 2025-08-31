using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class TeacherDashboardManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button createNewClassButton;
    public Button viewStudentProgressButton;
    public Button viewLeaderboardButton;

    [Header("Panels")]
    public GameObject LandingPage;
    public GameObject EmptyLandingPage;
    public GameObject StudentProgressPage;
    public GameObject LeaderboardPage;
    public GameObject CreateNewClassPanel;

    [Header("Dashboard Visibility")]
    public CanvasGroup landingPageCanvasGroup;

    [Header("Texts and Icons")]
    public TextMeshProUGUI teacherNameText;
    public Image teacherProfileIcon;

    [Header("Class List")]
    public GameObject classRowPrefab;
    public Transform classListContent;

    [Header("Class Detail Display")]
    public TextMeshProUGUI selectedClassCodeText;
    public TextMeshProUGUI selectedClassNameText;

    [Header("Colors")]
    public Color defaultTextColor = new Color32(75, 85, 99, 255);   // #4B5563
    public Color selectedTextColor = new Color32(37, 99, 235, 255); // #2563EB

    [Header("Icons")]
    public Sprite defaultIcon;
    public Sprite selectedIcon;

    private Button lastSelectedButton;
    public TeacherModel teacherData;

    [Header("Student Progress")]
    public GameObject studentProgressPrefab;
    public Transform studentProgressList;

    private void Awake()
    {
        FirebaseManager.Instance.GetUserData(userData =>
        {
            if (userData == null)
            {
                Debug.LogError("No user data found.");
                return;
            }
            else if (userData.role.ToLower() == "teacher")
            {
                FirebaseManager.Instance.GetTeacherData(userData.userId, teacherData =>
                {
                    if (teacherData == null)
                    {
                        Debug.LogError("No teacher data found.");
                        return;
                    }
                    this.teacherData = teacherData;
                    Debug.Log($"Welcome, {teacherData.teachLastName}!");
                    Debug.Log($"You have {teacherData.classCode.Count} classes.");

                    teacherNameText.text = teacherData.title + " " + teacherData.teachLastName;

                    if (teacherData.classCode == null || teacherData.classCode.Count == 0)
                    {
                        LandingPage.SetActive(false);
                        EmptyLandingPage.SetActive(true);
                        StudentProgressPage.SetActive(false);
                        LeaderboardPage.SetActive(false);
                        CreateNewClassPanel.SetActive(false);
                    }
                    else
                    {
                        LandingPage.SetActive(true);
                        EmptyLandingPage.SetActive(false);
                        StudentProgressPage.SetActive(false);
                        LeaderboardPage.SetActive(false);
                        CreateNewClassPanel.SetActive(false);

                        landingPageCanvasGroup.alpha = 0f;
                        landingPageCanvasGroup.interactable = false;
                        landingPageCanvasGroup.blocksRaycasts = false;

                        foreach (Transform child in classListContent)
                        {
                            Destroy(child.gameObject);
                        }

                        // Show all classes in the scroll view
                        foreach (var classEntry in teacherData.classCode)
                        {
                            GameObject row = Instantiate(classRowPrefab, classListContent);
                            TextMeshProUGUI rowText = row.GetComponentInChildren<TextMeshProUGUI>();
                            if (rowText != null)
                            {
                                rowText.text = classEntry.Value[0] + " - " + classEntry.Value[1];
                            }

                            Button classButton = row.GetComponent<Button>();
                            classButton.transition = Selectable.Transition.ColorTint;
                            string classCode = classEntry.Key;
                            string className = classEntry.Value[0] + " - " + classEntry.Value[1];

                            classButton.onClick.AddListener(() =>
                                ClassButtonClicked(classButton, classCode, className));

                            row.SetActive(true);
                        }
                        // Auto-select the first button in the scroll view
                        if (classListContent.childCount > 0)
                        {
                            Button firstButton = classListContent.GetChild(0).GetComponent<Button>();
                            if (firstButton != null)
                            {
                                string classCode = teacherData.classCode.Keys.First();
                                string className = teacherData.classCode[classCode][0] + " - " + teacherData.classCode[classCode][1];

                                ClassButtonClicked(firstButton, classCode, className);
                            }
                        }

                        // Show students' progress for the first class by default
                        FirebaseManager.Instance.GetStudentsInClass(teacherData.classCode.Keys.First(), students =>
                        {
                            foreach (Transform child in studentProgressList)
                            {
                                Destroy(child.gameObject);
                            }

                            foreach (var student in students)
                            {
                                GameObject studentRow = Instantiate(studentProgressPrefab, studentProgressList);
                                Component[] allComponents = studentRow.GetComponentsInChildren<Component>(true);
                                foreach (var comp in allComponents)
                                {
                                    Debug.Log($"Found component: {comp.GetType().Name} on {comp.gameObject.name}");
                                }
                                studentRow.SetActive(true);
                            }
                        });

                        StartCoroutine(ShowDashboardAfterRender());
                    }
                });
            }
            else
            {
                Debug.LogError("User is not a teacher.");
            }
        });
    }

    public void BacktoMainMenu()
    {
        SceneManager.LoadScene("TitleScreen");
    }

    // Class button selection logic
    private void ClassButtonClicked(Button clickedButton, string classCode, string className)
    {
        if (lastSelectedButton != null && lastSelectedButton != clickedButton)
        {
            ResetButton(lastSelectedButton);
        }

        HighlightButton(clickedButton);

        selectedClassCodeText.text = classCode;
        selectedClassNameText.text = className;

        lastSelectedButton = clickedButton;
    }

    private void HighlightButton(Button button)
    {
        var colors = button.colors;
        colors.normalColor = new Color(1, 1, 1, 1);
        button.colors = colors;

        var bg = button.GetComponent<Image>();
        if (bg != null) bg.color = new Color32(239, 246, 255, 255);

        var txt = button.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null) txt.color = selectedTextColor;

        var iconTransform = button.transform.Find("ClassIcon");
        if (iconTransform != null)
        {
            var icon = iconTransform.GetComponentInChildren<Image>();
            if (icon != null && selectedIcon != null)
                icon.sprite = selectedIcon;
        }
    }

    private void ResetButton(Button button)
    {
        var colors = button.colors;
        colors.normalColor = new Color(1, 1, 1, 0);
        button.colors = colors;

        var bg = button.GetComponent<Image>();
        if (bg != null) bg.color = new Color32(239, 246, 255, 255);

        var txt = button.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null) txt.color = defaultTextColor;

        var iconTransform = button.transform.Find("ClassIcon");
        if (iconTransform != null)
        {
            var icon = iconTransform.GetComponentInChildren<Image>();
            if (icon != null && defaultIcon != null)
                icon.sprite = defaultIcon;
        }
    }


    private IEnumerator ShowDashboardAfterRender()
    {
        yield return new WaitForEndOfFrame();

        // Make dashboard visible
        landingPageCanvasGroup.alpha = 1f;
        landingPageCanvasGroup.interactable = true;
        landingPageCanvasGroup.blocksRaycasts = true;
    }

    public void ShowCreateNewClassPanel()
    {
        CreateNewClassPanel.SetActive(true);
    }
}
