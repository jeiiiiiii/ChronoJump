using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Firebase.Firestore;
using System.Collections;

public class StudentOverviewPage : MonoBehaviour
{
    [Header("Student Info")]
    public TextMeshProUGUI studentNameText;
    public TextMeshProUGUI studentSectionText;

    [Header("Filter Buttons")]
    public Button mesopotamiaButton;
    public Button indusButton;
    public Button huangHeButton;
    public Button ehiptoButton;
    public Button publishedStoriesButton;
    public Button refreshButton;

    [Header("Content Section")]
    public GameObject contentSection;
    public TextMeshProUGUI chapterLabel;
    public TextMeshProUGUI chapterNameText;

    [Header("Story Prefabs")]
    public GameObject storyPrefab;
    public Transform storiesContent;

    [Header("Loading")]
    public GameObject loadingSpinner;
    public TextMeshProUGUI emptyStateText;

    // Current state
    private StudentModel _currentStudent;
    private string _currentClassCode;
    private StudentOverviewData _overviewData;
    private string _currentFilter = "Mesopotamia";
    private bool _isLoading = false;

    // Cache
    private Dictionary<string, StudentOverviewData> _studentOverviewCache = new Dictionary<string, StudentOverviewData>();

    // References
    private TeacherDashboardManager _dashboardManager;

    // Safety flag to prevent callbacks when page is inactive
    private bool _isPageActive = false;

    // Simple operation tracking
    private string _currentStudentId;

    private bool IsCurrentStudent(string studentId)
    {
        return _currentStudentId == studentId;
    }

    public void SetDashboardManager(TeacherDashboardManager manager)
    {
        _dashboardManager = manager;
    }

    private void OnEnable()
    {
        _isPageActive = true;
        Debug.Log("✅ StudentOverviewPage.OnEnable() - Page is now active.");
    }

    private void OnDisable()
    {
        Debug.Log("❌ StudentOverviewPage.OnDisable() - Page is now inactive.");
        ResetPage();
    }

    public void SetupStudentOverview(StudentModel student)
    {
        Debug.Log($"📄 === SETTING UP NEW STUDENT: {student.studName} ===");

        // 1. Stop all previous asynchronous operations
        StopAllCoroutines();
        CancelInvoke();

        // 2. Perform an immediate and full reset of the page state and UI
        ResetPage();

        // 3. CRITICAL: Ensure GameObject is active
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("⚠️ GameObject was inactive, activating now!");
            gameObject.SetActive(true);
        }

        // 4. Mark the page as active for the new setup
        _isPageActive = true;

        // 5. Set current student ID for operation tracking
        _currentStudentId = student.userId;

        // 6. Set the new student's data
        _currentStudent = student;
        _currentClassCode = student.classCode;
        _currentFilter = "Mesopotamia"; // Default filter

        // 7. Update static UI elements
        if (studentNameText != null)
            studentNameText.text = student.studName;

        if (studentSectionText != null)
        {
            studentSectionText.text = GetClassNameFromCode(student.classCode);
        }

        // 8. Set up filter button listeners
        SetupFilterButtons();

        // 9. Begin loading data
        ShowLoading(true);

        // Check cache
        if (_studentOverviewCache.ContainsKey(student.userId) && _studentOverviewCache[student.userId] != null)
        {
            Debug.Log($"📦 Loading from cache for: {student.studName}");
            _overviewData = _studentOverviewCache[student.userId];
            ShowOverviewData();
        }
        else
        {
            Debug.Log($"🔄 Loading fresh data for: {student.studName}");
            LoadStudentOverviewData();
        }
    }

    private string GetClassNameFromCode(string classCode)
    {
        if (_dashboardManager != null)
        {
            var dashboardState = _dashboardManager.GetDashboardState();
            if (dashboardState != null)
            {
                if (!string.IsNullOrEmpty(dashboardState.selectedClassName))
                {
                    return dashboardState.selectedClassName;
                }

                if (dashboardState.teacherData?.classCode?.ContainsKey(classCode) == true)
                {
                    var classData = dashboardState.teacherData.classCode[classCode];
                    if (classData != null && classData.Count >= 2)
                    {
                        return $"{classData[0]} - {classData[1]}";
                    }
                }
            }
        }

        return classCode;
    }

    private void SetupFilterButtons()
    {
        if (mesopotamiaButton != null)
        {
            mesopotamiaButton.onClick.RemoveAllListeners();
            mesopotamiaButton.onClick.AddListener(() => OnFilterButtonClicked("Mesopotamia"));
        }

        if (indusButton != null)
        {
            indusButton.onClick.RemoveAllListeners();
            indusButton.onClick.AddListener(() => OnFilterButtonClicked("Indus"));
        }

        if (huangHeButton != null)
        {
            huangHeButton.onClick.RemoveAllListeners();
            huangHeButton.onClick.AddListener(() => OnFilterButtonClicked("Huang He"));
        }

        if (ehiptoButton != null)
        {
            ehiptoButton.onClick.RemoveAllListeners();
            ehiptoButton.onClick.AddListener(() => OnFilterButtonClicked("Ehipto"));
        }

        if (publishedStoriesButton != null)
        {
            publishedStoriesButton.onClick.RemoveAllListeners();
            publishedStoriesButton.onClick.AddListener(() => OnFilterButtonClicked("Published Stories"));
        }

        if (refreshButton != null)
        {
            refreshButton.onClick.RemoveAllListeners();
            refreshButton.onClick.AddListener(OnRefreshButtonClicked);
        }
    }

    private bool _isTransitioning = false;

    private void OnFilterButtonClicked(string filter)
    {
        // Prevent filter clicks during transitions
        if (_isTransitioning)
        {
            Debug.LogWarning("⚠️ Filter click ignored - page is transitioning");
            return;
        }

        Debug.Log($"🔘 === FILTER CLICKED: {filter} ===");

        if (!_isPageActive)
        {
            Debug.LogWarning("⚠️ Page not active, ignoring click");
            return;
        }

        // Allow clicking same filter to refresh
        if (_currentFilter == filter && !_isLoading)
        {
            Debug.Log($"ℹ️ Same filter clicked, refreshing: {filter}");
        }

        // Cancel ALL ongoing operations
        StopAllCoroutines();
        _isLoading = false;

        _currentFilter = filter;
        
        UpdateContentDisplay();
    }

    private void UpdateContentDisplay()
    {
        Debug.Log($"🔄 UpdateContentDisplay - Filter: {_currentFilter}");

        if (!_isPageActive)
        {
            Debug.LogWarning("⚠️ UpdateContentDisplay cancelled - page not active");
            return;
        }

        // Clear everything first
        ClearContent();
        ShowLoading(false);

        if (string.IsNullOrEmpty(_currentFilter))
        {
            _currentFilter = "Mesopotamia";
        }

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
        Debug.Log($"📖 ShowChapterContent - Chapter: {chapterName}");

        if (!_isPageActive)
        {
            Debug.LogWarning("⚠️ ShowChapterContent cancelled - page not active");
            return;
        }

        // Show chapter labels
        if (contentSection != null)
            contentSection.SetActive(true);

        if (chapterLabel != null)
        {
            chapterLabel.text = "Chapter";
            chapterLabel.gameObject.SetActive(true);
        }

        if (chapterNameText != null)
        {
            string formattedChapterName = GetFormattedChapterName(chapterName);
            chapterNameText.text = formattedChapterName;
            chapterNameText.gameObject.SetActive(true);
        }

        // Check if data exists
        if (_overviewData == null)
        {
            Debug.LogWarning("⚠️ No overview data available");
            ShowLoading(false);
            ShowEmptyState($"Loading data for {chapterName}...");
            return;
        }

        if (!_overviewData.chapters.ContainsKey(chapterName))
        {
            Debug.LogWarning($"⚠️ Chapter not found: {chapterName}");
            ShowLoading(false);
            ShowEmptyState($"No data available for {chapterName}");
            return;
        }

        var chapter = _overviewData.chapters[chapterName];
        Debug.Log($"📚 Chapter has {chapter.stories.Count} stories");

        if (chapter.stories.Count > 0)
        {
            ShowStoriesImmediately(chapter.stories);
        }
        else
        {
            ShowLoading(false);
            ShowEmptyState($"No stories available for {chapterName}");
        }
    }

    private void ShowStoriesImmediately(List<StoryProgress> stories)
    {
        if (!_isPageActive)
        {
            Debug.LogWarning($"⚠️ ShowStoriesImmediately cancelled - page not active");
            return;
        }

        Debug.Log($"📋 ShowStoriesImmediately - {stories.Count} stories");
        
        ClearContent();

        var sortedStories = stories.OrderBy(s => s.storyId).ToList();

        foreach (var story in sortedStories)
        {
            if (!_isPageActive)
            {
                Debug.LogWarning($"⚠️ ShowStoriesImmediately interrupted");
                break;
            }

            CreateStoryItem(story);
        }

        Debug.Log($"✅ ShowStoriesImmediately completed - {sortedStories.Count} stories displayed");
    }

    private IEnumerator ShowStoriesWithDelay(List<StoryProgress> stories)
    {
        _isLoading = true;
        ShowLoading(true);

        ClearContent();

        Debug.Log($"⏳ ShowStoriesWithDelay started - {stories.Count} stories");

        yield return null; // Wait one frame

        if (!_isPageActive)
        {
            Debug.LogWarning($"⚠️ ShowStoriesWithDelay cancelled - page not active");
            ShowLoading(false);
            _isLoading = false;
            yield break;
        }

        var sortedStories = stories.OrderBy(s => s.storyId).ToList();

        for (int i = 0; i < sortedStories.Count; i++)
        {
            if (!_isPageActive)
            {
                Debug.LogWarning($"⚠️ ShowStoriesWithDelay interrupted at story {i + 1}/{sortedStories.Count}");
                break;
            }

            CreateStoryItem(sortedStories[i]);

            // Yield every 2 items to balance performance and responsiveness
            if (i % 2 == 0)
            {
                yield return null;
            }
        }

        // Final check before updating UI state
        if (_isPageActive)
        {
            Debug.Log($"✅ ShowStoriesWithDelay completed - {sortedStories.Count} stories displayed");
            ShowLoading(false);
        }
        else
        {
            Debug.LogWarning($"⚠️ ShowStoriesWithDelay finished but page inactive");
        }

        _isLoading = false;
    }

    private void OnDestroy()
    {
        _isPageActive = false;
        CancelInvoke();

        if (mesopotamiaButton != null) mesopotamiaButton.onClick.RemoveAllListeners();
        if (indusButton != null) indusButton.onClick.RemoveAllListeners();
        if (huangHeButton != null) huangHeButton.onClick.RemoveAllListeners();
        if (ehiptoButton != null) ehiptoButton.onClick.RemoveAllListeners();
        if (publishedStoriesButton != null) publishedStoriesButton.onClick.RemoveAllListeners();
        if (refreshButton != null) refreshButton.onClick.RemoveAllListeners(); // ADD THIS LINE
    }


    private void LoadStudentOverviewData()
    {
        if (!_isPageActive)
        {
            Debug.LogWarning("⚠️ LoadStudentOverviewData cancelled - page not active");
            return;
        }

        ShowLoading(true);
        ClearContent();

        _overviewData = new StudentOverviewData
        {
            studentId = _currentStudent.userId,
            studentName = _currentStudent.studName,
            classCode = _currentClassCode,
            lastUpdated = DateTime.Now,
            chapters = new Dictionary<string, ChapterOverview>()
        };

        LoadAllChapters();
    }

    private void LoadAllChapters()
    {
        Debug.Log($"📥 LoadAllChapters started");

        string currentStudentId = _currentStudentId;

        FirebaseManager.Instance.GetAllChapters(chapters =>
        {
            if (!_isPageActive || !IsCurrentStudent(currentStudentId))
            {
                Debug.LogWarning($"⚠️ GetAllChapters callback cancelled - wrong student");
                ShowLoading(false);
                return;
            }

            if (chapters == null || chapters.Count == 0)
            {
                Debug.LogWarning("⚠️ No chapters found");
                ShowLoading(false);
                ShowEmptyState("No chapters available");
                return;
            }

            Debug.Log($"✅ Loaded {chapters.Count} chapters");
            int totalChapters = chapters.Count;
            int chaptersProcessed = 0;

            foreach (var chapterEntry in chapters)
            {
                string chapterId = chapterEntry.Key;
                var chapterData = chapterEntry.Value;

                var chapterOverview = new ChapterOverview
                {
                    chapterId = chapterId,
                    chapterTitle = chapterData.ContainsKey("chaptTitle") ? chapterData["chaptTitle"].ToString() : "Unknown Title",
                    chapterName = chapterData.ContainsKey("chaptName") ? chapterData["chaptName"].ToString() : "Unknown Chapter",
                    stories = new List<StoryProgress>()
                };

                Debug.Log($"   -> Processing chapter '{chapterOverview.chapterName}' ({chapterId})");
                LoadStoriesForChapter(chapterId, chapterOverview, () =>
                {
                    if (!_isPageActive || !IsCurrentStudent(currentStudentId))
                    {
                        Debug.LogWarning($"⚠️ Chapter completion callback for '{chapterOverview.chapterName}' cancelled - wrong student.");
                        return;
                    }

                    chaptersProcessed++;
                    if (!_overviewData.chapters.ContainsKey(chapterOverview.chapterName))
                    {
                        _overviewData.chapters[chapterOverview.chapterName] = chapterOverview;
                        Debug.Log($"   ✅ Chapter '{chapterOverview.chapterName}' added to overview data with {chapterOverview.stories.Count} stories.");
                    }

                    if (chaptersProcessed >= totalChapters)
                    {
                        Debug.Log($"🎉 All {chaptersProcessed} chapters processed!");
                        _studentOverviewCache[_currentStudent.userId] = _overviewData;
                        ShowOverviewData();
                    }
                });
            }
        });
    }

    private void LoadStoriesForChapter(string chapterId, ChapterOverview chapterOverview, Action onComplete)
    {
        Debug.Log($"      -> Loading stories for chapter '{chapterOverview.chapterName}'...");
        
        string currentStudentId = _currentStudentId;

        FirebaseManager.Instance.GetStoriesByChapter(chapterId, stories =>
        {
            if (!_isPageActive || !IsCurrentStudent(currentStudentId))
            {
                Debug.LogWarning($"⚠️ GetStoriesByChapter callback for '{chapterOverview.chapterName}' cancelled - wrong student.");
                return;
            }

            if (stories == null || stories.Count == 0)
            {
                Debug.Log($"      -> No stories found for '{chapterOverview.chapterName}'");
                onComplete?.Invoke();
                return;
            }

            var sortedStories = stories.OrderBy(s => s.Key).ToList();
            int totalStories = sortedStories.Count;
            int storiesProcessed = 0;
            Debug.Log($"      -> Found {totalStories} stories for '{chapterOverview.chapterName}'. Loading quiz attempts...");

            foreach (var storyEntry in sortedStories)
            {
                string storyId = storyEntry.Key;
                var storyData = storyEntry.Value;

                string storyTitle = GetValueOrDefault(storyData, "storyTitle", "").ToString();
                string storyName = GetValueOrDefault(storyData, "storyName", "Unknown").ToString();

                if (string.IsNullOrEmpty(storyTitle))
                    storyTitle = storyName;

                var storyProgress = new StoryProgress
                {
                    storyId = storyId,
                    storyTitle = storyTitle,
                    storyName = storyName,
                    quizAttempts = new List<QuizAttempt>()
                };

                LoadQuizAttemptsForStory(storyId, storyProgress, () =>
                {
                    if (!_isPageActive || !IsCurrentStudent(currentStudentId))
                    {
                        Debug.LogWarning($"⚠️ Quiz attempt completion callback for story '{storyId}' cancelled - wrong student.");
                        return;
                    }

                    storiesProcessed++;
                    chapterOverview.stories.Add(storyProgress);

                    if (storiesProcessed >= totalStories)
                    {
                        chapterOverview.stories = chapterOverview.stories.OrderBy(s => s.storyId).ToList();
                        Debug.Log($"      ✅ Completed loading {storiesProcessed} stories for '{chapterOverview.chapterName}'");
                        onComplete?.Invoke();
                    }
                });
            }
        });
    }

    private void LoadQuizAttemptsForStory(string storyId, StoryProgress storyProgress, Action onComplete)
    {
        string studentStudId = _currentStudent.studId ?? _currentStudent.userId;
        string currentStudentId = _currentStudentId;

        FirebaseManager.Instance.GetQuizAttemptsByStudentAndStory(studentStudId, storyId, attempts =>
        {
            if (!_isPageActive || !IsCurrentStudent(currentStudentId))
            {
                Debug.LogWarning($"⚠️ GetQuizAttempts for story '{storyId}' cancelled - wrong student.");
                return;
            }

            if (attempts != null && attempts.Count > 0)
            {
                foreach (var attemptEntry in attempts)
                {
                    ProcessQuizAttempt(attemptEntry.Key, attemptEntry.Value, storyProgress);
                }
                storyProgress.quizAttempts = storyProgress.quizAttempts.OrderBy(a => a.attemptNumber).ToList();
            }
            onComplete?.Invoke();
        });
    }

    private void ShowOverviewData()
    {
        Debug.Log($"📊 ShowOverviewData");

        if (!_isPageActive)
        {
            Debug.LogWarning("⚠️ ShowOverviewData cancelled - page not active");
            ShowLoading(false);
            return;
        }

        ShowLoading(false);

        if (_overviewData == null)
        {
            Debug.LogError("⚠️ _overviewData is null in ShowOverviewData!");
            ShowEmptyState("Failed to load student data");
            return;
        }

        Debug.Log($"📊 Overview data loaded with {_overviewData.chapters.Count} chapters");
        foreach (var chapter in _overviewData.chapters)
        {
            Debug.Log($"   - {chapter.Key}: {chapter.Value.stories.Count} stories");
        }

        UpdateContentDisplay();
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
            else
            {
                string dateString = dateCompleted.ToString();
                if (DateTime.TryParse(dateString, out DateTime parsedDate))
                {
                    quizAttempt.dateCompleted = parsedDate;
                }
                else
                {
                    quizAttempt.dateCompleted = DateTime.Now;
                }
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

    private void CreateStoryItem(StoryProgress story)
    {
        if (!_isPageActive)
        {
            Debug.LogWarning("⚠️ Cannot create story item - page inactive");
            return;
        }

        if (storyPrefab == null || storiesContent == null)
        {
            Debug.LogError("❌ storyPrefab or storiesContent is NULL!");
            return;
        }

        GameObject storyItem = Instantiate(storyPrefab, storiesContent);
        var storyView = storyItem.GetComponent<StudentStoryView>();

        if (storyView != null)
        {
            storyView.SetupStory(story);
            Debug.Log($"   ✅ Created story item: {story.storyTitle} with {story.quizAttempts.Count} attempts");
        }
        else
        {
            Debug.LogError($"❌ StudentStoryView component missing on prefab!");
        }

        storyItem.SetActive(true);
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

    private void ShowLoading(bool show)
    {
        if (loadingSpinner != null)
            loadingSpinner.SetActive(show);

        if (show && emptyStateText != null)
            emptyStateText.gameObject.SetActive(false);
        
        Debug.Log($"🔄 Loading spinner: {(show ? "ON" : "OFF")}");
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
            int childCount = storiesContent.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(storiesContent.GetChild(i).gameObject);
            }
            Debug.Log($"🧹 Cleared {childCount} story items from content");
        }

        if (emptyStateText != null)
            emptyStateText.gameObject.SetActive(false);
    }

    public void ClearCache()
    {
        _studentOverviewCache.Clear();
        Debug.Log("🗑️ Cache cleared");
    }

    public void ResetPage()
    {
        if (!_isPageActive && _currentStudent == null)
        {
            return;
        }

        Debug.Log("🔄 Resetting StudentOverviewPage state and UI...");
        _isPageActive = false;
        _currentStudentId = null;

        StopAllCoroutines();
        CancelInvoke();
        _isLoading = false;
        
        _currentStudent = null;
        _overviewData = null;
        _currentFilter = "Mesopotamia";
        
        // Force immediate cleanup
        if (storiesContent != null)
        {
            int childCount = storiesContent.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(storiesContent.GetChild(i).gameObject);
            }
        }
        
        HideChapterLabels();
        ShowLoading(false);
    }

    private object GetValueOrDefault(Dictionary<string, object> dict, string key, object defaultValue = null)
    {
        if (dict != null && dict.ContainsKey(key))
        {
            return dict[key];
        }
        return defaultValue;
    }

    private void ShowPublishedStories()
    {
        Debug.Log($"📚 [START] ShowPublishedStories - Class: {_currentClassCode}, PageActive: {_isPageActive}, GameObject: {gameObject.activeInHierarchy}");

        if (!_isPageActive)
        {
            Debug.LogWarning($"⚠️ [EARLY EXIT] ShowPublishedStories cancelled - PageActive: {_isPageActive}");
            return;
        }

        // CRITICAL FIX: If GameObject is not active yet, wait for next frame using a different approach
        if (!gameObject.activeInHierarchy)
        {
            Debug.Log($"⏳ [WAIT] GameObject not active yet, using delayed call...");
            Invoke(nameof(DelayedShowPublishedStories), 0.1f);
            return;
        }

        Debug.Log($"🧹 [CLEAR] Clearing content and showing loading...");
        ClearContent();
        HideChapterLabels();
        ShowLoading(true);

        string currentStudentId = _currentStudentId;

        Debug.Log($"🔍 [FIREBASE] Requesting published stories from Firebase for class: {_currentClassCode}");
        FirebaseManager.Instance.GetPublishedStoriesByClass(_currentClassCode, stories =>
        {
            Debug.Log($"📥 [CALLBACK-1] GetPublishedStoriesByClass callback received - PageActive: {_isPageActive}, GameObject: {gameObject.activeInHierarchy}, IsCurrentStudent: {IsCurrentStudent(currentStudentId)}");

            if (!_isPageActive || !IsCurrentStudent(currentStudentId))
            {
                Debug.LogWarning($"⚠️ [CALLBACK-1 CANCELLED] GetPublishedStoriesByClass - PageActive: {_isPageActive}, IsCurrentStudent: {IsCurrentStudent(currentStudentId)}");
                if (IsCurrentStudent(currentStudentId))
                {
                    ShowLoading(false);
                }
                return;
            }

            if (stories == null || stories.Count == 0)
            {
                Debug.Log($"ℹ️ [NO STORIES] No published stories found - showing empty state");
                if (IsCurrentStudent(currentStudentId))
                {
                    ShowLoading(false);
                    ShowEmptyState("No published stories available for this class");
                }
                return;
            }

            Debug.Log($"✅ [STORIES FOUND] Found {stories.Count} published stories - preparing to fetch quiz attempts");

            int processed = 0;
            int totalStories = stories.Count;
            var storyProgressList = new List<StoryProgress>();

            if (totalStories == 0)
            {
                Debug.Log($"⚠️ [EDGE CASE] Total stories is 0 (should not reach here)");
                if (IsCurrentStudent(currentStudentId))
                {
                    ShowLoading(false);
                    ShowEmptyState("No published stories available for this class");
                }
                return;
            }

            Debug.Log($"🔄 [LOOP START] Starting to process {totalStories} stories...");
            foreach (var entry in stories)
            {
                string storyId = entry.Key;
                var storyData = entry.Value;

                string title = storyData.ContainsKey("title") ? storyData["title"].ToString() : "Untitled Story";
                Debug.Log($"   📖 [STORY {storyId}] Title: {title}");

                var storyProgress = new StoryProgress
                {
                    storyId = storyId,
                    storyTitle = title,
                    storyName = title,
                    quizAttempts = new List<QuizAttempt>()
                };

                string studentStudId = _currentStudent.studId ?? _currentStudent.userId;
                Debug.Log($"   🔍 [FIREBASE] Fetching quiz attempts for story {storyId}, student {studentStudId}");

                FirebaseManager.Instance.GetPublishedStoryQuizAttempts(storyId, studentStudId, attempts =>
                {
                    Debug.Log($"   📥 [CALLBACK-2] Quiz attempts callback for story {storyId} - PageActive: {_isPageActive}, GameObject: {gameObject.activeInHierarchy}, Attempts: {attempts?.Count ?? 0}");

                    if (!_isPageActive || !IsCurrentStudent(currentStudentId))
                    {
                        Debug.LogWarning($"   ⚠️ [CALLBACK-2 CANCELLED] Quiz attempts for story '{storyId}' - PageActive: {_isPageActive}, IsCurrentStudent: {IsCurrentStudent(currentStudentId)}");
                        return;
                    }

                    if (attempts != null && attempts.Count > 0)
                    {
                        Debug.Log($"   ✅ [ATTEMPTS] Processing {attempts.Count} quiz attempts for story {storyId}");
                        foreach (var attemptEntry in attempts)
                            ProcessQuizAttempt(attemptEntry.Key, attemptEntry.Value, storyProgress);

                        storyProgress.quizAttempts = storyProgress.quizAttempts
                            .OrderBy(a => a.attemptNumber)
                            .ToList();
                    }
                    else
                    {
                        Debug.Log($"   ℹ️ [NO ATTEMPTS] No quiz attempts found for story {storyId}");
                    }

                    storyProgressList.Add(storyProgress);
                    processed++;

                    Debug.Log($"   📊 [PROGRESS] Processed {processed}/{totalStories} published stories");

                    if (processed >= totalStories)
                    {
                        Debug.Log($"🎯 [ALL PROCESSED] All {totalStories} stories processed! PageActive: {_isPageActive}, GameObject: {gameObject.activeInHierarchy}, IsCurrentStudent: {IsCurrentStudent(currentStudentId)}");
                        
                        if (_isPageActive && IsCurrentStudent(currentStudentId))
                        {
                            Debug.Log($"✅ [DISPLAY CHECK] Checks passed - attempting to display {storyProgressList.Count} items");
                            
                            if (gameObject.activeInHierarchy)
                            {
                                Debug.Log($"🚀 [COROUTINE] Starting ShowStoriesWithDelay coroutine...");
                                StartCoroutine(ShowStoriesWithDelay(storyProgressList.OrderBy(s => s.storyId).ToList()));
                            }
                            else
                            {
                                Debug.LogWarning($"⚠️ [INACTIVE] GameObject inactive, cannot start coroutine - using immediate display");
                                ShowStoriesImmediately(storyProgressList.OrderBy(s => s.storyId).ToList());
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"⚠️ [CHECKS FAILED] Cannot display - PageActive: {_isPageActive}, IsCurrentStudent: {IsCurrentStudent(currentStudentId)}");
                            if (IsCurrentStudent(currentStudentId))
                            {
                                ShowLoading(false);
                            }
                        }
                    }
                });
            }
            Debug.Log($"🔄 [LOOP END] Finished initiating all {totalStories} Firebase requests");
        });
        
        Debug.Log($"📚 [END] ShowPublishedStories initial call completed - waiting for Firebase callbacks");
    }

    private void DelayedShowPublishedStories()
    {
        Debug.Log($"⏰ [DELAYED CALL] DelayedShowPublishedStories - PageActive: {_isPageActive}, GameObject: {gameObject.activeInHierarchy}");

        if (_isPageActive && gameObject.activeInHierarchy)
        {
            Debug.Log($"🔄 [RETRY] Calling ShowPublishedStories again now that GameObject is active");
            ShowPublishedStories();
        }
        else
        {
            Debug.LogWarning($"⚠️ [EXPIRED] Operation no longer valid in delayed call - PageActive: {_isPageActive}, GameObjectActive: {gameObject.activeInHierarchy}");
        }
    }

    public void OnRefreshButtonClicked()
    {
        Debug.Log($"🔄 Refresh button clicked - Current filter: {_currentFilter}");
        
        if (!_isPageActive)
        {
            Debug.LogWarning("⚠️ Page not active, cannot refresh");
            return;
        }
        
        if (_currentStudent == null)
        {
            Debug.LogWarning("⚠️ No student loaded, cannot refresh");
            return;
        }
        
        // Disable refresh button during refresh
        if (refreshButton != null)
        {
            refreshButton.interactable = false;
        }
        
        // Clear the cache for this student
        if (_studentOverviewCache.ContainsKey(_currentStudent.userId))
        {
            _studentOverviewCache.Remove(_currentStudent.userId);
            Debug.Log($"🗑️ Cleared cache for student: {_currentStudent.studName}");
        }
        
        StopAllCoroutines();
        _isLoading = false;
        ClearContent();
        ShowLoading(true);
        
        LoadStudentOverviewData();
        
        // Re-enable refresh button after a short delay
        StartCoroutine(ReEnableRefreshButton());
    }


    private IEnumerator ReEnableRefreshButton()
    {
        yield return new WaitForSeconds(1f);
        if (refreshButton != null)
        {
            refreshButton.interactable = true;
        }
    }

}