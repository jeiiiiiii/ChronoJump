using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string characterName;
    public string dialogueText;

    public DialogueLine(string name, string text)
    {
        characterName = name;
        dialogueText = text;
    }
}
