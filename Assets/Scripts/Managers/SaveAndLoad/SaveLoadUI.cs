using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SaveLoadUI : MonoBehaviour
{
    [Header("Save Slots")]
    public SaveSlotUI[] saveSlots = new SaveSlotUI[4];

    [Header("UI Buttons")]
    public Button backButton;

    // Confirmation dialog components (created dynamically)
    private GameObject confirmationPanel;
    private TextMeshProUGUI confirmationText;
    private Button yesButton;
    private Button noButton;
    private Canvas canvas;

    // Store the slot number for confirmation
    private int pendingSaveSlot = -1;

    [System.Serializable]
    public class SaveSlotUI
    {
        public GameObject slotPanel;
        public Button loadButton;
        public Button saveButton;
        public Button deleteButton;
        public TextMeshProUGUI slotText;
        public TextMeshProUGUI timestampText;
        public TextMeshProUGUI sceneText;
        public Image unlockedImage;
        public Image lockedImage;
    }

    void Start()
    {
        if (SaveLoadManager.Instance == null)
        {
            GameObject saveLoadManager = new GameObject("SaveLoadManager");
            saveLoadManager.AddComponent<SaveLoadManager>();
        }

        // Find the canvas
        canvas = FindObjectOfType<Canvas>();
        
        // NEW: Clear LoadOnly mode if we came from a story scene
        ClearLoadOnlyModeIfFromStoryScene();
        
        SetupSlots();
        UpdateSlotDisplay();
        CreateConfirmationDialog();

        if (backButton != null)
        {
            backButton.onClick.AddListener(GoBack);
        }
    }
    
    // NEW METHOD: Clear LoadOnly mode if we came from a story scene
    void ClearLoadOnlyModeIfFromStoryScene()
    {
        string saveSource = StudentPrefs.GetString("SaveSource", "");
        string lastScene = StudentPrefs.GetString("LastScene", "");
        
        // If we came from a story scene, clear the LoadOnly restriction
        if (saveSource == "StoryScene" && !string.IsNullOrEmpty(lastScene))
        {
            StudentPrefs.DeleteKey("AccessMode"); // Clear LoadOnly mode
            Debug.Log($"Cleared LoadOnly mode - accessed from story scene: {lastScene}");
        }
        else
        {
            Debug.Log($"Maintaining current access mode - SaveSource: {saveSource}, LastScene: {lastScene}");
        }
    }

    void SetupSlots()
    {
        for (int i = 0; i < saveSlots.Length; i++)
        {
            int slotNumber = i + 1;

            if (saveSlots[i].loadButton != null)
                saveSlots[i].loadButton.onClick.AddListener(() => LoadGame(slotNumber));

            if (saveSlots[i].saveButton != null)
            {
                saveSlots[i].saveButton.onClick.AddListener(() => ShowSaveConfirmation(slotNumber));
                
                // NEW: Check if we should disable save buttons
                CheckSaveButtonAvailability(i);
            }

            if (saveSlots[i].deleteButton != null)
            {
            saveSlots[i].deleteButton.onClick.AddListener(() => ShowDeleteConfirmation(slotNumber));
            }

            if (saveSlots[i].slotText != null)
                saveSlots[i].slotText.text = $"Slot {slotNumber}";
        }
    }

    // NEW METHOD: Check if save buttons should be enabled
    void CheckSaveButtonAvailability(int slotIndex)
    {
        // Check access mode - if LoadOnly, disable all save buttons
        string accessMode = StudentPrefs.GetString("AccessMode", "");
        if (accessMode == "LoadOnly")
        {
            DisableSaveButton(slotIndex, "Load Game mode - saving disabled");
            return;
        }
        
        // Check if we came from a story scene and have valid game state
        string lastScene = StudentPrefs.GetString("LastScene", "");
        string saveSource = StudentPrefs.GetString("SaveSource", "");
        
        // Valid story scenes that can save
        bool isValidStoryScene = (lastScene == "SumerianSceneOne" || lastScene == "SumerianScene1" || 
                                 lastScene == "SumerianSceneTwo" || lastScene == "SumerianScene2" ||
                                 lastScene == "SumerianSceneThree" || lastScene == "SumerianScene3" ||
                                 lastScene == "SumerianSceneFour" || lastScene == "SumerianScene4" ||
                                 lastScene == "SumerianSceneFive" || lastScene == "SumerianScene5" ||
                                 lastScene == "SumerianSceneSix" || lastScene == "SumerianScene6" ||
                                 lastScene == "SumerianSceneSeven" || lastScene == "SumerianScene7" ||
                                 lastScene == "AkkadianSceneOne" || lastScene == "AkkadianScene1" || 
                                 lastScene == "AkkadianSceneTwo" || lastScene == "AkkadianScene2" ||
                                 lastScene == "AkkadianSceneThree" || lastScene == "AkkadianScene3" ||
                                 lastScene == "AkkadianSceneFour" || lastScene == "AkkadianScene4" ||
                                 lastScene == "AkkadianSceneFive" || lastScene == "AkkadianScene5" || 
                                 lastScene == "AkkadianSceneSix" || lastScene == "AkkadianScene6" ||    
                                 lastScene == "BabylonianSceneOne" || lastScene == "BabylonianScene1" ||
                                 lastScene == "BabylonianSceneTwo" || lastScene == "BabylonianScene2" ||
                                 lastScene == "BabylonianSceneThree" || lastScene == "BabylonianScene3" ||
                                 lastScene == "BabylonianSceneFour" || lastScene == "BabylonianScene4" ||
                                 lastScene == "BabylonianSceneFive" || lastScene == "BabylonianScene5" ||
                                 lastScene == "BabylonianSceneSix" || lastScene == "BabylonianScene6" ||
                                 lastScene == "BabylonianSceneSeven" || lastScene == "BabylonianScene7" ||
                                 lastScene == "AssyrianSceneOne" || lastScene == "AssyrianScene1" ||
                                 lastScene == "AssyrianSceneTwo" || lastScene == "AssyrianScene2" ||
                                 lastScene == "AssyrianSceneThree" || lastScene == "AssyrianScene3" ||
                                 lastScene == "AssyrianSceneFour" || lastScene == "AssyrianScene4" ||
                                 lastScene == "AssyrianSceneFive" || lastScene == "AssyrianScene5" 
                                 );
        
        bool cameFromStoryScene = saveSource == "StoryScene" && isValidStoryScene;
        
        if (saveSlots[slotIndex].saveButton != null)
        {
            if (cameFromStoryScene)
            {
                // Enable save button if we came from a valid story scene
                saveSlots[slotIndex].saveButton.interactable = true;
                
                // Restore normal button color
                var buttonImage = saveSlots[slotIndex].saveButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = Color.white; // Normal button color
                }
                
                Debug.Log($"Save button {slotIndex + 1} enabled - came from story scene: {lastScene}");
            }
            else
            {
                DisableSaveButton(slotIndex, "No valid story scene state");
            }
        }
    }
    
    // Helper method to disable save button with consistent styling
    void DisableSaveButton(int slotIndex, string reason)
    {
        if (saveSlots[slotIndex].saveButton != null)
        {
            saveSlots[slotIndex].saveButton.interactable = false;
            
            // Gray out the button to show it's disabled
            var buttonImage = saveSlots[slotIndex].saveButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 0.7f); // Grayed out
            }
            
            Debug.Log($"Save button {slotIndex + 1} disabled - {reason}");
        }
    }

    void CreateConfirmationDialog()
    {
        // Create the main confirmation panel
        confirmationPanel = new GameObject("ConfirmationPanel");
        confirmationPanel.transform.SetParent(canvas.transform, false);
        
        // Add and setup the panel background
        Image panelImage = confirmationPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f); // Semi-transparent black background
        
        // Setup panel rect transform to fill screen
        RectTransform panelRect = confirmationPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Create the dialog box (inner panel) - ENLARGED
        GameObject dialogBox = new GameObject("DialogBox");
        dialogBox.transform.SetParent(confirmationPanel.transform, false);
        
        Image dialogBoxImage = dialogBox.AddComponent<Image>();
        dialogBoxImage.color = new Color(0.15f, 0.15f, 0.15f, 0.98f); // Darker, more opaque dialog box
        
        RectTransform dialogRect = dialogBox.GetComponent<RectTransform>();
        dialogRect.sizeDelta = new Vector2(700, 400); // INCREASED SIZE from 400x200 to 600x300
        dialogRect.anchoredPosition = Vector2.zero;

        // Add rounded corners effect (optional visual enhancement)
        dialogBoxImage.type = Image.Type.Sliced;

        // Create confirmation text - ENLARGED
        GameObject textObj = new GameObject("ConfirmationText");
        textObj.transform.SetParent(dialogBox.transform, false);
        
        confirmationText = textObj.AddComponent<TextMeshProUGUI>();
        confirmationText.text = "Confirm Save";
        confirmationText.fontSize = 24; // INCREASED from 18 to 24
        confirmationText.color = Color.white;
        confirmationText.alignment = TextAlignmentOptions.Center;
        confirmationText.fontStyle = FontStyles.Bold; // Make it bold for better visibility
        confirmationText.enableWordWrapping = true;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(640, 220); // INCREASED from 360x100 to 540x150
        textRect.anchoredPosition = new Vector2(0, 50); // Moved up slightly

        // Create Yes button - ENLARGED
        GameObject yesButtonObj = new GameObject("YesButton");
        yesButtonObj.transform.SetParent(dialogBox.transform, false);
        
        yesButton = yesButtonObj.AddComponent<Button>();
        Image yesButtonImage = yesButtonObj.AddComponent<Image>();
        yesButtonImage.color = new Color(0.2f, 0.7f, 0.2f, 1f); // Green
        
        RectTransform yesButtonRect = yesButtonObj.GetComponent<RectTransform>();
        yesButtonRect.sizeDelta = new Vector2(150, 50); // INCREASED from 120x40 to 150x50
        yesButtonRect.anchoredPosition = new Vector2(-90, -120); // Adjusted position

        // Yes button text - ENLARGED
        GameObject yesTextObj = new GameObject("YesText");
        yesTextObj.transform.SetParent(yesButtonObj.transform, false);
        
        TextMeshProUGUI yesButtonText = yesTextObj.AddComponent<TextMeshProUGUI>();
        yesButtonText.text = "Yes";
        yesButtonText.fontSize = 20; // INCREASED from 16 to 20
        yesButtonText.color = Color.white;
        yesButtonText.alignment = TextAlignmentOptions.Center;
        yesButtonText.fontStyle = FontStyles.Bold;
        
        RectTransform yesTextRect = yesTextObj.GetComponent<RectTransform>();
        yesTextRect.sizeDelta = new Vector2(150, 50);
        yesTextRect.anchoredPosition = Vector2.zero;

        // Create No button - ENLARGED
        GameObject noButtonObj = new GameObject("NoButton");
        noButtonObj.transform.SetParent(confirmationPanel.transform, false);
        
        noButton = noButtonObj.AddComponent<Button>();
        Image noButtonImage = noButtonObj.AddComponent<Image>();
        noButtonImage.color = new Color(0.7f, 0.2f, 0.2f, 1f); // Red
        
        RectTransform noButtonRect = noButtonObj.GetComponent<RectTransform>();
        noButtonRect.sizeDelta = new Vector2(150, 50); // INCREASED from 120x40 to 150x50
        noButtonRect.anchoredPosition = new Vector2(90, -120); // Adjusted position

        // No button text - ENLARGED
        GameObject noTextObj = new GameObject("NoText");
        noTextObj.transform.SetParent(noButtonObj.transform, false);
        
        TextMeshProUGUI noButtonText = noTextObj.AddComponent<TextMeshProUGUI>();
        noButtonText.text = "No";
        noButtonText.fontSize = 20; // INCREASED from 16 to 20
        noButtonText.color = Color.white;
        noButtonText.alignment = TextAlignmentOptions.Center;
        noButtonText.fontStyle = FontStyles.Bold;
        
        RectTransform noTextRect = noTextObj.GetComponent<RectTransform>();
        noTextRect.sizeDelta = new Vector2(150, 50);
        noTextRect.anchoredPosition = Vector2.zero;

        // Setup button callbacks
        yesButton.onClick.AddListener(ConfirmSave);
        noButton.onClick.AddListener(CancelSave);

        // Hide the confirmation panel initially
        confirmationPanel.SetActive(false);
    }

    void UpdateSlotDisplay()
    {
        // Re-check save button availability when updating display
        for (int i = 0; i < saveSlots.Length; i++)
        {
            int slotNumber = i + 1;
            var saveData = SaveLoadManager.Instance.GetSaveData(slotNumber);

            if (saveData != null)
            {
                if (saveSlots[i].timestampText != null)
                {
                    saveSlots[i].timestampText.text = saveData.timestamp;
                    saveSlots[i].timestampText.gameObject.SetActive(true);
                }

                if (saveSlots[i].sceneText != null)
                {
                    saveSlots[i].sceneText.text = GetFriendlySceneName(saveData.currentScene);
                    saveSlots[i].sceneText.gameObject.SetActive(true);
                }

                if (saveSlots[i].unlockedImage != null)
                    saveSlots[i].unlockedImage.gameObject.SetActive(true);
                if (saveSlots[i].lockedImage != null)
                    saveSlots[i].lockedImage.gameObject.SetActive(false);

                if (saveSlots[i].loadButton != null)
                    saveSlots[i].loadButton.interactable = true;
                if (saveSlots[i].deleteButton != null)
                    saveSlots[i].deleteButton.interactable = true;
            }
            else
            {
                if (saveSlots[i].timestampText != null)
                {
                    saveSlots[i].timestampText.text = "Empty";
                    saveSlots[i].timestampText.gameObject.SetActive(true);
                }

                if (saveSlots[i].sceneText != null)
                {
                    saveSlots[i].sceneText.text = "";
                    saveSlots[i].sceneText.gameObject.SetActive(false);
                }

                if (saveSlots[i].unlockedImage != null)
                    saveSlots[i].unlockedImage.gameObject.SetActive(false);
                if (saveSlots[i].lockedImage != null)
                    saveSlots[i].lockedImage.gameObject.SetActive(true);

                if (saveSlots[i].loadButton != null)
                    saveSlots[i].loadButton.interactable = false;
                if (saveSlots[i].deleteButton != null)
                    saveSlots[i].deleteButton.interactable = false;
            }

            // NEW: Always check save button availability regardless of slot state
            CheckSaveButtonAvailability(i);
        }
    }

    // Show save confirmation dialog
    void ShowSaveConfirmation(int slotNumber)
    {
        // NEW: Check access mode first
        string accessMode = StudentPrefs.GetString("AccessMode", "");
        if (accessMode == "LoadOnly")
        {
            Debug.Log("Cannot save - accessed via Load Game button");
            ShowCannotSaveMessage();
            return;
        }
        
        // Check if we came from a story scene before showing confirmation
        string lastScene = StudentPrefs.GetString("LastScene", "");
        string saveSource = StudentPrefs.GetString("SaveSource", "");
        
        bool isValidStoryScene = (lastScene == "SumerianSceneOne" || lastScene == "SumerianScene1" || 
                                 lastScene == "SumerianSceneTwo" || lastScene == "SumerianScene2" ||
                                 lastScene == "SumerianSceneThree" || lastScene == "SumerianScene3" ||
                                 lastScene == "SumerianSceneFour" || lastScene == "SumerianScene4" ||
                                 lastScene == "SumerianSceneFive" || lastScene == "SumerianScene5" ||
                                 lastScene == "SumerianSceneSix" || lastScene == "SumerianScene6" ||
                                 lastScene == "SumerianSceneSeven" || lastScene == "SumerianScene7" ||
                                 lastScene == "AkkadianSceneOne" || lastScene == "AkkadianScene1" ||
                                 lastScene == "AkkadianSceneTwo" || lastScene == "AkkadianScene2" ||
                                 lastScene == "AkkadianSceneThree" || lastScene == "AkkadianScene3" ||
                                 lastScene == "AkkadianSceneFour" || lastScene == "AkkadianScene4" || 
                                 lastScene == "AkkadianSceneFive" || lastScene == "AkkadianScene5" ||
                                 lastScene == "AkkadianSceneSix" || lastScene == "AkkadianScene6" ||         
                                 lastScene == "BabylonianSceneOne" || lastScene == "BabylonianScene1" ||
                                 lastScene == "BabylonianSceneTwo" || lastScene == "BabylonianScene2" ||
                                 lastScene == "BabylonianSceneThree" || lastScene == "BabylonianScene3" ||
                                 lastScene == "BabylonianSceneFour" || lastScene == "BabylonianScene4" ||
                                 lastScene == "BabylonianSceneFive" || lastScene == "BabylonianScene5" ||
                                 lastScene == "BabylonianSceneSix" || lastScene == "BabylonianScene6" ||
                                 lastScene == "BabylonianSceneSeven" || lastScene == "BabylonianScene7" ||
                                 lastScene == "AssyrianSceneOne" || lastScene == "AssyrianScene1" ||
                                 lastScene == "AssyrianSceneTwo" || lastScene == "AssyrianScene2" ||
                                 lastScene == "AssyrianSceneThree" || lastScene == "AssyrianScene3" ||
                                 lastScene == "AssyrianSceneFour" || lastScene == "AssyrianScene4" ||
                                 lastScene == "AssyrianSceneFive" || lastScene == "AssyrianScene5" 
                                );
        
        bool cameFromStoryScene = saveSource == "StoryScene" && isValidStoryScene;
        
        if (!cameFromStoryScene)
        {
            Debug.Log("Cannot save - not accessed from a story scene");
            ShowCannotSaveMessage();
            return;
        }

        pendingSaveSlot = slotNumber;
        
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
            
            // Reset button visibility for normal save confirmation
            if (yesButton != null) yesButton.gameObject.SetActive(true);
            if (noButton != null) 
            {
                noButton.gameObject.SetActive(true);
                var noButtonText = noButton.GetComponentInChildren<TextMeshProUGUI>();
                if (noButtonText != null) noButtonText.text = "No";
            }
            
            // Check if slot already has save data
            var existingSaveData = SaveLoadManager.Instance.GetSaveData(slotNumber);
            
            if (existingSaveData != null)
            {
                // Slot has existing data - ask if they want to overwrite
                confirmationText.text = $"Slot {slotNumber} already contains save data from:\n\n{GetFriendlySceneName(existingSaveData.currentScene)}\nSaved: {existingSaveData.timestamp}\n\nDo you want to overwrite this save?";
            }
            else
            {
                // Empty slot - ask if they want to save
                string currentGameScene = StudentPrefs.GetString("LastScene", "Unknown Scene");
                confirmationText.text = $"Do you want to save your current progress?\n\nScene: {GetFriendlySceneName(currentGameScene)}\nSlot: {slotNumber}";
            }
        }
    }
    
    // NEW METHOD: Show message when saving is not allowed
    void ShowCannotSaveMessage()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
            confirmationText.text = "Cannot save from here!\n\nTo save your progress, you need to access the save menu from within a story scene using the save button.";
            
            // Hide the Yes button and change No button to "OK"
            if (yesButton != null) yesButton.gameObject.SetActive(false);
            if (noButton != null) 
            {
                noButton.gameObject.SetActive(true);
                var noButtonText = noButton.GetComponentInChildren<TextMeshProUGUI>();
                if (noButtonText != null) noButtonText.text = "OK";
            }
        }
    }

    // NEW METHOD: Show delete confirmation dialog
    void ShowDeleteConfirmation(int slotNumber)
    {
        var existingSaveData = SaveLoadManager.Instance.GetSaveData(slotNumber);
    
        if (existingSaveData == null)
        {
            Debug.Log($"No save data to delete in slot {slotNumber}");
            return;
        }

        pendingSaveSlot = slotNumber; // Reuse this variable for delete operations
    
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
        
            // Show both buttons for delete confirmation
            if (yesButton != null) yesButton.gameObject.SetActive(true);
            if (noButton != null) 
            {
            noButton.gameObject.SetActive(true);
            var noButtonText = noButton.GetComponentInChildren<TextMeshProUGUI>();
            if (noButtonText != null) noButtonText.text = "No";
            }
        
        // Set up delete confirmation text
            confirmationText.text = $"Are you sure you want to delete this save?\n\nSLOT {slotNumber}:\n{GetFriendlySceneName(existingSaveData.currentScene)}\nSaved: {existingSaveData.timestamp}\n\nThis action cannot be undone!";
        
        // Change the Yes button callback to delete instead of save
            yesButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(ConfirmDelete);
        }
    }

    // Confirm save action
    void ConfirmSave()
    {
    if (pendingSaveSlot != -1)
    {
        // Call the method that actually performs the save
        PerformActualSave(pendingSaveSlot);
    }
    
    // Hide confirmation panel
    if (confirmationPanel != null)
    {
        confirmationPanel.SetActive(false);
    }
    
    pendingSaveSlot = -1;
    }
    // Cancel save action
    void CancelSave()
    {
        // Hide confirmation panel without saving
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
        
        pendingSaveSlot = -1;
        Debug.Log("Save cancelled by user");
    }

    // Confirm delete action
    void ConfirmDelete()
    {
        if (pendingSaveSlot != -1)
        {
            SaveLoadManager.Instance.DeleteSave(pendingSaveSlot);
            UpdateSlotDisplay();
            Debug.Log($"Save deleted from Slot {pendingSaveSlot}");
        }   
    
    // Hide confirmation panel
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    
    // Restore the Yes button callback for saving
        if (yesButton != null)
        {
            yesButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(ConfirmSave);
        }
    
        pendingSaveSlot = -1;
    }

    public void SaveGame(int slotNumber)
    {
    // This method should ONLY show the confirmation dialog, not actually save
    ShowSaveConfirmation(slotNumber);
    }

    void PerformActualSave(int slotNumber)
        {
    // Final validation before saving
    string lastScene = StudentPrefs.GetString("LastScene", "");
    bool hasValidGameState = !string.IsNullOrEmpty(lastScene) && lastScene != "SaveAndLoadScene";
    
    if (!hasValidGameState)
    {
        Debug.LogWarning("Cannot save - no valid game state available");
        return;
    }

    // Actually perform the save operation
    SaveLoadManager.Instance.SaveGame(slotNumber);
    UpdateSlotDisplay();
    Debug.Log($"Game saved to Slot {slotNumber}");
    }

    public void LoadGame(int slotNumber)
    {
        if (SaveLoadManager.Instance.LoadGame(slotNumber))
        {
            Debug.Log($"Loading game from Slot {slotNumber}");
        }
        else
        {
            Debug.LogError($"Failed to load game from Slot {slotNumber}");
        }
    }

    // Replace your GoBack() method in SaveLoadUI.cs with this:

    public void GoBack()
    {
    // Check if we came from a story scene (in-game save button)
    string saveSource = StudentPrefs.GetString("SaveSource", "");
    string lastScene = StudentPrefs.GetString("LastScene", "");
    
    if (saveSource == "StoryScene" && !string.IsNullOrEmpty(lastScene))
    {
        // We came from a story scene via the save button, go back to that scene
        Debug.Log($"Returning to story scene: {lastScene}");
        
        // Clear the save source flag since we're going back
        StudentPrefs.DeleteKey("SaveSource");
        StudentPrefs.Save();
        
        SceneManager.LoadScene(lastScene);
        }
        else
        {
        // Default behavior - go to title screen (for Load Game access)
        Debug.Log("Returning to Title Screen");
        SceneManager.LoadScene("TitleScreen");
        }
    }
    string GetFriendlySceneName(string sceneName)
    {
        switch (sceneName)
        {
            case "SumerianScene1":
            case "SumerianSceneOne": 
                return "Sumerian Scene 1";
            case "SumerianScene2":
            case "SumerianSceneTwo": 
                return "Sumerian Scene 2";
            case "SumerianFirstRecallChallenges": 
                return "Sumerian Challenges";
            case "SumerianScene3":
            case "SumerianSceneThree":
                return "Sumerian Scene 3";
            case "SumerianSecondRecallChallenges": 
                return "Sumerian Challenges";
            case "SumerianScene4":
            case "SumerianSceneFour":
                return "Sumerian Scene 4";
            case "SumerianThirdRecallChallenges": 
                return "Sumerian Challenges";
            case "SumerianScene5":
            case "SumerianSceneFive":
                return "Sumerian Scene 5";
            case "SumerianFourthRecallChallenges": 
                return "Sumerian Challenges";
            case "SumerianScene6":
            case "SumerianSceneSix":
                return "Sumerian Scene 6";
            case "SumerianFifthRecallChallenges": 
                return "Sumerian Challenges";
            case "SumerianScene7":
            case "SumerianSceneSeven":
                return "Sumerian Scene 7";            
            case "AkkadianScene1":
            case "AkkadianSceneOne": 
                return "Akkadian Scene 1";
            case "AkkadianScene2":
            case "AkkadianSceneTwo": 
                return "Akkadian Scene 2";
            case "AkkadianFirstRecallChallenges": 
                return "Akkadian Challenges";
            case "AkkadianScene3":
            case "AkkadianSceneThree": 
                return "Akkadian Scene 3";
            case "AkkadianScene4":
            case "AkkadianSceneFour": 
                return "Akkadian Scene 4";
            case "AkkadianSecondRecallChallenges": 
                return "Akkadian Challenges";
            case "AkkadianScene5":
            case "AkkadianSceneFive": 
                return "Akkadian Scene 5";
            case "AkkadianThirdRecallChallenges": 
                return "Akkadian Challenges";
            case "AkkadianScene6":
            case "AkkadianSceneSix": 
                return "Akkadian Scene 6";            
            case "BabylonianScene1":
            case "BabylonianSceneOne":  
                return "Babylonian Scene 1";
            case "BabylonianScene2":
            case "BabylonianSceneTwo":  
                return "Babylonian Scene 2";
            case "BabylonianScene3":
            case "BabylonianSceneThree":  
                return "Babylonian Scene 3";
            case "BabylonianScene4":
            case "BabylonianSceneFour":  
                return "Babylonian Scene 4";
            case "BabylonianScene5":
            case "BabylonianSceneFive":  
                return "Babylonian Scene 5";
            case "BabylonianScene6":
            case "BabylonianSceneSix":  
                return "Babylonian Scene 6";
            case "BabylonianScene7":
            case "BabylonianSceneSeven":  
                return "Babylonian Scene 7";
            case "AssyrianScene1":
            case "AssyrianSceneOne": 
                return "Assyrian Scene 1";
            case "AssyrianScene2":
            case "AssyrianSceneTwo": 
                return "Assyrian Scene 2";
            case "AssyrianScene3":
            case "AssyrianSceneThree": 
                return "Assyrian Scene 3";
            case "AssyrianScene4":
            case "AssyrianSceneFour": 
                return "Assyrian Scene 4";
            case "AssyrianScene5":
            case "AssyrianSceneFive": 
                return "Assyrian Scene 5";
            default: 
                return sceneName;
        }
    }

    public void DeleteSave(int slotNumber)
    {
        SaveLoadManager.Instance.DeleteSave(slotNumber);
        UpdateSlotDisplay();
    }
}