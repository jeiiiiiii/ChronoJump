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
        SceneManager.LoadScene("QuizTime");
    }
}