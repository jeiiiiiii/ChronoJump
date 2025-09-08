using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClassListView : MonoBehaviour, IClassListView
{
    [Header("Class List")]
    public GameObject classRowPrefab;
    public Transform classListContent;
    
    [Header("Visual Settings")]
    public Color defaultTextColor = new Color32(75, 85, 99, 255);
    public Color selectedTextColor = new Color32(37, 99, 235, 255);
    public Sprite defaultIcon;
    public Sprite selectedIcon;
    
    [Header("Action Buttons")]
    public Button editButton;
    public Button deleteButton;
    public GameObject actionButtonsPanel;

    private Button _lastSelectedButton;
    private string _selectedClassCode;
    private string _selectedClassName;

    // Dictionary to track class codes to buttons
    private Dictionary<string, Button> _classButtons = new Dictionary<string, Button>();

    // Events for delete and edit actions
    public System.Action<string, string> OnDeleteClassRequested { get; set; }
    public System.Action<string, string> OnEditClassRequested { get; set; }

    // Store selection callback so we can trigger it programmatically
    private System.Action<string, string> _onClassSelected;

    private void Start()
    {
        // Initially disable action buttons
        if (editButton != null) editButton.interactable = false;
        if (deleteButton != null) deleteButton.interactable = false;
        
        // Set up button listeners
        if (editButton != null)
            editButton.onClick.AddListener(() => OnEditClassRequested?.Invoke(_selectedClassCode, _selectedClassName));
        
        if (deleteButton != null)
            deleteButton.onClick.AddListener(() => OnDeleteClassRequested?.Invoke(_selectedClassCode, _selectedClassName));
    }
    
    public void ClearClassList()
    {
        foreach (Transform child in classListContent)
        {
            Destroy(child.gameObject);
        }
        _lastSelectedButton = null;
        _classButtons.Clear();
        _selectedClassCode = "";
        _selectedClassName = "";

        if (actionButtonsPanel != null) 
            actionButtonsPanel.SetActive(false);
    }

    public void AddClassToList(string classCode, string className, System.Action<string, string> onClassSelected)
    {
        GameObject row = Instantiate(classRowPrefab, classListContent);
        TextMeshProUGUI rowText = row.GetComponentInChildren<TextMeshProUGUI>();

        if (rowText != null)
        {
            rowText.text = className;
        }

        Button classButton = row.GetComponent<Button>();
        classButton.transition = Selectable.Transition.ColorTint;

        _classButtons[classCode] = classButton;
        _onClassSelected = onClassSelected; // store callback

        classButton.onClick.AddListener(() =>
        {
            OnClassButtonClicked(classButton, classCode, className);
            onClassSelected?.Invoke(classCode, className);
        });

        row.SetActive(true);

        if (actionButtonsPanel != null) 
            actionButtonsPanel.SetActive(true);
    }

    public void SelectFirstClass()
    {
        if (classListContent.childCount > 0)
        {
            Button firstButton = classListContent.GetChild(0).GetComponent<Button>();
            if (firstButton != null)
            {
                var firstClassData = GetClassDataFromButton(firstButton);
                if (firstClassData != null)
                {
                    OnClassButtonClicked(firstButton, firstClassData.Item1, firstClassData.Item2);
                    _onClassSelected?.Invoke(firstClassData.Item1, firstClassData.Item2);
                }
            }
        }
    }
    
    public void SelectClassByCode(string classCode)
    {
        if (_classButtons.TryGetValue(classCode, out Button button))
        {
            var classData = GetClassDataFromButton(button);
            if (classData != null)
            {
                OnClassButtonClicked(button, classCode, classData.Item2);
                _onClassSelected?.Invoke(classCode, classData.Item2);
            }
        }
    }
    
    private System.Tuple<string, string> GetClassDataFromButton(Button button)
    {
        foreach (var kvp in _classButtons)
        {
            if (kvp.Value == button)
            {
                TextMeshProUGUI rowText = button.GetComponentInChildren<TextMeshProUGUI>();
                string className = rowText != null ? rowText.text : "";
                return new System.Tuple<string, string>(kvp.Key, className);
            }
        }
        return null;
    }
    
    private void OnClassButtonClicked(Button clickedButton, string classCode, string className)
    {
        if (_lastSelectedButton != null && _lastSelectedButton != clickedButton)
        {
            ResetButton(_lastSelectedButton);
        }

        HighlightButton(clickedButton);
        _lastSelectedButton = clickedButton;
        _selectedClassCode = classCode;
        _selectedClassName = className;

        if (editButton != null) editButton.interactable = true;
        if (deleteButton != null) deleteButton.interactable = true;
    }
    
    public void RemoveClassFromList(string classCode)
    {
        if (_classButtons.TryGetValue(classCode, out Button button))
        {
            bool wasSelected = (_lastSelectedButton == button);

            if (wasSelected)
            {
                _lastSelectedButton = null;
                _selectedClassCode = "";
                _selectedClassName = "";
            }

            Destroy(button.gameObject);
            _classButtons.Remove(classCode);

            // Auto-select another class if we just removed the selected one
            if (wasSelected && _classButtons.Count > 0)
            {
                SelectFirstClass();
            }
        }

        if (_classButtons.Count == 0 && actionButtonsPanel != null)
        {
            actionButtonsPanel.SetActive(false);
        }
    }

    private void HighlightButton(Button button)
    {
        var colors = button.colors;
        colors.normalColor = new Color(1, 1, 1, 1);
        button.colors = colors;
        
        var bg = button.GetComponent<Image>();
        if (bg != null) bg.color = new Color32(239, 246, 255, 255);
        
        var txt = button.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null) txt.color = selectedTextColor;
        
        var iconTransform = button.transform.Find("ClassIcon");
        if (iconTransform != null)
        {
            var icon = iconTransform.GetComponentInChildren<Image>();
            if (icon != null && selectedIcon != null)
                icon.sprite = selectedIcon;
        }
    }
    
    private void ResetButton(Button button)
    {
        var colors = button.colors;
        colors.normalColor = new Color(1, 1, 1, 0);
        button.colors = colors;
        
        var bg = button.GetComponent<Image>();
        if (bg != null) bg.color = new Color32(239, 246, 255, 255);
        
        var txt = button.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null) txt.color = defaultTextColor;
        
        var iconTransform = button.transform.Find("ClassIcon");
        if (iconTransform != null)
        {
            var icon = iconTransform.GetComponentInChildren<Image>();
            if (icon != null && defaultIcon != null)
                icon.sprite = defaultIcon;
        }
    }

    private void OnDestroy()
    {
        if (editButton != null)
        {
            editButton.onClick.RemoveAllListeners();
        }
        
        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
        }
        
        foreach (var kvp in _classButtons)
        {
            if (kvp.Value != null)
            {
                kvp.Value.onClick.RemoveAllListeners();
            }
        }
        _classButtons.Clear();
    }
}
