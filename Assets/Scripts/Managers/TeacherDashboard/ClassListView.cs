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
    
    private Button _lastSelectedButton;
    
    public void ClearClassList()
    {
        foreach (Transform child in classListContent)
        {
            Destroy(child.gameObject);
        }
        _lastSelectedButton = null;
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
        
        classButton.onClick.AddListener(() =>
        {
            OnClassButtonClicked(classButton, classCode, className);
            onClassSelected?.Invoke(classCode, className);
        });
        
        row.SetActive(true);
    }
    
    public void SelectFirstClass()
    {
        if (classListContent.childCount > 0)
        {
            Button firstButton = classListContent.GetChild(0).GetComponent<Button>();
            if (firstButton != null)
            {
                OnClassButtonClicked(firstButton, "", "");
            }
        }
    }
    
    private void OnClassButtonClicked(Button clickedButton, string classCode, string className)
    {
        if (_lastSelectedButton != null && _lastSelectedButton != clickedButton)
        {
            ResetButton(_lastSelectedButton);
        }
        
        HighlightButton(clickedButton);
        _lastSelectedButton = clickedButton;
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
}