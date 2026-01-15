using UnityEngine;

// Plays local preview samples for voices.
// Put your mp3 files under: Assets/Resources/VoicePreviews/
// Name them with either the ElevenLabs voiceId or the voiceName (spaces allowed) — e.g.,
//   VoicePreviews/21m00Tcm4TlvDq8ikWAM.mp3
//   VoicePreviews/Rachel.mp3
//   VoicePreviews/Rachel_Female.mp3 (as alternative)
public class VoicePreviewPlayer : MonoBehaviour
{
    public static VoicePreviewPlayer Instance { get; private set; }

    private AudioSource audioSource;

    [System.Serializable]
    public class VoiceSample
    {
        public string voiceId;      // ElevenLabs voiceId (preferred)
        public string voiceName;    // Fallback by display name
        public AudioClip clip;      // Assign your local mp3 imported by Unity
    }

    [Header("Local Samples (drag your clips here)")]
    public VoiceSample[] samples;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    public void PlayPreview(string voiceId, string voiceName)
    {
        if (string.IsNullOrEmpty(voiceId) && string.IsNullOrEmpty(voiceName))
        {
            Debug.LogWarning("VoicePreviewPlayer: No voice id or name provided for preview.");
            return;
        }

        // 1) Inspector-mapped samples: works regardless of folder path (e.g., Assets/Assets/VoicePreview)
        AudioClip clip = FindMappedClip(voiceId, voiceName);

        // 2) Resources fallback: if you later move clips under Assets/Resources/VoicePreviews/
        if (clip == null)
        {
            clip = TryLoad($"VoicePreviews/{voiceId}")
                ?? TryLoad($"VoicePreviews/{voiceName}")
                ?? TryLoad($"VoicePreviews/{(voiceName ?? "").Replace(" ", "_")}");
        }

        if (clip == null)
        {
            Debug.LogWarning($"VoicePreviewPlayer: No local preview clip found for '{voiceName}' ({voiceId}). Assign in inspector or place under Assets/Resources/VoicePreviews/");
            return;
        }

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    private AudioClip TryLoad(string resourcesPath)
    {
        if (string.IsNullOrEmpty(resourcesPath)) return null;
        return Resources.Load<AudioClip>(resourcesPath);
    }

    private AudioClip FindMappedClip(string voiceId, string voiceName)
    {
        if (samples == null || samples.Length == 0) return null;
        // Prefer voiceId match
        foreach (var s in samples)
        {
            if (!string.IsNullOrEmpty(voiceId) && s != null && s.clip != null && s.voiceId == voiceId)
                return s.clip;
        }
        // Fallback by name (case-insensitive)
        foreach (var s in samples)
        {
            if (s != null && s.clip != null && !string.IsNullOrEmpty(s.voiceName) &&
                string.Equals(s.voiceName, voiceName, System.StringComparison.OrdinalIgnoreCase))
                return s.clip;
        }
        return null;
    }
}
