using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class ElevenLabsTTSManager : MonoBehaviour
{
    public static ElevenLabsTTSManager Instance { get; private set; }

    [Header("ElevenLabs API Settings")]
    [SerializeField] private string apiKey = "YOUR_API_KEY_HERE";
    
    private string apiUrl = "https://api.elevenlabs.io/v1/text-to-speech/";
    private string audioSaveDirectory = "DialogueAudio";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateAudioDirectory();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void CreateAudioDirectory()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, audioSaveDirectory);
        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
            Debug.Log($"Created audio directory: {path}");
        }
    }

    public IEnumerator GenerateTTS(DialogueLine dialogue, Action<bool, string> onComplete)
    {
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
                string sanitizedName = SanitizeFileName(dialogue.characterName);
                string fileName = $"{sanitizedName}_{voice.voiceName}_{DateTime.Now.Ticks}.mp3";
                string filePath = System.IO.Path.Combine(
                    Application.persistentDataPath,
                    audioSaveDirectory,
                    fileName
                );

                System.IO.File.WriteAllBytes(filePath, request.downloadHandler.data);

                dialogue.audioFilePath = filePath;
                dialogue.hasAudio = true;

                Debug.Log($"✅ TTS generated: '{dialogue.characterName}' ({voice.voiceName}) → {fileName}");
                onComplete?.Invoke(true, filePath);
            }
            else
            {
                Debug.LogError($"❌ TTS failed for '{dialogue.characterName}': {request.error}");
                onComplete?.Invoke(false, request.error);
            }
        }
    }

    public IEnumerator GenerateAllTTS(List<DialogueLine> dialogues, Action<int, int, string> onProgress, Action<bool> onComplete)
    {
        int total = dialogues.Count;
        int completed = 0;
        int failed = 0;

        Debug.Log($"🎙️ Starting TTS generation for {total} dialogues");

        for (int i = 0; i < dialogues.Count; i++)
        {
            DialogueLine dialogue = dialogues[i];
            var voice = VoiceLibrary.GetVoiceById(dialogue.selectedVoiceId);
            
            Debug.Log($"Processing dialogue {i+1}/{total}: '{dialogue.characterName}' (Voice: {voice.voiceName}) - HasAudio: {dialogue.hasAudio}");
            
            if (dialogue.hasAudio && !string.IsNullOrEmpty(dialogue.audioFilePath) && System.IO.File.Exists(dialogue.audioFilePath))
            {
                completed++;
                onProgress?.Invoke(completed, total, $"Skipped: {dialogue.characterName} ({voice.voiceName})");
                continue;
            }

            if (dialogue.hasAudio && !System.IO.File.Exists(dialogue.audioFilePath))
            {
                Debug.Log($"⚠️ Audio file missing for {dialogue.characterName}, regenerating...");
                dialogue.hasAudio = false;
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

    private string SanitizeFileName(string fileName)
    {
        foreach (char c in System.IO.Path.GetInvalidFileNameChars())
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
}