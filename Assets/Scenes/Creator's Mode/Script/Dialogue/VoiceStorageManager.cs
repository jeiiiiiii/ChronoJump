// VoiceStorageManager.cs - FIXED for Student Mode
using UnityEngine;
using System.Collections.Generic;

public static class VoiceStorageManager
{
    // Save voice selection for current teacher/story
    public static void SaveVoiceSelection(string dialogueKey, string voiceId)
    {
        string storyId = GetCurrentStoryId();

        // Key format: {storyId}_{dialogueKey}_VoiceId
        // TeacherPrefs will automatically prefix with teacher ID
        string storageKey = $"{storyId}_{dialogueKey}_VoiceId";
        TeacherPrefs.SetString(storageKey, voiceId);
        TeacherPrefs.Save();

        var voice = VoiceLibrary.GetVoiceById(voiceId);
        Debug.Log($"💾 VoiceStorage SAVE: Key='{storageKey}' Voice='{voice.voiceName}' ({voiceId})");
    }

    // Load voice selection for current teacher/story
    public static string LoadVoiceSelection(string dialogueKey)
    {
        string storyId = GetCurrentStoryId();

        // Key format: {storyId}_{dialogueKey}_VoiceId
        string storageKey = $"{storyId}_{dialogueKey}_VoiceId";
        string voiceId = TeacherPrefs.GetString(storageKey, "");

        // If not found, use default
        if (string.IsNullOrEmpty(voiceId))
        {
            voiceId = VoiceLibrary.GetDefaultVoice().voiceId;
            Debug.LogWarning($"⚠️ VoiceStorage LOAD: Key='{storageKey}' NOT FOUND - using default");
        }
        else
        {
            var voice = VoiceLibrary.GetVoiceById(voiceId);
            Debug.Log($"📖 VoiceStorage LOAD: Key='{storageKey}' Voice='{voice.voiceName}' ({voiceId})");
        }

        return voiceId;
    }

    // Save all dialogue voices for the current story
    public static void SaveAllDialogueVoices(List<DialogueLine> dialogues)
    {
        if (dialogues == null) return;

        string storyId = GetCurrentStoryId();

        Debug.Log($"💾 === SAVING ALL VOICES ===");
        Debug.Log($"💾 Story ID: {storyId}");

        for (int i = 0; i < dialogues.Count; i++)
        {
            SaveVoiceSelection($"Dialogue_{i}", dialogues[i].selectedVoiceId);
        }

        Debug.Log($"💾 Saved {dialogues.Count} dialogue voices");
    }

    // Load all dialogue voices for the current story
    public static void LoadAllDialogueVoices(List<DialogueLine> dialogues)
    {
        if (dialogues == null || dialogues.Count == 0)
        {
            Debug.LogWarning("⚠️ No dialogues to load voices for");
            return;
        }

        string storyId = GetCurrentStoryId();

        Debug.Log($"📖 === LOADING ALL VOICES ===");
        Debug.Log($"📖 Story ID: {storyId}");
        Debug.Log($"📖 Loading voices for {dialogues.Count} dialogues...");

        for (int i = 0; i < dialogues.Count; i++)
        {
            string loadedVoiceId = LoadVoiceSelection($"Dialogue_{i}");

            // Update the dialogue with loaded voice
            dialogues[i].selectedVoiceId = loadedVoiceId;

            var voice = VoiceLibrary.GetVoiceById(loadedVoiceId);
            Debug.Log($"📖   Dialogue {i}: '{dialogues[i].characterName}' → {voice.voiceName}");
        }

        Debug.Log($"📖 === LOAD COMPLETE ===");
    }

    private static string GetCurrentStoryId()
    {
        // ✅ FIXED: Try StoryManager first (for teachers)
        var story = StoryManager.Instance?.GetCurrentStory();
        if (story != null && !string.IsNullOrEmpty(story.storyId))
        {
            Debug.Log($"📚 Got Story ID from StoryManager: {story.storyId}");
            return story.storyId;
        }

        // ✅ CRITICAL FIX: For students, get from StudentPrefs
        string userRole = PlayerPrefs.GetString("UserRole", "student");
        if (userRole.ToLower() == "student")
        {
            string storyJson = StudentPrefs.GetString("CurrentStoryData", "");
            if (!string.IsNullOrEmpty(storyJson))
            {
                try
                {
                    var studentStory = JsonUtility.FromJson<StoryData>(storyJson);
                    if (!string.IsNullOrEmpty(studentStory.storyId))
                    {
                        Debug.Log($"📚 Got Story ID from StudentPrefs: {studentStory.storyId}");
                        return studentStory.storyId;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error parsing student story: {ex.Message}");
                }
            }
        }

        // Fallback: Use story index if available
        if (story != null && story.storyIndex >= 0)
        {
            string fallbackId = $"Story_{story.storyIndex}";
            Debug.Log($"📚 Using Story Index as ID: {fallbackId}");
            return fallbackId;
        }

        Debug.LogWarning("📚 No story found - using 'CurrentStory' as ID");
        return "CurrentStory";
    }

    // ✅ Debug method to check what's actually stored
    public static void DebugStoredVoices()
    {
        string storyId = GetCurrentStoryId();

        Debug.Log("=== VOICE STORAGE DEBUG ===");
        Debug.Log($"Story ID: {storyId}");

        var dialogues = DialogueStorage.GetAllDialogues();
        Debug.Log($"Dialogues in memory: {dialogues.Count}");

        // Check what's stored in TeacherPrefs
        for (int i = 0; i < 10; i++) // Check first 10 slots
        {
            string key = $"{storyId}_Dialogue_{i}_VoiceId";
            string storedValue = TeacherPrefs.GetString(key, "NOT_FOUND");

            if (storedValue != "NOT_FOUND")
            {
                var voice = VoiceLibrary.GetVoiceById(storedValue);
                Debug.Log($"[{i}] Key: {key}");
                Debug.Log($"    Stored: {storedValue} ({voice.voiceName})");

                if (i < dialogues.Count)
                {
                    Debug.Log($"    In Memory: {dialogues[i].selectedVoiceId}");
                    Debug.Log($"    Match: {storedValue == dialogues[i].selectedVoiceId}");
                }
            }
        }

        Debug.Log("=== END DEBUG ===");
    }
}
