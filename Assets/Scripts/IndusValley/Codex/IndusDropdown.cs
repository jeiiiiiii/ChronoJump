using UnityEngine;
using TMPro;

public class IndusDropdownHandler : MonoBehaviour
{
    public TMP_Dropdown dropdown;

    void Start()
    {
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    void OnDropdownValueChanged(int index)
    {
        string selectedOption = dropdown.options[index].text;
        Debug.Log("Selected: " + selectedOption);
    }
}
