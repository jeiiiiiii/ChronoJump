using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string characterName;
    public string dialogueText;
    public string audioFilePath;
    public bool hasAudio;

    public DialogueLine(string name, string text)
    {
        characterName = name;
        dialogueText = text;
        hasAudio = false;
        audioFilePath = "";
    }

    public DialogueLine()
    {
        hasAudio = false;
        audioFilePath = "";
    }
}
