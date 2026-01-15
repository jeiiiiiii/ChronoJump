using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestionItemUI : MonoBehaviour
{
    public TMP_Text questionText;
    public TMP_Text[] choiceTexts;
    public Button editButton;
    public Button deleteButton;

    private int index;
    private ReviewQuestionManager manager;

    public void Setup(Question q, int idx, ReviewQuestionManager mgr)
    {
        questionText.text = q.questionText;

        // 👇 Display all choices and color the correct one blue
        for (int i = 0; i < choiceTexts.Length && i < q.choices.Length; i++)
        {
            choiceTexts[i].text = q.choices[i];
            if (i == q.correctAnswerIndex)
                choiceTexts[i].color = Color.blue;
            else
                choiceTexts[i].color = Color.black; // or your default color
        }

        index = idx;
        manager = mgr;

        editButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();

        editButton.onClick.AddListener(() => manager.OnEditQuestion(index));
        deleteButton.onClick.AddListener(() => manager.OnDeleteQuestion(index));
    }
}
