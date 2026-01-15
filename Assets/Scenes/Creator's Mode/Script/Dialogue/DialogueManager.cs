[System.Serializable]
public class DialogueLine
{
    public string characterName;
    public string dialogueText;

    // ✅ CRITICAL: These must be public fields (not properties) for Unity serialization
    public string audioFilePath = "";
    public bool hasAudio = false;
    public string selectedVoiceId = "";

    // ✅ NEW: Additional audio fields for cross-device compatibility
    public string audioFileName = "";
    public long audioFileSize = 0;
    public string audioStoragePath = ""; // For Firebase storage

    // ✅ NEW: Track if dialogue was edited and needs audio regeneration
    public bool needsAudioRegeneration = false;

    public DialogueLine(string name, string text)
    {
        characterName = name;
        dialogueText = text;
        hasAudio = false;
        audioFilePath = "";
        selectedVoiceId = "";
        audioFileName = "";
        audioStoragePath = "";
    }

    public DialogueLine(string name, string text, string voiceId)
    {
        characterName = name;
        dialogueText = text;
        hasAudio = false;
        audioFilePath = "";
        // ✅ CHANGED: Don't force default voice - use what's provided (could be empty)
        selectedVoiceId = voiceId ?? "";
        audioFileName = "";
        audioStoragePath = "";

        UnityEngine.Debug.Log($"📝 Created DialogueLine: '{name}' with voice: {(string.IsNullOrEmpty(voiceId) ? "No Voice" : voiceId)}");
    }

    public DialogueLine()
    {
        characterName = "";
        dialogueText = "";
        hasAudio = false;
        audioFilePath = "";
        selectedVoiceId = "";
        audioFileName = "";
        audioStoragePath = "";
    }

    // ✅ NEW: Helper method to check if voice is selected
    public bool HasVoiceSelected()
    {
        return !string.IsNullOrEmpty(selectedVoiceId);
    }
}
