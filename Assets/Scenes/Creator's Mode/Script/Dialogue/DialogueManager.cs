// DialogueManager.cs - FIXED to ensure voice ID is always serialized
using System;

[System.Serializable]
public class DialogueLine
{
    public string characterName;
    public string dialogueText;

    // ✅ CRITICAL: These must be public fields (not properties) for Unity serialization
    public string audioFilePath = "";
    public bool hasAudio = false;
    public string selectedVoiceId = "";

    public DialogueLine(string name, string text)
    {
        characterName = name;
        dialogueText = text;
        hasAudio = false;
        audioFilePath = "";
        selectedVoiceId = "";
    }

    public DialogueLine(string name, string text, string voiceId)
    {
        characterName = name;
        dialogueText = text;
        hasAudio = false;
        audioFilePath = "";
        // ✅ CHANGED: Don't force default voice - use what's provided (could be empty)
        selectedVoiceId = voiceId ?? "";

        UnityEngine.Debug.Log($"📝 Created DialogueLine: '{name}' with voice: {(string.IsNullOrEmpty(voiceId) ? "No Voice" : voiceId)}");
    }

    public DialogueLine()
    {
        characterName = "";
        dialogueText = "";
        hasAudio = false;
        audioFilePath = "";
        selectedVoiceId = "";
    }
}
