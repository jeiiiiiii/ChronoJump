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
        // ✅ Ensure we always have a valid voice ID
        selectedVoiceId = !string.IsNullOrEmpty(voiceId) ? voiceId : "21m00Tcm4TlvDq8ikWAM";

        UnityEngine.Debug.Log($"📝 Created DialogueLine: '{name}' with voice: {selectedVoiceId}");
    }

    // ✅ Default constructor for JSON deserialization
    public DialogueLine()
    {
        characterName = "";
        dialogueText = "";
        hasAudio = false;
        audioFilePath = "";
        selectedVoiceId = "21m00Tcm4TlvDq8ikWAM";
    }
}
