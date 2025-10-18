using System;

[System.Serializable]
public class DialogueLine
{
    public string characterName;
    public string dialogueText;
    
    // TTS fields
    public string audioFilePath = "";
    public bool hasAudio = false;
    public string selectedVoiceId = "21m00Tcm4TlvDq8ikWAM"; // Default to Rachel
    
    public DialogueLine(string name, string text)
    {
        characterName = name;
        dialogueText = text;
        hasAudio = false;
        audioFilePath = "";
        selectedVoiceId = "21m00Tcm4TlvDq8ikWAM";
    }
    
    public DialogueLine(string name, string text, string voiceId)
    {
        characterName = name;
        dialogueText = text;
        hasAudio = false;
        audioFilePath = "";
        selectedVoiceId = voiceId;
    }
    
    public DialogueLine()
    {
        hasAudio = false;
        audioFilePath = "";
        selectedVoiceId = "21m00Tcm4TlvDq8ikWAM";
    }
}