using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ValidationManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject warningPanel;
    public TMP_Text warningTitleText;
    public TMP_Text warningMessageText;
    public Button confirmButton;
    public Button backButton;

    [Header("Validation Settings")]
    public int maxTitleLength = 100;
    public int maxDescriptionLength = 120;
    public int maxCharacterNameLength = 32; 
    public int maxDialogueLength = 88;     
    public int maxQuestionLength = 110;
    public int maxChoiceLength = 40;
    
    // NEW: Combined limit for name + dialogue
    public int maxNameDialogueCombinedLength = 118;

    private System.Action onConfirmAction;
    private System.Action onBackAction;

    public static ValidationManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirm);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBack);
        }
    }

    public void ShowWarning(string title, string message, System.Action onConfirm = null, System.Action onBack = null)
    {
        if (warningTitleText != null) warningTitleText.text = title;
        if (warningMessageText != null) warningMessageText.text = message;

        this.onConfirmAction = onConfirm;
        this.onBackAction = onBack;

        if (warningPanel != null)
        {
            warningPanel.SetActive(true);
        }
    }

    public void HideWarning()
    {
        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }
        onConfirmAction = null;
        onBackAction = null;
    }

    private void OnConfirm()
    {
        onConfirmAction?.Invoke();
        HideWarning();
    }

    private void OnBack()
    {
        onBackAction?.Invoke();
        HideWarning();
    }

    // Validation Methods
    public ValidationResult ValidateTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
        {
            return new ValidationResult { isValid = true, message = "Title is optional" };
        }

        if (title.Length > maxTitleLength)
        {
            return new ValidationResult
            {
                isValid = false,
                message = $"Title is too long! ({title.Length}/{maxTitleLength} characters)"
            };
        }

        return new ValidationResult { isValid = true };
    }

    public ValidationResult ValidateDescription(string description)
    {
        if (string.IsNullOrEmpty(description))
        {
            return new ValidationResult { isValid = true, message = "Description is optional" };
        }

        if (description.Length > maxDescriptionLength)
        {
            return new ValidationResult
            {
                isValid = false,
                message = $"Description is too long! ({description.Length}/{maxDescriptionLength} characters)"
            };
        }

        return new ValidationResult { isValid = true };
    }

    public ValidationResult ValidateCharacterName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return new ValidationResult { isValid = false, message = "Character name is required!" };
        }

        if (name.Length > maxCharacterNameLength)
        {
            return new ValidationResult
            {
                isValid = false,
                message = $"Character name is too long! ({name.Length}/{maxCharacterNameLength} characters)"
            };
        }

        return new ValidationResult { isValid = true };
    }

    public ValidationResult ValidateDialogue(string dialogue)
    {
        if (string.IsNullOrEmpty(dialogue))
        {
            return new ValidationResult { isValid = false, message = "Dialogue is required!" };
        }

        if (dialogue.Length > maxDialogueLength)
        {
            return new ValidationResult
            {
                isValid = false,
                message = $"Dialogue is too long! ({dialogue.Length}/{maxDialogueLength} characters)"
            };
        }

        return new ValidationResult { isValid = true };
    }

    public ValidationResult ValidateFrame(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
        {
            return new ValidationResult { isValid = false, message = "Background image is required! Please add an image." };
        }

        // Check for default/placeholder values
        if (imagePath.ToLower().Contains("default") ||
            imagePath.ToLower().Contains("none") ||
            imagePath.ToLower().Contains("empty"))
        {
            return new ValidationResult { isValid = false, message = "Please add a valid background image!" };
        }

        // Check if image actually exists
        if (!ImageStorage.ImageExists(imagePath))
        {
            return new ValidationResult { isValid = false, message = "Background image not found! Please add an image." };
        }

        return new ValidationResult { isValid = true };
    }

    public ValidationResult ValidateQuestion(string question)
    {
        if (string.IsNullOrEmpty(question))
        {
            return new ValidationResult { isValid = false, message = "Question is required!" };
        }

        if (question.Length > maxQuestionLength)
        {
            return new ValidationResult
            {
                isValid = false,
                message = $"Question is too long! ({question.Length}/{maxQuestionLength} characters)"
            };
        }

        return new ValidationResult { isValid = true };
    }

    public ValidationResult ValidateChoices(string[] choices)
    {
        if (choices == null || choices.Length != 4)
        {
            return new ValidationResult { isValid = false, message = "All 4 choices are required!" };
        }

        for (int i = 0; i < choices.Length; i++)
        {
            if (string.IsNullOrEmpty(choices[i]?.Trim()))
            {
                return new ValidationResult { isValid = false, message = $"Choice {i + 1} is required!" };
            }

            if (choices[i].Length > maxChoiceLength)
            {
                return new ValidationResult
                {
                    isValid = false,
                    message = $"Choice {i + 1} is too long! ({choices[i].Length}/{maxChoiceLength} characters)"
                };
            }
        }

        return new ValidationResult { isValid = true };
    }

    public bool HasDialogues(List<DialogueLine> dialogues)
    {
        return dialogues != null && dialogues.Count > 0;
    }

    public ValidationResult ValidateNameAndDialogueCombined(string name, string dialogue)
    {
        if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(dialogue))
        {
            return new ValidationResult { isValid = false, message = "Character name and dialogue are both required!" };
        }

        if (string.IsNullOrEmpty(name))
        {
            return new ValidationResult { isValid = false, message = "Character name is required!" };
        }

        if (string.IsNullOrEmpty(dialogue))
        {
            return new ValidationResult { isValid = false, message = "Dialogue is required!" };
        }

        // Calculate combined length
        int combinedLength = name.Length + dialogue.Length;
        
        if (combinedLength > maxNameDialogueCombinedLength)
        {
            return new ValidationResult 
            { 
                isValid = false, 
                message = $"Name + Dialogue is too long! ({combinedLength}/{maxNameDialogueCombinedLength} characters total)\nName: {name.Length} chars, Dialogue: {dialogue.Length} chars"
            };
        }

        return new ValidationResult { isValid = true };
    }
}

[System.Serializable]
public class ValidationResult
{
    public bool isValid;
    public string message;
}

