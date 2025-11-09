// DialogueStorage.cs - FIXED VERSION with sanitization safety check
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class DialogueStorage
{
    private static List<DialogueLine> fallbackDialogues = new List<DialogueLine>();

    private static List<DialogueLine> GetStoryDialogues()
    {
        var story = StoryManager.Instance?.GetCurrentStory();
        if (story != null)
        {
            if (story.dialogues == null)
                story.dialogues = new List<DialogueLine>();

            return story.dialogues;
        }

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

        Debug.Log("⚠️ DialogueStorage: No story manager or student data, using fallback storage");
        return fallbackDialogues;
    }

    public static void SetDialogues(List<DialogueLine> dialogues)
    {
        if (dialogues == null) return;

        var story = StoryManager.Instance?.GetCurrentStory();
        if (story != null)
        {
            story.dialogues = dialogues;
            Debug.Log($"✅ DialogueStorage: Set {dialogues.Count} dialogues in StoryManager");
            SaveCurrentStory();
            return;
        }

        fallbackDialogues = dialogues;
        Debug.Log($"✅ DialogueStorage: Set {dialogues.Count} dialogues in fallback storage");
    }

    public static void AddDialogue(string name, string text)
    {
        var dialogues = GetStoryDialogues();
        if (dialogues == null) return;

        string voiceId = VoiceLibrary.GetDefaultVoice().voiceId;
        var addDialoguePanel = Object.FindFirstObjectByType<AddDialogue>(FindObjectsInactive.Include);
        if (addDialoguePanel != null)
        {
            voiceId = addDialoguePanel.GetSelectedVoiceId();
        }

        var newDialogue = new DialogueLine(name, text, voiceId);
        dialogues.Add(newDialogue);

        VoiceStorageManager.SaveVoiceSelection($"Dialogue_{dialogues.Count - 1}", voiceId);

        var voice = VoiceLibrary.GetVoiceById(voiceId);
        Debug.Log($"✅ DialogueStorage: Added dialogue - {name}: {text} (Voice: {voice.voiceName})");

        SaveCurrentStory();
    }

    public static void LoadAllVoices()
    {
        var dialogues = GetStoryDialogues();
        if (dialogues == null || dialogues.Count == 0)
        {
            Debug.Log("ℹ️ No dialogues to load voices for");
            return;
        }

        Debug.Log($"🎤 Checking voices for {dialogues.Count} dialogues...");

        bool needsLoadingFromStorage = false;
        int voicesAlreadySet = 0;

        for (int i = 0; i < dialogues.Count; i++)
        {
            if (!string.IsNullOrEmpty(dialogues[i].selectedVoiceId))
            {
                voicesAlreadySet++;
                var voice = VoiceLibrary.GetVoiceById(dialogues[i].selectedVoiceId);
                Debug.Log($"   ✅ Dialogue {i}: '{dialogues[i].characterName}' already has voice from Firebase → {voice.voiceName}");
            }
            else
            {
                needsLoadingFromStorage = true;
                Debug.Log($"   ⚠️ Dialogue {i}: '{dialogues[i].characterName}' has NO voice - will try loading from TeacherPrefs");
            }
        }

        if (needsLoadingFromStorage)
        {
            Debug.Log($"🔄 Loading missing voices from TeacherPrefs ({voicesAlreadySet}/{dialogues.Count} already set from Firebase)...");
            VoiceStorageManager.LoadAllDialogueVoices(dialogues, false);
        }
        else
        {
            Debug.Log($"✅ All {dialogues.Count} dialogues have voices from Firebase - skipping TeacherPrefs load");
        }
    }

    public static void ClearDialogues()
    {
        var dialogues = GetStoryDialogues();
        if (dialogues == null) return;

        dialogues.Clear();
        Debug.Log("✅ DialogueStorage: Cleared all dialogues");
        SaveCurrentStory();
    }

    public static void DeleteDialogue(int index)
    {
        var dialogues = GetStoryDialogues();
        if (dialogues == null) return;

        if (index >= 0 && index < dialogues.Count)
        {
            var dialogue = dialogues[index];

            DeleteDialogueAudioFiles(index, dialogue);

            dialogues.RemoveAt(index);
            Debug.Log($"✅ DialogueStorage: Deleted dialogue {index} - {dialogue.characterName}: {dialogue.dialogueText}");

            ReindexVoiceStorageAfterDeletion(index);
            SaveCurrentStory();
        }
    }

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

            string[] patterns = {
                $"dialogue_{dialogueIndex}_{sanitizedName}_{voice.voiceName}.mp3",
                $"dialogue_{dialogueIndex}_{sanitizedName}_{voice.voiceName}_*.mp3",
                $"dialogue_{dialogueIndex}_{sanitizedName}_*.mp3",
                $"dialogue_{dialogueIndex}_*.mp3"
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

    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return fileName;

        foreach (char c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }

        fileName = fileName.Replace(' ', '_');
        fileName = fileName.Replace('#', '_');
        fileName = fileName.Replace('%', '_');
        fileName = fileName.Replace('&', '_');

        return fileName;
    }

    private static string GetCurrentTeacherId()
    {
        if (StoryManager.Instance != null && StoryManager.Instance.IsCurrentUserTeacher())
        {
            return StoryManager.Instance.GetCurrentTeacherId();
        }
        return TeacherPrefs.GetString("CurrentTeachId", "default");
    }

    private static int GetCurrentStoryIndex()
    {
        var story = StoryManager.Instance?.GetCurrentStory();
        if (story != null)
        {
            return story.storyIndex;
        }
        return 0;
    }

    private static void ReindexVoiceStorageAfterDeletion(int deletedIndex)
    {
        var dialogues = GetStoryDialogues();
        if (dialogues == null) return;

        string storyId = GetCurrentStoryId();

        for (int i = deletedIndex; i < dialogues.Count; i++)
        {
            string oldKey = $"{storyId}_Dialogue_{i + 1}_VoiceId";
            string newKey = $"{storyId}_Dialogue_{i}_VoiceId";

            string voiceId = TeacherPrefs.GetString(oldKey, "");

            if (!string.IsNullOrEmpty(voiceId))
            {
                TeacherPrefs.SetString(newKey, voiceId);
                TeacherPrefs.DeleteKey(oldKey);
                Debug.Log($"🔄 Re-indexed voice: {oldKey} → {newKey} ({voiceId})");
            }
            else
            {
                TeacherPrefs.DeleteKey(newKey);
            }
        }

        string lastKey = $"{storyId}_Dialogue_{dialogues.Count}_VoiceId";
        TeacherPrefs.DeleteKey(lastKey);

        TeacherPrefs.Save();
    }

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

    private static void SaveCurrentStory()
    {
        var story = StoryManager.Instance?.GetCurrentStory();
        if (story == null)
        {
            Debug.LogWarning("⚠️ Cannot save story - no current story in StoryManager");
            return;
        }

        if (StoryManager.Instance != null)
        {
            try
            {
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

    // ✅ CRITICAL FIX: Sanitize all inputs before saving
    public static void UpdateDialogueAudioInfo(int index, string relativePath, string fileName, string s3Url)
    {
        var dialogues = GetStoryDialogues();
        if (dialogues == null) return;

        if (index >= 0 && index < dialogues.Count)
        {
            // ✅ SAFETY CHECK: Sanitize the filename if it contains spaces
            string sanitizedFileName = fileName;
            if (!string.IsNullOrEmpty(fileName) && fileName.Contains(" "))
            {
                Debug.LogWarning($"⚠️ Sanitizing filename with spaces: {fileName}");
                sanitizedFileName = SanitizeFileName(fileName);
                Debug.Log($"   → Sanitized to: {sanitizedFileName}");
            }

            // ✅ SAFETY CHECK: Sanitize the relative path if it contains spaces
            string sanitizedRelativePath = relativePath;
            if (!string.IsNullOrEmpty(relativePath) && relativePath.Contains(" "))
            {
                Debug.LogWarning($"⚠️ Sanitizing relative path with spaces: {relativePath}");
                // Split path, sanitize filename only, keep directory structure
                string[] pathParts = relativePath.Split('/', '\\');
                for (int i = 0; i < pathParts.Length; i++)
                {
                    if (pathParts[i].Contains(" ") && pathParts[i].Contains(".mp3"))
                    {
                        pathParts[i] = SanitizeFileName(pathParts[i]);
                    }
                }
                sanitizedRelativePath = string.Join("/", pathParts);
                Debug.Log($"   → Sanitized to: {sanitizedRelativePath}");
            }

            // ✅ Update dialogue with sanitized values
            dialogues[index].audioFilePath = sanitizedRelativePath;
            dialogues[index].audioFileName = sanitizedFileName;
            dialogues[index].audioStoragePath = s3Url ?? "";
            dialogues[index].hasAudio = !string.IsNullOrEmpty(sanitizedRelativePath);

            // Calculate file size if local file exists
            if (!string.IsNullOrEmpty(sanitizedRelativePath))
            {
                string absolutePath = Path.Combine(Application.persistentDataPath, sanitizedRelativePath);
                if (File.Exists(absolutePath))
                {
                    var fileInfo = new FileInfo(absolutePath);
                    dialogues[index].audioFileSize = fileInfo.Length;
                }
            }

            Debug.Log($"💾 Updated audio info for dialogue {index}:");
            Debug.Log($"   fileName: {sanitizedFileName}");
            Debug.Log($"   relativePath: {sanitizedRelativePath}");
            Debug.Log($"   s3Url: {s3Url ?? "none"}");

            // ✅ Verify no spaces remain
            bool hasSpaces = sanitizedFileName?.Contains(" ") == true ||
                           sanitizedRelativePath?.Contains(" ") == true;

            if (hasSpaces)
            {
                Debug.LogError($"❌ CRITICAL: Spaces still present after sanitization!");
            }
            else
            {
                Debug.Log($"✅ Verified: No spaces in saved audio info");
            }

            SaveCurrentStory();
        }
    }

    public static void EditDialogue(int index, string newName, string newText)
    {
        var dialogues = GetStoryDialogues();
        if (dialogues == null) return;

        if (index >= 0 && index < dialogues.Count)
        {
            string existingVoiceId = dialogues[index].selectedVoiceId;
            bool dialogueTextChanged = dialogues[index].dialogueText != newText;
            bool characterNameChanged = dialogues[index].characterName != newName;

            dialogues[index].characterName = newName;
            dialogues[index].dialogueText = newText;
            dialogues[index].selectedVoiceId = existingVoiceId;

            if (dialogueTextChanged || characterNameChanged)
            {
                dialogues[index].hasAudio = false;
                dialogues[index].audioFilePath = "";
                dialogues[index].audioFileName = "";
                Debug.Log($"🔇 Invalidated audio for dialogue {index} (text or name changed)");
            }

            if (!string.IsNullOrEmpty(existingVoiceId))
            {
                VoiceStorageManager.SaveVoiceSelection($"Dialogue_{index}", existingVoiceId);
            }

            var voice = VoiceLibrary.GetVoiceById(existingVoiceId);
            Debug.Log($"✅ DialogueStorage: Edited dialogue {index} - {newName}: {newText}");

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

            if (oldVoiceId != newVoiceId)
            {
                dialogues[index].hasAudio = false;
                dialogues[index].audioFilePath = "";
                dialogues[index].audioFileName = "";

                DeleteDialogueAudioFiles(index, dialogues[index]);

                Debug.Log($"🔇 Invalidated audio for dialogue {index} (voice changed)");
            }

            VoiceStorageManager.SaveVoiceSelection($"Dialogue_{index}", newVoiceId);

            var voice = VoiceLibrary.GetVoiceById(newVoiceId);
            Debug.Log($"🎤 Updated dialogue {index} voice to: {voice.voiceName}");

            SaveCurrentStory();
        }
    }

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

    // Backward compatibility - keeping the old 3-parameter version
    public static void UpdateDialogueAudioInfo(int index, string relativePath, string fileName)
    {
        UpdateDialogueAudioInfo(index, relativePath, fileName, null);
    }
}
