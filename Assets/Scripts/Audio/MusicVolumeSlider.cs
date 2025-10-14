using UnityEngine;
using UnityEngine.UI;

public class MusicVolumeSlider : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    private bool isInitialized = false;

    void Start()
    {
        // ✅ Load saved volume
        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        volumeSlider.value = savedVolume;

        // ✅ Apply initial volume safely
        if (AudioSettingsManager.Instance != null)
            AudioSettingsManager.Instance.SetVolume(savedVolume);
        else
            Debug.LogWarning("AudioSettingsManager not found in scene!");

        // ✅ Add listener AFTER setting the initial value to avoid triggering
        volumeSlider.onValueChanged.AddListener(OnSliderValueChanged);

        // ✅ Delay initialization flag to avoid first-frame callback loop
        isInitialized = true;
    }

    private void OnSliderValueChanged(float value)
    {
        if (!isInitialized) return;

        if (AudioSettingsManager.Instance != null)
            AudioSettingsManager.Instance.SetVolume(value);
    }

    void OnDestroy()
    {
        // ✅ Clean up listener when the object is destroyed
        volumeSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }
}
