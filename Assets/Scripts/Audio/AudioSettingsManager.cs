using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioSettingsManager : MonoBehaviour
{
    public static AudioSettingsManager Instance;

    private const string MusicVolumeKey = "MusicVolume";
    private const string VoiceVolumeKey = "VoiceVolume";

    private float musicVolume = 0.5f;
    private float voiceVolume = 0.5f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // ✅ Apply volumes when new scenes load
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ✅ Reload volumes when a new scene loads (in case student changed)
        LoadVolumes();
        ApplyVolumesToScene();
    }

    // ✅ NEW: Public method to reload volumes (call this after student login)
    public void LoadVolumes()
    {
        musicVolume = StudentPrefs.GetFloat(MusicVolumeKey, 0.5f);
        voiceVolume = StudentPrefs.GetFloat(VoiceVolumeKey, 0.5f);
        Debug.Log($"[AudioSettingsManager] Loaded volumes - Music: {musicVolume}, Voice: {voiceVolume}");
    }

    // 🎵 --- MUSIC ---
    public float GetMusicVolume() => musicVolume;

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        StudentPrefs.SetFloat(MusicVolumeKey, volume);
        StudentPrefs.Save();
        ApplyVolumesToScene();
    }

    // 🗣️ --- VOICE NARRATION ---
    public float GetVoiceVolume() => voiceVolume;

    public void SetVoiceVolume(float volume)
    {
        voiceVolume = volume;
        StudentPrefs.SetFloat(VoiceVolumeKey, volume);
        StudentPrefs.Save();
        ApplyVolumesToScene();
    }

    // 📊 Apply both volumes to the active scene
    public void ApplyVolumesToScene()
    {
        AudioSource[] allAudio = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

        foreach (AudioSource source in allAudio)
        {
            if (source.CompareTag("Music"))
                source.volume = musicVolume;

            if (source.CompareTag("VoiceNarration"))
                source.volume = voiceVolume;
        }
    }
}
