using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReviewQuestionManager : MonoBehaviour
{
    public Transform contentParent;
    public GameObject questionItemPrefab;

    [Header("Edit Popup")]
    public GameObject editPanel;
    public TMP_InputField questionInput;
    public TMP_InputField[] choiceInputs;
    public Toggle[] choiceToggles;
    private int editingIndex = -1;

    [Header("Delete Popup")]
    public GameObject deletePanel;
    private int deleteIndex = -1;

    public TMP_Text errorText;

    private void Start()
    {
        Debug.Log("ReviewQuestionManager started — rebuilding list");
        RebuildList();
    }

    public void RebuildList()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        for (int i = 0; i < AddQuiz.quizQuestions.Count; i++)
        {
            Question q = AddQuiz.quizQuestions[i];
            GameObject go = Instantiate(questionItemPrefab, contentParent);
            go.GetComponent<QuestionItemUI>().Setup(q, i, this);  // 👈 pass full question
        }
    }

    // --- Edit ---
    public void OnEditQuestion(int index)
    {
        editingIndex = index;
        var q = AddQuiz.quizQuestions[index];

        // Fill fields with existing data
        questionInput.text = q.questionText;

        for (int i = 0; i < 4; i++)
        {
            choiceInputs[i].text = q.choices[i];
            choiceToggles[i].isOn = (i == q.correctAnswerIndex); // ✅ Make sure your Question class uses "correctIndex"
        }

        editPanel.SetActive(true);
    }

    public void CancelEdit()
    {
        editingIndex = -1;
        editPanel.SetActive(false);
        if (errorText != null) errorText.text = "";
    }

    public void SaveEdit()
    {
        if (editingIndex < 0) return;

        // ensure at least one toggle is selected
        int selected = -1;
        for (int i = 0; i < 4; i++)
            if (choiceToggles[i].isOn) { selected = i; break; }

        if (selected == -1)
        {
            if (errorText != null) errorText.text = "Please select the correct answer.";
            else Debug.LogWarning("Please select the correct answer.");
            return;
        }

        // ✅ Update the data
        var q = AddQuiz.quizQuestions[editingIndex];
        q.questionText = questionInput.text;

        for (int i = 0; i < 4; i++)
        {
            q.choices[i] = choiceInputs[i].text;
        }

        q.correctAnswerIndex = selected;

        // Save back to list
        AddQuiz.quizQuestions[editingIndex] = q;

        editPanel.SetActive(false);
        editingIndex = -1;
        if (errorText != null) errorText.text = "";

        // Rebuild UI
        RebuildList();
    }

    // --- Delete ---
    public void OnDeleteQuestion(int index)
    {
        if (index >= 0 && index < AddQuiz.quizQuestions.Count)
        {
            Debug.Log("🗑 Deleted question: " + AddQuiz.quizQuestions[index].questionText);
            AddQuiz.quizQuestions.RemoveAt(index);
            RebuildList();
        }
    }
    public void ConfirmDelete()
    {
        if (deleteIndex >= 0)
        {
            AddQuiz.quizQuestions.RemoveAt(deleteIndex);
            deleteIndex = -1;
            RebuildList();
        }
        deletePanel.SetActive(false);
    }

    public void CancelDelete()
    {
        deletePanel.SetActive(false);
    }
}
