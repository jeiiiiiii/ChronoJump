using System.Collections.Generic;
using UnityEngine;

public static class DialogueStorage
{
    private static List<DialogueLine> fallbackDialogues = new List<DialogueLine>();

    // Convenience: get dialogues for the current story
    private static List<DialogueLine> GetStoryDialogues()
    {
        // First try: Get from StoryManager (for teacher/creator mode)
        var story = StoryManager.Instance?.GetCurrentStory();
        if (story != null)
        {
            if (story.dialogues == null)
                story.dialogues = new List<DialogueLine>();

            return story.dialogues;
        }

        // Second try: Get from StudentPrefs (for student mode)
        string storyJson = StudentPrefs.GetString("CurrentStoryData", "");
        if (!string.IsNullOrEmpty(storyJson))
        {
            try
            {
                var studentStory = JsonUtility.FromJson<StoryData>(storyJson);
                if (studentStory != null && studentStory.dialogues != null)
                {
                    Debug.Log($"✅ DialogueStorage: Loaded {studentStory.dialogues.Count} dialogues from StudentPrefs");
                    return studentStory.dialogues;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ DialogueStorage: Failed to load from StudentPrefs: {ex.Message}");
            }
        }

        // Third try: Use fallback storage
        Debug.Log("⚠️ DialogueStorage: No story manager or student data, using fallback storage");
        return fallbackDialogues;
    }

    public static void SetDialogues(List<DialogueLine> dialogues)
    {
        if (dialogues == null) return;

        // Try to set in StoryManager first
        var story = StoryManager.Instance?.GetCurrentStory();
        if (story != null)
        {
            story.dialogues = dialogues;
            Debug.Log($"✅ DialogueStorage: Set {dialogues.Count} dialogues in StoryManager");
            return;
        }

        // Fallback: use local storage
        fallbackDialogues = dialogues;
        Debug.Log($"✅ DialogueStorage: Set {dialogues.Count} dialogues in fallback storage");
    }

    public static void AddDialogue(string name, string text)
    {
        var dialogues = GetStoryDialogues();
        if (dialogues == null) return;

        // Try to pick up the voice the user selected on the AddDialogue panel
        string voiceId = VoiceLibrary.GetDefaultVoice().voiceId;
    var addDialoguePanel = Object.FindFirstObjectByType<AddDialogue>(FindObjectsInactive.Include);
        if (addDialoguePanel != null)
        {
            voiceId = addDialoguePanel.GetSelectedVoiceId();
        }

        dialogues.Add(new DialogueLine(name, text, voiceId));
        var voice = VoiceLibrary.GetVoiceById(voiceId);
        Debug.Log($"✅ DialogueStorage: Added dialogue - {name}: {text} (Voice: {voice.voiceName})");
    }

    public static void ClearDialogues()
    {
        var dialogues = GetStoryDialogues();
        if (dialogues == null) return;

        dialogues.Clear();
        Debug.Log("✅ DialogueStorage: Cleared all dialogues");
    }

    public static void DeleteDialogue(int index)
    {
        var dialogues = GetStoryDialogues();
        if (dialogues == null) return;

        if (index >= 0 && index < dialogues.Count)
        {
            var dialogue = dialogues[index];
            dialogues.RemoveAt(index);
            Debug.Log($"✅ DialogueStorage: Deleted dialogue {index} - {dialogue.characterName}: {dialogue.dialogueText}");
        }
    }

    public static void EditDialogue(int index, string newName, string newText)
    {
        var dialogues = GetStoryDialogues();
        if (dialogues == null) return;

        if (index >= 0 && index < dialogues.Count)
        {
            dialogues[index].characterName = newName;
            dialogues[index].dialogueText = newText;
            Debug.Log($"✅ DialogueStorage: Edited dialogue {index} - {newName}: {newText}");
        }
    }

    public static List<DialogueLine> GetAllDialogues()
    {
        var dialogues = GetStoryDialogues();
        
        if (dialogues == null || dialogues.Count == 0)
        {
            Debug.Log("⚠️ DialogueStorage: No dialogues found, returning empty list");
            return new List<DialogueLine>();
        }

        Debug.Log($"✅ DialogueStorage: Returning {dialogues.Count} dialogues");
        return dialogues;
    }

    // New method to check where dialogues are being loaded from
    public static string GetDataSourceInfo()
    {
        if (StoryManager.Instance?.GetCurrentStory() != null)
            return "StoryManager";
        
        if (!string.IsNullOrEmpty(StudentPrefs.GetString("CurrentStoryData", "")))
            return "StudentPrefs";
        
        return "FallbackStorage";
    }
}