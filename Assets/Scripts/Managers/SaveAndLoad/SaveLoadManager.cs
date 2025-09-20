using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance;
    
    // Store the current game state when entering save menu
    private string currentGameScene = "";
    private int currentGameDialogueIndex = 0;

    private FirebaseFirestore db => FirebaseManager.Instance.DB;

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // When SaveAndLoadScene is loaded, capture the current game state
        if (scene.name == "SaveAndLoadScene")
        {
            CaptureCurrentGameState();
            // Load save slots from Firebase when entering save menu
            LoadSaveSlotsFromFirebase();
        }
    }

    private void CaptureCurrentGameState()
    {
        string studentId = GameProgressManager.Instance?.CurrentStudentState?.StudentId ?? "default";

        currentGameScene = PlayerPrefs.GetString(studentId + "_LastScene", "");
        
        if (!string.IsNullOrEmpty(currentGameScene))
        {
            string prefKey = studentId + "_" + GetDialogueIndexKey(currentGameScene);
            currentGameDialogueIndex = PlayerPrefs.GetInt(prefKey, 0);
            Debug.Log($"Captured game state - Scene: {currentGameScene}, Dialogue: {currentGameDialogueIndex}");
        }
        else
        {
            Debug.LogWarning("No scene information found when entering save menu");
        }
    }


    #region Firebase Save Operations

    /// <summary>
    /// Load save slots from Firebase and sync with local files
    /// </summary>
    private void LoadSaveSlotsFromFirebase()
    {
        if (GameProgressManager.Instance?.CurrentStudentState == null)
        {
            Debug.LogWarning("No student logged in, using local saves only");
            return;
        }

        string studentId = GameProgressManager.Instance.CurrentStudentState.StudentId;
        
        // Load all 4 save slots from Firebase
        for (int slot = 1; slot <= 4; slot++)
        {
            LoadSaveSlotFromFirebase(slot, studentId);
        }
    }

    private void LoadSaveSlotFromFirebase(int slotNumber, string studentId)
{
    string documentId = $"{studentId}_slot_{slotNumber}";
    
    db.Collection("saveData").Document(documentId).GetSnapshotAsync().ContinueWith(task =>
    {
        if (task.IsCompletedSuccessfully && task.Result.Exists)
        {
            var firebaseSave = task.Result.ConvertTo<SaveData>();
            
            // Convert Firebase SaveData to Local SaveData and save to file
            var localSave = new LocalSaveData(firebaseSave.currentScene, firebaseSave.dialogueIndex)
            {
                timestamp = firebaseSave.timestamp
            };

            // ✅ Sync into PlayerPrefs as well (per student)
            PlayerPrefs.SetString(studentId + "_LastScene", firebaseSave.currentScene);
            PlayerPrefs.SetInt(studentId + "_" + GetDialogueIndexKey(firebaseSave.currentScene), firebaseSave.dialogueIndex);
            PlayerPrefs.Save();

            // Save to local file for UI display
            string json = JsonUtility.ToJson(localSave, true);
            string filePath = GetSaveFilePath(slotNumber);
            
            try
            {
                File.WriteAllText(filePath, json);
                Debug.Log($"Synced save slot {slotNumber} from Firebase to local file and PlayerPrefs");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to sync save slot {slotNumber} to local: {e.Message}");
            }
        }
        else
        {
            Debug.Log($"No Firebase save found for slot {slotNumber}");
        }
    });
}


    /// <summary>
    /// Save to both local file and Firebase
    /// </summary>
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

        // Save to local file (for UI display)
        SaveToLocalFile(slotNumber, saveData);
        
        // Save to Firebase (for persistence across devices)
        SaveToFirebase(slotNumber, saveData);

        // Commit GameProgressManager progress
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

        // Convert Local SaveData to Firebase SaveData
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

    public void OverwriteAllSavesAfterChallenge(string nextSceneName, int nextDialogueIndex = 0)
    {
        LocalSaveData newSaveData = new LocalSaveData(nextSceneName, nextDialogueIndex);
        
        for (int slot = 1; slot <= 4; slot++) 
        {
            if (HasSaveFile(slot))
            {
                // Update local file
                SaveToLocalFile(slot, newSaveData);
                
                // Update Firebase
                SaveToFirebase(slot, newSaveData);
                
                Debug.Log($"Overwritten save slot {slot} after challenge completion - Scene: {nextSceneName}, Dialogue: {nextDialogueIndex}");
            }
        }
        
        Debug.Log($"All existing saves have been updated to prevent challenge replay exploit");

        // Commit progress when overwriting saves
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.CommitProgress();
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

        // ✅ Use student-specific PlayerPrefs keys
        string studentId = GameProgressManager.Instance?.CurrentStudentState?.StudentId ?? "default";
        PlayerPrefs.SetInt(studentId + "_LoadedDialogueIndex", saveData.dialogueIndex);
        PlayerPrefs.SetString(studentId + "_LoadedFromSave", "true");
        PlayerPrefs.Save();

        Debug.Log($"Loading game for student {studentId} - Scene: {saveData.currentScene}, Dialogue Index: {saveData.dialogueIndex}");

        // Reload GameProgressManager progress (keep scores)
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.RefreshProgress(keepScores: true);
        }

        // Load the saved scene
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

    /// <summary>
    /// Delete save from both local and Firebase
    /// </summary>
    public void DeleteSave(int slotNumber)
    {
        // Delete local file
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

        // Delete from Firebase
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

    /// <summary>
    /// Check if save exists in Firebase (for cross-device sync)
    /// </summary>
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

        return 0; // Default
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

    private string GetSaveFilePath(int slotNumber)
    {
        return Path.Combine(Application.persistentDataPath, $"savegame_slot_{slotNumber}.json");
    }

    public void SetCurrentGameState(string sceneName, int dialogueIndex)
    {
        currentGameScene = sceneName;
        currentGameDialogueIndex = dialogueIndex;
        Debug.Log($"Manually set game state - Scene: {sceneName}, Dialogue: {dialogueIndex}");
    }
}