using UnityEngine;
using UnityEngine.SceneManagement;

public class QuickSaveLoad : MonoBehaviour
{
    [Header("Keyboard Shortcuts")]
    public KeyCode quickSaveKey = KeyCode.F5;
    public KeyCode quickLoadKey = KeyCode.F9;
    public KeyCode saveMenuKey = KeyCode.Escape;
    
    [Header("Quick Save Slot")]
    public int quickSaveSlot = 1; // Which slot to use for quick save/load
    
    void Start()
    {
        // Ensure SaveLoadManager exists
        if (SaveLoadManager.Instance == null)
        {
            GameObject saveLoadManager = new GameObject("SaveLoadManager");
            saveLoadManager.AddComponent<SaveLoadManager>();
        }
    }
    
    void Update()
    {
        // Quick Save
        if (Input.GetKeyDown(quickSaveKey))
        {
            QuickSave();
        }
        
        // Quick Load
        if (Input.GetKeyDown(quickLoadKey))
        {
            QuickLoad();
        }
        
        // Open Save/Load Menu
        if (Input.GetKeyDown(saveMenuKey))
        {
            OpenSaveLoadMenu();
        }
    }
    
    public void QuickSave()
    {
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SaveGame(quickSaveSlot);
            Debug.Log($"Quick saved to slot {quickSaveSlot}");
            
            // Optional: Show quick save notification
            ShowNotification("Game Saved!");
        }
    }
    
    public void QuickLoad()
    {
        if (SaveLoadManager.Instance != null)
        {
            if (SaveLoadManager.Instance.HasSaveFile(quickSaveSlot))
            {
                SaveLoadManager.Instance.LoadGame(quickSaveSlot);
                Debug.Log($"Quick loaded from slot {quickSaveSlot}");
            }
            else
            {
                Debug.LogWarning("No quick save file found!");
                ShowNotification("No save file found!");
            }
        }
    }
    
    public void OpenSaveLoadMenu()
    {
        SceneManager.LoadScene("SaveAndLoadScene");
    }
    
    // Simple notification system (you can expand this)
    void ShowNotification(string message)
    {
        Debug.Log($"Notification: {message}");
        // You could implement a UI popup here
    }
    
    // Public methods for UI buttons
    public void SaveToSlot(int slotNumber)
    {
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SaveGame(slotNumber);
        }
    }
    
    public void LoadFromSlot(int slotNumber)
    {
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.LoadGame(slotNumber);
        }
    }
}