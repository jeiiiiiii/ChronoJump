using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public class ReviewQuestionConfirm : MonoBehaviour
{
    public Image BlurBGImage;
    public Image ConfirmImage;
    public Button FinishButton;
    public Button BackButton;
    public Button SaveButton; // Reference to your Save button
    public Button SaveAndPublishButton; // Reference to your Save & Publish button
    
    [Header("Save Progress UI")]
    public GameObject saveProgressPanel;
    public TextMeshProUGUI saveStatusText;

    private bool isSaving = false;

    void Start()
    {
        BlurBGImage.gameObject.SetActive(false);
        ConfirmImage.gameObject.SetActive(false);
        
        if (saveProgressPanel != null)
        {
            saveProgressPanel.SetActive(false);
        }

        if (FinishButton != null)
        {
            FinishButton.onClick.AddListener(OnFinishButtonClicked);
        }
        if (BackButton != null)
        {
            BackButton.onClick.AddListener(Back);
        }
    }

    void OnFinishButtonClicked()
    {
        BlurBGImage.gameObject.SetActive(true);
        ConfirmImage.gameObject.SetActive(true);
    }

    void Back()
    {
        BlurBGImage.gameObject.SetActive(false);
        ConfirmImage.gameObject.SetActive(false);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Creator'sModeScene");
    }

    public void Backk()
    {
        SceneManager.LoadScene("CreateNewAddQuizScene");
    }

    // ✅ SUPER SIMPLE: Just disable buttons and proceed
    public void Save()
    {
        if (isSaving) return;
        
        Debug.Log("💾 Save button clicked");
        isSaving = true;
        SetButtonsInteractable(false);
        ShowSaveProgress("Saving...");
        
        // Save locally immediately
        SaveCurrentStoryLocal();
        
        // Firestore save happens in background (fire and forget)
        if (StoryManager.Instance.UseFirestore)
        {
            _ = StoryManager.Instance.SaveCurrentStoryToFirestore();
        }
        
        // Proceed to next scene immediately
        SceneManager.LoadScene("Creator'sModeScene");
    }

    public void SaveAndPublish()
    {
        if (isSaving) return;

        Debug.Log("🚀 Save and Publish button clicked");
        isSaving = true;
        SetButtonsInteractable(false);
        ShowSaveProgress("Saving...");

        // Save locally immediately
        SaveCurrentStoryLocal();

        // Firestore save happens in background (fire and forget)
        if (StoryManager.Instance.UseFirestore)
        {
            _ = StoryManager.Instance.SaveCurrentStoryToFirestore();
        }

        // Proceed to publish scene immediately
        SceneManager.LoadScene("StoryPublish");
    }


    // ✅ NEW: Enable/disable all action buttons
    private void SetButtonsInteractable(bool interactable)
    {
        if (SaveButton != null)
            SaveButton.interactable = interactable;
        
        if (SaveAndPublishButton != null)
            SaveAndPublishButton.interactable = interactable;
        
        if (FinishButton != null)
            FinishButton.interactable = interactable;
        
        // You can add more buttons here as needed
    }

    // Local save only (synchronous)
    private void SaveCurrentStoryLocal()
    {
        if (StoryManager.Instance?.currentStory == null)
        {
            Debug.LogError("❌ No current story to save!");
            return;
        }

        var s = StoryManager.Instance.currentStory;

        Debug.Log($"💾 Saving story: {s.storyTitle} (ID: {s.storyId})");

        // Ensure story ID exists
        if (string.IsNullOrEmpty(s.storyId))
            s.storyId = System.Guid.NewGuid().ToString();

        // Fill with data from your scene
        s.dialogues = DialogueStorage.GetAllDialogues();
        s.quizQuestions = StoryManager.Instance.currentStory.quizQuestions;

        // Set timestamps
        if (string.IsNullOrEmpty(s.createdAt))
        {
            s.createdAt = System.DateTime.Now.ToString();
        }
        s.updatedAt = System.DateTime.Now.ToString();

        // Get story index
        int storyIndex = ImageStorage.CurrentStoryIndex;
        if (storyIndex < 0 || storyIndex > 5)
        {
            Debug.LogError($"❌ Invalid story index from ImageStorage: {storyIndex}");
            storyIndex = 0;
        }

        s.storyIndex = storyIndex;

        var stories = StoryManager.Instance.allStories;

        // Ensure list is big enough
        while (stories.Count <= storyIndex)
        {
            stories.Add(null);
        }

        // Add/replace at the correct slot
        stories[storyIndex] = s;
        Debug.Log($"✅ Story added to slot {storyIndex}");

        // Always save locally
        StoryManager.Instance.SaveStories();
        Debug.Log($"✅ Story saved locally: {s.storyTitle}");
    }

    private void ShowSaveProgress(string message)
    {
        if (saveProgressPanel != null)
        {
            saveProgressPanel.SetActive(true);
        }
        if (saveStatusText != null)
        {
            saveStatusText.text = message;
        }
    }

    private void HideSaveProgress()
    {
        if (saveProgressPanel != null)
        {
            saveProgressPanel.SetActive(false);
        }
    }
}