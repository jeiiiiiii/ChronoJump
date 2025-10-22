using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.IO;
using System.Linq;

public class ElevenLabsTTSManager : MonoBehaviour
{
    public static ElevenLabsTTSManager Instance { get; private set; }

    [Header("ElevenLabs API Settings")]
    [SerializeField] private string apiKey = "YOUR_API_KEY_HERE";

    private string apiUrl = "https://api.elevenlabs.io/v1/text-to-speech/";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private string GetAudioSaveDirectory()
    {
        string teacherId = GetCurrentTeacherId();
        int storyIndex = GetCurrentStoryIndex();

        // Follow the same pattern as ImageStorage: teacherid/story_{index}/audio/
        string relativeDir = Path.Combine(teacherId, $"story_{storyIndex}", "audio");
        string absolutePath = Path.Combine(Application.persistentDataPath, relativeDir);

        if (!Directory.Exists(absolutePath))
        {
            Directory.CreateDirectory(absolutePath);
            Debug.Log($"✅ Created audio directory: {absolutePath}");
        }
        return absolutePath;
    }

    private string GetAudioRelativePath(string fileName)
    {
        string teacherId = GetCurrentTeacherId();
        int storyIndex = GetCurrentStoryIndex();

        // Return relative path: teacherid/story_{index}/audio/filename.mp3
        return Path.Combine(teacherId, $"story_{storyIndex}", "audio", fileName);
    }

    private string GetCurrentTeacherId()
    {
        // Use the same logic as ImageStorage
        if (StoryManager.Instance != null && StoryManager.Instance.IsCurrentUserTeacher())
        {
            return StoryManager.Instance.GetCurrentTeacherId();
        }

        // For students, try to extract from story data
        string studentStoryJson = StudentPrefs.GetString("CurrentStoryData", "");
        if (!string.IsNullOrEmpty(studentStoryJson))
        {
            try
            {
                var studentStory = JsonUtility.FromJson<StoryData>(studentStoryJson);
                // Extract teacher ID from the path if available
                if (!string.IsNullOrEmpty(studentStory.backgroundPath))
                {
                    string[] pathParts = studentStory.backgroundPath.Split(Path.DirectorySeparatorChar);
                    if (pathParts.Length > 0)
                    {
                        return pathParts[0]; // First part is teacher ID
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Could not extract teacher ID from student story: {ex.Message}");
            }
        }

        return TeacherPrefs.GetString("CurrentTeachId", "default");
    }

    private int GetCurrentStoryIndex()
    {
        var story = StoryManager.Instance?.GetCurrentStory();
        if (story != null)
        {
            return story.storyIndex;
        }

        // For students, try to extract from story data path
        string studentStoryJson = StudentPrefs.GetString("CurrentStoryData", "");
        if (!string.IsNullOrEmpty(studentStoryJson))
        {
            try
            {
                var studentStory = JsonUtility.FromJson<StoryData>(studentStoryJson);
                if (!string.IsNullOrEmpty(studentStory.backgroundPath))
                {
                    string[] pathParts = studentStory.backgroundPath.Split(Path.DirectorySeparatorChar);
                    if (pathParts.Length > 1 && pathParts[1].StartsWith("story_"))
                    {
                        string indexStr = pathParts[1].Replace("story_", "");
                        if (int.TryParse(indexStr, out int storyIndex))
                        {
                            return storyIndex;
                        }
                    }
                }
                return studentStory.storyIndex;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Could not extract story index from student story: {ex.Message}");
            }
        }

        return 0;
    }

    private string GetCurrentStoryId()
    {
        var story = StoryManager.Instance?.GetCurrentStory();
        if (story != null && !string.IsNullOrEmpty(story.storyId))
        {
            return story.storyId;
        }
        return "unknown_story";
    }

    // In TTSManager.cs - Update the GenerateTTS method
    public IEnumerator GenerateTTS(DialogueLine dialogue, Action<bool, string> onComplete)
    {
        if (string.IsNullOrEmpty(dialogue.selectedVoiceId) || VoiceLibrary.IsNoVoice(dialogue.selectedVoiceId))
        {
            Debug.Log($"🔇 Skipping TTS for '{dialogue.characterName}' - No voice selected (intentional)");
            dialogue.hasAudio = false;
            dialogue.audioFilePath = "";
            onComplete?.Invoke(true, "No Voice Selected - Skipped");
            yield break;
        }


        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
        {
            Debug.LogError("ElevenLabs API Key not set or invalid!");
            onComplete?.Invoke(false, "API Key not configured");
            yield break;
        }

        // Use the dialogue's selected voice ID
        string voiceId = dialogue.selectedVoiceId;
        var voice = VoiceLibrary.GetVoiceById(voiceId);
        string url = apiUrl + voiceId;

        Debug.Log($"🎤 Generating TTS for '{dialogue.characterName}' using voice: {voice.voiceName}");

        string jsonPayload = $@"{{
        ""text"": ""{EscapeJson(dialogue.dialogueText)}"",
        ""model_id"": ""eleven_monolingual_v1"",
        ""voice_settings"": {{
            ""stability"": 0.5,
            ""similarity_boost"": 0.75
        }}
    }}";

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("xi-api-key", apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string teacherId = GetCurrentTeacherId();
                int storyIndex = GetCurrentStoryIndex();
                string sanitizedName = SanitizeFileName(dialogue.characterName);

                // ✅ FIXED: Use consistent filename WITHOUT timestamp
                int dialogueIndex = GetDialogueIndex(dialogue);

                // ✅ NEW: Delete existing audio files for this dialogue index first
                DeleteExistingAudioFiles(dialogueIndex, sanitizedName, voice.voiceName);

                // ✅ FIXED: Create filename WITHOUT timestamp: dialogue_{index}_{character}_{voice}.mp3
                string fileName = $"dialogue_{dialogueIndex}_{sanitizedName}_{voice.voiceName}.mp3";

                string audioDir = GetAudioSaveDirectory();
                string absolutePath = Path.Combine(audioDir, fileName);
                string relativePath = GetAudioRelativePath(fileName);

                File.WriteAllBytes(absolutePath, request.downloadHandler.data);

                // ✅ UPDATED: Store relative path and additional audio info
                dialogue.audioFilePath = relativePath; // Store RELATIVE path for cross-device
                dialogue.audioFileName = fileName;     // Store filename separately
                dialogue.hasAudio = true;
                dialogue.needsAudioRegeneration = false;

                // ✅ NEW: Update audio info in storage to persist it
                DialogueStorage.UpdateDialogueAudioInfo(dialogueIndex, relativePath, fileName);

                Debug.Log($"✅ TTS generated: '{dialogue.characterName}' → {relativePath}");

                onComplete?.Invoke(true, relativePath);
            }

            else
            {
                Debug.LogError($"❌ TTS failed for '{dialogue.characterName}': {request.error}");
                onComplete?.Invoke(false, request.error);
            }
        }
    }

    // ✅ NEW: Delete existing audio files for a dialogue to prevent duplicates
    private void DeleteExistingAudioFiles(int dialogueIndex, string characterName, string voiceName)
    {
        try
        {
            string audioDir = GetAudioSaveDirectory();
            if (!Directory.Exists(audioDir)) return;

            string sanitizedName = SanitizeFileName(characterName);

            // Pattern 1: Exact match (without timestamp)
            string exactPattern = $"dialogue_{dialogueIndex}_{sanitizedName}_{voiceName}.mp3";

            // Pattern 2: Old pattern with timestamps
            string timestampPattern = $"dialogue_{dialogueIndex}_{sanitizedName}_{voiceName}_*.mp3";

            // Delete exact match files
            string[] exactFiles = Directory.GetFiles(audioDir, exactPattern);
            foreach (string file in exactFiles)
            {
                File.Delete(file);
                Debug.Log($"🗑️ Deleted existing audio: {Path.GetFileName(file)}");
            }

            // Delete timestamped files
            string[] timestampFiles = Directory.GetFiles(audioDir, timestampPattern);
            foreach (string file in timestampFiles)
            {
                File.Delete(file);
                Debug.Log($"🗑️ Deleted old timestamped audio: {Path.GetFileName(file)}");
            }

            if (exactFiles.Length > 0 || timestampFiles.Length > 0)
            {
                Debug.Log($"✅ Cleared {exactFiles.Length + timestampFiles.Length} existing audio files for dialogue {dialogueIndex}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"⚠️ Error deleting existing audio files: {ex.Message}");
        }
    }



    private int GetDialogueIndex(DialogueLine targetDialogue)
    {
        var dialogues = DialogueStorage.GetAllDialogues();
        for (int i = 0; i < dialogues.Count; i++)
        {
            if (dialogues[i] == targetDialogue)
                return i;
        }
        return -1;
    }

    public IEnumerator GenerateAllTTS(List<DialogueLine> dialogues, Action<int, int, string> onProgress, Action<bool> onComplete)
    {
        int total = dialogues.Count;
        int completed = 0;
        int failed = 0;

        string teacherId = GetCurrentTeacherId();
        int storyIndex = GetCurrentStoryIndex();

        Debug.Log($"🎙️ Starting TTS generation for {total} dialogues (Teacher: {teacherId}, Story: {storyIndex})");

        for (int i = 0; i < dialogues.Count; i++)
        {
            DialogueLine dialogue = dialogues[i];
            var voice = VoiceLibrary.GetVoiceById(dialogue.selectedVoiceId);

            Debug.Log($"Processing dialogue {i + 1}/{total}: '{dialogue.characterName}' (Voice: {voice.voiceName})");

            // Check if audio already exists for this dialogue
            string existingAudio = FindExistingAudioFile(dialogue, i);
            if (!string.IsNullOrEmpty(existingAudio))
            {
                dialogue.audioFilePath = existingAudio;
                dialogue.hasAudio = true;
                completed++;
                onProgress?.Invoke(completed, total, $"Skipped: {dialogue.characterName} ({voice.voiceName})");
                continue;
            }

            bool success = false;
            string errorMsg = "";

            yield return GenerateTTS(dialogue, (isSuccess, message) =>
            {
                success = isSuccess;
                errorMsg = message;
            });

            if (success)
            {
                completed++;
                onProgress?.Invoke(completed, total, $"Generated: {dialogue.characterName} ({voice.voiceName})");
            }
            else
            {
                failed++;
                onProgress?.Invoke(completed, total, $"Failed: {dialogue.characterName} - {errorMsg}");
            }

            yield return new WaitForSeconds(0.3f);
        }

        bool allSuccess = (failed == 0 && completed == total);
        Debug.Log($"🎬 TTS Complete: {completed}/{total} succeeded, {failed} failed");
        onComplete?.Invoke(allSuccess);
    }

    private string FindExistingAudioFile(DialogueLine dialogue, int dialogueIndex)
    {
        try
        {
            string audioDir = GetAudioSaveDirectory();
            if (!Directory.Exists(audioDir)) return null;

            string sanitizedName = SanitizeFileName(dialogue.characterName);
            var voice = VoiceLibrary.GetVoiceById(dialogue.selectedVoiceId);

            // Look for files matching the pattern: dialogue_{index}_{character}_{voice}_*.mp3
            string searchPattern = $"dialogue_{dialogueIndex}_{sanitizedName}_{voice.voiceName}_*.mp3";
            string[] files = Directory.GetFiles(audioDir, searchPattern);

            if (files.Length > 0)
            {
                // Return the most recent file
                Array.Sort(files);
                return files[files.Length - 1];
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Error finding existing audio: {ex.Message}");
            return null;
        }
    }

    // Method for students to find audio files
    public string FindStudentAudioFile(string teacherId, int storyIndex, int dialogueIndex, string characterName, string voiceId)
    {
        try
        {
            // Build the audio directory path following the same pattern
            string relativeDir = Path.Combine(teacherId, $"story_{storyIndex}", "audio");
            string audioDir = Path.Combine(Application.persistentDataPath, relativeDir);

            if (!Directory.Exists(audioDir))
            {
                Debug.LogWarning($"Audio directory not found: {audioDir}");
                return null;
            }

            string sanitizedName = SanitizeFileName(characterName);
            var voice = VoiceLibrary.GetVoiceById(voiceId);

            // Look for files matching the pattern
            string searchPattern = $"dialogue_{dialogueIndex}_{sanitizedName}_{voice.voiceName}_*.mp3";
            string[] files = Directory.GetFiles(audioDir, searchPattern);

            if (files.Length > 0)
            {
                // Return the most recent file
                Array.Sort(files);
                string latestFile = files[files.Length - 1];
                Debug.Log($"✅ Found student audio: {latestFile}");
                return latestFile;
            }

            Debug.LogWarning($"No audio file found for pattern: {searchPattern}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error finding student audio: {ex.Message}");
            return null;
        }
    }

    // Update DialoguePlayer to use this method for students
    public string GetAudioFilePathForDialogue(DialogueLine dialogue, int dialogueIndex)
    {
        // If we're in teacher mode or the path already exists, use it directly
        if (!string.IsNullOrEmpty(dialogue.audioFilePath) && File.Exists(dialogue.audioFilePath))
        {
            return dialogue.audioFilePath;
        }

        // For students, we need to find the audio file
        string userRole = PlayerPrefs.GetString("UserRole", "student");
        if (userRole.ToLower() == "student")
        {
            // Extract teacher ID and story index from the story data
            string storyJson = StudentPrefs.GetString("CurrentStoryData", "");
            if (!string.IsNullOrEmpty(storyJson))
            {
                try
                {
                    var studentStory = JsonUtility.FromJson<StoryData>(storyJson);
                    if (!string.IsNullOrEmpty(studentStory.backgroundPath))
                    {
                        string[] pathParts = studentStory.backgroundPath.Split(Path.DirectorySeparatorChar);
                        if (pathParts.Length >= 2)
                        {
                            string teacherId = pathParts[0];
                            int storyIndex = int.Parse(pathParts[1].Replace("story_", ""));

                            return FindStudentAudioFile(teacherId, storyIndex, dialogueIndex,
                                dialogue.characterName, dialogue.selectedVoiceId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error getting audio path for student: {ex.Message}");
                }
            }
        }

        return dialogue.audioFilePath;
    }

    private string SanitizeFileName(string fileName)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }
        return fileName;
    }

    private string EscapeJson(string text)
    {
        return text
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    [ContextMenu("Debug Audio File Structure")]
    public void DebugAudioFileStructure()
    {
        string teacherId = GetCurrentTeacherId();
        int storyIndex = GetCurrentStoryIndex();
        string audioDir = GetAudioSaveDirectory();

        Debug.Log($"🔍 Audio file structure for Teacher: {teacherId}, Story: {storyIndex}");
        Debug.Log($"📁 Directory: {audioDir}");

        if (Directory.Exists(audioDir))
        {
            string[] files = Directory.GetFiles(audioDir, "*.mp3");
            foreach (string file in files)
            {
                FileInfo info = new FileInfo(file);
                Debug.Log($"   🔊 {Path.GetFileName(file)} ({info.Length / 1024} KB)");
            }
            Debug.Log($"📊 Total audio files: {files.Length}");
        }
        else
        {
            Debug.Log("   No audio directory found");
        }
    }
}
