using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class VoiceSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject voiceButtonPrefab;
    public Transform voiceButtonsParent;
    public TextMeshProUGUI selectedVoiceText;
    public Button confirmButton;
    public Button closeButton;

    private string currentSelectedVoiceId;
    private System.Action<string> onVoiceSelected;
    private RectTransform buttonsParentRT;

    private void OnEnable()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(ConfirmSelection);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Cancel);
        }
    }

    public void ShowVoiceSelection(string currentVoiceId, System.Action<string> callback)
    {
        currentSelectedVoiceId = currentVoiceId;
        onVoiceSelected = callback;

        buttonsParentRT = ResolveButtonsParent();

        foreach (Transform child in buttonsParentRT)
        {
            Destroy(child.gameObject);
        }

        // ✅ NEW: Add "No Voice" option at the top
        CreateVoiceButton(VoiceLibrary.GetNoVoiceProfile());

        // Create buttons for each voice
        var voices = VoiceLibrary.GetAvailableVoices();
        foreach (var voice in voices)
        {
            CreateVoiceButton(voice);
        }

        UpdateSelectedVoiceDisplay();
        gameObject.SetActive(true);

        StartCoroutine(RebuildLayoutNextFrame());
    }

    void CreateVoiceButton(VoiceProfile voice)
    {
        if (voice == null)
        {
            Debug.LogError("❌ VoiceProfile is NULL in CreateVoiceButton!");
            return;
        }

        if (buttonsParentRT == null)
            buttonsParentRT = ResolveButtonsParent();

        GameObject buttonObj = Instantiate(voiceButtonPrefab, buttonsParentRT, false);

        var layout = buttonObj.GetComponent<LayoutElement>();
        if (layout == null) layout = buttonObj.AddComponent<LayoutElement>();
        layout.minHeight = 80f;
        layout.preferredHeight = 100f;
        layout.flexibleHeight = 0f;

        // Set up button visuals
        TextMeshProUGUI[] texts = buttonObj.GetComponentsInChildren<TextMeshProUGUI>(true);
        if (texts.Length >= 3)
        {
            texts[0].text = voice.voiceName;
            texts[1].text = voice.voiceName;
            texts[2].text = voice.description;

            // ✅ Optional: Style "No Voice" option differently
            if (VoiceLibrary.IsNoVoice(voice.voiceId))
            {
                texts[0].fontStyle = FontStyles.Italic;
                texts[1].fontStyle = FontStyles.Italic;
                texts[2].fontStyle = FontStyles.Italic;
                texts[0].color = new Color(0.7f, 0.7f, 0.7f);
            }
        }

        Image highlightImage = buttonObj.GetComponent<Image>();
        if (highlightImage == null)
        {
            highlightImage = buttonObj.GetComponentInChildren<Image>(true);
        }

        if (highlightImage == null)
        {
            Debug.LogWarning($"⚠️ No Image found to highlight in prefab '{buttonObj.name}'.");
        }

        // ✅ FIXED: Properly compare empty strings for highlighting
        bool isSelected = (string.IsNullOrEmpty(voice.voiceId) && string.IsNullOrEmpty(currentSelectedVoiceId)) ||
                         voice.voiceId == currentSelectedVoiceId;

        if (isSelected && highlightImage != null)
        {
            highlightImage.color = new Color(0.3f, 0.6f, 1f, 0.5f);
        }
        else if (highlightImage != null)
        {
            highlightImage.color = Color.white;
        }

        var allButtons = buttonObj.GetComponentsInChildren<Button>(true);
        foreach (var b in allButtons)
        {
            b.onClick.AddListener(() =>
            {
                Debug.Log($"Voice row clicked → {voice.voiceName} ({(string.IsNullOrEmpty(voice.voiceId) ? "NO_VOICE" : voice.voiceId)})");

                // ✅ Only play preview if it's an actual voice (not "No Voice")
                if (!VoiceLibrary.IsNoVoice(voice.voiceId))
                {
                    var player = VoicePreviewPlayer.Instance;
                    if (player != null)
                    {
                        player.PlayPreview(voice.voiceId, voice.voiceName);
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ VoicePreviewPlayer.Instance is null.");
                    }
                }

                SelectVoice(voice.voiceId, highlightImage);
            });
        }
    }

    private RectTransform ResolveButtonsParent()
    {
        if (voiceButtonsParent != null)
        {
            var sr = voiceButtonsParent.GetComponentInParent<ScrollRect>();
            if (sr != null && sr.content != null)
                return sr.content;

            var layout = voiceButtonsParent.GetComponentInChildren<VerticalLayoutGroup>(true);
            if (layout != null)
                return layout.transform as RectTransform;

            var ownLayout = voiceButtonsParent.GetComponent<VerticalLayoutGroup>();
            if (ownLayout != null)
                return voiceButtonsParent as RectTransform;
        }

        return transform as RectTransform;
    }

    void SelectVoice(string voiceId, Image buttonImage)
    {
        currentSelectedVoiceId = voiceId;
        UpdateSelectedVoiceDisplay();

        var parent = buttonsParentRT != null ? buttonsParentRT : voiceButtonsParent;
        foreach (Transform child in parent)
        {
            Button btn = child.GetComponentInChildren<Button>(true);
            if (btn != null && btn.TryGetComponent(out Image img))
            {
                img.color = Color.white;
            }
        }

        if (buttonImage != null)
            buttonImage.color = new Color(0.3f, 0.6f, 1f, 0.5f);
    }

    void UpdateSelectedVoiceDisplay()
    {
        if (selectedVoiceText != null)
        {
            // ✅ FIXED: Handle empty voice properly
            if (string.IsNullOrEmpty(currentSelectedVoiceId))
            {
                selectedVoiceText.text = "Selected: No Voice";
            }
            else
            {
                var voice = VoiceLibrary.GetVoiceById(currentSelectedVoiceId);
                if (voice != null)
                {
                    selectedVoiceText.text = $"Selected: {voice.voiceName}";
                }
            }
        }
    }

    public void ConfirmSelection()
    {
        onVoiceSelected?.Invoke(currentSelectedVoiceId);
        gameObject.SetActive(false);
    }

    public void Cancel()
    {
        gameObject.SetActive(false);
    }

    private System.Collections.IEnumerator RebuildLayoutNextFrame()
    {
        yield return null;
        if (buttonsParentRT != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(buttonsParentRT);
        }
    }
}
