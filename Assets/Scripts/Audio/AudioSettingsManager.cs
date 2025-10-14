using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioSettingsManager : MonoBehaviour
{
    public static AudioSettingsManager Instance;

    private const string VolumePrefKey = "MusicVolume";
    private float currentVolume = 0.5f; // Default

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            currentVolume = StudentPrefs.GetFloat(VolumePrefKey, 0.5f);

            // ✅ Listen for scene changes to reapply volume automatically
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // ✅ Clean up the event subscription if destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ✅ Called automatically when a new scene loads
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyVolumeToScene();
    }

    public float GetVolume() => currentVolume;

    public void SetVolume(float volume)
    {
        currentVolume = volume;
        StudentPrefs.SetFloat(VolumePrefKey, volume);
        StudentPrefs.Save();

        ApplyVolumeToScene();
    }

    public void ApplyVolumeToScene()
    {
        // ✅ Find all AudioSources tagged as "Music" in the active scene
        AudioSource[] allMusic = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in allMusic)
        {
            if (source.CompareTag("Music"))
            {
                source.volume = currentVolume;
            }
        }
    }
}
