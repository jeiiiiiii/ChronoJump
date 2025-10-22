// DialogueStorage.cs - COMPLETE FIXED VERSION
using System.Collections.Generic;
using System.IO;
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

            // ✅ Save story after setting dialogues
            SaveCurrentStory();
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

        var newDialogue = new DialogueLine(name, text, voiceId);
        dialogues.Add(newDialogue);

        // Save voice selection persistently
        VoiceStorageManager.SaveVoiceSelection($"Dialogue_{dialogues.Count - 1}", voiceId);

        var voice = VoiceLibrary.GetVoiceById(voiceId);
        Debug.Log($"✅ DialogueStorage: Added dialogue - {name}: {text} (Voice: {voice.voiceName})");

        // ✅ CRITICAL: Save story after adding
        SaveCurrentStory();
    }

    // ✅ CRITICAL: Load all voices from persistent storage
    public static void LoadAllVoices()
    {
        var dialogues = GetStoryDialogues();
        if (dialogues == null || dialogues.Count == 0)
        {
            Debug.Log("ℹ️ No dialogues to load voices for");
            return;
        }

        Debug.Log($"🎤 Loading voices for {dialogues.Count} dialogues from VoiceStorageManager...");

        // Load voices from TeacherPrefs
        VoiceStorageManager.LoadAllDialogueVoices(dialogues);

        // ✅ FIX: For students, verify we actually have voice IDs
        bool hasChanges = false;
        for (int i = 0; i < dialogues.Count; i++)
        {
            if (string.IsNullOrEmpty(dialogues[i].selectedVoiceId))
            {
                Debug.Log($"ℹ️ Dialogue {i} '{dialogues[i].characterName}' has no voice - this is allowed");
                // Keep it empty - no need to assign a default
                dialogues[i].selectedVoiceId = ""; // This should stay empty
            }
            var voice = VoiceLibrary.GetVoiceById(dialogues[i].selectedVoiceId);
            Debug.Log($"   🎤 Dialogue {i}: '{dialogues[i].characterName}' → {voice.voiceName} ({dialogues[i].selectedVoiceId})");
        }

        // ✅ CRITICAL: Save story to persist voice assignments in StoryData
        if (hasChanges)
        {
            Debug.Log("💾 Saving story to persist voice assignments...");
            SaveCurrentStory();
        }
    }


    public static void ClearDialogues()
    {
        var dialogues = GetStoryDialogues();
        if (dialogues == null) return;

        dialogues.Clear();
        Debug.Log("✅ DialogueStorage: Cleared all dialogues");

        // ✅ Save after clearing
        SaveCurrentStory();
    }

    public static void DeleteDialogue(int index)
    {
        var dialogues = GetStoryDialogues();
        if (dialogues == null) return;

        if (index >= 0 && index < dialogues.Count)
        {
            var dialogue = dialogues[index];

            // ✅ NEW: Delete corresponding audio files before removing dialogue
            DeleteDialogueAudioFiles(index, dialogue);

            dialogues.RemoveAt(index);
            Debug.Log($"✅ DialogueStorage: Deleted dialogue {index} - {dialogue.characterName}: {dialogue.dialogueText}");

            // ✅ CRITICAL FIX: Re-index all subsequent voice storage keys
            ReindexVoiceStorageAfterDeletion(index);

            // ✅ Save story after deletion
            SaveCurrentStory();
        }
    }


    // ✅ NEW: Delete audio files for a specific dialogue
    private static void DeleteDialogueAudioFiles(int dialogueIndex, DialogueLine dialogue)
    {
        try
        {
            string teacherId = GetCurrentTeacherId();
            int storyIndex = GetCurrentStoryIndex();

            string audioDir = Path.Combine(
                Application.persistentDataPath,
                teacherId,
                $"story_{storyIndex}",
                "audio"
            );

            if (!Directory.Exists(audioDir))
            {
                Debug.Log($"ℹ️ Audio directory not found: {audioDir}");
                return;
            }

            string sanitizedName = SanitizeFileName(dialogue.characterName);
            var voice = VoiceLibrary.GetVoiceById(dialogue.selectedVoiceId);

            // Delete all possible audio file patterns for this dialogue
            string[] patterns = {
            $"dialogue_{dialogueIndex}_{sanitizedName}_{voice.voiceName}.mp3",           // Exact match
            $"dialogue_{dialogueIndex}_{sanitizedName}_{voice.voiceName}_*.mp3",         // Timestamped versions
            $"dialogue_{dialogueIndex}_{sanitizedName}_*.mp3",                           // Voice mismatch fallback
            $"dialogue_{dialogueIndex}_*.mp3"                                            // Index-only fallback
        };

            int deletedCount = 0;
            foreach (string pattern in patterns)
            {
                string[] files = Directory.GetFiles(audioDir, pattern);
                foreach (string file in files)
                {
                    try
                    {
                        File.Delete(file);
                        deletedCount++;
                        Debug.Log($"🗑️ Deleted audio file: {Path.GetFileName(file)}");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"⚠️ Could not delete audio file {file}: {ex.Message}");
                    }
                }
            }

            if (deletedCount > 0)
            {
                Debug.Log($"✅ Deleted {deletedCount} audio files for dialogue {dialogueIndex}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Error deleting audio files for dialogue {dialogueIndex}: {ex.Message}");
        }
    }


    // ✅ Helper method to sanitize file names (add this if not exists)
    private static string SanitizeFileName(string fileName)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }
        return fileName;
    }

    // ✅ Helper method to get current teacher ID (add this if not exists)
    private static string GetCurrentTeacherId()
    {
        if (StoryManager.Instance != null && StoryManager.Instance.IsCurrentUserTeacher())
        {
            return StoryManager.Instance.GetCurrentTeacherId();
        }
        return TeacherPrefs.GetString("CurrentTeachId", "default");
    }

        // ✅ Helper method to get current story index (add this if not exists)
        private static int GetCurrentStoryIndex()
        {
            var story = StoryManager.Instance?.GetCurrentStory();
            if (story != null)
            {
                return story.storyIndex;
            }
            return 0;
        }



    // ✅ NEW: Re-index voice storage after deletion
    private static void ReindexVoiceStorageAfterDeletion(int deletedIndex)
    {
        var dialogues = GetStoryDialogues();
        if (dialogues == null) return;

        string storyId = GetCurrentStoryId();

        // For all dialogues after the deleted one, shift their voice storage down by one
        for (int i = deletedIndex; i < dialogues.Count; i++)
        {
            string oldKey = $"{storyId}_Dialogue_{i + 1}_VoiceId";
            string newKey = $"{storyId}_Dialogue_{i}_VoiceId";

            string voiceId = TeacherPrefs.GetString(oldKey, "");

            if (!string.IsNullOrEmpty(voiceId))
            {
                // Move the voice from old position to new position
                TeacherPrefs.SetString(newKey, voiceId);
                TeacherPrefs.DeleteKey(oldKey);
                Debug.Log($"🔄 Re-indexed voice: {oldKey} → {newKey} ({voiceId})");
            }
            else
            {
                // Clear the new position if no voice exists
                TeacherPrefs.DeleteKey(newKey);
            }
        }

        // Delete the last key (which is now empty)
        string lastKey = $"{storyId}_Dialogue_{dialogues.Count}_VoiceId";
        TeacherPrefs.DeleteKey(lastKey);

        TeacherPrefs.Save();
    }


    // ✅ NEW: Helper method to get current story ID (extracted from existing code)
    private static string GetCurrentStoryId()
    {
        var story = StoryManager.Instance?.GetCurrentStory();
        if (story != null && !string.IsNullOrEmpty(story.storyId))
        {
            return story.storyId;
        }

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
                        return studentStory.storyId;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error parsing student story: {ex.Message}");
                }
            }
        }

        if (story != null && story.storyIndex >= 0)
        {
            return $"Story_{story.storyIndex}";
        }

        return "CurrentStory";
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

    public static string GetDataSourceInfo()
    {
        if (StoryManager.Instance?.GetCurrentStory() != null)
            return "StoryManager";

        if (!string.IsNullOrEmpty(StudentPrefs.GetString("CurrentStoryData", "")))
            return "StudentPrefs";

        return "FallbackStorage";
    }

    // ✅ CRITICAL: Save the current story
    private static void SaveCurrentStory()
    {
        var story = StoryManager.Instance?.GetCurrentStory();
        if (story == null)
        {
            Debug.LogWarning("⚠️ Cannot save story - no current story in StoryManager");
            return;
        }

        // Call StoryManager's save method
        if (StoryManager.Instance != null)
        {
            try
            {
                // Try SaveStories method first
                var saveMethod = StoryManager.Instance.GetType().GetMethod("SaveStories");
                if (saveMethod != null)
                {
                    saveMethod.Invoke(StoryManager.Instance, null);
                    Debug.Log($"💾 Story '{story.storyTitle}' saved via StoryManager.SaveStories()");
                }
                else
                {
                    Debug.LogWarning("⚠️ StoryManager.SaveStories() method not found");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Error saving story: {ex.Message}");
            }
        }
    }

    public static void UpdateDialogueAudioInfo(int index, string relativePath, string fileName)
    {
        var dialogues = GetStoryDialogues();
        if (dialogues == null) return;

        if (index >= 0 && index < dialogues.Count)
        {
            dialogues[index].audioFilePath = relativePath;
            dialogues[index].audioFileName = fileName;
            dialogues[index].hasAudio = !string.IsNullOrEmpty(relativePath);

            Debug.Log($"💾 Updated audio info for dialogue {index}: {fileName}");

            // ✅ CRITICAL: Save story to persist audio information
            SaveCurrentStory();
        }
    }


    public static void EditDialogue(int index, string newName, string newText)
    {
        var dialogues = GetStoryDialogues();
        if (dialogues == null) return;

        if (index >= 0 && index < dialogues.Count)
        {
            // ✅ PRESERVE voice selection
            string existingVoiceId = dialogues[index].selectedVoiceId;

            // ✅ NEW: Check if dialogue text actually changed
            bool dialogueTextChanged = dialogues[index].dialogueText != newText;
            bool characterNameChanged = dialogues[index].characterName != newName;

            dialogues[index].characterName = newName;
            dialogues[index].dialogueText = newText;
            dialogues[index].selectedVoiceId = existingVoiceId;

            // ✅ FIXED: Always invalidate audio if dialogue text changed
            // (because the audio content is now outdated)
            if (dialogueTextChanged || characterNameChanged)
            {   
                dialogues[index].hasAudio = false;
                dialogues[index].audioFilePath = "";
                dialogues[index].audioFileName = "";
                Debug.Log($"🔇 Invalidated audio for dialogue {index} (text or name changed)");
            }

            // Save the voice ID again to ensure persistence
            if (!string.IsNullOrEmpty(existingVoiceId))
            {
                VoiceStorageManager.SaveVoiceSelection($"Dialogue_{index}", existingVoiceId);
            }

            var voice = VoiceLibrary.GetVoiceById(existingVoiceId);
            Debug.Log($"✅ DialogueStorage: Edited dialogue {index} - {newName}: {newText} (Voice: {voice.voiceName}, Audio Invalidated: {dialogueTextChanged || characterNameChanged})");

            // ✅ Save story after editing
            SaveCurrentStory();
        }
    }



    public static void UpdateDialogueVoice(int index, string newVoiceId)
    {
        var dialogues = GetStoryDialogues();
        if (dialogues == null) return;

        if (index >= 0 && index < dialogues.Count)
        {
            string oldVoiceId = dialogues[index].selectedVoiceId;
            dialogues[index].selectedVoiceId = newVoiceId;

            // Only invalidate audio if voice actually changed
            if (oldVoiceId != newVoiceId)
            {
                dialogues[index].hasAudio = false;
                dialogues[index].audioFilePath = "";
                dialogues[index].audioFileName = "";
            }

            // Save to persistent storage
            VoiceStorageManager.SaveVoiceSelection($"Dialogue_{index}", newVoiceId);

            var voice = VoiceLibrary.GetVoiceById(newVoiceId);
            Debug.Log($"🎤 Updated dialogue {index} voice to: {voice.voiceName}");

            // ✅ Save story after updating voice
            SaveCurrentStory();
        }
    }


    // ✅ Debug method
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void DebugVoiceStorage()
    {
        string teacherId = TeacherPrefs.GetString("CurrentTeachId", "default");
        var story = StoryManager.Instance?.GetCurrentStory();

        Debug.Log("=== VOICE STORAGE DEBUG ===");
        Debug.Log($"Teacher ID: {teacherId}");
        Debug.Log($"Story: {story?.storyTitle ?? "NULL"}");
        Debug.Log($"Story Index: {story?.storyIndex ?? -1}");
        Debug.Log($"Story ID: {story?.storyId ?? "NULL"}");

        var dialogues = GetAllDialogues();
        Debug.Log($"Total Dialogues: {dialogues.Count}");

        for (int i = 0; i < dialogues.Count; i++)
        {
            string key = $"{teacherId}_{story?.storyId ?? "CurrentStory"}_Dialogue_{i}_VoiceId";
            string storedVoice = TeacherPrefs.GetString(key, "NOT_FOUND");
            var voice = VoiceLibrary.GetVoiceById(dialogues[i].selectedVoiceId);

            Debug.Log($"Dialogue {i}: '{dialogues[i].characterName}'");
            Debug.Log($"  In Memory: {dialogues[i].selectedVoiceId} ({voice.voiceName})");
            Debug.Log($"  In TeacherPrefs: {storedVoice}");
            Debug.Log($"  Storage Key: {key}");
        }

        Debug.Log("=== END DEBUG ===");
    }

    // ✅ NEW: Debug method to check voice storage alignment
    public static void DebugVoiceStorageAlignment()
    {
        var dialogues = GetAllDialogues();
        string storyId = GetCurrentStoryId();

        Debug.Log("=== VOICE STORAGE ALIGNMENT CHECK ===");
        Debug.Log($"Total Dialogues: {dialogues.Count}");

        for (int i = 0; i < dialogues.Count; i++)
        {
            string storageKey = $"{storyId}_Dialogue_{i}_VoiceId";
            string storedVoiceId = TeacherPrefs.GetString(storageKey, "NOT_FOUND");
            string memoryVoiceId = dialogues[i].selectedVoiceId ?? "";

            var storedVoice = VoiceLibrary.GetVoiceById(storedVoiceId);
            var memoryVoice = VoiceLibrary.GetVoiceById(memoryVoiceId);

            bool matches = storedVoiceId == memoryVoiceId;

            Debug.Log($"Dialogue {i}: '{dialogues[i].characterName}'");
            Debug.Log($"  Memory: {memoryVoiceId} ({memoryVoice.voiceName})");
            Debug.Log($"  Storage: {storedVoiceId} ({storedVoice.voiceName})");
            Debug.Log($"  Match: {matches}");
            Debug.Log($"  Storage Key: {storageKey}");

            if (!matches)
            {
                Debug.LogError($"❌ MISMATCH at index {i}!");
            }
        }
        Debug.Log("=== END ALIGNMENT CHECK ===");
    }

}
