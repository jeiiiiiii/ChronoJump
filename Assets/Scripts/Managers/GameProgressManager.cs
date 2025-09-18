using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using System.Linq;
using Firebase.Extensions;

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
        
        LoadProgress();
    }

    #region Gameplay Methods
    // Hearts setter
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

        // Reset game progress
        CurrentStudentState.SetGameProgress(new GameProgressModel
        {
            currentHearts = 3,
            unlockedChapters = new List<string> { "CH001" }, // Default chapter
            unlockedStories = new List<string> { "ST001" }, // Default story
            unlockedAchievements = new List<string>(),
            unlockedArtifacts = new List<string>(),
            unlockedCodex = new Dictionary<string, object>(),
            unlockedCivilizations = new List<string> { "Sumerian" }, // Default civ unlocked
            lastUpdated = Timestamp.GetCurrentTimestamp(),
            isRemoved = false
        });

        // Reset StudentProgress model
        CurrentStudentState.SetProgress(new StudentProgressModel
        {
            currentStory = db.Document("stories/ST001"), // default starting story
            overallScore = "0",
            successRate = "0%",
            isRemoved = false,
            dateUpdated = Timestamp.GetCurrentTimestamp()
        });

        // Save everywhere
        SaveProgress();

        Debug.Log("Started a new game for student: " + CurrentStudentState.StudentId);
    }

    #endregion

    #region Additional Helper Methods

    /// <summary>
    /// Gets all unlocked civilizations for the current student
    /// </summary>
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

    /// <summary>
    /// Gets all locked civilizations for the current student
    /// </summary>
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

    /// <summary>
    /// Checks if any specific content is unlocked
    /// </summary>
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

    /// <summary>
    /// Get current hearts count
    /// </summary>
    public int GetCurrentHearts()
    {
        return CurrentStudentState?.GameProgress?.currentHearts ?? 3;
    }

    /// <summary>
    /// Check if player has enough hearts
    /// </summary>
    public bool HasHearts(int required = 1)
    {
        return GetCurrentHearts() >= required;
    }

    /// <summary>
    /// Add hearts (for purchases, rewards, etc.)
    /// </summary>
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
    /// Migration method to be called when student logs in
    /// </summary>
    public void MigrateFromPlayerPrefs()
    {
        PlayerProgressManager.MigrateToGameProgressManager();
    }

    #endregion

    #region PlayerPrefs Caching

    private string GetKey(string field) => $"{CurrentStudentState.StudentId}_{field}";

    private void SaveProgressToPlayerPrefs()
    {
        var gp = CurrentStudentState.GameProgress;

        PlayerPrefs.SetInt(GetKey("currentHearts"), gp.currentHearts);

        PlayerPrefs.SetString(GetKey("unlockedChapters"),
            JsonUtility.ToJson(new StringListWrapper { list = gp.unlockedChapters }));

        PlayerPrefs.SetString(GetKey("unlockedStories"),
            JsonUtility.ToJson(new StringListWrapper { list = gp.unlockedStories }));

        PlayerPrefs.SetString(GetKey("unlockedAchievements"),
            JsonUtility.ToJson(new StringListWrapper { list = gp.unlockedAchievements }));

        PlayerPrefs.SetString(GetKey("unlockedArtifacts"),
            JsonUtility.ToJson(new StringListWrapper { list = gp.unlockedArtifacts }));

        PlayerPrefs.SetString(GetKey("unlockedCivilizations"),
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

        PlayerPrefs.SetString(GetKey("unlockedCodex"), JsonUtility.ToJson(new CodexWrapper { entries = codexList }));

        PlayerPrefs.Save();
        Debug.Log("GameProgress saved to PlayerPrefs for " + CurrentStudentState.StudentId);
    }

    private void LoadProgress()
    {
        var gp = CurrentStudentState.GameProgress;

        Debug.Log($"Starting LoadProgress for student {CurrentStudentState.StudentId}");

        if (PlayerPrefs.HasKey(GetKey("unlockedChapters")))
        {
            try
            {
                Debug.Log("Found cached PlayerPrefs data, attempting to load...");

                gp.currentHearts = PlayerPrefs.GetInt(GetKey("currentHearts"), 3);

                var chaptersJson = PlayerPrefs.GetString(GetKey("unlockedChapters"), "");
                if (!string.IsNullOrEmpty(chaptersJson))
                {
                    gp.unlockedChapters = JsonUtility.FromJson<StringListWrapper>(chaptersJson).list ?? new List<string> { "CH001" };
                }

                var storiesJson = PlayerPrefs.GetString(GetKey("unlockedStories"), "");
                if (!string.IsNullOrEmpty(storiesJson))
                {
                    gp.unlockedStories = JsonUtility.FromJson<StringListWrapper>(storiesJson).list ?? new List<string> { "ST001" };
                }

                var achievementsJson = PlayerPrefs.GetString(GetKey("unlockedAchievements"), "");
                if (!string.IsNullOrEmpty(achievementsJson))
                {
                    gp.unlockedAchievements = JsonUtility.FromJson<StringListWrapper>(achievementsJson).list ?? new List<string>();
                }

                var artifactsJson = PlayerPrefs.GetString(GetKey("unlockedArtifacts"), "");
                if (!string.IsNullOrEmpty(artifactsJson))
                {
                    gp.unlockedArtifacts = JsonUtility.FromJson<StringListWrapper>(artifactsJson).list ?? new List<string>();
                }

                var civilizationsJson = PlayerPrefs.GetString(GetKey("unlockedCivilizations"), "");
                if (!string.IsNullOrEmpty(civilizationsJson))
                {
                    gp.unlockedCivilizations = JsonUtility.FromJson<StringListWrapper>(civilizationsJson).list ?? new List<string> { "Sumerian" };
                }

                var codexJson = PlayerPrefs.GetString(GetKey("unlockedCodex"), "");
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

                Debug.Log("Successfully loaded GameProgress from PlayerPrefs for " + CurrentStudentState.StudentId);
                
                // Signal completion
                OnInitializationComplete?.Invoke();
                OnInitializationComplete = null;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load from PlayerPrefs: {e.Message}. Falling back to Firebase.");
                
                // Clear corrupted PlayerPrefs and load from Firebase
                ClearPlayerPrefsCache();
                FetchProgressFromFirebase();
            }
        }
        else
        {
            Debug.Log("No cached PlayerPrefs data found, fetching from Firebase...");
            FetchProgressFromFirebase();
        }
    }

    private void ClearPlayerPrefsCache()
    {
        PlayerPrefs.DeleteKey(GetKey("currentHearts"));
        PlayerPrefs.DeleteKey(GetKey("unlockedChapters"));
        PlayerPrefs.DeleteKey(GetKey("unlockedStories"));
        PlayerPrefs.DeleteKey(GetKey("unlockedAchievements"));
        PlayerPrefs.DeleteKey(GetKey("unlockedArtifacts"));
        PlayerPrefs.DeleteKey(GetKey("unlockedCivilizations"));
        PlayerPrefs.DeleteKey(GetKey("unlockedCodex"));
        PlayerPrefs.Save();
        Debug.Log("Cleared corrupted PlayerPrefs cache");
    }

    #endregion

    #region Firebase Saving/Loading

    private void FetchProgressFromFirebase()
    {
        var gp = CurrentStudentState.GameProgress;
        string studId = CurrentStudentState.StudentId;

        Debug.Log($"Fetching GameProgress from Firebase for student {studId}");

        db.Collection("gameProgress").Document(studId).GetSnapshotAsync().ContinueWith(task =>
        {
            UnityDispatcher.RunOnMainThread(() => {
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

                        SaveProgressToPlayerPrefs();

                        Debug.Log("Successfully loaded GameProgress from Firebase for " + studId);
                    }
                    else if (task.IsCompletedSuccessfully && !task.Result.Exists)
                    {
                        Debug.LogWarning($"No GameProgress document found in Firebase for {studId}. Using default values.");
                        // GameProgress already has default values from Initialize(), so we're good
                    }
                    else if (task.IsFaulted)
                    {
                        Debug.LogError($"Firebase fetch failed for {studId}: {task.Exception}");
                        // Use default values already set in GameProgress
                    }
                    else if (task.IsCanceled)
                    {
                        Debug.LogWarning($"Firebase fetch was canceled for {studId}. Using default values.");
                        // Use default values already set in GameProgress
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error processing Firebase data for {studId}: {e.Message}");
                    // Use default values already set in GameProgress
                }
                finally
                {
                    // ALWAYS call the completion callback, regardless of success or failure
                    Debug.Log($"Firebase fetch completed for {studId}, triggering callback");
                    OnInitializationComplete?.Invoke();
                    OnInitializationComplete = null;
                }
            });
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
        SaveProgressToPlayerPrefs();
        SaveProgressToFirebase();
    }
}