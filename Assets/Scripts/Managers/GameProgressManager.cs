using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using System.Linq;
using Firebase.Extensions;
using System;
using System.Threading.Tasks;

[System.Serializable]
public class StringListWrapper
{
    public List<string> list;
}

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    public StudentState CurrentStudentState { get; private set; }

    private FirebaseFirestore db => FirebaseManager.Instance.DB;

    private readonly string[] allCivilizations = { "Sumerian", "Akkadian", "Babylonian", "Assyrian", "Indus" };

    #region Events
    public System.Action<string> OnCivilizationUnlocked;
    public System.Action<string> OnChapterUnlocked;
    public System.Action<string> OnStoryUnlocked;
    public System.Action<string> OnAchievementUnlocked;
    public System.Action<string> OnArtifactUnlocked;
    public System.Action<int> OnHeartsChanged;
    public System.Action OnInitializationComplete;

    private bool? _cachedHasProgress = null;

    // Add these fields at the top of your GameProgressManager class
    private bool _isRecalculatingScores = false;
    private bool _isLoadingFromFirebase = false;
    private System.DateTime _lastSaveTime = System.DateTime.MinValue;
    private const float SAVE_COOLDOWN_SECONDS = 2f;

    // Civilization to Story mapping (1:1 relationship)
    private readonly Dictionary<string, string> civilizationToStory = new Dictionary<string, string>
    {
        { "Sumerian", "ST001" },
        { "Akkadian", "ST002" },
        { "Babylonian", "ST003" },
        { "Assyrian", "ST004" },
        { "Indus", "ST005" }
    };
    #endregion

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetStudentState(StudentState state, System.Action onComplete = null)
    {
        Initialize(state, onComplete);
    }

    public void Initialize(StudentState studentState, System.Action onComplete = null)
    {
        // Invalidate cache when initializing with a new student state
        InvalidateProgressCache();

        CurrentStudentState = studentState;

        if (CurrentStudentState.GameProgress == null)
        {
            Debug.LogWarning("GameProgress is NULL in StudentState! Creating new GameProgress.");

            CurrentStudentState.SetGameProgress(new GameProgressModel
            {
                currentHearts = 3,
                unlockedChapters = new List<string> { "CH001" },
                unlockedStories = new List<string> { "ST001" },
                unlockedAchievements = new List<string>(),
                unlockedArtifacts = new List<string>(),
                unlockedCivilizations = new List<string> { "Sumerian" },
                lastUpdated = Timestamp.GetCurrentTimestamp(),
                isRemoved = false
            });
        }

        var gp = CurrentStudentState.GameProgress;

        // Ensure all lists are initialized
        if (gp.unlockedChapters == null) gp.unlockedChapters = new List<string> { "CH001" };
        if (gp.unlockedStories == null) gp.unlockedStories = new List<string> { "ST001" };
        if (gp.unlockedAchievements == null) gp.unlockedAchievements = new List<string>();
        if (gp.unlockedArtifacts == null) gp.unlockedArtifacts = new List<string>();
        if (gp.unlockedCivilizations == null) gp.unlockedCivilizations = new List<string> { "Sumerian" };

        Debug.Log($"GameProgressManager initialized for student {CurrentStudentState.StudentId}");

        OnInitializationComplete = onComplete;
        LoadProgressWithFallback();
    }

    #region Improved Loading Logic

    private void LoadProgressWithFallback()
    {
        Debug.Log($"Starting LoadProgressWithFallback for student {CurrentStudentState.StudentId}");

        if (StudentPrefs.HasKey("currentHearts"))
        {
            Debug.Log("Found StudentPrefs data, loading...");
            LoadFromStudentPrefs();
            TriggerInitializationComplete();
        }
        else
        {
            Debug.Log("No StudentPrefs data found, checking Firebase...");
            LoadFromFirebaseWithCallback();
        }
    }

    private void LoadFromStudentPrefs()
    {
        var gp = CurrentStudentState.GameProgress;

        try
        {
            gp.currentHearts = StudentPrefs.GetInt("currentHearts", 3);

            var chaptersJson = StudentPrefs.GetString("unlockedChapters", "");
            if (!string.IsNullOrEmpty(chaptersJson))
            {
                gp.unlockedChapters = JsonUtility.FromJson<StringListWrapper>(chaptersJson).list ?? new List<string> { "CH001" };
            }

            var storiesJson = StudentPrefs.GetString("unlockedStories", "");
            if (!string.IsNullOrEmpty(storiesJson))
            {
                gp.unlockedStories = JsonUtility.FromJson<StringListWrapper>(storiesJson).list ?? new List<string> { "ST001" };
            }

            var achievementsJson = StudentPrefs.GetString("unlockedAchievements", "");
            if (!string.IsNullOrEmpty(achievementsJson))
            {
                gp.unlockedAchievements = JsonUtility.FromJson<StringListWrapper>(achievementsJson).list ?? new List<string>();
            }

            var artifactsJson = StudentPrefs.GetString("unlockedArtifacts", "");
            if (!string.IsNullOrEmpty(artifactsJson))
            {
                gp.unlockedArtifacts = JsonUtility.FromJson<StringListWrapper>(artifactsJson).list ?? new List<string>();
            }

            var civilizationsJson = StudentPrefs.GetString("unlockedCivilizations", "");
            if (!string.IsNullOrEmpty(civilizationsJson))
            {
                gp.unlockedCivilizations = JsonUtility.FromJson<StringListWrapper>(civilizationsJson).list ?? new List<string> { "Sumerian" };
            }

            // FIXED: Update currentStory to the latest unlocked story
            UpdateCurrentStoryFromGameProgress();

            Debug.Log("Successfully loaded GameProgress from StudentPrefs");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load from StudentPrefs: {e.Message}. Falling back to Firebase.");
            ClearStudentPrefsCache();
            LoadFromFirebaseWithCallback();
        }
    }

    private void LoadFromFirebaseWithCallback()
{
    var gp = CurrentStudentState.GameProgress;
    string studId = CurrentStudentState.StudentId;

    Debug.Log($"Fetching GameProgress from Firebase for student {studId}");
    _isLoadingFromFirebase = true;

    db.Collection("gameProgress").Document(studId).GetSnapshotAsync().ContinueWith(task =>
    {
        UnityDispatcher.RunOnMainThread(() =>
        {
            _isLoadingFromFirebase = false;
            
            try
            {
                if (task.IsCompletedSuccessfully && task.Result.Exists)
                {
                    var data = task.Result.ToDictionary();

                    gp.currentHearts = data.ContainsKey("currentHearts") ? (int)(long)data["currentHearts"] : 3;
                    gp.unlockedChapters = data.ContainsKey("unlockedChapters") ? ((List<object>)data["unlockedChapters"]).Cast<string>().ToList() : new List<string> { "CH001" };
                    gp.unlockedStories = data.ContainsKey("unlockedStories") ? ((List<object>)data["unlockedStories"]).Cast<string>().ToList() : new List<string> { "ST001" };
                    gp.unlockedAchievements = data.ContainsKey("unlockedAchievements") ? ((List<object>)data["unlockedAchievements"]).Cast<string>().ToList() : new List<string>();
                    gp.unlockedArtifacts = data.ContainsKey("unlockedArtifacts") ? ((List<object>)data["unlockedArtifacts"]).Cast<string>().ToList() : new List<string>();
                    gp.unlockedCivilizations = data.ContainsKey("unlockedCivilizations") ? ((List<object>)data["unlockedCivilizations"]).Cast<string>().ToList() : new List<string> { "Sumerian" };

                    gp.lastUpdated = data.ContainsKey("lastUpdated") ? (Timestamp)data["lastUpdated"] : Timestamp.GetCurrentTimestamp();

                    UpdateCurrentStoryFromGameProgress();
                    SaveToStudentPrefs();
                    Debug.Log("Successfully loaded GameProgress from Firebase and cached to StudentPrefs");
                    
                    // FIX: After loading from Firebase, check if scores need recalculation
                    CheckAndRecalculateScoresAfterLoad();
                }
                else if (task.IsCompletedSuccessfully && !task.Result.Exists)
                {
                    Debug.LogWarning($"No GameProgress document found in Firebase for {studId}. Using default values.");
                    
                    if (CurrentStudentState?.Progress != null)
                    {
                        CurrentStudentState.Progress.currentStory = db.Document("stories/ST001");
                    }
                    
                    SaveToStudentPrefs();
                    
                    // FIX: For new games, also check if scores need recalculation
                    CheckAndRecalculateScoresAfterLoad();
                }
                else if (task.IsFaulted)
                {
                    Debug.LogError($"Firebase fetch failed for {studId}: {task.Exception}");
                    TriggerInitializationComplete();
                }
                else if (task.IsCanceled)
                {
                    Debug.LogWarning($"Firebase fetch was canceled for {studId}. Using default values.");
                    TriggerInitializationComplete();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error processing Firebase data for {studId}: {e.Message}");
                TriggerInitializationComplete();
            }
        });
    });
}

// NEW: Method to check if scores need recalculation after loading
private void CheckAndRecalculateScoresAfterLoad()
{
    if (CurrentStudentState?.Progress == null)
    {
        Debug.LogWarning("Cannot check scores: StudentProgress is null");
        TriggerInitializationComplete();
        return;
    }

    // Check if scores are zero/default and need recalculation
    bool needsRecalculation = string.IsNullOrEmpty(CurrentStudentState.Progress.overallScore) || 
                             string.IsNullOrEmpty(CurrentStudentState.Progress.successRate) ||
                             CurrentStudentState.Progress.overallScore == "0" ||
                             CurrentStudentState.Progress.successRate == "0%" ||
                             CurrentStudentState.Progress.successRate == "0";

    if (needsRecalculation)
    {
        Debug.Log("Scores need recalculation after loading from Firebase");
        RecalculateScoresAfterInitialLoad();
    }
    else
    {
        Debug.Log($"Scores are already set - Overall: {CurrentStudentState.Progress.overallScore}, Success: {CurrentStudentState.Progress.successRate}");
        TriggerInitializationComplete();
    }
}

// NEW: Safe score recalculation for initial load
private void RecalculateScoresAfterInitialLoad()
{
    if (CurrentStudentState == null) 
    {
        TriggerInitializationComplete();
        return;
    }
    
    if (_isRecalculatingScores)
    {
        Debug.Log("Score recalculation already in progress, skipping...");
        TriggerInitializationComplete();
        return;
    }

    _isRecalculatingScores = true;
    string studId = CurrentStudentState.StudentId;
    
    Debug.Log($"Starting initial score recalculation for student {studId}");
    
    var quizAttemptsRef = db.Collection("quizAttempts").Document(studId).Collection("attempts");
    quizAttemptsRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
    {
        try
        {
            if (!task.IsCompletedSuccessfully || task.Result == null)
            {
                Debug.LogWarning("Failed to fetch quiz attempts for initial recalculation");
                // Set default scores
                if (CurrentStudentState.Progress != null)
                {
                    CurrentStudentState.Progress.overallScore = "0";
                    CurrentStudentState.Progress.successRate = "0%";
                }
                _isRecalculatingScores = false;
                TriggerInitializationComplete();
                return;
            }

            QuerySnapshot snapshot = task.Result;
            var allAttempts = snapshot.Documents;

            if (allAttempts.Count() == 0)
            {
                Debug.Log("No quiz attempts found, setting default scores");
                if (CurrentStudentState.Progress != null)
                {
                    CurrentStudentState.Progress.overallScore = "0";
                    CurrentStudentState.Progress.successRate = "0%";
                }
                _isRecalculatingScores = false;
                TriggerInitializationComplete();
                return;
            }

            int totalAttempts = allAttempts.Count();
            int passedAttempts = allAttempts.Count(doc =>
            {
                var attempt = doc.ConvertTo<QuizAttemptModel>();
                return attempt.isPassed;
            });

            string successRate = totalAttempts > 0
                ? $"{(int)((float)passedAttempts / totalAttempts * 100)}%"
                : "0%";

            var bestScoresPerQuiz = new Dictionary<string, int>();
            
            foreach (var doc in allAttempts)
            {
                var attempt = doc.ConvertTo<QuizAttemptModel>();
                if (!bestScoresPerQuiz.ContainsKey(attempt.quizId) || attempt.score > bestScoresPerQuiz[attempt.quizId])
                {
                    bestScoresPerQuiz[attempt.quizId] = attempt.score;
                }
            }

            int overallScore = bestScoresPerQuiz.Values.Sum();

            // Update local progress
            if (CurrentStudentState.Progress != null)
            {
                CurrentStudentState.Progress.overallScore = overallScore.ToString();
                CurrentStudentState.Progress.successRate = successRate;
                Debug.Log($"Recalculated initial scores - OverallScore: {overallScore}, SuccessRate: {successRate}");
            }

            // Save to Firebase without triggering loops
            SaveStudentProgressSafely(overallScore, successRate, bestScoresPerQuiz, totalAttempts, passedAttempts);
        }
        catch (System.Exception e)
        {
            _isRecalculatingScores = false;
            Debug.LogError($"Exception during initial score recalculation: {e.Message}");
            TriggerInitializationComplete();
        }
    });
}

// NEW: Safe method to save student progress without causing loops
private void SaveStudentProgressSafely(int overallScore, string successRate, Dictionary<string, int> bestScores, int totalAttempts, int passedAttempts)
{
    if (CurrentStudentState?.Progress == null || string.IsNullOrEmpty(CurrentStudentState.StudentId))
    {
        _isRecalculatingScores = false;
        TriggerInitializationComplete();
        return;
    }

    var progressDoc = db.Collection("studentProgress").Document(CurrentStudentState.StudentId);
    var progressUpdate = new Dictionary<string, object>
    {
        { "overallScore", overallScore.ToString() },
        { "successRate", successRate },
        { "perQuizBestScores", bestScores },
        { "totalAttempts", totalAttempts },
        { "passedAttempts", passedAttempts },
        { "dateUpdated", Timestamp.GetCurrentTimestamp() }
    };

    progressDoc.SetAsync(progressUpdate, SetOptions.MergeAll).ContinueWithOnMainThread(updateTask =>
    {
        _isRecalculatingScores = false;
        
        if (updateTask.IsCompletedSuccessfully)
        {
            Debug.Log($"Successfully saved recalculated scores for student {CurrentStudentState.StudentId}");
        }
        else
        {
            Debug.LogError($"Failed to save recalculated scores: {updateTask.Exception}");
        }
        
        TriggerInitializationComplete();
    });
}

// FIXED: Helper method to get civilization from story ID
public string GetCivilizationFromStory(string storyId)
{
    foreach (var kvp in civilizationToStory)
    {
        if (kvp.Value == storyId)
            return kvp.Key;
    }
    return null;
}

// FIXED: Helper method to get story from civilization
public string GetStoryFromCivilization(string civName)
{
    return civilizationToStory.ContainsKey(civName) ? civilizationToStory[civName] : null;
}
    private void TriggerInitializationComplete()
    {
        Debug.Log($"Initialization completed for {CurrentStudentState.StudentId}");
        OnInitializationComplete?.Invoke();
        OnInitializationComplete = null;
    }

    #endregion

    #region FIXED: Helper Methods for Current Story Management

    /// <summary>
    /// Updates currentStory in StudentProgress based on the latest unlocked story or civilization
    /// </summary>
    // FIXED: UpdateCurrentStoryFromGameProgress now uses the simplified logic
private void UpdateCurrentStoryFromGameProgress()
{
    if (CurrentStudentState?.Progress == null || CurrentStudentState?.GameProgress == null)
    {
        Debug.LogWarning("Cannot update currentStory: Progress or GameProgress is null");
        return;
    }

    var gp = CurrentStudentState.GameProgress;
    string latestStoryId = GetLatestUnlockedStory();

    if (!string.IsNullOrEmpty(latestStoryId))
    {
        // FIXED: Create proper DocumentReference for currentStory
        CurrentStudentState.Progress.currentStory = db.Document($"stories/{latestStoryId}");
        
        // FIXED: Ensure this story is in unlockedStories
        if (!gp.unlockedStories.Contains(latestStoryId))
        {
            gp.unlockedStories.Add(latestStoryId);
            Debug.Log($"Added {latestStoryId} to unlockedStories during currentStory update");
        }
        
        Debug.Log($"Updated currentStory to DocumentReference: stories/{latestStoryId}");
        
        // FIXED: Save StudentProgress immediately
        SaveStudentProgressToFirebase();
    }
    else
    {
        Debug.LogWarning("Could not determine latest story for currentStory update");
    }
}

    /// <summary>
    /// Determines the latest unlocked story based on civilizations and stories
    /// </summary>
    // FIXED: Simplified GetLatestUnlockedStory method
private string GetLatestUnlockedStory()
{
    var gp = CurrentStudentState.GameProgress;
    
    // Priority 1: Highest story ID in unlockedStories
    if (gp.unlockedStories != null && gp.unlockedStories.Count > 0)
    {
        var sortedStories = gp.unlockedStories
            .Where(s => s.StartsWith("ST"))
            .OrderBy(s => 
            {
                if (int.TryParse(s.Substring(2), out int num))
                    return num;
                return 0;
            })
            .ToList();

        if (sortedStories.Count > 0)
        {
            string latest = sortedStories.Last();
            Debug.Log($"Latest story from unlockedStories: {latest}");
            return latest;
        }
    }

    // Priority 2: Highest story from latest unlocked civilization
    if (gp.unlockedCivilizations != null && gp.unlockedCivilizations.Count > 0)
    {
        // Get the "highest" civilization based on progression order
        var civOrder = new List<string> { "Sumerian", "Akkadian", "Babylonian", "Assyrian", "Indus" };
        var sortedCivs = gp.unlockedCivilizations
            .OrderBy(c => civOrder.IndexOf(c))
            .ToList();

        string latestCiv = sortedCivs.Last();
        if (civilizationToStory.ContainsKey(latestCiv))
        {
            string storyId = civilizationToStory[latestCiv];
            Debug.Log($"Latest story from civilization {latestCiv}: {storyId}");
            return storyId;
        }
    }

    // Fallback to Sumerian/ST001
    Debug.Log("Using fallback story ST001");
    return "ST001";
}

    #endregion

    #region Gameplay Methods    

    public async Task<bool> HasProgressAsync(string userId)
    {
        // Only use cache if we have a value AND the current student state matches
        if (_cachedHasProgress.HasValue &&
            CurrentStudentState?.StudentId == userId)
        {
            Debug.Log($"[HasProgress] Using cached result: {_cachedHasProgress.Value}");
            return _cachedHasProgress.Value;
        }

        // If student ID changed, invalidate cache
        if (CurrentStudentState?.StudentId != userId)
        {
            _cachedHasProgress = null;
        }

        try
        {
            var db = FirebaseManager.Instance.DB;
            var studentsRef = db.Collection("students");

            var snapshot = await studentsRef.WhereEqualTo("userId", userId).GetSnapshotAsync();
            if (!snapshot.Documents.Any())
            {
                Debug.LogWarning($"[HasProgress] No student document found for userId '{userId}'");
                _cachedHasProgress = false;
                return false;
            }

            var studentDoc = snapshot.Documents.First();
            string studentId = studentDoc.Id;

            var progressRef = db.Collection("gameProgress").Document(studentId);
            var progressSnap = await progressRef.GetSnapshotAsync();

            _cachedHasProgress = progressSnap.Exists;
            Debug.Log($"[HasProgress] Firestore progress exists? {_cachedHasProgress}");

            return _cachedHasProgress.Value;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[HasProgress] Error while checking progress: {ex}");
            _cachedHasProgress = false;
            return false;
        }
    
}
    #region Cache Management

    public void InvalidateProgressCache()
    {
        _cachedHasProgress = null;
        Debug.Log("[HasProgress] Cache invalidated - next check will query Firebase");
    }

    public void SetProgressCache(bool hasProgress)
    {
        _cachedHasProgress = hasProgress;
        Debug.Log($"[HasProgress] Cache manually set to: {hasProgress}");
    }

    #endregion

    public void SetHearts(int newHearts)
    {
        if (CurrentStudentState?.GameProgress == null) return;

        CurrentStudentState.GameProgress.currentHearts = newHearts;
        SaveProgress();
        OnHeartsChanged?.Invoke(newHearts);
    }

    public void SetScore(int newScore)
    {
        if (CurrentStudentState?.Progress == null) return;

        CurrentStudentState.Progress.overallScore = newScore.ToString();
        SaveStudentProgressToFirebase();
        Debug.Log($"Set overall score to {newScore} for student {CurrentStudentState.StudentId}");
    }

    public int GetScore()
    {
        if (CurrentStudentState?.Progress == null) return 0;

        if (int.TryParse(CurrentStudentState.Progress.overallScore, out int parsed))
            return parsed;

        return 0;
    }

    public void UnlockChapter(string chapterId)
    {
        var gp = CurrentStudentState.GameProgress;
        if (!gp.unlockedChapters.Contains(chapterId))
        {
            gp.unlockedChapters.Add(chapterId);
            SaveProgress();
            OnChapterUnlocked?.Invoke(chapterId);
            Debug.Log($"Chapter {chapterId} unlocked for student {CurrentStudentState.StudentId}");
        }
    }
    
    // FIXED: UnlockStory with proper DocumentReference handling
    public void UnlockStory(string storyId)
    {
        if (CurrentStudentState?.GameProgress == null)
        {
            Debug.LogError("Cannot unlock story: GameProgress is null");
            return;
        }

        var gp = CurrentStudentState.GameProgress;
        bool wasUnlocked = gp.unlockedStories.Contains(storyId);

        if (!wasUnlocked)
        {
            gp.unlockedStories.Add(storyId);
            Debug.Log($"Added {storyId} to unlockedStories list for student {CurrentStudentState.StudentId}");
        }

        // FIXED: Always update currentStory to this newly unlocked story
        UpdateCurrentStoryTo(storyId);

        // FIXED: Save both GameProgress and StudentProgress immediately
        SaveProgress();

        if (!wasUnlocked)
        {
            OnStoryUnlocked?.Invoke(storyId);
            Debug.Log($"Story {storyId} unlocked for student {CurrentStudentState.StudentId}");
        }
        else
        {
            Debug.Log($"Story {storyId} was already unlocked, but currentStory was updated");
        }
    }

    public void UnlockAchievement(string achievementName)
    {
        var gp = CurrentStudentState.GameProgress;
        if (!gp.unlockedAchievements.Contains(achievementName))
        {
            gp.unlockedAchievements.Add(achievementName);
            SaveProgress();
            Debug.Log("Achievement unlocked: " + achievementName);
        }
    }

    public void AddAchievement(string achievementId)
    {
        var gp = CurrentStudentState.GameProgress;
        if (!gp.unlockedAchievements.Contains(achievementId))
        {
            gp.unlockedAchievements.Add(achievementId);
            SaveProgress();
            OnAchievementUnlocked?.Invoke(achievementId);
            Debug.Log($"Achievement {achievementId} unlocked for student {CurrentStudentState.StudentId}");
        }
    }

    // FIXED: Artifact unlocking now properly saves to the unlockedArtifacts field
    public void AddArtifact(string artifactId)
    {
        var gp = CurrentStudentState.GameProgress;
        if (gp.unlockedArtifacts == null)
            gp.unlockedArtifacts = new List<string>();

        if (!gp.unlockedArtifacts.Contains(artifactId))
        {
            gp.unlockedArtifacts.Add(artifactId);
            SaveProgress(); // This will save to both StudentPrefs and Firebase
            OnArtifactUnlocked?.Invoke(artifactId);
            Debug.Log($"Artifact {artifactId} unlocked for student {CurrentStudentState.StudentId}");
        }
        else
        {
            Debug.Log($"Artifact {artifactId} was already unlocked for student {CurrentStudentState.StudentId}");
        }
    }

    public void UseHeart()
    {
        var gp = CurrentStudentState.GameProgress;
        if (gp.currentHearts > 0)
        {
            gp.currentHearts--;
            SaveProgress();
            OnHeartsChanged?.Invoke(gp.currentHearts);
            Debug.Log($"Heart used. Remaining hearts: {gp.currentHearts}");
        }
    }

   // FIXED: UnlockCivilization now unlocks the corresponding story
public void UnlockCivilization(string civName)
{
    if (!allCivilizations.Contains(civName))
    {
        Debug.LogWarning("Invalid civilization name: " + civName);
        return;
    }

    var gp = CurrentStudentState.GameProgress;
    if (gp.unlockedCivilizations == null)
        gp.unlockedCivilizations = new List<string> { "Sumerian" };

    if (!gp.unlockedCivilizations.Contains(civName))
    {
        gp.unlockedCivilizations.Add(civName);
        
        // FIXED: Unlock the corresponding story for this civilization
        if (civilizationToStory.ContainsKey(civName))
        {
            string storyId = civilizationToStory[civName];
            if (!gp.unlockedStories.Contains(storyId))
            {
                gp.unlockedStories.Add(storyId);
                Debug.Log($"Automatically unlocked story {storyId} for civilization {civName}");
                
                // FIXED: Update currentStory to this newly unlocked story
                UpdateCurrentStoryTo(storyId);
            }
        }
        
        SaveProgress();

        OnCivilizationUnlocked?.Invoke(civName);
        Debug.Log($"Civilization {civName} unlocked for student {CurrentStudentState.StudentId}");
    }
}

// FIXED: Simplified method to update currentStory to a specific story
private void UpdateCurrentStoryTo(string storyId)
{
    if (CurrentStudentState?.Progress == null)
    {
        Debug.LogWarning("Cannot update currentStory: Progress is null");
        return;
    }

    // FIXED: Create proper DocumentReference for currentStory
    CurrentStudentState.Progress.currentStory = db.Document($"stories/{storyId}");
    
    Debug.Log($"Updated currentStory to DocumentReference: stories/{storyId}");
    
    // FIXED: Save StudentProgress immediately to ensure currentStory is persisted
    SaveStudentProgressToFirebase();
}
    public bool IsCivilizationUnlocked(string civName)
    {
        if (!allCivilizations.Contains(civName))
        {
            Debug.LogWarning("Invalid civilization name: " + civName);
            return false;
        }

        var gp = CurrentStudentState.GameProgress;

        // Default: Sumerian unlocked at start
        return gp.unlockedCivilizations.Contains(civName) || civName == "Sumerian";
    }

    // FIXED: Add safeguards to prevent loops in StartNewGame
public void StartNewGame()
{
    if (CurrentStudentState == null)
    {
        Debug.LogError("No student logged in. Cannot start a new game.");
        return;
    }
    
    InvalidateProgressCache();
    var previousAchievements = CurrentStudentState.GameProgress?.unlockedAchievements ?? new List<string>();

    CurrentStudentState.SetGameProgress(new GameProgressModel
    {
        currentHearts = 3,
        unlockedChapters = new List<string> { "CH001" },
        unlockedStories = new List<string> { "ST001" },
        unlockedAchievements = new List<string>(previousAchievements),
        unlockedArtifacts = new List<string>(),
        unlockedCivilizations = new List<string> { "Sumerian" },
        lastUpdated = Timestamp.GetCurrentTimestamp(),
        isRemoved = false
    });

    // FIX: Initialize with proper default values that will trigger recalculation if needed
    CurrentStudentState.SetProgress(new StudentProgressModel
    {
        currentStory = db.Document("stories/ST001"),
        overallScore = "0", // This will trigger recalculation
        successRate = "0%", // This will trigger recalculation
        isRemoved = false,
        dateUpdated = Timestamp.GetCurrentTimestamp()
    });

    // Save the initial progress
    SaveProgress();
    Debug.Log("Started a new game for student: " + CurrentStudentState.StudentId);
    
    // Force recalculation for new games
    CheckAndRecalculateScoresAfterLoad();
}

// FIXED: Add method to manually break any loops
public void ForceStopAllOperations()
{
    _isRecalculatingScores = false;
    _isLoadingFromFirebase = false;
    _lastSaveTime = System.DateTime.MinValue;
    Debug.Log("Force stopped all GameProgressManager operations");
}


// FIXED: Completely rewrite this to prevent loops
private void RecalculateScoresFromQuizAttempts()
{
    if (CurrentStudentState == null) return;
    
    if (_isRecalculatingScores)
    {
        Debug.Log("Score recalculation already in progress, skipping...");
        return;
    }

    _isRecalculatingScores = true;
    string studId = CurrentStudentState.StudentId;
    
    Debug.Log($"Starting score recalculation for student {studId}");
    
    var quizAttemptsRef = db.Collection("quizAttempts").Document(studId).Collection("attempts");
    quizAttemptsRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
    {
        try
        {
            if (!task.IsCompletedSuccessfully || task.Result == null)
            {
                Debug.LogWarning("Failed to fetch quiz attempts for recalculation, using default scores");
                _isRecalculatingScores = false;
                // DO NOT CALL SaveProgress here - this is what causes the loop!
                return;
            }

            QuerySnapshot snapshot = task.Result;
            var allAttempts = snapshot.Documents;

            if (allAttempts.Count() == 0)
            {
                Debug.Log("No quiz attempts found, using default scores");
                _isRecalculatingScores = false;
                // DO NOT CALL SaveProgress here either!
                return;
            }

            int totalAttempts = allAttempts.Count();
            int passedAttempts = allAttempts.Count(doc =>
            {
                var attempt = doc.ConvertTo<QuizAttemptModel>();
                return attempt.isPassed;
            });

            string successRate = totalAttempts > 0
                ? $"{(int)((float)passedAttempts / totalAttempts * 100)}%"
                : "0%";

            var bestScoresPerQuiz = new Dictionary<string, int>();
            
            foreach (var doc in allAttempts)
            {
                var attempt = doc.ConvertTo<QuizAttemptModel>();
                if (!bestScoresPerQuiz.ContainsKey(attempt.quizId) || attempt.score > bestScoresPerQuiz[attempt.quizId])
                {
                    bestScoresPerQuiz[attempt.quizId] = attempt.score;
                }
            }

            int overallScore = bestScoresPerQuiz.Values.Sum();

            if (CurrentStudentState.Progress != null)
            {
                CurrentStudentState.Progress.overallScore = overallScore.ToString();
                CurrentStudentState.Progress.successRate = successRate;
                
                Debug.Log($"Recalculated scores - OverallScore: {overallScore}, SuccessRate: {successRate}");
            }

            var progressDoc = db.Collection("studentProgress").Document(studId);
            var progressUpdate = new Dictionary<string, object>
            {
                { "overallScore", overallScore.ToString() },
                { "successRate", successRate },
                { "perQuizBestScores", bestScoresPerQuiz },
                { "totalAttempts", totalAttempts },
                { "passedAttempts", passedAttempts },
                { "dateUpdated", Timestamp.GetCurrentTimestamp() }
            };

            progressDoc.SetAsync(progressUpdate, SetOptions.MergeAll).ContinueWithOnMainThread(updateTask =>
            {
                _isRecalculatingScores = false;
                
                if (updateTask.IsCompletedSuccessfully)
                {
                    Debug.Log($"Successfully recalculated and saved scores for student {studId}");
                }
                else
                {
                    Debug.LogError($"Failed to update recalculated scores: {updateTask.Exception}");
                }
                
                // CRITICAL: Only save GameProgress to Firebase directly, skip the SaveProgress method
                SaveGameProgressToFirebaseOnly();
            });
        }
        catch (System.Exception e)
        {
            _isRecalculatingScores = false;
            Debug.LogError($"Exception during score recalculation: {e.Message}");
        }
    });
}

// NEW: Method to save only GameProgress without triggering other saves
private void SaveGameProgressToFirebaseOnly()
{
    if (CanSave())
    {
        _lastSaveTime = System.DateTime.Now;
        SaveProgressToFirebase();
        SaveToStudentPrefs();
        Debug.Log("GameProgress saved directly to Firebase (bypassing SaveProgress)");
    }
}

// NEW: Cooldown check to prevent rapid saves
private bool CanSave()
{
    return (System.DateTime.Now - _lastSaveTime).TotalSeconds >= SAVE_COOLDOWN_SECONDS;
}


    #endregion

    #region Additional Helper Methods

    public List<string> GetUnlockedCivilizations()
    {
        if (CurrentStudentState?.GameProgress == null)
        {
            Debug.LogWarning("No student state available");
            return new List<string> { "Sumerian" };
        }

        var unlocked = new List<string>(CurrentStudentState.GameProgress.unlockedCivilizations);

        if (!unlocked.Contains("Sumerian"))
        {
            unlocked.Add("Sumerian");
        }

        return unlocked;
    }

    public List<string> GetLockedCivilizations()
    {
        var unlockedCivs = GetUnlockedCivilizations();
        var lockedCivs = new List<string>();

        foreach (string civ in allCivilizations)
        {
            if (!unlockedCivs.Contains(civ))
            {
                lockedCivs.Add(civ);
            }
        }

        return lockedCivs;
    }

    public bool IsChapterUnlocked(string chapterId)
    {
        return CurrentStudentState?.GameProgress?.unlockedChapters?.Contains(chapterId) ?? false;
    }

    public bool IsStoryUnlocked(string storyId)
    {
        return CurrentStudentState?.GameProgress?.unlockedStories?.Contains(storyId) ?? false;
    }

    public bool IsAchievementUnlocked(string achievementId)
    {
        return CurrentStudentState?.GameProgress?.unlockedAchievements?.Contains(achievementId) ?? false;
    }

    public bool IsArtifactUnlocked(string artifactId)
    {
        return CurrentStudentState?.GameProgress?.unlockedArtifacts?.Contains(artifactId) ?? false;
    }

    public int GetCurrentHearts()
    {
        return CurrentStudentState?.GameProgress?.currentHearts ?? 3;
    }

    public bool HasHearts(int required = 1)
    {
        return GetCurrentHearts() >= required;
    }

    public void AddHearts(int amount)
    {
        if (amount <= 0) return;

        var gp = CurrentStudentState.GameProgress;
        gp.currentHearts += amount;
        SaveProgress();
        OnHeartsChanged?.Invoke(gp.currentHearts);
        Debug.Log($"Added {amount} hearts. Total hearts: {gp.currentHearts}");
    }

    #endregion

    #region StudentPrefs Caching

    private void SaveToStudentPrefs()
    {
        var gp = CurrentStudentState.GameProgress;

        StudentPrefs.SetInt("currentHearts", gp.currentHearts);

        StudentPrefs.SetString("unlockedChapters",
            JsonUtility.ToJson(new StringListWrapper { list = gp.unlockedChapters }));

        StudentPrefs.SetString("unlockedStories",
            JsonUtility.ToJson(new StringListWrapper { list = gp.unlockedStories }));

        StudentPrefs.SetString("unlockedAchievements",
            JsonUtility.ToJson(new StringListWrapper { list = gp.unlockedAchievements }));

        StudentPrefs.SetString("unlockedArtifacts",
            JsonUtility.ToJson(new StringListWrapper { list = gp.unlockedArtifacts }));

        StudentPrefs.SetString("unlockedCivilizations",
            JsonUtility.ToJson(new StringListWrapper { list = gp.unlockedCivilizations }));

        StudentPrefs.Save();
        Debug.Log("GameProgress saved to StudentPrefs for " + CurrentStudentState.StudentId);
    }

    private void ClearStudentPrefsCache()
    {
        StudentPrefs.DeleteKey("currentHearts");
        StudentPrefs.DeleteKey("unlockedChapters");
        StudentPrefs.DeleteKey("unlockedStories");
        StudentPrefs.DeleteKey("unlockedAchievements");
        StudentPrefs.DeleteKey("unlockedArtifacts");
        StudentPrefs.DeleteKey("unlockedCivilizations");
        StudentPrefs.Save();
        Debug.Log("Cleared corrupted StudentPrefs cache");
    }

    #endregion

    #region Firebase Saving/Loading

    // FIXED: Safer SaveStudentProgressToFirebase
public void SaveStudentProgressToFirebase()
{
    if (CurrentStudentState?.Progress == null || string.IsNullOrEmpty(CurrentStudentState.StudentId))
    {
        Debug.LogWarning("Cannot save StudentProgress: missing state or StudentId");
        return;
    }

    if (_isRecalculatingScores)
    {
        Debug.Log("Score recalculation in progress, skipping StudentProgress save");
        return;
    }

    var sp = CurrentStudentState.Progress;
    var docRef = db.Collection("studentProgress").Document(CurrentStudentState.StudentId);

    var data = new Dictionary<string, object>
    {
        { "currentStory", sp.currentStory },
        { "overallScore", sp.overallScore ?? "0" },
        { "successRate", sp.successRate ?? "0%" },
        { "isRemoved", sp.isRemoved },
        { "dateUpdated", Timestamp.GetCurrentTimestamp() }
    };

    Debug.Log($"Saving StudentProgress - overallScore: {sp.overallScore}, successRate: {sp.successRate}");

    docRef.SetAsync(data, SetOptions.MergeAll).ContinueWithOnMainThread(task =>
    {
        if (task.IsCompletedSuccessfully)
            Debug.Log($"StudentProgress saved successfully for {CurrentStudentState.StudentId}");
        else
            Debug.LogError("Failed to save StudentProgress: " + task.Exception);
    });
}

    private void SaveProgressToFirebase()
    {
        var gp = CurrentStudentState.GameProgress;
        string studId = CurrentStudentState.StudentId;

        if (string.IsNullOrEmpty(studId))
        {
            Debug.LogWarning("Cannot save GameProgress: StudentId is null or empty");
            return;
        }

        var docRef = db.Collection("gameProgress").Document(studId);

        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "unlockedChapters", gp.unlockedChapters },
            { "unlockedStories", gp.unlockedStories },
            { "unlockedAchievements", gp.unlockedAchievements },
            { "unlockedArtifacts", gp.unlockedArtifacts },
            { "unlockedCivilizations", gp.unlockedCivilizations },
            { "currentHearts", gp.currentHearts },
            { "lastUpdated", gp.lastUpdated },
            { "isRemoved", gp.isRemoved }
        };

        Debug.Log($"Saving GameProgress - unlockedStories count: {gp.unlockedStories?.Count ?? 0}, stories: {string.Join(", ", gp.unlockedStories ?? new List<string>())}");
        Debug.Log($"Saving GameProgress - unlockedArtifacts count: {gp.unlockedArtifacts?.Count ?? 0}, artifacts: {string.Join(", ", gp.unlockedArtifacts ?? new List<string>())}");
        Debug.Log($"Saving GameProgress - unlockedCivilizations count: {gp.unlockedCivilizations?.Count ?? 0}, civilizations: {string.Join(", ", gp.unlockedCivilizations ?? new List<string>())}");

        docRef.SetAsync(data).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
                Debug.Log("GameProgress saved to Firebase successfully!");
            else
                Debug.LogError("Failed to save GameProgress to Firebase: " + task.Exception);
        });
    }

    #endregion

    // FIXED: Add safeguards to the main SaveProgress method
private void SaveProgress(bool skipStudentProgressSave = false)
{
    if (CurrentStudentState?.GameProgress == null) return;
    
    // Prevent saves during loading or recalculation
    if (_isLoadingFromFirebase || _isRecalculatingScores)
    {
        Debug.Log("Skipping SaveProgress - operation in progress");
        return;
    }

    if (!CanSave())
    {
        Debug.Log("SaveProgress called too frequently, skipping");
        return;
    }

    _lastSaveTime = System.DateTime.Now;
    CurrentStudentState.GameProgress.lastUpdated = Timestamp.GetCurrentTimestamp();
    
    SaveToStudentPrefs();
    SaveProgressToFirebase();
    _cachedHasProgress = true;

    if (!skipStudentProgressSave)
    {
        SaveStudentProgressToFirebase();
    }
}



    #region FIXED: Save Game Loading with GameProgress Restoration

    /// <summary>
/// FIXED: Loads game progress from a save file, including restoring GameProgress data with score recalculation
/// </summary>
public void LoadGameProgressFromSave(SaveData saveData)
{
    if (saveData?.gameProgress == null || CurrentStudentState == null)
    {
        Debug.LogWarning("Cannot load GameProgress from save: saveData.gameProgress or CurrentStudentState is null");
        return;
    }

    Debug.Log($"Loading GameProgress from save data for student {CurrentStudentState.StudentId}");

    try
    {
        // Restore GameProgress from save data
        var savedGP = saveData.gameProgress;
        var currentGP = CurrentStudentState.GameProgress;

        // Update all fields from the saved GameProgress
        currentGP.currentHearts = savedGP.currentHearts;
        currentGP.unlockedChapters = savedGP.unlockedChapters ?? new List<string> { "CH001" };
        currentGP.unlockedStories = savedGP.unlockedStories ?? new List<string> { "ST001" };
        currentGP.unlockedAchievements = savedGP.unlockedAchievements ?? new List<string>();
        currentGP.unlockedArtifacts = savedGP.unlockedArtifacts ?? new List<string>();
        currentGP.unlockedCivilizations = savedGP.unlockedCivilizations ?? new List<string> { "Sumerian" };
        currentGP.lastUpdated = savedGP.lastUpdated;
        currentGP.isRemoved = savedGP.isRemoved;

        Debug.Log($"Restored GameProgress - Hearts: {currentGP.currentHearts}, Stories: {string.Join(", ", currentGP.unlockedStories)}");
        Debug.Log($"Restored GameProgress - Artifacts: {string.Join(", ", currentGP.unlockedArtifacts)}, Civilizations: {string.Join(", ", CurrentStudentState.GameProgress.unlockedCivilizations)}");

        // Update currentStory based on restored GameProgress
        UpdateCurrentStoryFromGameProgress();

        // Check if scores need recalculation
        bool needsRecalculation = CurrentStudentState.Progress == null || 
                                 string.IsNullOrEmpty(CurrentStudentState.Progress.overallScore) || 
                                 string.IsNullOrEmpty(CurrentStudentState.Progress.successRate) ||
                                 CurrentStudentState.Progress.overallScore == "0" ||
                                 CurrentStudentState.Progress.successRate == "0%";

        if (needsRecalculation)
        {
            Debug.Log("Scores are null/zero, recalculating from quiz attempts after loading save...");
            RecalculateScoresFromQuizAttemptsAfterSave();
        }
        else
        {
            // Save the restored progress to cache and Firebase
            SaveProgress();
            Debug.Log("GameProgress successfully loaded and restored from save data");
        }
    }
    catch (System.Exception e)
    {
        Debug.LogError($"Failed to load GameProgress from save data: {e.Message}");
    }
}

// Also add this similar fix for RecalculateScoresFromQuizAttemptsAfterSave:
private void RecalculateScoresFromQuizAttemptsAfterSave()
{
    if (CurrentStudentState == null) return;
    
    if (_isRecalculatingScores)
    {
        Debug.Log("Score recalculation already in progress, skipping after-save recalculation...");
        return;
    }

    _isRecalculatingScores = true;
    string studId = CurrentStudentState.StudentId;
    
    var quizAttemptsRef = db.Collection("quizAttempts").Document(studId).Collection("attempts");
    quizAttemptsRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
    {
        try
        {
            if (!task.IsCompletedSuccessfully || task.Result == null)
            {
                Debug.LogWarning("Failed to fetch quiz attempts for recalculation after save load, using default scores");
                _isRecalculatingScores = false;
                SaveProgress();
                return;
            }

            QuerySnapshot snapshot = task.Result;
            var allAttempts = snapshot.Documents;

            if (allAttempts.Count() == 0)
            {
                Debug.Log("No quiz attempts found after save load, using default scores");
                _isRecalculatingScores = false;
                SaveProgress();
                return;
            }

            // Calculate success rate
            int totalAttempts = allAttempts.Count();
            int passedAttempts = allAttempts.Count(doc =>
            {
                var attempt = doc.ConvertTo<QuizAttemptModel>();
                return attempt.isPassed;
            });

            string successRate = totalAttempts > 0
                ? $"{(int)((float)passedAttempts / totalAttempts * 100)}%"
                : "0%";

            // Calculate overall score (sum of best scores per quiz)
            var bestScoresPerQuiz = new Dictionary<string, int>();
            
            foreach (var doc in allAttempts)
            {
                var attempt = doc.ConvertTo<QuizAttemptModel>();
                if (!bestScoresPerQuiz.ContainsKey(attempt.quizId) || attempt.score > bestScoresPerQuiz[attempt.quizId])
                {
                    bestScoresPerQuiz[attempt.quizId] = attempt.score;
                }
            }

            int overallScore = bestScoresPerQuiz.Values.Sum();

            // Update the student progress with recalculated values
            if (CurrentStudentState.Progress != null)
            {
                CurrentStudentState.Progress.overallScore = overallScore.ToString();
                CurrentStudentState.Progress.successRate = successRate;
                
                Debug.Log($"Recalculated scores after save load - OverallScore: {overallScore}, SuccessRate: {successRate}");
            }

            // Update the studentProgress document
            var progressDoc = db.Collection("studentProgress").Document(studId);
            var progressUpdate = new Dictionary<string, object>
            {
                { "overallScore", overallScore.ToString() },
                { "successRate", successRate },
                { "perQuizBestScores", bestScoresPerQuiz },
                { "totalAttempts", totalAttempts },
                { "passedAttempts", passedAttempts },
                { "dateUpdated", Timestamp.GetCurrentTimestamp() }
            };

            progressDoc.SetAsync(progressUpdate, SetOptions.MergeAll).ContinueWithOnMainThread(updateTask =>
            {
                _isRecalculatingScores = false; // Reset flag here
                
                if (updateTask.IsCompletedSuccessfully)
                {
                    Debug.Log($"Successfully recalculated and saved scores after save load for student {studId}");
                }
                else
                {
                    Debug.LogError($"Failed to update recalculated scores after save load: {updateTask.Exception}");
                }
                
                // Save the game progress
                SaveProgress(true); // Skip student progress save
                Debug.Log("Save game loaded with recalculated scores for student: " + studId);
            });
        }
        catch (System.Exception e)
        {
            _isRecalculatingScores = false;
            Debug.LogError($"Exception during after-save score recalculation: {e.Message}");
            SaveProgress();
        }
    });
}

    #endregion

    #region Public Save/Load API

    public void CommitProgress()
    {
        SaveProgress();
    }

    public void RefreshProgress()
    {
        LoadProgressWithFallback();
    }

    public void RefreshProgress(bool keepScores)
    {
        if (CurrentStudentState == null) return;

        db.Collection("studentProgress").Document(CurrentStudentState.StudentId)
        .GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsCompletedSuccessfully || !task.Result.Exists)
            {
                Debug.LogWarning("No progress found in Firestore");
                return;
            }

            var updatedProgress = task.Result.ConvertTo<StudentProgressModel>();

            if (keepScores && CurrentStudentState.Progress != null)
            {
                updatedProgress.overallScore = CurrentStudentState.Progress.overallScore;
                updatedProgress.successRate = CurrentStudentState.Progress.successRate;
            }

            CurrentStudentState.SetProgress(updatedProgress);
            Debug.Log("Student progress refreshed (keepScores = " + keepScores + ")");
        });
    }

    #endregion

    // FIXED: Proper quiz attempt recording with correct success rate calculation
    public void RecordQuizAttempt(string quizId, int score, int total, bool isPassed)
    {
        string studId = CurrentStudentState.StudentId;
        if (string.IsNullOrEmpty(studId))
        {
            Debug.LogError("No student ID found for recording quiz attempt.");
            return;
        }

        var quizAttemptsRef = db.Collection("quizAttempts").Document(studId).Collection("attempts");

        // Create attempt model first
        var attempt = new QuizAttemptModel(quizId, 0, score, isPassed); // Attempt number will be calculated

        // Step 1: Get existing attempts for this quiz to calculate attemptNumber
        quizAttemptsRef.WhereEqualTo("quizId", quizId).GetSnapshotAsync().ContinueWith(task =>
        {
            if (task.IsFaulted || !task.IsCompletedSuccessfully)
            {
                Debug.LogError("Failed to fetch quiz attempts: " + task.Exception);
                return;
            }

            int attemptNumber = task.Result.Count + 1;
            attempt.attemptNumber = attemptNumber;

            // Step 2: Save to Firebase
            quizAttemptsRef.AddAsync(attempt).ContinueWith(addTask =>
            {
                if (addTask.IsCompletedSuccessfully)
                {
                    Debug.Log($"Quiz attempt saved: {quizId}, Attempt {attemptNumber}, Score {score}");

                    // Step 3: Update StudentProgress and Leaderboard with ALL attempts
                    UpdateStudentProgressAndLeaderboard(studId, quizId, score, attemptNumber, isPassed);
                }
                else
                {
                    Debug.LogError("Failed to save quiz attempt: " + addTask.Exception);
                }
            });
        });
    }

    // FIXED: Correct success rate calculation using ALL quiz attempts
    private void UpdateStudentProgressAndLeaderboard(string studId, string quizId, int score, int attemptNumber, bool isPassed)
    {
        if (string.IsNullOrEmpty(studId) || string.IsNullOrEmpty(quizId))
        {
            Debug.LogError("UpdateStudentProgressAndLeaderboard: studId or quizId is null/empty");
            return;
        }

        // Get ALL quiz attempts for this student to calculate correct totals
        var quizAttemptsRef = db.Collection("quizAttempts").Document(studId).Collection("attempts");
        quizAttemptsRef.GetSnapshotAsync().ContinueWithOnMainThread(allAttemptsTask =>
        {
            if (!allAttemptsTask.IsCompletedSuccessfully)
            {
                Debug.LogError($"Failed to fetch all quiz attempts for {studId}: {allAttemptsTask.Exception}");
                return;
            }

            if (!allAttemptsTask.IsCompletedSuccessfully || allAttemptsTask.Result == null)
            {
                Debug.LogError($"Failed to fetch quiz attempts for {studId}");
                return;
            }

            QuerySnapshot snapshot = allAttemptsTask.Result;
            var allAttempts = snapshot.Documents;

            int totalAttempts = ((IReadOnlyCollection<DocumentSnapshot>)allAttempts).Count;
            int passedAttempts = allAttempts.Count(doc =>
            {
                var attempt = doc.ConvertTo<QuizAttemptModel>();
                return attempt.isPassed;
            });

            string successRate = totalAttempts > 0
                ? $"{(int)((float)passedAttempts / totalAttempts * 100)}%"
                : "0%";

            // --- Student Progress ---
            var progressDoc = db.Collection("studentProgress").Document(studId);
            progressDoc.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (!task.IsCompletedSuccessfully)
                {
                    Debug.LogError($"Failed to fetch progress doc for {studId}: {task.Exception}");
                    return;
                }

                var snapshot = task.Result;
                var progressData = snapshot.Exists ? snapshot.ToDictionary() : new Dictionary<string, object>();

                // Extract per-quiz best scores
                var perQuizBest = progressData.ContainsKey("perQuizBestScores")
                    ? progressData["perQuizBestScores"] as Dictionary<string, object>
                    : new Dictionary<string, object>();

                // Current best score for this quiz
                int currentBest = perQuizBest.ContainsKey(quizId) ? Convert.ToInt32(perQuizBest[quizId]) : 0;

                // Update if this attempt is better
                if (score > currentBest) perQuizBest[quizId] = score;

                // Recompute overall score (sum of bests)
                int overallScore = perQuizBest.Values.Sum(v => Convert.ToInt32(v));

                // Keep old passedQuizzes
                var passedQuizzes = progressData.ContainsKey("passedQuizzes")
                    ? (progressData["passedQuizzes"] as List<object>).Select(q => q.ToString()).ToHashSet()
                    : new HashSet<string>();

                if (isPassed) passedQuizzes.Add(quizId);

                // Prepare update with CORRECT totals
                var progressUpdate = new Dictionary<string, object>
                {
                    { "overallScore", overallScore.ToString() },
                    { "successRate", successRate },
                    { "perQuizBestScores", perQuizBest },
                    { "totalAttempts", totalAttempts },
                    { "passedAttempts", passedAttempts },
                    { "passedQuizzes", passedQuizzes.ToList() },
                    { "dateUpdated", Timestamp.GetCurrentTimestamp() }
                };

                // Update local CurrentStudentState
                if (CurrentStudentState?.Progress != null)
                {
                    CurrentStudentState.Progress.overallScore = overallScore.ToString();
                    CurrentStudentState.Progress.successRate = successRate;
                    Debug.Log($"Updated local StudentProgress - overallScore: {overallScore}, successRate: {successRate}, totalAttempts: {totalAttempts}, passedAttempts: {passedAttempts}");
                }

                progressDoc.SetAsync(progressUpdate, SetOptions.MergeAll).ContinueWithOnMainThread(uTask =>
                {
                    if (uTask.IsCompletedSuccessfully)
                        Debug.Log($"studentProgress updated for {studId}: overallScore={overallScore}, successRate={successRate}, totalAttempts={totalAttempts}, passedAttempts={passedAttempts}");
                    else
                        Debug.LogError("Failed to update studentProgress: " + uTask.Exception);
                });
            });

            // --- Student Leaderboard ---
            var leaderboardDoc = db.Collection("studentLeaderboards").Document(studId);
            leaderboardDoc.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (!task.IsCompletedSuccessfully)
                {
                    Debug.LogError($"Failed to fetch leaderboard doc for {studId}: {task.Exception}");
                    return;
                }

                var snapshot = task.Result;
                var leaderboardData = snapshot.Exists ? snapshot.ToDictionary() : new Dictionary<string, object>();

                // Extract per-quiz first scores
                var perQuizFirst = leaderboardData.ContainsKey("perQuizFirstScores")
                    ? leaderboardData["perQuizFirstScores"] as Dictionary<string, object>
                    : new Dictionary<string, object>();

                // Only update if this is the first attempt
                if (!perQuizFirst.ContainsKey(quizId) || attemptNumber == 1)
                    perQuizFirst[quizId] = score;

                // Recompute leaderboard total (sum of first attempts)
                int leaderboardScore = perQuizFirst.Values.Sum(v => Convert.ToInt32(v));

                var leaderboardUpdate = new Dictionary<string, object>
                {
                    { "displayName", CurrentStudentState?.Identity?.studName ?? "Unknown" },
                    { "classCode", CurrentStudentState?.Identity?.classCode ?? "" },
                    { "overallScore", leaderboardScore.ToString() },
                    { "perQuizFirstScores", perQuizFirst },
                    { "isRemoved", false },
                    { "dateUpdated", Timestamp.GetCurrentTimestamp() }
                };

                leaderboardDoc.SetAsync(leaderboardUpdate, SetOptions.MergeAll).ContinueWithOnMainThread(uTask =>
                {
                    if (uTask.IsCompletedSuccessfully)
                        Debug.Log($"Leaderboard updated for {studId}: overallScore={leaderboardScore}");
                    else
                        Debug.LogError("Failed to update leaderboard: " + uTask.Exception);
                });
            });
        });
    }
}