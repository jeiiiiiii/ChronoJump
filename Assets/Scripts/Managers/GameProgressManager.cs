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

[System.Serializable]
public class CodexWrapper
{
    public List<CodexEntry> entries;
}

[System.Serializable]
public class CodexEntry
{
    public string characterId;
    public List<string> stories;
}

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    public StudentState CurrentStudentState { get; private set; }

    private FirebaseFirestore db => FirebaseManager.Instance.DB;

    private readonly string[] allCivilizations = { "Sumerian", "Akkadian", "Babylonian", "Assyrian", "Indus" };

    #region Events
    /// <summary>
    /// Event system for UI updates when progress changes
    /// </summary>
    public System.Action<string> OnCivilizationUnlocked;
    public System.Action<string> OnChapterUnlocked;
    public System.Action<string> OnStoryUnlocked;
    public System.Action<string> OnAchievementUnlocked;
    public System.Action<string> OnArtifactUnlocked;
    public System.Action<int> OnHeartsChanged;
    public System.Action OnInitializationComplete;
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

    /// <summary>
    /// Initialize the manager with the currently logged-in student's state.
    /// </summary>
    public void SetStudentState(StudentState state, System.Action onComplete = null)
    {
        Initialize(state, onComplete);
    }

    public void Initialize(StudentState studentState, System.Action onComplete = null)
    {
        CurrentStudentState = studentState;

        // Initialize GameProgress if it doesn't exist
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
                unlockedCodex = new Dictionary<string, object>(),
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
        if (gp.unlockedCodex == null) gp.unlockedCodex = new Dictionary<string, object>();
        if (gp.unlockedCivilizations == null) gp.unlockedCivilizations = new List<string> { "Sumerian" };

        Debug.Log($"GameProgressManager initialized for student {CurrentStudentState.StudentId}");

        // Store the completion callback
        OnInitializationComplete = onComplete;

        // NEW: Load progress with proper fallback chain
        LoadProgressWithFallback();
    }

    #region Improved Loading Logic

    /// <summary>
    /// Load progress with proper fallback chain: StudentPrefs -> Firebase -> Default
    /// </summary>
    private void LoadProgressWithFallback()
    {
        Debug.Log($"Starting LoadProgressWithFallback for student {CurrentStudentState.StudentId}");

        // Try StudentPrefs first
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

            var codexJson = StudentPrefs.GetString("unlockedCodex", "");
            if (!string.IsNullOrEmpty(codexJson))
            {
                var codexWrapper = JsonUtility.FromJson<CodexWrapper>(codexJson);
                gp.unlockedCodex = new Dictionary<string, object>();
                if (codexWrapper?.entries != null)
                {
                    foreach (var entry in codexWrapper.entries)
                        gp.unlockedCodex[entry.characterId] = entry.stories;
                }
            }

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

        db.Collection("gameProgress").Document(studId).GetSnapshotAsync().ContinueWith(task =>
        {
            UnityDispatcher.RunOnMainThread(() =>
            {
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
                        gp.unlockedCodex = data.ContainsKey("unlockedCodex")
                            ? ((Dictionary<string, object>)data["unlockedCodex"])
                            : new Dictionary<string, object>();

                        gp.lastUpdated = data.ContainsKey("lastUpdated") ? (Timestamp)data["lastUpdated"] : Timestamp.GetCurrentTimestamp();

                        // Save to StudentPrefs for future loads
                        SaveToStudentPrefs();

                        Debug.Log("Successfully loaded GameProgress from Firebase and cached to StudentPrefs");
                    }
                    else if (task.IsCompletedSuccessfully && !task.Result.Exists)
                    {
                        Debug.LogWarning($"No GameProgress document found in Firebase for {studId}. Using default values.");
                        // Save defaults to StudentPrefs so we don't hit Firebase again
                        SaveToStudentPrefs();
                    }
                    else if (task.IsFaulted)
                    {
                        Debug.LogError($"Firebase fetch failed for {studId}: {task.Exception}");
                        // Use default values already set in GameProgress
                    }
                    else if (task.IsCanceled)
                    {
                        Debug.LogWarning($"Firebase fetch was canceled for {studId}. Using default values.");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error processing Firebase data for {studId}: {e.Message}");
                }
                finally
                {
                    TriggerInitializationComplete();
                }
            });
        });
    }

    private void TriggerInitializationComplete()
    {
        Debug.Log($"Initialization completed for {CurrentStudentState.StudentId}");
        OnInitializationComplete?.Invoke();
        OnInitializationComplete = null;
    }

    #endregion

    #region Gameplay Methods    
    // Hearts setter

    public async Task<bool> HasProgressAsync(string userId)
{
    try
    {
        var db = FirebaseManager.Instance.DB;
        var studentsRef = db.Collection("students");

        // 🔹 1. Get the student document from userId
        var snapshot = await studentsRef.WhereEqualTo("userId", userId).GetSnapshotAsync();

        if (!snapshot.Documents.Any())
        {
            Debug.LogWarning($"[HasProgress] No student document found for userId '{userId}'");
            return false;
        }

        var studentDoc = snapshot.Documents.First();
        string studentId = studentDoc.Id;

        // 🔹 2. Check if gameProgress exists in Firestore
        var progressRef = db.Collection("gameProgress").Document(studentId);
        var progressSnap = await progressRef.GetSnapshotAsync();

        if (progressSnap.Exists)
        {
            Debug.Log($"[HasProgress] Found Firestore progress for student '{studentId}' (user '{userId}').");
            return true;
        }
        else
        {
            Debug.Log($"[HasProgress] No Firestore progress found for student '{studentId}' (user '{userId}').");
            return false;
        }
    }
    catch (Exception ex)
    {
        Debug.LogError($"[HasProgress] Error while checking progress: {ex}");
        return false;
    }
}






    public void SetHearts(int newHearts)
    {
        if (CurrentStudentState?.GameProgress == null) return;

        CurrentStudentState.GameProgress.currentHearts = newHearts;
        SaveProgress();
        OnHeartsChanged?.Invoke(newHearts);
    }

    // Score setter
    public void SetScore(int newScore)
    {
        if (CurrentStudentState?.Progress == null) return;

        CurrentStudentState.Progress.overallScore = newScore.ToString();
        SaveProgress();
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

    public void UnlockStory(string storyId)
    {
        var gp = CurrentStudentState.GameProgress;
        if (!gp.unlockedStories.Contains(storyId))
        {
            gp.unlockedStories.Add(storyId);

            // Update studentProgress
            if (CurrentStudentState?.Progress != null)
            {
                CurrentStudentState.Progress.currentStory = db.Document($"stories/{storyId}");
            }

            SaveProgress();
            OnStoryUnlocked?.Invoke(storyId);
            Debug.Log($"Story {storyId} unlocked for student {CurrentStudentState.StudentId}");
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

    public void AddArtifact(string artifactId)
    {
        var gp = CurrentStudentState.GameProgress;
        if (!gp.unlockedArtifacts.Contains(artifactId))
        {
            gp.unlockedArtifacts.Add(artifactId);
            SaveProgress();
            OnArtifactUnlocked?.Invoke(artifactId);
            Debug.Log($"Artifact {artifactId} unlocked for student {CurrentStudentState.StudentId}");
        }
    }

    public void AddCodexEntry(string characterId, List<string> stories)
    {
        var gp = CurrentStudentState.GameProgress;
        if (!gp.unlockedCodex.ContainsKey(characterId))
            gp.unlockedCodex[characterId] = stories;

        SaveProgress();
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

    public void UnlockCivilization(string civName)
    {
        if (!allCivilizations.Contains(civName))
        {
            Debug.LogWarning("Invalid civilization name: " + civName);
            return;
        }

        var gp = CurrentStudentState.GameProgress;
        if (!gp.unlockedCivilizations.Contains(civName))
        {
            gp.unlockedCivilizations.Add(civName);
            SaveProgress();

            // Trigger event for UI updates
            OnCivilizationUnlocked?.Invoke(civName);

            Debug.Log($"Civilization {civName} unlocked for student {CurrentStudentState.StudentId}");
        }
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

    public void StartNewGame()
    {
        if (CurrentStudentState == null)
        {
            Debug.LogError("No student logged in. Cannot start a new game.");
            return;
        }

        // Preserve unlocked achievements
        var previousAchievements = CurrentStudentState.GameProgress?.unlockedAchievements ?? new List<string>();

        // Preserve score and success rate
        var previousOverallScore = CurrentStudentState.Progress?.overallScore ?? "0";
        var previousSuccessRate = CurrentStudentState.Progress?.successRate ?? "0%";

        // Reset game progress but keep achievements
        CurrentStudentState.SetGameProgress(new GameProgressModel
        {
            currentHearts = 3,
            unlockedChapters = new List<string> { "CH001" },
            unlockedStories = new List<string> { "ST001" },
            unlockedAchievements = new List<string>(previousAchievements),
            unlockedArtifacts = new List<string>(),
            unlockedCodex = new Dictionary<string, object>(),
            unlockedCivilizations = new List<string> { "Sumerian" },
            lastUpdated = Timestamp.GetCurrentTimestamp(),
            isRemoved = false
        });

        // Reset student progress but preserve overallScore & successRate
        CurrentStudentState.SetProgress(new StudentProgressModel
        {
            currentStory = db.Document("stories/ST001"),
            overallScore = previousOverallScore,
            successRate = previousSuccessRate,
            isRemoved = false,
            dateUpdated = Timestamp.GetCurrentTimestamp()
        });

        SaveProgress();
        SaveStudentProgressToFirebase();

        Debug.Log("Started a new game for student: " + CurrentStudentState.StudentId);
    }

    #endregion

    #region Additional Helper Methods

    public List<string> GetUnlockedCivilizations()
    {
        if (CurrentStudentState?.GameProgress == null)
        {
            Debug.LogWarning("No student state available");

            return new List<string> { "Sumerian" }; // Default
        }

        var unlocked = new List<string>(CurrentStudentState.GameProgress.unlockedCivilizations);

        // Ensure Sumerian is always included as the default
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

    /// <summary>
    /// Migration method to convert old PlayerPrefs to StudentPrefs
    /// </summary>
    public void MigrateFromLegacyPlayerPrefs()
    {
        if (CurrentStudentState == null) return;

        string studId = CurrentStudentState.StudentId;
        Debug.Log($"Migrating legacy PlayerPrefs to StudentPrefs for {studId}");

        // Check if we have old-style keys and migrate them
        if (PlayerPrefs.HasKey("currentHearts") && !StudentPrefs.HasKey("currentHearts"))
        {
            StudentPrefs.SetInt("currentHearts", PlayerPrefs.GetInt("currentHearts", 3));

            var chapters = PlayerPrefs.GetString("unlockedChapters", "");
            if (!string.IsNullOrEmpty(chapters)) StudentPrefs.SetString("unlockedChapters", chapters);

            var stories = PlayerPrefs.GetString("unlockedStories", "");
            if (!string.IsNullOrEmpty(stories)) StudentPrefs.SetString("unlockedStories", stories);

            var achievements = PlayerPrefs.GetString("unlockedAchievements", "");
            if (!string.IsNullOrEmpty(achievements)) StudentPrefs.SetString("unlockedAchievements", achievements);

            var artifacts = PlayerPrefs.GetString("unlockedArtifacts", "");
            if (!string.IsNullOrEmpty(artifacts)) StudentPrefs.SetString("unlockedArtifacts", artifacts);

            var civilizations = PlayerPrefs.GetString("unlockedCivilizations", "");
            if (!string.IsNullOrEmpty(civilizations)) StudentPrefs.SetString("unlockedCivilizations", civilizations);

            var codex = PlayerPrefs.GetString("unlockedCodex", "");
            if (!string.IsNullOrEmpty(codex)) StudentPrefs.SetString("unlockedCodex", codex);

            StudentPrefs.Save();

            // Clear old keys
            PlayerPrefs.DeleteKey("currentHearts");
            PlayerPrefs.DeleteKey("unlockedChapters");
            PlayerPrefs.DeleteKey("unlockedStories");
            PlayerPrefs.DeleteKey("unlockedAchievements");
            PlayerPrefs.DeleteKey("unlockedArtifacts");
            PlayerPrefs.DeleteKey("unlockedCivilizations");
            PlayerPrefs.DeleteKey("unlockedCodex");
            PlayerPrefs.Save();

            Debug.Log("Legacy PlayerPrefs migration completed");
        }
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

        // Codex: convert dictionary to list for serialization
        var codexList = new List<CodexEntry>();
        foreach (var kvp in gp.unlockedCodex)
        {
            codexList.Add(new CodexEntry
            {
                characterId = kvp.Key,
                stories = kvp.Value as List<string> ?? new List<string>()
            });
        }

        StudentPrefs.SetString("unlockedCodex", JsonUtility.ToJson(new CodexWrapper { entries = codexList }));

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
        StudentPrefs.DeleteKey("unlockedCodex");
        StudentPrefs.Save();
        Debug.Log("Cleared corrupted StudentPrefs cache");
    }

    #endregion

    #region Firebase Saving/Loading

    private void SaveStudentProgressToFirebase()
    {
        if (CurrentStudentState?.Progress == null || string.IsNullOrEmpty(CurrentStudentState.StudentId))
        {
            Debug.LogWarning("Cannot save StudentProgress: missing state or StudentId");
            return;
        }

        var sp = CurrentStudentState.Progress;
        var docRef = db.Collection("studentProgress").Document(CurrentStudentState.StudentId);

        var data = new Dictionary<string, object>
        {
            { "currentStory", sp.currentStory },
            { "overallScore", sp.overallScore },
            { "successRate", sp.successRate },
            { "isRemoved", sp.isRemoved },
            { "dateUpdated", Timestamp.GetCurrentTimestamp() }
        };

        docRef.SetAsync(data, SetOptions.MergeAll).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
                Debug.Log($"StudentProgress saved for {CurrentStudentState.StudentId}");
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
            { "unlockedCodex", gp.unlockedCodex },
            { "currentHearts", gp.currentHearts },
            { "lastUpdated", gp.lastUpdated },
            { "isRemoved", gp.isRemoved }
        };

        docRef.SetAsync(data).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
                Debug.Log("GameProgress saved to Firebase successfully!");
            else
                Debug.LogError("Failed to save GameProgress to Firebase: " + task.Exception);
        });
    }

    #endregion

    private void SaveProgress()
    {
        CurrentStudentState.GameProgress.lastUpdated = Timestamp.GetCurrentTimestamp();
        SaveToStudentPrefs();
        SaveProgressToFirebase();
        SaveStudentProgressToFirebase();
    }

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

    // ... (rest of your methods remain the same - RecordQuizAttempt, UpdateStudentProgressAndLeaderboard, etc.)
    // I'll continue with the rest if you need them, but they don't need changes for the PlayerPrefs refactor

    public void RecordQuizAttempt(string quizId, int score, int total, bool isPassed)
    {
        string studId = CurrentStudentState.StudentId;
        if (string.IsNullOrEmpty(studId))
        {
            Debug.LogError("No student ID found for recording quiz attempt.");
            return;
        }

        var quizAttemptsRef = db.Collection("quizAttempts").Document(studId).Collection("attempts");

        // Step 1: Get existing attempts for this quiz to calculate attemptNumber
        quizAttemptsRef.WhereEqualTo("quizId", quizId).GetSnapshotAsync().ContinueWith(task =>
        {
            if (task.IsFaulted || !task.IsCompletedSuccessfully)
            {
                Debug.LogError("Failed to fetch quiz attempts: " + task.Exception);
                return;
            }

            int attemptNumber = task.Result.Count + 1;

            // Create attempt model
            var attempt = new QuizAttemptModel(quizId, attemptNumber, score, isPassed);

            // Step 2: Save to Firebase
            quizAttemptsRef.AddAsync(attempt).ContinueWith(addTask =>
            {
                if (addTask.IsCompletedSuccessfully)
                {
                    Debug.Log($"Quiz attempt saved: {quizId}, Attempt {attemptNumber}, Score {score}");

                    // Step 3: Update StudentProgress and Leaderboard
                    UpdateStudentProgressAndLeaderboard(studId, quizId, score, attemptNumber, isPassed);
                }
                else
                {
                    Debug.LogError("Failed to save quiz attempt: " + addTask.Exception);
                }
            });
        });
    }

    private void UpdateStudentProgressAndLeaderboard(string studId, string quizId, int score, int attemptNumber, bool isPassed)
    {
        if (string.IsNullOrEmpty(studId) || string.IsNullOrEmpty(quizId))
        {
            Debug.LogError("UpdateStudentProgressAndLeaderboard: studId or quizId is null/empty");
            return;
        }

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

            // --- Track Attempts (fix for successRate) ---
            int totalAttempts = progressData.ContainsKey("totalAttempts")
                ? Convert.ToInt32(progressData["totalAttempts"])
                : 0;

            int passedAttempts = progressData.ContainsKey("passedAttempts")
                ? Convert.ToInt32(progressData["passedAttempts"])
                : 0;

            totalAttempts++;
            if (isPassed) passedAttempts++;

            string successRate = totalAttempts > 0
                ? $"{(int)((float)passedAttempts / totalAttempts * 100)}%"
                : "0%";

            // Keep old passedQuizzes (for reference if you still need it)
            var passedQuizzes = progressData.ContainsKey("passedQuizzes")
                ? (progressData["passedQuizzes"] as List<object>).Select(q => q.ToString()).ToHashSet()
                : new HashSet<string>();

            if (isPassed) passedQuizzes.Add(quizId);

            // Prepare update
            var progressUpdate = new Dictionary<string, object>
            {
                { "overallScore", overallScore.ToString() },
                { "successRate", successRate },
                { "perQuizBestScores", perQuizBest },
                { "totalAttempts", totalAttempts },
                { "passedAttempts", passedAttempts },
                { "passedQuizzes", passedQuizzes.ToList() }, // optional but kept
                { "dateUpdated", Timestamp.GetCurrentTimestamp() }
            };

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
    }
}