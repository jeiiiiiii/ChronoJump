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
        SceneManager.LoadScene("Creator'sModeScene");
    }
    public void SaveAndPublish()
    {
        SceneManager.LoadScene("Gamescene");
    }

    // Save in Json file
    public void SaveCurrentStory()
    {
        if (StoryManager.Instance.currentStory == null)
            StoryManager.Instance.currentStory = new StoryData();

        var s = StoryManager.Instance.currentStory;

    // Fill with data from your scene
        if (string.IsNullOrEmpty(s.storyId))
            s.storyId = System.Guid.NewGuid().ToString(); // only if new

        s.dialogues = DialogueStorage.GetAllDialogues();
        s.quizQuestions = StoryManager.Instance.currentStory.quizQuestions;
        s.backgroundPath = s.backgroundPath; // already stored inside story

        // Add or replace in list
        var index = StoryManager.Instance.allStories.FindIndex(st => st.storyId == s.storyId);
        if (index >= 0)
            StoryManager.Instance.allStories[index] = s;
        else
            StoryManager.Instance.allStories.Add(s);

        StoryManager.Instance.SaveStories();
    }

}