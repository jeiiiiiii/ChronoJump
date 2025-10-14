using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ReviewQuestionManager : MonoBehaviour
{
    public Transform contentParent;
    public GameObject questionItemPrefab;

    [Header("Edit Popup")]
    public GameObject editPanel;
    public TMP_InputField questionInput;
    public TMP_InputField[] choiceInputs;
    public Toggle[] choiceToggles;
    public Button saveButton;
    public Button cancelButton;
    private int editingIndex = -1;

    [Header("Delete Popup")]
    public GameObject deletePanel;
    private int deleteIndex = -1;

    public TMP_Text errorText;

    private StoryData CurrentStory => StoryManager.Instance.currentStory; // 👈 always fetch current story

    private void Start()
    {
        Debug.Log("ReviewQuestionManager started — rebuilding list");
        RebuildList();

        if (saveButton != null) saveButton.onClick.AddListener(SaveEdit);
        if (cancelButton != null) cancelButton.onClick.AddListener(CancelEdit);

        for (int i = 0; i < choiceToggles.Length; i++)
        {
            int index = i;
            choiceToggles[i].onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    // Uncheck all other toggles
                    for (int j = 0; j < choiceToggles.Length; j++)
                    {
                        if (j != index)
                            choiceToggles[j].isOn = false;
                    }
                }
            });
        }
    }

    public void RebuildList()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        if (CurrentStory == null || CurrentStory.quizQuestions == null)
        {
            Debug.LogWarning("⚠ No story or quiz questions found");
            return;
        }

        for (int i = 0; i < CurrentStory.quizQuestions.Count; i++)
        {
            Question q = CurrentStory.quizQuestions[i];
            GameObject go = Instantiate(questionItemPrefab, contentParent);
            go.GetComponent<QuestionItemUI>().Setup(q, i, this);
        }
    }

    // --- Edit ---
    public void OnEditQuestion(int index)
    {
        if (CurrentStory == null) return;

        Debug.Log("✏ Editing question at index: " + index);
        editPanel.SetActive(true);
        editingIndex = index;
        var q = CurrentStory.quizQuestions[index];

        // Fill fields with existing data
        questionInput.text = q.questionText;

        for (int i = 0; i < 4; i++)
        {
            choiceInputs[i].text = q.choices[i];
            choiceToggles[i].isOn = (i == q.correctAnswerIndex);
        }
    }

    public void CancelEdit()
    {
        editingIndex = -1;
        editPanel.SetActive(false);
        if (errorText != null) errorText.text = "";
    }

    public void SaveEdit()
    {
        if (editingIndex < 0 || CurrentStory == null) return;

        // Validate question text
        var questionValidation = ValidationManager.Instance.ValidateQuestion(questionInput.text);
        if (!questionValidation.isValid)
        {
            ValidationManager.Instance.ShowWarning("Invalid Question", questionValidation.message);
            return;
        }

        // Gather choices
        string[] updatedChoices = new string[4];
        for (int i = 0; i < 4; i++) updatedChoices[i] = choiceInputs[i].text.Trim();

        // Validate choices
        var choiceValidation = ValidationManager.Instance.ValidateChoices(updatedChoices);
        if (!choiceValidation.isValid)
        {
            ValidationManager.Instance.ShowWarning("Invalid Choices", choiceValidation.message);
            return;
        }

        // Ensure correct answer
        int selected = -1;
        for (int i = 0; i < 4; i++) if (choiceToggles[i].isOn) { selected = i; break; }
        if (selected == -1)
        {
            ValidationManager.Instance.ShowWarning("Missing Answer", "Please select the correct answer!");
            return;
        }

        // ✅ If all passed, update the question
        var q = CurrentStory.quizQuestions[editingIndex];
        q.questionText = questionInput.text;
        q.choices = updatedChoices;
        q.correctAnswerIndex = selected;
        editPanel.SetActive(false);
        editingIndex = -1;
        RebuildList();
    }


    // --- Delete ---
    public void OnDeleteQuestion(int index)
    {
        if (CurrentStory == null) return;

        if (index >= 0 && index < CurrentStory.quizQuestions.Count)
        {
            deleteIndex = index;
            deletePanel.SetActive(true);
        }
    }

    public void ConfirmDelete()
    {
        if (CurrentStory == null) return;

        if (deleteIndex >= 0)
        {
            Debug.Log("🗑 Deleted question: " + CurrentStory.quizQuestions[deleteIndex].questionText);
            CurrentStory.quizQuestions.RemoveAt(deleteIndex);
            deleteIndex = -1;
            RebuildList();
        }
        deletePanel.SetActive(false);

        // ✅ Check if all questions are gone
        if (CurrentStory.quizQuestions == null || CurrentStory.quizQuestions.Count == 0)
        {
                            ValidationManager.Instance.ShowWarning(
                    "No Questions Left",
                    "You’ve deleted all questions! Please add at least one before reviewing.",
                    () => SceneManager.LoadScene("CreateNewAddQuizScene"),
                    () => SceneManager.LoadScene("CreateNewAddQuizScene")
                );  
        }
    }



    public void CancelDelete()
    {
        deleteIndex = -1;
        deletePanel.SetActive(false);
    }
}
