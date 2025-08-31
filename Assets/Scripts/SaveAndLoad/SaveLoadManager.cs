using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance;
    
    // Store the current game state when entering save menu
    private string currentGameScene = "";
    private int currentGameDialogueIndex = 0;

    [System.Serializable]
    public class SaveData
    {
        public string currentScene;
        public int dialogueIndex;
        public string timestamp;

        public SaveData()
        {
            currentScene = "";
            dialogueIndex = 0;
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        
        public SaveData(string scene, int dialogue)
        {
            currentScene = scene;
            dialogueIndex = dialogue;
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
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
        }
    }

    private void CaptureCurrentGameState()
    {
        // Get the scene info from PlayerPrefs (set by the scene before coming to save menu)
        currentGameScene = PlayerPrefs.GetString("LastScene", "");
        
        if (!string.IsNullOrEmpty(currentGameScene))
        {
            string prefKey = GetDialogueIndexKey(currentGameScene);
            currentGameDialogueIndex = PlayerPrefs.GetInt(prefKey, 0);
            Debug.Log($"Captured game state - Scene: {currentGameScene}, Dialogue: {currentGameDialogueIndex}");
        }
        else
        {
            Debug.LogWarning("No scene information found when entering save menu");
        }
    }

    public void SaveGame(int slotNumber)
    {
        SaveData saveData;
        string currentScene = SceneManager.GetActiveScene().name;

        // If we are in a gameplay scene (direct save)
        if (currentScene != "SaveAndLoadScene")
        {
            int currentDialogue = GetCurrentDialogueIndex();
            saveData = new SaveData(currentScene, currentDialogue);
            Debug.Log($"Direct save from {currentScene} at dialogue {currentDialogue}");
        }
        else
        {
            // If we're in SaveAndLoadScene, use the captured state
            if (!string.IsNullOrEmpty(currentGameScene))
            {
                saveData = new SaveData(currentGameScene, currentGameDialogueIndex);
                Debug.Log($"Save from menu - Scene: {currentGameScene}, Dialogue: {currentGameDialogueIndex}");
            }
            else
            {
                Debug.LogWarning("No game state captured to save");
                return;
            }
        }

        string json = JsonUtility.ToJson(saveData, true);
        string filePath = GetSaveFilePath(slotNumber);

        try
        {
            File.WriteAllText(filePath, json);
            Debug.Log($"Game saved to slot {slotNumber} - Scene: {saveData.currentScene}, Dialogue: {saveData.dialogueIndex}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
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
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            // Store the dialogue index for the scene to load
            PlayerPrefs.SetInt("LoadedDialogueIndex", saveData.dialogueIndex);
            PlayerPrefs.SetString("LoadedFromSave", "true");
            PlayerPrefs.Save();

            Debug.Log($"Loading game - Scene: {saveData.currentScene}, Dialogue Index: {saveData.dialogueIndex}");

            // Load the saved gameplay scene
            SceneManager.LoadScene(saveData.currentScene);

            Debug.Log($"Game loaded from slot {slotNumber}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
            return false;
        }
    }

    // Helper method to get current dialogue index from active scene
    private int GetCurrentDialogueIndex()
    {
        // Try to find dialogue manager in current scene
        var sumerianScene = FindObjectOfType<SumerianScene1>();
        if (sumerianScene != null)
        {
            return sumerianScene.currentDialogueIndex;
        }

        // Add other scene types here as needed
        // var sumerianScene2 = FindObjectOfType<SumerianScene2>();
        // if (sumerianScene2 != null)
        // {
        //     return sumerianScene2.currentDialogueIndex;
        // }

        return 0; // Default if no dialogue manager found
    }

    // Helper method to get the correct PlayerPrefs key for dialogue index
    private string GetDialogueIndexKey(string sceneName)
    {
        switch (sceneName)
        {
            case "SumerianSceneOne":
            case "SumerianScene1":
                return "SumerianSceneOne_DialogueIndex";
            case "SumerianSceneTwo":
            case "SumerianScene2":
                return "SumerianSceneTwo_DialogueIndex";
            // Add more scenes as needed
            default:
                return sceneName + "_DialogueIndex";
        }
    }

    public SaveData GetSaveData(int slotNumber)
    {
        string filePath = GetSaveFilePath(slotNumber);

        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<SaveData>(json);
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

    public void DeleteSave(int slotNumber)
    {
        string filePath = GetSaveFilePath(slotNumber);

        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                Debug.Log($"Save file for slot {slotNumber} deleted");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to delete save file: {e.Message}");
            }
        }
    }

    private string GetSaveFilePath(int slotNumber)
    {
        return Path.Combine(Application.persistentDataPath, $"savegame_slot_{slotNumber}.json");
    }

    // Method to manually set current game state (alternative approach)
    public void SetCurrentGameState(string sceneName, int dialogueIndex)
    {
        currentGameScene = sceneName;
        currentGameDialogueIndex = dialogueIndex;
        Debug.Log($"Manually set game state - Scene: {sceneName}, Dialogue: {dialogueIndex}");
    }
}