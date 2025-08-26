using UnityEngine;
using TMPro;

public class ReviewQuestions : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentParent;
    public GameObject questionItemPrefab;

    void Start()
    {
        PopulateList();
    }

    public void PopulateList()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        if (AddQuiz.quizQuestions == null || AddQuiz.quizQuestions.Count == 0)
        {
            return;
        }

        foreach (var q in AddQuiz.quizQuestions)
        {
            var go = Instantiate(questionItemPrefab, contentParent);
            var ui = go.GetComponent<QuestionItemUI>();
            if (ui != null) ui.Bind(q);
        }
    }
    public void Refresh() => PopulateList();
}
