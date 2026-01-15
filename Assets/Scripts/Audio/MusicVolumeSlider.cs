using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public enum VolumeType { Music, VoiceNarration }
    public VolumeType volumeType;

    [SerializeField] private Slider volumeSlider;

    private bool isInitialized = false;

    void Start()
    {
        float savedValue = 0.5f;

        // Load correct saved value depending on type
        if (volumeType == VolumeType.Music)
            savedValue = StudentPrefs.GetFloat("MusicVolume", 0.5f);
        else if (volumeType == VolumeType.VoiceNarration)
            savedValue = StudentPrefs.GetFloat("VoiceVolume", 0.5f);

        volumeSlider.value = savedValue;

        // Apply immediately
        if (AudioSettingsManager.Instance != null)
            ApplyVolume(savedValue);

        // Add listener
        volumeSlider.onValueChanged.AddListener(OnSliderValueChanged);
        isInitialized = true;
    }

    private void OnSliderValueChanged(float value)
    {
        if (!isInitialized) return;
        ApplyVolume(value);
    }

    private void ApplyVolume(float value)
    {
        if (AudioSettingsManager.Instance == null) return;

        if (volumeType == VolumeType.Music)
            AudioSettingsManager.Instance.SetMusicVolume(value);
        else if (volumeType == VolumeType.VoiceNarration)
            AudioSettingsManager.Instance.SetVoiceVolume(value);
    }

    void OnDestroy()
    {
        volumeSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }
}
