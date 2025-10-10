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

    // Operation tracking
    private string _currentOperationId;

    private void StartNewOperation()
    {
        _currentOperationId = System.Guid.NewGuid().ToString();
        Debug.Log($"🆕 Starting new operation: {_currentOperationId}");
    }

    private bool IsCurrentOperation(string operationId)
    {
        if (string.IsNullOrEmpty(_currentOperationId) || string.IsNullOrEmpty(operationId))
        {
            return false;
        }
        
        bool isCurrent = _currentOperationId == operationId;
        if (!isCurrent)
        {
            Debug.Log($"⛔️ Operation mismatch - Current: {_currentOperationId}, Received: {operationId}");
        }
        return isCurrent;
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

        // 2. Perform an immediate and full reset of the page state and UI
        ResetPage();

        // 3. Mark the page as active for the new setup
        _isPageActive = true;

        // 4. Start a new, unique operation for this student
        StartNewOperation();
        string currentOpId = _currentOperationId;

        // 5. Set the new student's data
        _currentStudent = student;
        _currentClassCode = student.classCode;
        _currentFilter = "Mesopotamia"; // Default filter

        // 6. Update static UI elements
        if (studentNameText != null)
            studentNameText.text = student.studName;

        if (studentSectionText != null)
        {
            studentSectionText.text = GetClassNameFromCode(student.classCode);
        }

        // 7. Set up filter button listeners
        SetupFilterButtons();

        // 8. Begin loading data
        ShowLoading(true);

        // Check cache
        if (_studentOverviewCache.ContainsKey(student.userId) && _studentOverviewCache[student.userId] != null)
        {
            Debug.Log($"📦 Loading from cache for: {student.studName}");
            _overviewData = _studentOverviewCache[student.userId];
            ShowOverviewData(currentOpId);
        }
        else
        {
            Debug.Log($"🔄 Loading fresh data for: {student.studName}");
            LoadStudentOverviewData(currentOpId);
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
    }

    private void OnFilterButtonClicked(string filter)
    {
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
        
        // Start fresh operation
        StartNewOperation();
        string currentOpId = _currentOperationId;

        _currentFilter = filter;
        
        UpdateContentDisplay(currentOpId);
    }

    private void UpdateContentDisplay(string operationId)
    {
        Debug.Log($"🔄 UpdateContentDisplay - Filter: {_currentFilter}, OpId: {operationId}");

        if (!_isPageActive || !IsCurrentOperation(operationId))
        {
            Debug.LogWarning("⚠️ UpdateContentDisplay cancelled - invalid state");
            return;
        }

        // Clear everything first
        ClearContent();
        ShowLoading(false); // FIXED: Turn off loading when switching filters

        if (string.IsNullOrEmpty(_currentFilter))
        {
            _currentFilter = "Mesopotamia";
        }

        if (_currentFilter == "Published Stories")
        {
            HideChapterLabels();
            ShowPublishedStories(operationId);
        }
        else
        {
            ShowChapterLabels();
            ShowChapterContent(_currentFilter, operationId);
        }
    }

    private void ShowChapterContent(string chapterName, string operationId)
    {
        Debug.Log($"📖 ShowChapterContent - Chapter: {chapterName}, OpId: {operationId}");

        if (!_isPageActive || !IsCurrentOperation(operationId))
        {
            Debug.LogWarning("⚠️ ShowChapterContent cancelled - invalid state");
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
            // FIXED: Show stories immediately without coroutine delay
            ShowStoriesImmediately(chapter.stories, operationId);
        }
        else
        {
            ShowLoading(false);
            ShowEmptyState($"No stories available for {chapterName}");
        }
    }

    // NEW METHOD: Show stories immediately without delay
    private void ShowStoriesImmediately(List<StoryProgress> stories, string operationId)
    {
        if (!_isPageActive || !IsCurrentOperation(operationId))
        {
            Debug.LogWarning($"⚠️ ShowStoriesImmediately cancelled - OpId: {operationId}");
            return;
        }

        Debug.Log($"📋 ShowStoriesImmediately - {stories.Count} stories, OpId: {operationId}");
        
        ClearContent();

        var sortedStories = stories.OrderBy(s => s.storyId).ToList();

        foreach (var story in sortedStories)
        {
            if (!_isPageActive || !IsCurrentOperation(operationId))
            {
                Debug.LogWarning($"⚠️ ShowStoriesImmediately interrupted");
                break;
            }

            CreateStoryItem(story);
        }

        Debug.Log($"✅ ShowStoriesImmediately completed - {sortedStories.Count} stories displayed");
    }

    private IEnumerator ShowStoriesWithDelay(List<StoryProgress> stories, string operationId)
    {
        _isLoading = true;
        ShowLoading(true);

        ClearContent();

        Debug.Log($"⏳ ShowStoriesWithDelay started - {stories.Count} stories, OpId: {operationId}");

        yield return null; // Wait one frame

        if (!_isPageActive || !IsCurrentOperation(operationId))
        {
            Debug.LogWarning($"⚠️ ShowStoriesWithDelay cancelled - OpId: {operationId}");
            ShowLoading(false);
            _isLoading = false;
            yield break;
        }

        var sortedStories = stories.OrderBy(s => s.storyId).ToList();

        for (int i = 0; i < sortedStories.Count; i++)
        {
            if (!_isPageActive || !IsCurrentOperation(operationId))
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
        if (_isPageActive && IsCurrentOperation(operationId))
        {
            Debug.Log($"✅ ShowStoriesWithDelay completed - {sortedStories.Count} stories displayed");
            ShowLoading(false);
        }
        else
        {
            Debug.LogWarning($"⚠️ ShowStoriesWithDelay finished but operation outdated");
        }

        _isLoading = false;
    }

    private void OnDestroy()
    {
        _isPageActive = false;

        if (mesopotamiaButton != null) mesopotamiaButton.onClick.RemoveAllListeners();
        if (indusButton != null) indusButton.onClick.RemoveAllListeners();
        if (huangHeButton != null) huangHeButton.onClick.RemoveAllListeners();
        if (ehiptoButton != null) ehiptoButton.onClick.RemoveAllListeners();
        if (publishedStoriesButton != null) publishedStoriesButton.onClick.RemoveAllListeners();
    }

    private void LoadStudentOverviewData(string operationId)
    {
        if (!_isPageActive || !IsCurrentOperation(operationId))
        {
            Debug.LogWarning("⚠️ LoadStudentOverviewData cancelled");
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

        LoadAllChapters(operationId);
    }

    private void LoadAllChapters(string operationId)
    {
        Debug.Log($"📥 LoadAllChapters started - OpId: {operationId}");

        FirebaseManager.Instance.GetAllChapters(chapters =>
        {
            if (!_isPageActive || !IsCurrentOperation(operationId))
            {
                Debug.LogWarning($"⚠️ GetAllChapters callback cancelled - OpId: {operationId}");
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
                LoadStoriesForChapter(chapterId, chapterOverview, operationId, () =>
                {
                    if (!_isPageActive || !IsCurrentOperation(operationId))
                    {
                        Debug.LogWarning($"⚠️ Chapter completion callback for '{chapterOverview.chapterName}' cancelled - OpId mismatch.");
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
                        Debug.Log($"🎉 All {chaptersProcessed} chapters processed for OpId: {operationId}!");
                        _studentOverviewCache[_currentStudent.userId] = _overviewData;
                        ShowOverviewData(operationId);
                    }
                });
            }
        });
    }

    private void LoadStoriesForChapter(string chapterId, ChapterOverview chapterOverview, string operationId, Action onComplete)
    {
        Debug.Log($"      -> Loading stories for chapter '{chapterOverview.chapterName}'...");
        FirebaseManager.Instance.GetStoriesByChapter(chapterId, stories =>
        {
            if (!_isPageActive || !IsCurrentOperation(operationId))
            {
                Debug.LogWarning($"⚠️ GetStoriesByChapter callback for '{chapterOverview.chapterName}' cancelled - OpId mismatch.");
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

                LoadQuizAttemptsForStory(storyId, storyProgress, operationId, () =>
                {
                    if (!_isPageActive || !IsCurrentOperation(operationId))
                    {
                        Debug.LogWarning($"⚠️ Quiz attempt completion callback for story '{storyId}' cancelled - OpId mismatch.");
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

    private void LoadQuizAttemptsForStory(string storyId, StoryProgress storyProgress, string operationId, Action onComplete)
    {
        string studentStudId = _currentStudent.studId ?? _currentStudent.userId;

        FirebaseManager.Instance.GetQuizAttemptsByStudentAndStory(studentStudId, storyId, attempts =>
        {
            if (!_isPageActive || !IsCurrentOperation(operationId))
            {
                Debug.LogWarning($"⚠️ GetQuizAttempts for story '{storyId}' cancelled - OpId mismatch.");
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

    private void ShowOverviewData(string operationId)
    {
        Debug.Log($"📊 ShowOverviewData - OpId: {operationId}");

        if (!_isPageActive || !IsCurrentOperation(operationId))
        {
            Debug.LogWarning("⚠️ ShowOverviewData cancelled");
            ShowLoading(false);
            return;
        }

        ShowLoading(false);

        // FIXED: Add null check and ensure game object is active
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

        // The _isPageActive check at the start of this method is the reliable guard.
        // We can now safely call the update method.
        UpdateContentDisplay(operationId);
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
                // Use DestroyImmediate to ensure cleanup happens right away
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
    
    StopAllCoroutines();
    _isLoading = false;
    
    // Don't null the operation ID immediately - let callbacks check it
    // _currentOperationId = null;
    
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
    
    // Set operation ID to null after a short delay to allow current callbacks to complete
    StartCoroutine(DelayedOperationReset());
}

    private IEnumerator DelayedOperationReset()
    {
        yield return new WaitForSeconds(0.1f); // Small delay
        _currentOperationId = null;
    }


    private object GetValueOrDefault(Dictionary<string, object> dict, string key, object defaultValue = null)
    {
        if (dict != null && dict.ContainsKey(key))
        {
            return dict[key];
        }
        return defaultValue;
    }

    private void ShowPublishedStories(string operationId)
    {
        Debug.Log($"📚 ShowPublishedStories - Class: {_currentClassCode}, OpId: {operationId}");

        if (!_isPageActive || !IsCurrentOperation(operationId))
        {
            Debug.LogWarning("⚠️ ShowPublishedStories cancelled");
            return;
        }

        ClearContent();
        HideChapterLabels();
        ShowLoading(true);

        FirebaseManager.Instance.GetPublishedStoriesByClass(_currentClassCode, stories =>
        {
            // CRITICAL: Check if we can even proceed before processing
            if (!_isPageActive || !IsCurrentOperation(operationId) || !gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"⚠️ GetPublishedStoriesByClass callback cancelled - Page inactive or operation outdated");
                if (IsCurrentOperation(operationId))
                {
                    ShowLoading(false);
                }
                return;
            }

            if (stories == null || stories.Count == 0)
            {
                Debug.Log("ℹ️ No published stories found");
                if (IsCurrentOperation(operationId) && _isPageActive && gameObject.activeInHierarchy)
                {
                    ShowLoading(false);
                    ShowEmptyState("No published stories available for this class");
                }
                return;
            }

            Debug.Log($"✅ Found {stories.Count} published stories");

            int processed = 0;
            int totalStories = stories.Count;
            var storyProgressList = new List<StoryProgress>();

            foreach (var entry in stories)
            {
                string storyId = entry.Key;
                var storyData = entry.Value;

                string title = storyData.ContainsKey("title") ? storyData["title"].ToString() : "Untitled Story";

                var storyProgress = new StoryProgress
                {
                    storyId = storyId,
                    storyTitle = title,
                    storyName = title,
                    quizAttempts = new List<QuizAttempt>()
                };

                string studentStudId = _currentStudent.studId ?? _currentStudent.userId;

                FirebaseManager.Instance.GetPublishedStoryQuizAttempts(storyId, studentStudId, attempts =>
                {
                    // CRITICAL: Check before processing each callback
                    if (!_isPageActive || !IsCurrentOperation(operationId) || !gameObject.activeInHierarchy)
                    {
                        Debug.LogWarning($"⚠️ GetPublishedStoryQuizAttempts callback cancelled - Page inactive");
                        return;
                    }

                    if (attempts != null && attempts.Count > 0)
                    {
                        foreach (var attemptEntry in attempts)
                            ProcessQuizAttempt(attemptEntry.Key, attemptEntry.Value, storyProgress);

                        storyProgress.quizAttempts = storyProgress.quizAttempts
                            .OrderBy(a => a.attemptNumber)
                            .ToList();
                    }

                    storyProgressList.Add(storyProgress);
                    processed++;

                    Debug.Log($"📊 Processed {processed}/{totalStories} published stories");

                    if (processed >= totalStories)
                    {
                        // FINAL CHECK: Only proceed if ALL conditions are met
                        if (_isPageActive && IsCurrentOperation(operationId) && gameObject.activeInHierarchy)
                        {
                            Debug.Log($"✅ All published stories loaded, displaying {storyProgressList.Count} items");
                            // Use the immediate method instead of coroutine to avoid inactive GameObject issues
                            ShowStoriesImmediately(storyProgressList.OrderBy(s => s.storyId).ToList(), operationId);
                        }
                        else
                        {
                            Debug.LogWarning($"⚠️ Cannot display published stories - PageActive: {_isPageActive}, CurrentOp: {IsCurrentOperation(operationId)}, GameObjectActive: {gameObject.activeInHierarchy}");
                            if (IsCurrentOperation(operationId) && _isPageActive)
                            {
                                ShowLoading(false);
                            }
                        }
                    }
                });
            }
        });
    }


}