using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using Firebase.Firestore;

public class StudentSelfOverviewManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject overviewPanel; // The main overview panel to show/hide
    public Button closeButton; // Button to go back
    public Button studentNameButton; // The button on the main page that opens this
    public Button editNameButton; // NEW: Button to edit student name

    [Header("Student Info")]
    public TextMeshProUGUI studentNameText;
    public TextMeshProUGUI studentSectionText;

    [Header("Edit Dialog")]
    public EditStudentNameDialog editNameDialog;

    [Header("Filter Buttons")]
    public Button mesopotamiaButton;
    public Button indusButton;
    public Button huangHeButton;
    public Button ehiptoButton;
    public Button publishedStoriesButton;

    [Header("Content Section")]
    public GameObject contentSection;
    public TextMeshProUGUI chapterLabel;
    public TextMeshProUGUI chapterNameText;

    [Header("Story Prefabs")]
    public GameObject storyPrefab; // Use StudentStoryView prefab
    public Transform storiesContent;

    [Header("Loading")]
    public GameObject loadingSpinner;
    public TextMeshProUGUI emptyStateText;

    [Header("References")]
    public ClassInfo classInfoComponent;

    // Current state
    private StudentClassData _currentClass;
    private StudentOverviewData _overviewData;
    private string _currentFilter = "Mesopotamia";
    private bool _isLoading = false;
    private bool _isPageActive = false;

    // Cache
    private StudentOverviewData _cachedOverviewData;
    private string _currentStudentUserId; // NEW: Store current user ID

    // Add this property to fix the currentClass error
    private StudentClassData currentClass => _currentClass;

    private void OnEnable()
{
    StudentClassroomManager.OnStudentDataChanged += OnStudentDataChanged;
    LoadStudentInfoImmediately();
}

private void OnDisable()
{
    StudentClassroomManager.OnStudentDataChanged -= OnStudentDataChanged;
}

private void OnStudentDataChanged(string name, StudentClassData classData)
{
    if (studentNameText != null) 
        studentNameText.text = name;
    
    if (studentSectionText != null && classData != null) 
    {
        if (!string.IsNullOrEmpty(classData.classLevel))
            studentSectionText.text = $"{classData.classLevel} - {classData.className}";
        else
            studentSectionText.text = classData.className;
    }
}

    private void LoadStudentInfoImmediately()
    {
        ClassInfo classInfo = FindAnyObjectByType<ClassInfo>();
        if (classInfo != null && classInfo.IsDataLoaded())
        {
            var classData = classInfo.GetCurrentClassData();
            if (studentSectionText != null && classData != null)
            {
                if (!string.IsNullOrEmpty(classData.classLevel))
                    studentSectionText.text = $"{classData.classLevel} - {classData.className}";
                else
                    studentSectionText.text = classData.className;
            }
        }
    }



    private void Awake()
    {
        // Hide the overview panel initially
        if (overviewPanel != null)
            overviewPanel.SetActive(false);

        // Setup button listeners
        if (studentNameButton != null)
            studentNameButton.onClick.AddListener(ShowOverview);

        if (closeButton != null)
            closeButton.onClick.AddListener(HideOverview);

        // NEW: Setup edit name button
        if (editNameButton != null)
            editNameButton.onClick.AddListener(OnEditNameClicked);

        SetupFilterButtons();
    }

    private void OnDestroy()
    {
        if (studentNameButton != null)
            studentNameButton.onClick.RemoveAllListeners();

        if (closeButton != null)
            closeButton.onClick.RemoveAllListeners();

        // NEW: Remove edit button listener
        if (editNameButton != null)
            editNameButton.onClick.RemoveAllListeners();

        RemoveFilterButtonListeners();
    }

    public void ShowOverview()
    {
        Debug.Log("📊 Opening student self-overview");

        if (classInfoComponent == null || !classInfoComponent.IsDataLoaded())
        {
            Debug.LogError("❌ ClassInfo not ready");
            return;
        }

        _currentClass = classInfoComponent.GetCurrentClassData();

        if (_currentClass == null)
        {
            Debug.LogError("❌ No class data available");
            return;
        }

        // Show the panel
        if (overviewPanel != null)
            overviewPanel.SetActive(true);

        _isPageActive = true;
        _currentFilter = "Mesopotamia";

        // Update student info
        UpdateStudentInfo();

        // Load data
        if (_cachedOverviewData != null)
        {
            Debug.Log("📦 Using cached overview data");
            _overviewData = _cachedOverviewData;
            ShowOverviewData();
        }
        else
        {
            Debug.Log("🔄 Loading fresh overview data");
            LoadStudentOverviewData();
        }
    }

    public void HideOverview()
    {
        Debug.Log("👋 Closing student self-overview");

        _isPageActive = false;
        StopAllCoroutines();

        if (overviewPanel != null)
            overviewPanel.SetActive(false);

        ClearContent();
    }

    private void UpdateStudentInfo()
    {
        if (studentNameText == null)
        {
            Debug.LogWarning("Student name text component not assigned");
            return;
        }

        // Set immediate placeholder values using cached class data
        studentNameText.text = "Loading...";

        // IMMEDIATELY set class section from cached ClassInfo data
        if (studentSectionText != null)
        {
            string formattedClassName = GetFormattedClassNameFromClassInfo();
            studentSectionText.text = formattedClassName;
            Debug.Log($"✅ Immediate class section: {formattedClassName}");
        }

        Debug.Log("🎯 UpdateStudentInfo() called");

        // Load student name from Firebase and store user ID
        LoadStudentNameFromFirebase();
    }


    private string GetFormattedClassNameFromClassInfo()
    {
        if (classInfoComponent == null || _currentClass == null)
            return "8 - Unknown Class";

        string className = _currentClass.className;
        string classCode = _currentClass.classCode;

        // If className is already in "Grade - Section" format, extract just the section
        if (!string.IsNullOrEmpty(className) && className.Contains(" - "))
        {
            var parts = className.Split(new[] { " - " }, StringSplitOptions.None);
            if (parts.Length >= 2)
            {
                return $"8 - {parts[1].Trim()}"; // Always format as "8 - {sectionName}"
            }
        }

        // If we only have a raw class code or name, use it as the section name
        if (!string.IsNullOrEmpty(className))
        {
            return $"8 - {className}";
        }

        // Last resort
        return $"8 - {classCode}";
    }
    


    private void LoadStudentNameFromFirebase()
    {
        FirebaseManager.Instance.GetUserData(userData =>
        {
            if (userData == null)
            {
                SetStudentName("Student");
                return;
            }

            // Store the current user ID
            _currentStudentUserId = userData.userId;

            FirebaseManager.Instance.GetStudentByUserId(userData.userId, studentData =>
            {
                if (studentData == null)
                {
                    SetStudentName(userData.displayName ?? "Student");
                    return;
                }

                SetStudentName(studentData.studName ?? userData.displayName ?? "Student");
            });
        });
    }



    // Keep your existing SetStudentName method
    private void SetStudentName(string name)
    {
        if (studentNameText != null)
        {
            studentNameText.text = name;
            Debug.Log($"✅ Student name set to: {name}");
        }
    }


    private void SetupFilterButtons()
    {
        if (mesopotamiaButton != null)
            mesopotamiaButton.onClick.AddListener(() => OnFilterButtonClicked("Mesopotamia"));

        if (indusButton != null)
            indusButton.onClick.AddListener(() => OnFilterButtonClicked("Indus"));

        if (huangHeButton != null)
            huangHeButton.onClick.AddListener(() => OnFilterButtonClicked("Huang He"));

        if (ehiptoButton != null)
            ehiptoButton.onClick.AddListener(() => OnFilterButtonClicked("Ehipto"));

        if (publishedStoriesButton != null)
            publishedStoriesButton.onClick.AddListener(() => OnFilterButtonClicked("Published Stories"));
    }

    private void RemoveFilterButtonListeners()
    {
        if (mesopotamiaButton != null) mesopotamiaButton.onClick.RemoveAllListeners();
        if (indusButton != null) indusButton.onClick.RemoveAllListeners();
        if (huangHeButton != null) huangHeButton.onClick.RemoveAllListeners();
        if (ehiptoButton != null) ehiptoButton.onClick.RemoveAllListeners();
        if (publishedStoriesButton != null) publishedStoriesButton.onClick.RemoveAllListeners();
    }

    private void OnFilterButtonClicked(string filter)
    {
        if (!_isPageActive)
        {
            Debug.LogWarning("⚠️ Page not active, ignoring filter click");
            return;
        }

        Debug.Log($"📘 Filter clicked: {filter}");

        _currentFilter = filter;
        UpdateContentDisplay();
    }

    private void UpdateContentDisplay()
    {
        if (!_isPageActive) return;

        ClearContent();
        ShowLoading(false);

        if (_currentFilter == "Published Stories")
        {
            HideChapterLabels();
            ShowPublishedStories();
        }
        else
        {
            ShowChapterLabels();
            ShowChapterContent(_currentFilter);
        }
    }

    private void ShowChapterContent(string chapterName)
    {
        Debug.Log($"📖 Showing chapter: {chapterName}");

        if (!_isPageActive) return;

        // Show chapter labels
        if (contentSection != null)
            contentSection.SetActive(true);

        if (chapterLabel != null)
            chapterLabel.text = "Chapter";

        if (chapterNameText != null)
        {
            string formattedName = GetFormattedChapterName(chapterName);
            chapterNameText.text = formattedName;
        }

        // Check if data exists
        if (_overviewData == null || !_overviewData.chapters.ContainsKey(chapterName))
        {
            ShowEmptyState($"No data available for {chapterName}");
            return;
        }

        var chapter = _overviewData.chapters[chapterName];

        if (chapter.stories.Count > 0)
        {
            DisplayStories(chapter.stories);
        }
        else
        {
            ShowEmptyState($"No stories available for {chapterName}");
        }
    }

    private void ShowPublishedStories()
    {
        Debug.Log("📚 Showing published stories");

        if (!_isPageActive) return;

        ClearContent();
        HideChapterLabels();
        ShowLoading(true);

        // Get current student data
        FirebaseManager.Instance.GetUserData(userData =>
        {
            if (!_isPageActive || userData == null)
            {
                ShowLoading(false);
                return;
            }

            // Get student data to access studId
            FirebaseManager.Instance.GetStudentByUserId(userData.userId, studentData =>
            {
                if (!_isPageActive)
                {
                    ShowLoading(false);
                    return;
                }

                FirebaseManager.Instance.GetPublishedStoriesByClass(_currentClass.classCode, stories =>
                {
                    if (!_isPageActive)
                    {
                        ShowLoading(false);
                        return;
                    }

                    if (stories == null || stories.Count == 0)
                    {
                        ShowLoading(false);
                        ShowEmptyState("No published stories available");
                        return;
                    }

                    int processed = 0;
                    int totalStories = stories.Count;
                    var storyProgressList = new List<StoryProgress>();

                    foreach (var entry in stories)
                    {
                        string storyId = entry.Key;
                        var storyData = entry.Value;

                        string title = storyData.ContainsKey("title") ? storyData["title"].ToString() : "Untitled";

                        var storyProgress = new StoryProgress
                        {
                            storyId = storyId,
                            storyTitle = title,
                            storyName = title,
                            quizAttempts = new List<QuizAttempt>()
                        };

                        // Use studId from studentData or fall back to userId
                        string studentStudId = studentData?.studId ?? userData.userId;

                        FirebaseManager.Instance.GetPublishedStoryQuizAttempts(storyId, studentStudId, attempts =>
                        {
                            if (!_isPageActive) return;

                            if (attempts != null && attempts.Count > 0)
                            {
                                foreach (var attemptEntry in attempts)
                                {
                                    ProcessQuizAttempt(attemptEntry.Key, attemptEntry.Value, storyProgress);
                                }

                                storyProgress.quizAttempts = storyProgress.quizAttempts
                                    .OrderBy(a => a.attemptNumber)
                                    .ToList();
                            }

                            storyProgressList.Add(storyProgress);
                            processed++;

                            if (processed >= totalStories)
                            {
                                if (_isPageActive)
                                {
                                    ShowLoading(false);
                                    DisplayStories(storyProgressList.OrderBy(s => s.storyId).ToList());
                                }
                            }
                        });
                    }
                });
            });
        });
    }


    private void LoadStudentOverviewData()
    {
        if (!_isPageActive) return;

        ShowLoading(true);
        ClearContent();

        FirebaseManager.Instance.GetUserData(userData =>
        {
            if (!_isPageActive || userData == null)
            {
                ShowLoading(false);
                return;
            }

            // Get student data from students collection to access studName and studId
            FirebaseManager.Instance.GetStudentByUserId(userData.userId, studentData =>
            {
                if (!_isPageActive) return;

                _overviewData = new StudentOverviewData
                {
                    studentId = userData.userId,
                    // Use studName from studentData
                    studentName = studentData?.studName ?? userData.displayName ?? "Student",
                    classCode = _currentClass.classCode,
                    lastUpdated = DateTime.Now,
                    chapters = new Dictionary<string, ChapterOverview>()
                };

                // Use studId from studentData or fall back to userId
                string studentStudId = studentData?.studId ?? userData.userId;
                LoadAllChapters(studentStudId);
            });
        });
    }


    private void LoadAllChapters(string studentStudId)
    {
        FirebaseManager.Instance.GetAllChapters(chapters =>
        {
            if (!_isPageActive)
            {
                ShowLoading(false);
                return;
            }

            if (chapters == null || chapters.Count == 0)
            {
                ShowLoading(false);
                ShowEmptyState("No chapters available");
                return;
            }

            int totalChapters = chapters.Count;
            int chaptersProcessed = 0;

            foreach (var chapterEntry in chapters)
            {
                string chapterId = chapterEntry.Key;
                var chapterData = chapterEntry.Value;

                var chapterOverview = new ChapterOverview
                {
                    chapterId = chapterId,
                    chapterTitle = chapterData.ContainsKey("chaptTitle") ? chapterData["chaptTitle"].ToString() : "Unknown",
                    chapterName = chapterData.ContainsKey("chaptName") ? chapterData["chaptName"].ToString() : "Unknown",
                    stories = new List<StoryProgress>()
                };

                LoadStoriesForChapter(chapterId, chapterOverview, studentStudId, () =>
                {
                    if (!_isPageActive) return;

                    chaptersProcessed++;

                    if (!_overviewData.chapters.ContainsKey(chapterOverview.chapterName))
                    {
                        _overviewData.chapters[chapterOverview.chapterName] = chapterOverview;
                    }

                    if (chaptersProcessed >= totalChapters)
                    {
                        _cachedOverviewData = _overviewData;
                        ShowOverviewData();
                    }
                });
            }
        });
    }

    private void LoadStoriesForChapter(string chapterId, ChapterOverview chapterOverview, string studentStudId, Action onComplete)
    {
        FirebaseManager.Instance.GetStoriesByChapter(chapterId, stories =>
        {
            if (!_isPageActive)
            {
                onComplete?.Invoke();
                return;
            }

            if (stories == null || stories.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            var sortedStories = stories.OrderBy(s => s.Key).ToList();
            int totalStories = sortedStories.Count;
            int storiesProcessed = 0;

            foreach (var storyEntry in sortedStories)
            {
                string storyId = storyEntry.Key;
                var storyData = storyEntry.Value;

                string storyTitle = storyData.ContainsKey("storyTitle") ? storyData["storyTitle"].ToString() : "";
                string storyName = storyData.ContainsKey("storyName") ? storyData["storyName"].ToString() : "Unknown";

                if (string.IsNullOrEmpty(storyTitle))
                    storyTitle = storyName;

                var storyProgress = new StoryProgress
                {
                    storyId = storyId,
                    storyTitle = storyTitle,
                    storyName = storyName,
                    quizAttempts = new List<QuizAttempt>()
                };

                FirebaseManager.Instance.GetQuizAttemptsByStudentAndStory(studentStudId, storyId, attempts =>
                {
                    if (!_isPageActive) return;

                    if (attempts != null && attempts.Count > 0)
                    {
                        foreach (var attemptEntry in attempts)
                        {
                            ProcessQuizAttempt(attemptEntry.Key, attemptEntry.Value, storyProgress);
                        }

                        storyProgress.quizAttempts = storyProgress.quizAttempts
                            .OrderBy(a => a.attemptNumber)
                            .ToList();
                    }

                    storiesProcessed++;
                    chapterOverview.stories.Add(storyProgress);

                    if (storiesProcessed >= totalStories)
                    {
                        chapterOverview.stories = chapterOverview.stories.OrderBy(s => s.storyId).ToList();
                        onComplete?.Invoke();
                    }
                });
            }
        });
    }

    private void ShowOverviewData()
    {
        if (!_isPageActive)
        {
            ShowLoading(false);
            return;
        }

        Debug.Log($"📊 Showing overview data with {_overviewData.chapters.Count} chapters");

        ShowLoading(false);
        UpdateContentDisplay();
    }

    private void DisplayStories(List<StoryProgress> stories)
    {
        if (!_isPageActive) return;

        ClearContent();

        foreach (var story in stories.OrderBy(s => s.storyId))
        {
            CreateStoryItem(story);
        }

        Debug.Log($"✅ Displayed {stories.Count} stories");
    }

    private void CreateStoryItem(StoryProgress story)
    {
        if (!_isPageActive || storyPrefab == null || storiesContent == null) return;

        GameObject storyItem = Instantiate(storyPrefab, storiesContent);
        var storyView = storyItem.GetComponent<StudentStoryView>();

        if (storyView != null)
        {
            storyView.SetupStory(story);
        }

        storyItem.SetActive(true);
    }

    private void ProcessQuizAttempt(string attemptId, Dictionary<string, object> attemptData, StoryProgress storyProgress)
    {
        var quizAttempt = new QuizAttempt
        {
            attemptId = attemptId,
            attemptNumber = int.Parse(GetValueOrDefault(attemptData, "attemptNumber", 1).ToString()),
            isPassed = bool.Parse(GetValueOrDefault(attemptData, "isPassed", false).ToString()),
            quizId = GetValueOrDefault(attemptData, "quizId", "").ToString(),
            score = int.Parse(GetValueOrDefault(attemptData, "score", 0).ToString())
        };

        var dateCompleted = GetValueOrDefault(attemptData, "dateCompleted", null);
        if (dateCompleted != null)
        {
            if (dateCompleted is Timestamp)
            {
                quizAttempt.dateCompleted = ((Timestamp)dateCompleted).ToDateTime();
            }
            else if (dateCompleted is DateTime)
            {
                quizAttempt.dateCompleted = (DateTime)dateCompleted;
            }
            else if (DateTime.TryParse(dateCompleted.ToString(), out DateTime parsedDate))
            {
                quizAttempt.dateCompleted = parsedDate;
            }
            else
            {
                quizAttempt.dateCompleted = DateTime.Now;
            }
        }
        else
        {
            quizAttempt.dateCompleted = DateTime.Now;
        }

        quizAttempt.remarks = quizAttempt.isPassed ? "Passed" : "Failed";
        quizAttempt.remarksColor = quizAttempt.isPassed ? "#00A33F" : "#D50004";

        storyProgress.quizAttempts.Add(quizAttempt);
    }

    private string GetFormattedChapterName(string chapterName)
    {
        if (_overviewData != null && _overviewData.chapters.ContainsKey(chapterName))
        {
            var chapter = _overviewData.chapters[chapterName];
            return $"{chapter.chapterTitle} ({chapter.chapterName})";
        }
        return chapterName;
    }

    private void ShowLoading(bool show)
    {
        if (loadingSpinner != null)
            loadingSpinner.SetActive(show);

        if (show && emptyStateText != null)
            emptyStateText.gameObject.SetActive(false);
    }

    private void ShowEmptyState(string message)
    {
        if (!_isPageActive) return;

        if (emptyStateText != null)
        {
            emptyStateText.text = message;
            emptyStateText.gameObject.SetActive(true);
        }
    }

    private void HideChapterLabels()
    {
        if (chapterLabel != null)
            chapterLabel.gameObject.SetActive(false);

        if (chapterNameText != null)
            chapterNameText.gameObject.SetActive(false);
    }

    private void ShowChapterLabels()
    {
        if (chapterLabel != null)
            chapterLabel.gameObject.SetActive(true);

        if (chapterNameText != null)
            chapterNameText.gameObject.SetActive(true);
    }

    private void ClearContent()
    {
        if (storiesContent != null)
        {
            foreach (Transform child in storiesContent)
            {
                Destroy(child.gameObject);
            }
        }

        if (emptyStateText != null)
            emptyStateText.gameObject.SetActive(false);
    }

    public void ClearCache()
    {
        _cachedOverviewData = null;
        Debug.Log("🗑️ Cache cleared");
    }

    // NEW: Handle edit name button click
    private void OnEditNameClicked()
    {
        if (editNameDialog == null)
        {
            Debug.LogError("❌ EditStudentNameDialog is not assigned!");
            return;
        }

        if (studentNameText == null)
        {
            Debug.LogError("❌ studentNameText is null!");
            return;
        }

        string currentName = studentNameText.text;
        Debug.Log($"✏️ Opening edit dialog for: {currentName}");

        editNameDialog.ShowDialog(currentName, OnNameSaved);
    }

    private void OnNameSaved(string newName)
    {
        Debug.Log($"💾 Saving new name: {newName}");

        if (string.IsNullOrEmpty(_currentStudentUserId))
        {
            Debug.LogError("❌ No user ID available");
            if (editNameDialog != null)
                editNameDialog.OnSaveComplete(false, "User session expired. Please log in again.");
            return;
        }

        // Update in Firebase
        FirebaseManager.Instance.UpdateStudentName(_currentStudentUserId, newName, success =>
        {
            if (success)
            {
                Debug.Log("✅ Name updated successfully in Firebase");

                // Update UI immediately in overview
                if (studentNameText != null)
                    studentNameText.text = newName;

                // Update the button on main page if it exists
                if (studentNameButton != null)
                {
                    var buttonText = studentNameButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                        buttonText.text = newName;
                }

                // NEW: Trigger the static event
                StudentClassroomManager.OnStudentNameChanged?.Invoke(newName);

                // Clear cache to force reload with new name
                ClearCache();

                // Refresh the leaderboard to show updated name
                RefreshLeaderboard();

                // Notify dialog of success
                if (editNameDialog != null)
                    editNameDialog.OnSaveComplete(true);
            }
            else
            {
                Debug.LogError("❌ Failed to update name in Firebase");

                // Notify dialog of failure
                if (editNameDialog != null)
                    editNameDialog.OnSaveComplete(false, "Failed to save name. Please try again.");
            }
        });
    }


    // Helper method to refresh leaderboard
    private void RefreshLeaderboard()
    {
        // Find the StudentClassroomLeaderboard component in the scene
        var leaderboard = FindFirstObjectByType<StudentClassroomLeaderboard>();
        if (leaderboard != null)
        {
            Debug.Log("🔄 Refreshing leaderboard after name change");
            leaderboard.RefreshLeaderboard();
        }
        else
        {
            Debug.LogWarning("⚠️ StudentClassroomLeaderboard not found in scene");
        }
    }

    private object GetValueOrDefault(Dictionary<string, object> dict, string key, object defaultValue = null)
    {
        if (dict != null && dict.ContainsKey(key))
        {
            return dict[key];
        }
        return defaultValue;
    }
}
