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

    void Start()
    {
        BlurBGImage.gameObject.SetActive(false);
        ConfirmImage.gameObject.SetActive(false);

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

    public void Save()
    {
        Debug.Log("💾 Save button clicked - Saving story to Firestore");
        SaveCurrentStory(true); // ✅ CHANGED: Now saves to Firestore too
        SceneManager.LoadScene("Creator'sModeScene");
    }

    public void SaveAndPublish()
    {
        Debug.Log("🚀 Save and Publish button clicked - Saving to Firestore and preparing for publish");
        SaveCurrentStory(true); // ✅ Save to Firestore
        SceneManager.LoadScene("StoryPublish");
    }

    public void SaveCurrentStory(bool saveToFirestore = true)
    {
        if (StoryManager.Instance.currentStory == null)
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

        // ✅ FIXED: Get the intended slot index from ImageStorage
        int storyIndex = ImageStorage.CurrentStoryIndex;
        if (storyIndex < 0 || storyIndex > 5)
        {
            Debug.LogError($"❌ Invalid story index from ImageStorage: {storyIndex}");
            storyIndex = 0; // fallback to first slot
        }

        s.storyIndex = storyIndex; // Ensure the story knows its index

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

        // Save to Firestore if requested
        if (saveToFirestore && StoryManager.Instance.UseFirestore)
        {
            Debug.Log($"🔥 Saving story to Firestore: {s.storyTitle}");
            StoryManager.Instance.SaveCurrentStoryToFirestore();
        }
        else if (saveToFirestore && !StoryManager.Instance.UseFirestore)
        {
            Debug.Log("ℹ️ Firestore not available, local save only");
        }
    }

}