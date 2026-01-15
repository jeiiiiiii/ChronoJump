using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
using System;
using System.Collections;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance;

    // Store the current game state when entering save menu
    private string currentGameScene = "";
    private int currentGameDialogueIndex = 0;

    private FirebaseFirestore db => FirebaseManager.Instance.DB;

    private bool firebaseSlotsLoaded = false;

    // ✅ Event to notify UI when Firebase finishes loading
    public event Action OnFirebaseSlotsLoaded;

    [System.Serializable]
    public class LocalSaveData
    {
        public string currentScene;
        public int dialogueIndex;
        public string timestamp;

        public LocalSaveData()
        {
            currentScene = "";
            dialogueIndex = 0;
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd h:mm:tt");
        }

        public LocalSaveData(string scene, int dialogue)
        {
            currentScene = scene;
            dialogueIndex = dialogue;
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd h:mm:tt");
        }
    }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ForceLoadFirebaseSaves()
    {
        CaptureCurrentGameState();
        LoadSaveSlotsFromFirebase();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        // NEW: Handle case where SaveLoadManager is created AFTER the scene is loaded
        if (SceneManager.GetActiveScene().name == "SaveAndLoadScene")
        {
            StartCoroutine(WaitForStudentStateAndLoad());
        }
    }


    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // NEW: Wait until GameProgressManager.CurrentStudentState exists
    private IEnumerator WaitForStudentStateAndLoad()
    {
        float timeout = 5f; // seconds
        float elapsed = 0f;

        while ((GameProgressManager.Instance == null || GameProgressManager.Instance.CurrentStudentState == null)
                && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (GameProgressManager.Instance?.CurrentStudentState == null)
        {
            Debug.LogWarning("[SaveLoadManager] WaitForStudentStateAndLoad timed out — student state still null.");
            yield break;
        }

        LoadSaveSlotsFromFirebase();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"SaveLoadManager: Scene loaded - {scene.name}");

        if (scene.name == "SaveAndLoadScene")
        {
            Debug.Log("SaveAndLoadScene detected, starting Firebase load process");
            CaptureCurrentGameState();

            // FIXED: Add a timeout to prevent infinite waiting
            StartCoroutine(WaitForStudentStateWithTimeout());
        }
    }

    // FIXED: Add timeout to prevent infinite waiting
    private IEnumerator WaitForStudentStateWithTimeout()
    {
        float timeout = 10f; // seconds
        float elapsed = 0f;

        while ((GameProgressManager.Instance == null || GameProgressManager.Instance.CurrentStudentState == null)
                && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (GameProgressManager.Instance?.CurrentStudentState == null)
        {
            Debug.LogWarning("[SaveLoadManager] WaitForStudentStateWithTimeout timed out — student state still null.");

            // FIXED: Don't crash or reload, just show empty slots
            var saveLoadUI = FindFirstObjectByType<SaveLoadUI>();
            if (saveLoadUI != null)
            {
                saveLoadUI.RefreshSlotDisplay();
            }
            yield break;
        }

        LoadSaveSlotsFromFirebase();
    }


    private void CaptureCurrentGameState()
    {
        // Use StudentPrefs instead of student-specific PlayerPrefs keys
        currentGameScene = StudentPrefs.GetString("LastScene", "");

        if (!string.IsNullOrEmpty(currentGameScene))
        {
            string prefKey = GetDialogueIndexKey(currentGameScene);
            currentGameDialogueIndex = StudentPrefs.GetInt(prefKey, 0);
            Debug.Log($"Captured game state - Scene: {currentGameScene}, Dialogue: {currentGameDialogueIndex}");
        }
        else
        {
            Debug.LogWarning("No scene information found when entering save menu");
        }
    }

    #region Firebase Save Operations

    private void LoadSaveSlotFromFirebase(int slotNumber, string studentId, System.Action onComplete = null)
    {
        string documentId = $"{studentId}_slot_{slotNumber}";

        Debug.Log($"[SaveLoadManager] Fetching slot {slotNumber}: {documentId}");

        db.Collection("saveData").Document(documentId).GetSnapshotAsync().ContinueWith(task =>
        {
            UnityDispatcher.RunOnMainThread(() =>
            {
                try
                {
                    if (task.IsCompletedSuccessfully && task.Result.Exists)
                    {
                        Debug.Log($"[SaveLoadManager] ✅ Found Firebase data for slot {slotNumber}");

                        var firebaseSave = task.Result.ConvertTo<SaveData>();
                        Debug.Log($"[SaveLoadManager] Firebase data - Scene: {firebaseSave.currentScene}, Dialogue: {firebaseSave.dialogueIndex}, Timestamp: {firebaseSave.timestamp}");

                        var localSave = new LocalSaveData(firebaseSave.currentScene, firebaseSave.dialogueIndex)
                        {
                            timestamp = firebaseSave.timestamp
                        };

                        // Check if local file already exists
                        string filePath = GetSaveFilePath(slotNumber);
                        bool fileExistedBefore = File.Exists(filePath);
                        Debug.Log($"[SaveLoadManager] Local file path: {filePath}");
                        Debug.Log($"[SaveLoadManager] File existed before: {fileExistedBefore}");

                        // Ensure directory exists
                        string directory = Path.GetDirectoryName(filePath);
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                            Debug.Log($"[SaveLoadManager] ✅ Created directory: {directory}");
                        }

                        // Write to local file
                        string json = JsonUtility.ToJson(localSave, true);
                        File.WriteAllText(filePath, json);

                        // Verify the file was written
                        bool fileExistsAfter = File.Exists(filePath);
                        Debug.Log($"[SaveLoadManager] ✅ File written successfully: {fileExistsAfter}");
                        Debug.Log($"[SaveLoadManager] ✅ Synced slot {slotNumber} - {firebaseSave.currentScene} -> {filePath}");

                        // Show file content for debugging
                        if (fileExistsAfter)
                        {
                            string writtenContent = File.ReadAllText(filePath);
                            Debug.Log($"[SaveLoadManager] Written file content: {writtenContent}");
                        }
                    }
                    else if (task.IsCompletedSuccessfully && !task.Result.Exists)
                    {
                        Debug.Log($"[SaveLoadManager] ❌ No Firebase save found for slot {slotNumber}");
                    }
                    else
                    {
                        Debug.LogError($"[SaveLoadManager] ❌ Firebase fetch failed for slot {slotNumber}: {task.Exception}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[SaveLoadManager] ❌ Exception syncing slot {slotNumber}: {e.Message}");
                    Debug.LogError($"[SaveLoadManager] Stack trace: {e.StackTrace}");
                }
                finally
                {
                    Debug.Log($"[SaveLoadManager] 🔄 Completed processing slot {slotNumber}, calling callback");
                    onComplete?.Invoke();
                }
            });
        });
    }

    // FIXED: Remove the infinite reload loop
    private IEnumerator DelayedVerifyAndReloadIfNeeded()
    {
        yield return new WaitForSeconds(0.15f); // let IO/UI settle

        bool anyLocal = false;
        for (int i = 1; i <= 4; i++)
        {
            if (HasSaveFile(i))
            {
                anyLocal = true;
                break;
            }
        }

        if (!anyLocal)
        {
            Debug.LogWarning("[SaveLoadManager] No local saves detected — this is normal for new players.");
            // REMOVED the SceneManager.LoadScene call that was causing infinite loop
        }
    }
    // FIXED: Add this method to safely handle user switching
    public void OnUserSwitched(string newStudentId)
    {
        Debug.Log($"[SaveLoadManager] User switched to: {newStudentId}");

        // Reset state
        firebaseSlotsLoaded = false;
        currentGameScene = "";
        currentGameDialogueIndex = 0;
    }


    private void LoadSaveSlotsFromFirebase()
    {
        if (GameProgressManager.Instance?.CurrentStudentState == null)
        {
            Debug.LogWarning("[SaveLoadManager] ❌ No student logged in, using local saves only");
            return;
        }

        string studentId = GameProgressManager.Instance.CurrentStudentState.StudentId;
        int completedSlots = 0;

        Debug.Log($"[SaveLoadManager] 🚀 Starting LoadSaveSlotsFromFirebase for student: {studentId}");

        // FIXED: Check if we have any local saves first to avoid unnecessary Firebase calls
        bool hasAnyLocalSaves = false;
        for (int i = 1; i <= 4; i++)
        {
            if (HasSaveFile(i))
            {
                hasAnyLocalSaves = true;
                break;
            }
        }

        for (int slot = 1; slot <= 4; slot++)
        {
            LoadSaveSlotFromFirebase(slot, studentId, () =>
            {
                completedSlots++;
                Debug.Log($"[SaveLoadManager] 📊 Progress: {completedSlots}/4 slots completed");

                if (completedSlots >= 4)
                {
                    Debug.Log("[SaveLoadManager] ✅ All Firebase slots processed, updating UI");

                    // Notify listeners
                    firebaseSlotsLoaded = true;
                    OnFirebaseSlotsLoaded?.Invoke();

                    var saveLoadUI = FindFirstObjectByType<SaveLoadUI>();
                    if (saveLoadUI != null)
                    {
                        Debug.Log("[SaveLoadManager] 🎯 Found SaveLoadUI, calling RefreshSlotDisplay()");
                        UnityDispatcher.RunOnMainThread(() =>
                        {
                            saveLoadUI.RefreshSlotDisplay();
                        });
                    }
                    else
                    {
                        Debug.LogWarning("[SaveLoadManager] ❌ SaveLoadUI not found - cannot refresh display!");
                    }

                    // FIXED: Only verify, don't reload scene
                    StartCoroutine(DelayedVerifyAndReloadIfNeeded());
                }
            });
        }
    }


    public void SaveGame(int slotNumber)
    {
        LocalSaveData saveData;
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene != "SaveAndLoadScene")
        {
            int currentDialogue = GetCurrentDialogueIndex();
            saveData = new LocalSaveData(currentScene, currentDialogue);
            Debug.Log($"Direct save from {currentScene} at dialogue {currentDialogue}");
        }
        else
        {
            if (!string.IsNullOrEmpty(currentGameScene))
            {
                saveData = new LocalSaveData(currentGameScene, currentGameDialogueIndex);
                Debug.Log($"Save from menu - Scene: {currentGameScene}, Dialogue: {currentGameDialogueIndex}");
            }
            else
            {
                Debug.LogWarning("No game state captured to save");
                return;
            }
        }

        SaveToLocalFile(slotNumber, saveData);
        SaveToFirebase(slotNumber, saveData);

        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.CommitProgress();
        }
    }

    private void SaveToLocalFile(int slotNumber, LocalSaveData saveData)
    {
        string json = JsonUtility.ToJson(saveData, true);
        string filePath = GetSaveFilePath(slotNumber);

        try
        {
            File.WriteAllText(filePath, json);
            Debug.Log($"Game saved to local slot {slotNumber} - Scene: {saveData.currentScene}, Dialogue: {saveData.dialogueIndex}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game locally: {e.Message}");
        }
    }

    private void SaveToFirebase(int slotNumber, LocalSaveData localSave)
    {
        if (GameProgressManager.Instance?.CurrentStudentState == null)
        {
            Debug.LogWarning("No student logged in, skipping Firebase save");
            return;
        }

        string studentId = GameProgressManager.Instance.CurrentStudentState.StudentId;
        string documentId = $"{studentId}_slot_{slotNumber}";

        var firebaseSave = new SaveData(
            studentId,
            localSave.currentScene,
            localSave.dialogueIndex,
            GameProgressManager.Instance.CurrentStudentState.GameProgress
        )
        {
            timestamp = localSave.timestamp
        };

        db.Collection("saveData").Document(documentId).SetAsync(firebaseSave).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log($"Game saved to Firebase slot {slotNumber}");
            }
            else
            {
                Debug.LogError($"Failed to save to Firebase: {task.Exception}");
            }
        });
    }

    #endregion

    // Helper method to determine if a save should be overwritten
    private bool ShouldOverwriteSave(string existingScene, string newScene)
    {
        if (string.IsNullOrEmpty(existingScene)) return true;

        // Define chapter progression order
        Dictionary<string, int> chapterOrder = new Dictionary<string, int>()
    {
        // Sumerian chapters
        {"SumerianSceneOne", 1}, {"SumerianScene1", 1},
        {"SumerianSceneTwo", 2}, {"SumerianScene2", 2},
        {"SumerianSceneThree", 3}, {"SumerianScene3", 3},
        {"SumerianSceneFour", 4}, {"SumerianScene4", 4},
        {"SumerianSceneFive", 5}, {"SumerianScene5", 5},
        {"SumerianSceneSix", 6}, {"SumerianScene6", 6},
        {"SumerianSceneSeven", 7}, {"SumerianScene7", 7},
        
        // Sumerian Challenge scenes
        {"SumerianFirstRecallChallenges", 2}, {"SumerianSecondRecallChallenges", 4},
        {"SumerianThirdRecallChallenges", 6}, {"SumerianFourthRecallChallenges", 8},
        {"SumerianFifthRecallChallenges", 10},
        
        // Sumerian Quiz and Artifact scenes
        {"SumerianQuizTime", 11}, {"SumerianArtifactScene", 12},
        
        // Akkadian chapters  
        {"AkkadianSceneOne", 13}, {"AkkadianScene1", 13},
        {"AkkadianSceneTwo", 14}, {"AkkadianScene2", 14},
        {"AkkadianSceneThree", 15}, {"AkkadianScene3", 15},
        {"AkkadianSceneFour", 16}, {"AkkadianScene4", 16},
        {"AkkadianSceneFive", 17}, {"AkkadianScene5", 17},
        {"AkkadianSceneSix", 18}, {"AkkadianScene6", 18},
        
        // Akkadian Challenge scenes
        {"AkkadianFirstRecallChallenges", 14}, {"AkkadianSecondRecallChallenges", 16},
        {"AkkadianThirdRecallChallenges", 18},
        
        // Akkadian Quiz and Artifact scenes
        {"AkkadianQuizTime", 19}, {"AkkadianArtifactScene", 20},
        
        // Babylonian chapters
        {"BabylonianSceneOne", 21}, {"BabylonianScene1", 21},
        {"BabylonianSceneTwo", 22}, {"BabylonianScene2", 22},
        {"BabylonianSceneThree", 23}, {"BabylonianScene3", 23},
        {"BabylonianSceneFour", 24}, {"BabylonianScene4", 24},
        {"BabylonianSceneFive", 25}, {"BabylonianScene5", 25},
        {"BabylonianSceneSix", 26}, {"BabylonianScene6", 26},
        {"BabylonianSceneSeven", 27}, {"BabylonianScene7", 27},
        
        // Babylonian Challenge scenes
        {"BabylonianFirstRecallChallenges", 22}, {"BabylonianSecondRecallChallenges", 24},
        {"BabylonianThirdRecallChallenges", 26},
        
        // Babylonian Quiz and Artifact scenes
        {"BabylonianQuizTime", 28}, {"BabylonianArtifactScene", 29},
        
        // Assyrian chapters
        {"AssyrianSceneOne", 30}, {"AssyrianScene1", 30},
        {"AssyrianSceneTwo", 31}, {"AssyrianScene2", 31},
        {"AssyrianSceneThree", 32}, {"AssyrianScene3", 32},
        {"AssyrianSceneFour", 33}, {"AssyrianScene4", 33},
        {"AssyrianSceneFive", 34}, {"AssyrianScene5", 34},
        
        // Assyrian Challenge scenes
        {"AssyrianFirstRecallChallenges", 31}, {"AssyrianSecondRecallChallenges", 33},
        
        // Assyrian Quiz and Artifact scenes
        {"AssyrianQuizTime", 35}, {"AssyrianArtifactScene", 36},
        
        // Harappa chapters
        {"HarappaSceneOne", 37}, {"HarappaScene1", 37},
        {"HarappaSceneTwo", 38}, {"HarappaScene2", 38},
        {"HarappaSceneThree", 39}, {"HarappaScene3", 39},
        {"HarappaSceneFour", 40}, {"HarappaScene4", 40},
        {"HarappaSceneFive", 41}, {"HarappaScene5", 41},
        
        // Harappa Challenge scenes
        {"HarappaFirstRecallChallenges", 38}, {"HarappaSecondRecallChallenges", 40},
        
        // Harappa Quiz and Artifact scenes
        {"HarappaQuizTime", 42}, {"HarappaArtifactScene", 43},

        // Sining chapters
        {"SiningSceneOne", 44}, {"SiningScene1", 44},
        {"SiningSceneTwo", 45}, {"SiningScene2", 45},
        {"SiningSceneThree", 46}, {"SiningScene3", 46},
        {"SiningSceneFour", 47}, {"SiningScene4", 47},
        {"SiningSceneFive", 48}, {"SiningScene5", 48},
        
        // Sining Challenge scenes
        {"SiningFirstRecallChallenges", 45}, {"SiningSecondRecallChallenges", 47},
        
        // Sining Quiz and Artifact scenes
        {"SiningQuizTime", 49}, {"SiningArtifactScene", 50},

        // Huanghe chapters
        {"HuangHeSceneOne", 51}, {"HuangHeScene1", 51},
        {"HuangHeSceneTwo", 52}, {"HuangHeScene2", 52},
        {"HuangHeSceneThree", 53}, {"HuangHeScene3", 53},
        {"HuangHeSceneFour", 54}, {"HuangHeScene4", 54},
        {"HuangHeSceneFive", 55}, {"HuangHeScene5", 55},

        // Huanghe Challenge scenes
        {"HuangHeFirstRecallChallenges", 52}, {"HuangHeSecondRecallChallenges", 54},

        // Huanghe Quiz and Artifact scenes
        {"HuangHeQuizTime", 56}, {"HuangHeArtifactScene", 57},

        // Shang chapters
        {"ShangSceneOne", 58}, {"ShangScene1", 58},
        {"ShangSceneTwo", 59}, {"ShangScene2", 59},
        {"ShangSceneThree", 60}, {"ShangScene3", 60},
        {"ShangSceneFour", 61}, {"ShangScene4", 61},
        {"ShangSceneFive", 62}, {"ShangScene5", 62},

        // Shang Challenge scenes
        {"ShangFirstRecallChallenges", 59}, {"ShangSecondRecallChallenges", 61},

        // Shang Quiz and Artifact scenes
        {"ShangQuizTime", 63}, {"ShangArtifactScene", 64},

        // Nile chapters
        {"NileSceneOne", 65}, {"NileScene1", 65},
        {"NileSceneTwo", 66}, {"NileScene2", 66},
        {"NileSceneThree", 67}, {"NileScene3", 67},
        {"NileSceneFour", 68}, {"NileScene4", 68},
        {"NileSceneFive", 69}, {"NileScene5", 69},

        // Nile Challenge scenes
        {"NileFirstRecallChallenges", 66}, {"NileSecondRecallChallenges", 68},

        // Nile Quiz and Artifact scenes
        {"NileQuizTime", 70}, {"NileArtifactScene", 71},

        // Kingdom chapters
        {"KingdomSceneOne", 72}, {"KingdomScene1", 72},
        {"KingdomSceneTwo", 73}, {"KingdomScene2", 73},
        {"KingdomSceneThree", 74}, {"KingdomScene3", 74},
        {"KingdomSceneFour", 75}, {"KingdomScene4", 75},
        {"KingdomSceneFive", 76}, {"KingdomScene5", 76},

        // Kingdom Challenge scenes
        {"KingdomFirstRecallChallenges", 73}, {"KingdomSecondRecallChallenges", 75},

        // Kingdom Quiz and Artifact scenes
        {"KingdomQuizTime", 77}, {"KingdomArtifactScene", 78},
        };

        int existingChapter = chapterOrder.ContainsKey(existingScene) ? chapterOrder[existingScene] : 0;
        int newChapter = chapterOrder.ContainsKey(newScene) ? chapterOrder[newScene] : 0;

        // Only overwrite if existing save is from same or earlier chapter
        return existingChapter <= newChapter;
    }

    // Optional: Add this method for cleaner code
    public void UpdateSaveSlot(int slotNumber, string sceneName, int dialogueIndex)
    {
        var newSaveData = new LocalSaveData(sceneName, dialogueIndex);
        SaveToLocalFile(slotNumber, newSaveData);
        SaveToFirebase(slotNumber, newSaveData);
    }

    // Then use it in OverwriteAllSavesAfterChallenge:
    public void OverwriteAllSavesAfterChallenge(string nextSceneName, int nextDialogueIndex = 0)
    {
        for (int slot = 1; slot <= 4; slot++)
        {
            if (HasSaveFile(slot))
            {
                var existingSave = GetSaveData(slot);
                if (ShouldOverwriteSave(existingSave.currentScene, nextSceneName))
                {
                    UpdateSaveSlot(slot, nextSceneName, nextDialogueIndex); // Cleaner call
                    Debug.Log($"Overwritten save slot {slot}");
                }
            }
        }
    }

    public bool LoadGame(int slotNumber)
    {
        string filePath = GetSaveFilePath(slotNumber);

        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"Save file for slot {slotNumber} does not exist");
            return false;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            LocalSaveData saveData = JsonUtility.FromJson<LocalSaveData>(json);

            // FIXED: Also load the corresponding Firebase save data to restore GameProgress
            if (GameProgressManager.Instance?.CurrentStudentState != null)
            {
                string studentId = GameProgressManager.Instance.CurrentStudentState.StudentId;
                string documentId = $"{studentId}_slot_{slotNumber}";

                Debug.Log($"Loading GameProgress from Firebase save slot {slotNumber}");

                db.Collection("saveData").Document(documentId).GetSnapshotAsync().ContinueWith(task =>
                {
                    UnityDispatcher.RunOnMainThread(() =>
                    {
                        try
                        {
                            if (task.IsCompletedSuccessfully && task.Result.Exists)
                            {
                                var firebaseSave = task.Result.ConvertTo<SaveData>();

                                // FIXED: Restore GameProgress from the save data
                                if (GameProgressManager.Instance != null)
                                {
                                    GameProgressManager.Instance.LoadGameProgressFromSave(firebaseSave);
                                    Debug.Log($"GameProgress restored from save slot {slotNumber}");
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"No Firebase save data found for slot {slotNumber}, using current GameProgress");
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError($"Failed to load GameProgress from Firebase save: {e.Message}");
                        }
                    });
                });
            }

            StudentPrefs.SetInt("LoadedDialogueIndex", saveData.dialogueIndex);
            StudentPrefs.SetString("LoadedFromSave", "true");
            StudentPrefs.Save();

            Debug.Log($"Loading game - Scene: {saveData.currentScene}, Dialogue Index: {saveData.dialogueIndex}");

            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.RefreshProgress(keepScores: true);
            }

            SceneManager.LoadScene(saveData.currentScene);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
            return false;
        }
    }

    #region Firebase Save Slot Management

    public void DeleteSave(int slotNumber)
    {
        string filePath = GetSaveFilePath(slotNumber);
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                Debug.Log($"Local save file for slot {slotNumber} deleted");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to delete local save file: {e.Message}");
            }
        }

        DeleteFromFirebase(slotNumber);
    }

    private void DeleteFromFirebase(int slotNumber)
    {
        if (GameProgressManager.Instance?.CurrentStudentState == null)
        {
            Debug.LogWarning("No student logged in, skipping Firebase delete");
            return;
        }

        string studentId = GameProgressManager.Instance.CurrentStudentState.StudentId;
        string documentId = $"{studentId}_slot_{slotNumber}";

        db.Collection("saveData").Document(documentId).DeleteAsync().ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log($"Firebase save slot {slotNumber} deleted");
            }
            else
            {
                Debug.LogError($"Failed to delete Firebase save: {task.Exception}");
            }
        });
    }

    public void CheckFirebaseSaveExists(int slotNumber, System.Action<bool> callback)
    {
        if (GameProgressManager.Instance?.CurrentStudentState == null)
        {
            callback?.Invoke(false);
            return;
        }

        string studentId = GameProgressManager.Instance.CurrentStudentState.StudentId;
        string documentId = $"{studentId}_slot_{slotNumber}";

        db.Collection("saveData").Document(documentId).GetSnapshotAsync().ContinueWith(task =>
        {
            bool exists = task.IsCompletedSuccessfully && task.Result.Exists;
            callback?.Invoke(exists);
        });
    }

    #endregion

    private int GetCurrentDialogueIndex()
    {
        var sumerianScene = FindFirstObjectByType<SumerianScene1>();
        if (sumerianScene != null) return sumerianScene.currentDialogueIndex;

        var sumerianScene2 = FindFirstObjectByType<SumerianScene2>();
        if (sumerianScene2 != null) return sumerianScene2.currentDialogueIndex;

        var sumerianScene3 = FindFirstObjectByType<SumerianScene3>();
        if (sumerianScene3 != null) return sumerianScene3.currentDialogueIndex;

        var sumerianScene4 = FindFirstObjectByType<SumerianScene4>();
        if (sumerianScene4 != null) return sumerianScene4.currentDialogueIndex;

        var sumerianScene5 = FindFirstObjectByType<SumerianScene5>();
        if (sumerianScene5 != null) return sumerianScene5.currentDialogueIndex;

        var sumerianScene6 = FindFirstObjectByType<SumerianScene6>();
        if (sumerianScene6 != null) return sumerianScene6.currentDialogueIndex;

        var sumerianScene7 = FindFirstObjectByType<SumerianScene7>();
        if (sumerianScene7 != null) return sumerianScene7.currentDialogueIndex;

        return 0;
    }

    private string GetDialogueIndexKey(string sceneName)
    {
        switch (sceneName)
        {
            case "SumerianSceneOne":
            case "SumerianScene1": return "SumerianSceneOne_DialogueIndex";
            case "SumerianSceneTwo":
            case "SumerianScene2": return "SumerianSceneTwo_DialogueIndex";
            case "SumerianSceneThree":
            case "SumerianScene3": return "SumerianSceneThree_DialogueIndex";
            case "SumerianSceneFour":
            case "SumerianScene4": return "SumerianSceneFour_DialogueIndex";
            case "SumerianSceneFive":
            case "SumerianScene5": return "SumerianSceneFive_DialogueIndex";
            case "SumerianSceneSix":
            case "SumerianScene6": return "SumerianSceneSix_DialogueIndex";
            case "SumerianSceneSeven":
            case "SumerianScene7": return "SumerianSceneSeven_DialogueIndex";
            case "AkkadianSceneOne":
            case "AkkadianScene1": return "AkkadianSceneOne_DialogueIndex";
            case "AkkadianSceneTwo":
            case "AkkadianScene2": return "AkkadianSceneTwo_DialogueIndex";
            case "AkkadianSceneThree":
            case "AkkadianScene3": return "AkkadianSceneThree_DialogueIndex";
            case "AkkadianSceneFour":
            case "AkkadianScene4": return "AkkadianSceneFour_DialogueIndex";
            case "AkkadianSceneFive":
            case "AkkadianScene5": return "AkkadianSceneFive_DialogueIndex";
            case "AkkadianSceneSix":
            case "AkkadianScene6": return "AkkadianSceneSix_DialogueIndex";
            case "BabylonianSceneOne":
            case "BabylonianScene1": return "BabylonianSceneOne_DialogueIndex";
            case "BabylonianSceneTwo":
            case "BabylonianScene2": return "BabylonianSceneTwo_DialogueIndex";
            case "BabylonianSceneThree":
            case "BabylonianScene3": return "BabylonianSceneThree_DialogueIndex";
            case "BabylonianSceneFour":
            case "BabylonianScene4": return "BabylonianSceneFour_DialogueIndex";
            case "BabylonianSceneFive":
            case "BabylonianScene5": return "BabylonianSceneFive_DialogueIndex";
            case "BabylonianSceneSix":
            case "BabylonianScene6": return "BabylonianSceneSix_DialogueIndex";
            case "BabylonianSceneSeven":
            case "BabylonianScene7": return "BabylonianSceneSeven_DialogueIndex";
            case "AssyrianSceneOne":
            case "AssyrianScene1": return "AssyrianSceneOne_DialogueIndex";
            case "AssyrianSceneTwo":
            case "AssyrianScene2": return "AssyrianSceneTwo_DialogueIndex";
            case "AssyrianSceneThree":
            case "AssyrianScene3": return "AssyrianSceneThree_DialogueIndex";
            case "AssyrianSceneFour":
            case "AssyrianScene4": return "AssyrianSceneFour_DialogueIndex";
            case "AssyrianSceneFive":
            case "AssyrianScene5": return "AssyrianSceneFive_DialogueIndex";
            case "HarappaSceneOne":
            case "HarappaScene1": return "HarappaSceneOne_DialogueIndex";
            case "HarappaSceneTwo":
            case "HarappaScene2": return "HarappaSceneTwo_DialogueIndex";
            case "HarappaSceneThree":
            case "HarappaScene3": return "HarappaSceneThree_DialogueIndex";
            case "HarappaSceneFour":
            case "HarappaScene4": return "HarappaSceneFour_DialogueIndex";
            case "HarappaSceneFive":
            case "HarappaScene5": return "HarappaSceneFive_DialogueIndex";
            case "SiningSceneOne":
            case "SiningScene1": return "SiningSceneOne_DialogueIndex";
            case "SiningSceneTwo":
            case "SiningScene2": return "SiningSceneTwo_DialogueIndex";
            case "SiningSceneThree":
            case "SiningScene3": return "SiningSceneThree_DialogueIndex";
            case "SiningSceneFour":
            case "SiningScene4": return "SiningSceneFour_DialogueIndex";
            case "SiningSceneFive":
            case "SiningScene5": return "SiningSceneFive_DialogueIndex";
            case "HuangHeSceneOne":
            case "HuangHeScene1": return "HuangHeSceneOne_DialogueIndex";
            case "HuangHeSceneTwo":
            case "HuangHeScene2": return "HuangHeSceneTwo_DialogueIndex";
            case "HuangHeSceneThree":
            case "HuangHeScene3": return "HuangHeSceneThree_DialogueIndex";
            case "HuangHeSceneFour":
            case "HuangHeScene4": return "HuangHeSceneFour_DialogueIndex";
            case "HuangHeSceneFive":
            case "HuangHeScene5": return "HuangHeSceneFive_DialogueIndex";
            case "ShangSceneOne":
            case "ShangScene1": return "ShangSceneOne_DialogueIndex";
            case "ShangSceneTwo":
            case "ShangScene2": return "ShangSceneTwo_DialogueIndex";
            case "ShangSceneThree":
            case "ShangScene3": return "ShangSceneThree_DialogueIndex";
            case "ShangSceneFour":
            case "ShangScene4": return "ShangSceneFour_DialogueIndex";
            case "ShangSceneFive":
            case "ShangScene5": return "ShangSceneFive_DialogueIndex";
            case "NileSceneOne":
            case "NileScene1": return "NileSceneOne_DialogueIndex";
            case "NileSceneTwo":
            case "NileScene2": return "NileSceneTwo_DialogueIndex";
            case "NileSceneThree":
            case "NileScene3": return "NileSceneThree_DialogueIndex";
            case "NileSceneFour":
            case "NileScene4": return "NileSceneFour_DialogueIndex";
            case "NileSceneFive":
            case "NileScene5": return "NileSceneFive_DialogueIndex";
            case "KingdomSceneOne":
            case "KingdomScene1": return "KingdomSceneOne_DialogueIndex";
            case "KingdomSceneTwo":
            case "KingdomScene2": return "KingdomSceneTwo_DialogueIndex";
            case "KingdomSceneThree":
            case "KingdomScene3": return "KingdomSceneThree_DialogueIndex";
            case "KingdomSceneFour":
            case "KingdomScene4": return "KingdomSceneFour_DialogueIndex";
            case "KingdomSceneFive":
            case "KingdomScene5": return "KingdomSceneFive_DialogueIndex";
            default: return sceneName + "_DialogueIndex";
        }
    }

    public LocalSaveData GetSaveData(int slotNumber)
    {
        string filePath = GetSaveFilePath(slotNumber);
        if (!File.Exists(filePath)) return null;

        try
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<LocalSaveData>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to read save data: {e.Message}");
            return null;
        }
    }

    public bool HasSaveFile(int slotNumber)
    {
        return File.Exists(GetSaveFilePath(slotNumber));
    }

    // -----------------------------
    // Student-specific save path
    // -----------------------------
    // FIXED: Safer file path handling
    private string GetSaveFilePath(int slotNumber)
    {
        string studentId = GameProgressManager.Instance?.CurrentStudentState?.StudentId ?? "DefaultStudent";

        // FIXED: Validate studentId to prevent invalid paths
        if (string.IsNullOrEmpty(studentId) || studentId.Length < 3)
        {
            studentId = "InvalidStudent";
        }

        string studentFolder = Path.Combine(Application.persistentDataPath, studentId);

        if (!Directory.Exists(studentFolder))
        {
            try
            {
                Directory.CreateDirectory(studentFolder);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to create directory {studentFolder}: {e.Message}");
                return Path.Combine(Application.persistentDataPath, $"savegame_slot_{slotNumber}.json");
            }
        }

        return Path.Combine(studentFolder, $"savegame_slot_{slotNumber}.json");
    }

    public void SetCurrentGameState(string sceneName, int dialogueIndex)
    {
        currentGameScene = sceneName;
        currentGameDialogueIndex = dialogueIndex;
        Debug.Log($"Manually set game state - Scene: {sceneName}, Dialogue: {dialogueIndex}");
    }
}