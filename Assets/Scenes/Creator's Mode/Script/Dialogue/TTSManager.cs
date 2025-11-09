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

        // ✅ FIX: Use forward slashes for consistency (works on all platforms)
        return $"{teacherId}/story_{storyIndex}/audio/{fileName}";
    }

    private string GetCurrentTeacherId()
    {
        if (StoryManager.Instance != null && StoryManager.Instance.IsCurrentUserTeacher())
        {
            return StoryManager.Instance.GetCurrentTeacherId();
        }

        string studentStoryJson = StudentPrefs.GetString("CurrentStoryData", "");
        if (!string.IsNullOrEmpty(studentStoryJson))
        {
            try
            {
                var studentStory = JsonUtility.FromJson<StoryData>(studentStoryJson);
                if (!string.IsNullOrEmpty(studentStory.backgroundPath))
                {
                    string[] pathParts = studentStory.backgroundPath.Split(Path.DirectorySeparatorChar);
                    if (pathParts.Length > 0)
                    {
                        return pathParts[0];
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

    public IEnumerator GenerateTTS(DialogueLine dialogue, Action<bool, string> onComplete)
    {
        if (string.IsNullOrEmpty(dialogue.selectedVoiceId) || VoiceLibrary.IsNoVoice(dialogue.selectedVoiceId))
        {
            Debug.Log($"🔇 Skipping TTS for '{dialogue.characterName}' - No voice selected");
            dialogue.hasAudio = false;
            dialogue.audioFilePath = "";
            dialogue.audioStoragePath = "";
            onComplete?.Invoke(true, "No Voice Selected - Skipped");
            yield break;
        }

        int dialogueIndex = GetDialogueIndex(dialogue);
        if (dialogueIndex < 0)
        {
            Debug.LogError($"❌ Could not find dialogue index for: {dialogue.characterName}");
            onComplete?.Invoke(false, "Dialogue index not found");
            yield break;
        }

        DeleteAllAudioFilesForDialogue(dialogueIndex, dialogue.characterName);

        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
        {
            Debug.LogError("ElevenLabs API Key not set!");
            onComplete?.Invoke(false, "API Key not configured");
            yield break;
        }

        string voiceId = dialogue.selectedVoiceId;
        var voice = VoiceLibrary.GetVoiceById(voiceId);
        string url = apiUrl + voiceId;

        Debug.Log($"🎤 Generating TTS: '{dialogue.characterName}' - {voice.voiceName} (Index: {dialogueIndex})");

        string jsonPayload = $@"{{
        ""text"": ""{EscapeJson(dialogue.dialogueText)}"",
        ""model_id"": ""eleven_turbo_v2_5"",
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

                // ✅ KEY FIX: Sanitize EVERYTHING before creating filenames
                string sanitizedCharName = SanitizeFileName(dialogue.characterName);
                string sanitizedVoiceName = SanitizeFileName(voice.voiceName);

                // ✅ This filename is now GUARANTEED to have no spaces
                string sanitizedFileName = $"dialogue_{dialogueIndex}_{sanitizedCharName}_{sanitizedVoiceName}.mp3";

                Debug.Log($"📝 Sanitized filename: {sanitizedFileName}");
                Debug.Log($"   Character: '{dialogue.characterName}' → '{sanitizedCharName}'");
                Debug.Log($"   Voice: '{voice.voiceName}' → '{sanitizedVoiceName}'");

                // ========== STEP 1: SAVE LOCALLY ==========
                string audioDir = GetAudioSaveDirectory();
                string absolutePath = Path.Combine(audioDir, sanitizedFileName);
                string relativePath = GetAudioRelativePath(sanitizedFileName);

                File.WriteAllBytes(absolutePath, request.downloadHandler.data);
                Debug.Log($"💾 Audio saved locally: {relativePath}");

                // ========== STEP 2: UPLOAD TO S3 (Async) ==========
                string s3Url = null;
                bool uploadSuccess = false;

                if (S3StorageService.Instance != null && S3StorageService.Instance.IsReady)
                {
                    Debug.Log($"☁️ Uploading audio to S3...");

                    // ✅ FIX: Pass sanitized filename to S3 (without .mp3 extension)
                    string s3BaseFileName = $"{dialogueIndex}_{sanitizedCharName}_{sanitizedVoiceName}";

                    var uploadTask = S3StorageService.Instance.UploadVoiceAudio(
                        request.downloadHandler.data,
                        teacherId,
                        storyIndex,
                        s3BaseFileName
                    );

                    while (!uploadTask.IsCompleted)
                    {
                        yield return null;
                    }

                    s3Url = uploadTask.Result;

                    if (!string.IsNullOrEmpty(s3Url))
                    {
                        uploadSuccess = true;
                        Debug.Log($"✅ Audio uploaded to S3: {s3Url}");

                        // ✅ VERIFY: Check that S3 URL has no spaces
                        if (s3Url.Contains(" "))
                        {
                            Debug.LogError($"❌ CRITICAL: S3 URL still contains spaces!");
                            Debug.LogError($"   This means S3StorageService.UploadVoiceAudio needs fixing!");
                        }
                        else
                        {
                            Debug.Log($"✅ S3 URL verified: no spaces detected");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ S3 upload failed, using local only");
                    }
                }
                else
                {
                    Debug.LogWarning($"⚠️ S3 not available, using local storage only");
                }

                // ========== STEP 3: UPDATE DIALOGUE - USE SANITIZED VALUES ==========
                // ✅ CRITICAL FIX: Use sanitized filename for ALL fields
                dialogue.audioFilePath = relativePath;              // Already uses sanitizedFileName
                dialogue.audioFileName = sanitizedFileName;         // ✅ Now sanitized
                dialogue.audioStoragePath = s3Url ?? "";           // S3 should already be sanitized
                dialogue.audioFileSize = request.downloadHandler.data.Length;
                dialogue.hasAudio = true;
                dialogue.needsAudioRegeneration = false;

                // ✅ Persist to storage with sanitized values
                DialogueStorage.UpdateDialogueAudioInfo(dialogueIndex, relativePath, sanitizedFileName, s3Url);

                // ✅ VERIFICATION LOG
                Debug.Log($"📋 Dialogue updated with:");
                Debug.Log($"   audioFileName: {dialogue.audioFileName}");
                Debug.Log($"   audioFilePath: {dialogue.audioFilePath}");
                Debug.Log($"   audioStoragePath: {dialogue.audioStoragePath}");

                // Check for spaces
                bool hasSpaces = dialogue.audioFileName.Contains(" ") ||
                                dialogue.audioFilePath.Contains(" ") ||
                                (dialogue.audioStoragePath?.Contains(" ") ?? false);

                if (hasSpaces)
                {
                    Debug.LogError($"❌ WARNING: Dialogue fields still contain spaces!");
                }
                else
                {
                    Debug.Log($"✅ All dialogue fields verified: no spaces");
                }

                string cloudStatus = uploadSuccess ? $"Cloud: {s3Url}" : "Local only";
                Debug.Log($"✅ TTS complete: '{dialogue.characterName}' → {cloudStatus}");

                onComplete?.Invoke(true, relativePath);
            }
            else
            {
                Debug.LogError($"❌ TTS failed for '{dialogue.characterName}': {request.error}");
                onComplete?.Invoke(false, request.error);
            }
        }
    }

    private void DeleteAllAudioFilesForDialogue(int dialogueIndex, string characterName)
    {
        try
        {
            string audioDir = GetAudioSaveDirectory();
            if (!Directory.Exists(audioDir)) return;

            string sanitizedName = SanitizeFileName(characterName);

            string[] patterns = {
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
            Debug.LogError($"❌ Error deleting audio files: {ex.Message}");
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
            string sanitizedVoice = SanitizeFileName(voice.voiceName);

            string searchPattern = $"dialogue_{dialogueIndex}_{sanitizedName}_{sanitizedVoice}.mp3";
            string exactPath = Path.Combine(audioDir, searchPattern);

            if (File.Exists(exactPath))
            {
                return exactPath;
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Error finding existing audio: {ex.Message}");
            return null;
        }
    }

    public string FindStudentAudioFile(string teacherId, int storyIndex, int dialogueIndex, string characterName, string voiceId)
    {
        try
        {
            string relativeDir = Path.Combine(teacherId, $"story_{storyIndex}", "audio");
            string audioDir = Path.Combine(Application.persistentDataPath, relativeDir);

            if (!Directory.Exists(audioDir))
            {
                Debug.LogWarning($"Audio directory not found: {audioDir}");
                return null;
            }

            string sanitizedName = SanitizeFileName(characterName);
            var voice = VoiceLibrary.GetVoiceById(voiceId);
            string sanitizedVoice = SanitizeFileName(voice.voiceName);

            string searchPattern = $"dialogue_{dialogueIndex}_{sanitizedName}_{sanitizedVoice}.mp3";
            string exactPath = Path.Combine(audioDir, searchPattern);

            if (File.Exists(exactPath))
            {
                Debug.Log($"✅ Found student audio: {exactPath}");
                return exactPath;
            }

            Debug.LogWarning($"No audio file found for: {searchPattern}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error finding student audio: {ex.Message}");
            return null;
        }
    }

    public string GetAudioFilePathForDialogue(DialogueLine dialogue, int dialogueIndex)
    {
        if (!string.IsNullOrEmpty(dialogue.audioFilePath) && File.Exists(dialogue.audioFilePath))
        {
            return dialogue.audioFilePath;
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

    // ✅ COMPREHENSIVE: Sanitize all problematic characters
    private string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return fileName;

        // Remove invalid file system characters
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }

        // Replace spaces and problematic URL/S3 characters
        fileName = fileName.Replace(' ', '_');
        fileName = fileName.Replace('#', '_');
        fileName = fileName.Replace('%', '_');
        fileName = fileName.Replace('&', '_');
        fileName = fileName.Replace('{', '_');
        fileName = fileName.Replace('}', '_');
        fileName = fileName.Replace('\\', '_');
        fileName = fileName.Replace('<', '_');
        fileName = fileName.Replace('>', '_');
        fileName = fileName.Replace('*', '_');
        fileName = fileName.Replace('?', '_');
        fileName = fileName.Replace('/', '_');
        fileName = fileName.Replace('$', '_');
        fileName = fileName.Replace('!', '_');
        fileName = fileName.Replace('\'', '_');
        fileName = fileName.Replace('"', '_');
        fileName = fileName.Replace(':', '_');
        fileName = fileName.Replace('@', '_');
        fileName = fileName.Replace('+', '_');
        fileName = fileName.Replace('`', '_');
        fileName = fileName.Replace('|', '_');
        fileName = fileName.Replace('=', '_');

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
            Debug.Log($"🔊 Total audio files: {files.Length}");
        }
        else
        {
            Debug.Log("   No audio directory found");
        }
    }

    [ContextMenu("Debug Audio Storage")]
    public void DebugAudioStorage()
    {
        var dialogues = DialogueStorage.GetAllDialogues();
        Debug.Log($"🔊 === AUDIO STORAGE DEBUG ===");

        for (int i = 0; i < dialogues.Count; i++)
        {
            var d = dialogues[i];
            Debug.Log($"Dialogue {i}: '{d.characterName}'");
            Debug.Log($"  Has Audio: {d.hasAudio}");
            Debug.Log($"  audioFileName: {d.audioFileName}");
            Debug.Log($"  audioFilePath: {d.audioFilePath}");
            Debug.Log($"  audioStoragePath: {d.audioStoragePath}");
            Debug.Log($"  File Size: {d.audioFileSize} bytes");

            // Check for spaces
            bool hasSpaces = d.audioFileName?.Contains(" ") == true ||
                           d.audioFilePath?.Contains(" ") == true ||
                           d.audioStoragePath?.Contains(" ") == true;

            if (hasSpaces)
            {
                Debug.LogWarning($"  ⚠️ WARNING: Contains spaces!");
            }

            if (!string.IsNullOrEmpty(d.audioFilePath))
            {
                bool localExists = File.Exists(Path.Combine(Application.persistentDataPath, d.audioFilePath));
                Debug.Log($"  Local Exists: {localExists}");
            }
        }
    }
}
