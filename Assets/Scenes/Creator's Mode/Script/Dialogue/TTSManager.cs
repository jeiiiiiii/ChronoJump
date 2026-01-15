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

        Debug.Log($"Starting TTS generation for {total} dialogues (Teacher: {teacherId}, Story: {storyIndex})");

        for (int i = 0; i < dialogues.Count; i++)
        {
            DialogueLine dialogue = dialogues[i];
            var voice = VoiceLibrary.GetVoiceById(dialogue.selectedVoiceId);

            Debug.Log($"Processing dialogue {i + 1}/{total}: '{dialogue.characterName}' (Voice: {voice.voiceName})");
            Debug.Log($"  hasAudio: {dialogue.hasAudio}");
            Debug.Log($"  audioFilePath: {dialogue.audioFilePath}");

            // CRITICAL FIX: Check hasAudio flag FIRST, not file existence
            // This respects when audio has been intentionally invalidated by edits
            if (dialogue.hasAudio && !string.IsNullOrEmpty(dialogue.audioFilePath))
            {
                // Verify the file actually exists
                string fullPath = Path.Combine(Application.persistentDataPath, dialogue.audioFilePath);
                if (File.Exists(fullPath))
                {
                    Debug.Log($"  Audio valid, skipping generation");
                    completed++;
                    onProgress?.Invoke(completed, total, $"Skipped: {dialogue.characterName} ({voice.voiceName})");
                    continue;
                }
                else
                {
                    // File marked as having audio but doesn't exist - regenerate
                    Debug.LogWarning($"  Audio file missing, will regenerate");
                }
            }
            else
            {
                Debug.Log($"  No valid audio, generating...");
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
        Debug.Log($"TTS Complete: {completed}/{total} succeeded, {failed} failed");
        onComplete?.Invoke(allSuccess);
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

    // Add this to your ElevenLabsTTSManager class or create a new script

[ContextMenu("Validate All Voices")]
public void ValidateAllVoices()
{
    if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
    {
        Debug.LogError("❌ API Key not set! Cannot validate voices.");
        return;
    }
    
    StartCoroutine(SafeValidateAllVoicesCoroutine());
}

private IEnumerator SafeValidateAllVoicesCoroutine()
{
    Debug.Log("🔍 === VOICE VALIDATION STARTED (SAFE MODE) ===");
    Debug.Log($"Testing API Key: {apiKey.Substring(0, Math.Min(15, apiKey.Length))}...");
    
    List<VoiceProfile> voices = null;
    
    // Try to get voices without yield
    bool voiceLoadSuccess = false;
    try
    {
        voices = VoiceLibrary.GetAvailableVoices();
        voiceLoadSuccess = true;
    }
    catch (Exception ex)
    {
        Debug.LogError($"❌ Could not get voice library: {ex.Message}");
        Debug.LogError("Make sure VoiceLibrary class exists and GetAvailableVoices() method is accessible.");
    }
    
    if (!voiceLoadSuccess || voices == null || voices.Count == 0)
    {
        Debug.LogWarning("⚠️ No voices found in VoiceLibrary");
        yield break;
    }
    
    int totalVoices = voices.Count;
    int workingVoices = 0;
    int failedVoices = 0;
    
    List<string> workingList = new List<string>();
    List<string> failedList = new List<string>();
    
    for (int i = 0; i < voices.Count; i++)
    {
        VoiceProfile voice = voices[i];
        
        if (voice == null)
        {
            Debug.LogWarning($"⚠️ Voice at index {i} is null, skipping...");
            continue;
        }
        
        Debug.Log($"\n📋 Testing {i + 1}/{totalVoices}: {voice.voiceName ?? "Unknown"} (ID: {voice.voiceId ?? "Unknown"})");
        
        bool isWorking = false;
        string errorMessage = "";
        bool callbackCompleted = false;
        
        // Call the safe test coroutine
        yield return SafeTestSingleVoice(voice.voiceId, voice.voiceName, (success, error) =>
        {
            isWorking = success;
            errorMessage = error;
            callbackCompleted = true;
        });
        
        // Wait for callback with timeout
        float timeout = 15f;
        float elapsed = 0f;
        while (!callbackCompleted && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (!callbackCompleted)
        {
            Debug.LogWarning($"   ⏱️ TIMEOUT: {voice.voiceName} took too long to respond");
            failedVoices++;
            failedList.Add($"{voice.voiceName} ({voice.voiceId}) - Request Timeout");
        }
        else if (isWorking)
        {
            workingVoices++;
            workingList.Add($"{voice.voiceName} ({voice.voiceId})");
            Debug.Log($"   ✅ WORKING: {voice.voiceName}");
        }
        else
        {
            failedVoices++;
            failedList.Add($"{voice.voiceName} ({voice.voiceId}) - {errorMessage}");
            Debug.LogWarning($"   ❌ FAILED: {voice.voiceName} - {errorMessage}");
        }
        
        // Small delay to avoid rate limiting
        yield return new WaitForSeconds(0.5f);
    }
    
    // Final Report
    Debug.Log("\n" + "=".PadRight(60, '='));
    Debug.Log("📊 === VOICE VALIDATION COMPLETE ===");
    Debug.Log("=".PadRight(60, '='));
    Debug.Log($"Total Voices: {totalVoices}");
    Debug.Log($"✅ Working: {workingVoices}");
    Debug.Log($"❌ Failed: {failedVoices}");
    
    if (workingList.Count > 0)
    {
        Debug.Log("\n✅ WORKING VOICES:");
        foreach (var voice in workingList)
        {
            Debug.Log($"   • {voice}");
        }
    }
    
    if (failedList.Count > 0)
    {
        Debug.Log("\n❌ FAILED VOICES:");
        foreach (var voice in failedList)
        {
            Debug.Log($"   • {voice}");
        }
    }
    
    Debug.Log("\n" + "=".PadRight(60, '='));
    
    // Provide recommendations
    if (failedVoices == totalVoices)
    {
        Debug.LogError("⚠️ ALL VOICES FAILED!");
        Debug.LogError("Possible issues:");
        Debug.LogError("   1. API key is invalid or expired");
        Debug.LogError("   2. API key lacks 'Voice Generation' or 'Voices' permissions");
        Debug.LogError("   3. Voices are in a different workspace");
        Debug.LogError("   4. Network connectivity issues");
    }
    else if (failedVoices > 0)
    {
        Debug.LogWarning("⚠️ SOME VOICES FAILED!");
        Debug.LogWarning("These voices may not exist in your workspace or you lack permission to use them.");
        Debug.LogWarning("Consider removing failed voices from VoiceLibrary or creating them in your workspace.");
    }
    else
    {
        Debug.Log("🎉 ALL VOICES ARE WORKING! Your setup is perfect!");
    }
}

private IEnumerator SafeTestSingleVoice(string voiceId, string voiceName, Action<bool, string> onComplete)
{
    // Validate inputs first (no yield needed)
    if (string.IsNullOrEmpty(voiceId))
    {
        if (onComplete != null)
        {
            onComplete(false, "Voice ID is null or empty");
        }
        yield break;
    }
    
    UnityWebRequest request = null;
    bool requestCreated = false;
    
    // Create request without try-catch
    string testText = "Test";
    string url = apiUrl + voiceId;
    
    string jsonPayload = $@"{{
        ""text"": ""{testText}"",
        ""model_id"": ""eleven_turbo_v2_5"",
        ""voice_settings"": {{
            ""stability"": 0.5,
            ""similarity_boost"": 0.75
        }}
    }}";
    
    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
    
    request = new UnityWebRequest(url, "POST");
    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
    request.downloadHandler = new DownloadHandlerBuffer();
    request.SetRequestHeader("Content-Type", "application/json");
    request.SetRequestHeader("xi-api-key", apiKey.Trim());
    request.timeout = 10;
    requestCreated = true;
    
    // Send request (yield allowed here, no catch)
    yield return request.SendWebRequest();
    
    // Process response without try-catch around yield
    bool success = false;
    string errorMsg = "";
    
    if (request.result == UnityWebRequest.Result.Success)
    {
        success = true;
    }
    else
    {
        errorMsg = $"HTTP {request.responseCode}: {request.error}";
        
        // Try to get more detailed error from response
        if (!string.IsNullOrEmpty(request.downloadHandler.text))
        {
            string errorJson = request.downloadHandler.text;
            if (errorJson.Contains("detail") || errorJson.Contains("message"))
            {
                int maxLength = Math.Min(200, errorJson.Length);
                errorMsg += $" | {errorJson.Substring(0, maxLength)}";
            }
        }
    }
    
    // Cleanup
    if (requestCreated && request != null)
    {
        request.Dispose();
    }
    
    // Call callback
    if (onComplete != null)
    {
        onComplete(success, errorMsg);
    }
}

[ContextMenu("Check API Permissions")]
public void CheckAPIPermissions()
{
    if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
    {
        Debug.LogError("❌ API Key not set! Cannot check permissions.");
        return;
    }
    
    StartCoroutine(SafeCheckAPIPermissionsCoroutine());
}

private IEnumerator SafeCheckAPIPermissionsCoroutine()
{
    Debug.Log("🔑 === CHECKING API KEY PERMISSIONS (SAFE MODE) ===");
    
    // Test 1: Get available voices
    string voicesUrl = "https://api.elevenlabs.io/v1/voices";
    UnityWebRequest voicesRequest = UnityWebRequest.Get(voicesUrl);
    voicesRequest.SetRequestHeader("xi-api-key", apiKey.Trim());
    voicesRequest.timeout = 10;
    
    yield return voicesRequest.SendWebRequest();
    
    if (voicesRequest.result == UnityWebRequest.Result.Success)
    {
        Debug.Log("✅ Can access Voices endpoint");
        
        string response = voicesRequest.downloadHandler.text;
        int voiceCount = response.Split(new[] { "voice_id" }, StringSplitOptions.None).Length - 1;
        Debug.Log($"   Found {voiceCount} voices in your workspace");
    }
    else
    {
        Debug.LogError($"❌ Cannot access Voices endpoint: {voicesRequest.error}");
        Debug.LogError("   Your API key may not have 'Voices' permission enabled");
    }
    
    voicesRequest.Dispose();
    
    yield return new WaitForSeconds(0.5f);
    
    // Test 2: Get user info
    string userUrl = "https://api.elevenlabs.io/v1/user";
    UnityWebRequest userRequest = UnityWebRequest.Get(userUrl);
    userRequest.SetRequestHeader("xi-api-key", apiKey.Trim());
    userRequest.timeout = 10;
    
    yield return userRequest.SendWebRequest();
    
    if (userRequest.result == UnityWebRequest.Result.Success)
    {
        Debug.Log("✅ Can access User endpoint");
        string response = userRequest.downloadHandler.text;
        int maxLength = Math.Min(200, response.Length);
        Debug.Log($"   Response preview: {response.Substring(0, maxLength)}...");
    }
    else
    {
        Debug.LogError($"❌ Cannot access User endpoint: {userRequest.error}");
    }
    
    userRequest.Dispose();
    
    Debug.Log("\n=== PERMISSION CHECK COMPLETE ===");
}

[ContextMenu("Quick Voice Test")]
public void QuickVoiceTest()
{
    if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
    {
        Debug.LogError("❌ API Key not set!");
        return;
    }
    
    StartCoroutine(QuickVoiceTestCoroutine());
}

    private IEnumerator QuickVoiceTestCoroutine()
    {
        Debug.Log("⚡ === QUICK VOICE TEST ===");
        Debug.Log("Testing with a known public voice...");

        // Test with Rachel (a public ElevenLabs voice)
        string testVoiceId = "21m00Tcm4TlvDq8ikWAM";
        string testVoiceName = "Rachel (Public Test Voice)";

        bool success = false;
        string error = "";

        yield return SafeTestSingleVoice(testVoiceId, testVoiceName, (s, e) =>
        {
            success = s;
            error = e;
        });

        if (success)
        {
            Debug.Log("✅ API KEY IS WORKING! Your custom voices may be in a different workspace.");
        }
        else
        {
            Debug.LogError($"❌ API KEY TEST FAILED: {error}");
            Debug.LogError("Your API key may be invalid or lack permissions.");
        }
    }

    [ContextMenu("Check Account Status")]
    public void CheckAccountStatus()
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
        {
            Debug.LogError("❌ API Key not set!");
            return;
        }

        StartCoroutine(CheckAccountStatusCoroutine());
    }


    private IEnumerator CheckAccountStatusCoroutine()
    {
        Debug.Log("🔍 === CHECKING ACCOUNT STATUS ===");

        // Test with a simple voice generation request
        string testVoiceId = "21m00Tcm4TlvDq8ikWAM"; // Rachel - public voice
        string url = apiUrl + testVoiceId;

        string jsonPayload = @"{
        ""text"": ""Hello, this is a test."",
        ""model_id"": ""eleven_turbo_v2_5"",
        ""voice_settings"": {
            ""stability"": 0.5,
            ""similarity_boost"": 0.75
        }
    }";

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("xi-api-key", apiKey.Trim());
            request.timeout = 15;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Account is ACTIVE - Voice generation working!");
            }
            else
            {
                Debug.LogError($"❌ Account Issue: {request.error}");

                if (!string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    string response = request.downloadHandler.text;
                    Debug.LogError($"Full Response: {response}");

                    if (response.Contains("unusual_activity"))
                    {
                        Debug.LogError("🚨 ACCOUNT FLAGGED FOR UNUSUAL ACTIVITY");
                        Debug.LogError("Solutions:");
                        Debug.LogError("1. Wait 24-48 hours for automatic reset");
                        Debug.LogError("2. Disable any VPN/Proxy");
                        Debug.LogError("3. Use a different network");
                        Debug.LogError("4. Upgrade to paid plan");
                        Debug.LogError("5. Contact support@elevenlabs.io");
                    }
                    else if (response.Contains("character_limit"))
                    {
                        Debug.LogError("📊 Character limit exceeded - wait for reset or upgrade");
                    }
                }
            }
        }
    }

}