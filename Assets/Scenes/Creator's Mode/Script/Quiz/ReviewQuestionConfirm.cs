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

    // ✅ UPDATED: Save to Firestore by default
    public void SaveCurrentStory(bool saveToFirestore = true)
    {
        if (StoryManager.Instance.currentStory == null)
        {
            Debug.LogError("❌ No current story to save!");
            return;
        }

        var s = StoryManager.Instance.currentStory;

        Debug.Log($"💾 Saving story: {s.storyTitle} (ID: {s.storyId})");

        // Fill with data from your scene
        if (string.IsNullOrEmpty(s.storyId))
            s.storyId = System.Guid.NewGuid().ToString();

        s.dialogues = DialogueStorage.GetAllDialogues();
        s.quizQuestions = StoryManager.Instance.currentStory.quizQuestions;
        s.backgroundPath = s.backgroundPath; // already stored inside story

        // Add or replace in list
        var index = StoryManager.Instance.allStories.FindIndex(st => st.storyId == s.storyId);
        if (index >= 0)
            StoryManager.Instance.allStories[index] = s;
        else
            StoryManager.Instance.allStories.Add(s);

        // ✅ Always save locally
        StoryManager.Instance.SaveStories();
        Debug.Log($"✅ Story saved locally: {s.storyTitle}");

        // ✅ Save to Firestore if requested AND available
        if (saveToFirestore && StoryManager.Instance.UseFirestore)
        {
            Debug.Log($"🔥 Saving story to Firestore: {s.storyTitle}");
            StoryManager.Instance.SaveCurrentStoryToFirestore();
        }
        else if (saveToFirestore && !StoryManager.Instance.UseFirestore)
        {
            Debug.Log("ℹ️ Firestore not available, local save only");
        }
        else
        {
            Debug.Log("ℹ️ Story saved locally only (Firestore save disabled)");
        }
    }
}