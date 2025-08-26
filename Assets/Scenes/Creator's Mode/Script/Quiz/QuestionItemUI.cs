using UnityEngine;
using TMPro;

public class QuestionItemUI : MonoBehaviour
{
    [Header("Text fields")]
    public TMP_Text questionText;
    public TMP_Text[] choiceTexts = new TMP_Text[4];

    public void Bind(Question q)
    {
        if (questionText != null)
            questionText.text = q.questionText;

        for (int i = 0; i < choiceTexts.Length; i++)
        {
            if (choiceTexts[i] == null) continue;
            choiceTexts[i].text = q.choices != null && i < q.choices.Length ? q.choices[i] : "";

            choiceTexts[i].color = (i == q.correctAnswerIndex) ? Color.blue : Color.black;
            choiceTexts[i].fontStyle = (i == q.correctAnswerIndex) ? FontStyles.Bold : FontStyles.Normal;
        }
    }
}
