// VoiceDebugHelper.cs
// Add this script to a GameObject in your scene and use the context menu to debug

using UnityEngine;
using System.IO;

public class VoiceDebugHelper : MonoBehaviour
{
    [ContextMenu("1. Debug Current Story")]
    public void DebugCurrentStory()
    {
        Debug.Log("=== CURRENT STORY DEBUG ===");

        var story = StoryManager.Instance?.GetCurrentStory();
        if (story == null)
        {
            Debug.LogError("❌ No current story!");
            return;
        }

        Debug.Log($"Story Title: {story.storyTitle}");
        Debug.Log($"Story Index: {story.storyIndex}");
        Debug.Log($"Story ID: {story.storyId}");
        Debug.Log($"Dialogues Count: {story.dialogues?.Count ?? 0}");

        if (story.dialogues != null)
        {
            for (int i = 0; i < story.dialogues.Count; i++)
            {
                var dialogue = story.dialogues[i];
                var voice = VoiceLibrary.GetVoiceById(dialogue.selectedVoiceId);
                Debug.Log($"  [{i}] '{dialogue.characterName}'");
                Debug.Log($"      Text: {dialogue.dialogueText}");
                Debug.Log($"      Voice ID: {dialogue.selectedVoiceId}");
                Debug.Log($"      Voice Name: {voice.voiceName}");
                Debug.Log($"      Has Audio: {dialogue.hasAudio}");
                Debug.Log($"      Audio Path: {dialogue.audioFilePath}");
            }
        }
    }

    [ContextMenu("2. Debug Teacher ID")]
    public void DebugTeacherId()
    {
        Debug.Log("=== TEACHER ID DEBUG ===");

        // Method 1: From StoryManager
        if (StoryManager.Instance != null && StoryManager.Instance.IsCurrentUserTeacher())
        {
            string id = StoryManager.Instance.GetCurrentTeacherId();
            Debug.Log($"StoryManager Teacher ID: {id}");
        }
        else
        {
            Debug.Log("StoryManager: Not a teacher or no instance");
        }

        // Method 2: From TeacherPrefs
        string prefId = TeacherPrefs.GetString("CurrentTeachId", "NOT_FOUND");
        Debug.Log($"TeacherPrefs CurrentTeachId: {prefId}");

        // Method 3: From PlayerPrefs (for comparison)
        string userRole = PlayerPrefs.GetString("UserRole", "NOT_FOUND");
        Debug.Log($"UserRole: {userRole}");
    }

    [ContextMenu("3. Debug Voice Storage Keys")]
    public void DebugVoiceStorageKeys()
    {
        Debug.Log("=== VOICE STORAGE KEYS DEBUG ===");

        var story = StoryManager.Instance?.GetCurrentStory();
        if (story == null)
        {
            Debug.LogError("❌ No current story!");
            return;
        }

        string teacherId = StoryManager.Instance.IsCurrentUserTeacher()
            ? StoryManager.Instance.GetCurrentTeacherId()
            : TeacherPrefs.GetString("CurrentTeachId", "default");

        string storyId = !string.IsNullOrEmpty(story.storyId)
            ? story.storyId
            : $"Story_{story.storyIndex}";

        Debug.Log($"Teacher ID: {teacherId}");
        Debug.Log($"Story ID: {storyId}");

        // ✅ FIXED: TeacherPrefs automatically adds teacher ID, so key should be: {storyId}_Dialogue_{i}_VoiceId
        for (int i = 0; i < 10; i++)
        {
            string key = $"{storyId}_Dialogue_{i}_VoiceId";
            string value = TeacherPrefs.GetString(key, "NOT_FOUND");

            Debug.Log($"Checking key: {key}");

            if (value != "NOT_FOUND")
            {
                var voice = VoiceLibrary.GetVoiceById(value);
                Debug.Log($"✅ Found: {key}");
                Debug.Log($"   Value: {value} ({voice.voiceName})");
            }
            else
            {
                Debug.Log($"❌ Not Found: {key}");
            }
        }
    }

    [ContextMenu("4. Debug Audio Files")]
    public void DebugAudioFiles()
    {
        Debug.Log("=== AUDIO FILES DEBUG ===");

        var story = StoryManager.Instance?.GetCurrentStory();
        if (story == null)
        {
            Debug.LogError("❌ No current story!");
            return;
        }

        string teacherId = StoryManager.Instance.IsCurrentUserTeacher()
            ? StoryManager.Instance.GetCurrentTeacherId()
            : TeacherPrefs.GetString("CurrentTeachId", "default");

        string audioDir = Path.Combine(Application.persistentDataPath, teacherId, $"story_{story.storyIndex}", "audio");

        Debug.Log($"Audio Directory: {audioDir}");
        Debug.Log($"Directory Exists: {Directory.Exists(audioDir)}");

        if (Directory.Exists(audioDir))
        {
            string[] files = Directory.GetFiles(audioDir, "*.mp3");
            Debug.Log($"Found {files.Length} MP3 files:");

            foreach (string file in files)
            {
                FileInfo info = new FileInfo(file);
                Debug.Log($"  📁 {Path.GetFileName(file)} ({info.Length / 1024} KB)");
            }
        }
    }

    [ContextMenu("5. Test Save and Load Voice")]
    public void TestSaveLoadVoice()
    {
        Debug.Log("=== TEST SAVE/LOAD VOICE ===");

        // Save a test voice
        string testVoiceId = "pNInz6obpgDQGcFmaJgB"; // Adam
        VoiceStorageManager.SaveVoiceSelection("Test_Dialogue_0", testVoiceId);
        Debug.Log($"💾 Saved test voice: {testVoiceId}");

        // Load it back
        string loaded = VoiceStorageManager.LoadVoiceSelection("Test_Dialogue_0");
        Debug.Log($"📖 Loaded test voice: {loaded}");

        if (loaded == testVoiceId)
        {
            Debug.Log("✅ Save/Load working correctly!");
        }
        else
        {
            Debug.LogError($"❌ Save/Load FAILED! Expected {testVoiceId}, got {loaded}");
        }
    }

    [ContextMenu("6. Force Save Current Story")]
    public void ForceSaveCurrentStory()
    {
        Debug.Log("=== FORCE SAVE STORY ===");

        var story = StoryManager.Instance?.GetCurrentStory();
        if (story == null)
        {
            Debug.LogError("❌ No current story to save!");
            return;
        }

        // Save voices to TeacherPrefs
        if (story.dialogues != null)
        {
            VoiceStorageManager.SaveAllDialogueVoices(story.dialogues);
        }

        // Save story to StoryManager
        if (StoryManager.Instance != null)
        {
            StoryManager.Instance.SaveStories();
            Debug.Log("✅ Story saved via StoryManager.SaveStories()");
        }

        Debug.Log($"💾 Saved story: {story.storyTitle}");
        Debug.Log($"💾 Dialogues: {story.dialogues?.Count ?? 0}");
    }

    [ContextMenu("7. Force Load Voices")]
    public void ForceLoadVoices()
    {
        Debug.Log("=== FORCE LOAD VOICES ===");

        DialogueStorage.LoadAllVoices();

        var dialogues = DialogueStorage.GetAllDialogues();
        Debug.Log($"✅ Loaded {dialogues.Count} dialogues");

        foreach (var dialogue in dialogues)
        {
            var voice = VoiceLibrary.GetVoiceById(dialogue.selectedVoiceId);
            Debug.Log($"  🎤 '{dialogue.characterName}' → {voice.voiceName}");
        }
    }

    [ContextMenu("8. Full Diagnostic")]
    public void FullDiagnostic()
    {
        Debug.Log("\n\n========================================");
        Debug.Log("FULL VOICE PERSISTENCE DIAGNOSTIC");
        Debug.Log("========================================\n");

        DebugCurrentStory();
        Debug.Log("\n");
        DebugTeacherId();
        Debug.Log("\n");
        DebugVoiceStorageKeys();
        Debug.Log("\n");
        DebugAudioFiles();
        Debug.Log("\n");

        Debug.Log("========================================");
        Debug.Log("END DIAGNOSTIC");
        Debug.Log("========================================\n\n");
    }

    [ContextMenu("9. Fix Voice Mismatch")]
    public void FixVoiceMismatch()
    {
        Debug.Log("=== FIX VOICE MISMATCH ===");

        var story = StoryManager.Instance?.GetCurrentStory();
        if (story == null || story.dialogues == null)
        {
            Debug.LogError("❌ No story or dialogues!");
            return;
        }

        string teacherId = StoryManager.Instance.IsCurrentUserTeacher()
            ? StoryManager.Instance.GetCurrentTeacherId()
            : TeacherPrefs.GetString("CurrentTeachId", "default");

        string audioDir = Path.Combine(Application.persistentDataPath, teacherId, $"story_{story.storyIndex}", "audio");

        if (!Directory.Exists(audioDir))
        {
            Debug.LogError($"❌ Audio directory not found: {audioDir}");
            return;
        }

        // For each dialogue, check if audio file exists and extract correct voice
        for (int i = 0; i < story.dialogues.Count; i++)
        {
            var dialogue = story.dialogues[i];
            string sanitizedName = dialogue.characterName;
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                sanitizedName = sanitizedName.Replace(c, '_');
            }

            // Find any audio file for this dialogue
            string searchPattern = $"dialogue_{i}_{sanitizedName}_*.mp3";
            string[] files = Directory.GetFiles(audioDir, searchPattern);

            if (files.Length > 0)
            {
                string fileName = Path.GetFileName(files[0]);
                // Extract voice name from filename: dialogue_0_name_VoiceName_timestamp.mp3
                string[] parts = fileName.Split('_');
                if (parts.Length >= 4)
                {
                    string voiceNameInFile = parts[parts.Length - 2]; // Voice name is second to last

                    // Find matching voice ID
                    var voices = VoiceLibrary.GetAvailableVoices();
                    var matchingVoice = voices.Find(v => v.voiceName == voiceNameInFile);

                    if (matchingVoice != null)
                    {
                        Debug.Log($"🔧 Dialogue {i}: '{dialogue.characterName}'");
                        Debug.Log($"   Current Voice: {VoiceLibrary.GetVoiceById(dialogue.selectedVoiceId).voiceName} ({dialogue.selectedVoiceId})");
                        Debug.Log($"   Audio File Voice: {voiceNameInFile}");

                        if (dialogue.selectedVoiceId != matchingVoice.voiceId)
                        {
                            Debug.Log($"   ✅ FIXING: Changing to {matchingVoice.voiceName} ({matchingVoice.voiceId})");
                            dialogue.selectedVoiceId = matchingVoice.voiceId;
                            dialogue.audioFilePath = files[0];
                            dialogue.hasAudio = true;

                            // Save to VoiceStorageManager
                            VoiceStorageManager.SaveVoiceSelection($"Dialogue_{i}", matchingVoice.voiceId);
                        }
                        else
                        {
                            Debug.Log($"   ✅ Already correct");
                        }
                    }
                }
            }
        }

        // Save the story
        if (StoryManager.Instance != null)
        {
            StoryManager.Instance.SaveStories();
            Debug.Log("💾 Story saved with corrected voices");
        }

        Debug.Log("=== FIX COMPLETE ===");
    }
}
