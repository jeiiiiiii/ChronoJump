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
    private RectTransform buttonsParentRT; // resolved parent (ScrollRect.content or a layout RectTransform)
    
    private void OnEnable()
    {
        // Wire buttons
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
        
        // Resolve to the proper Content under the Scroll View
        buttonsParentRT = ResolveButtonsParent();

        // Clear existing buttons under the resolved parent
        foreach (Transform child in buttonsParentRT)
        {
            Destroy(child.gameObject);
        }
        
        // Create buttons for each voice
        var voices = VoiceLibrary.GetAvailableVoices();
        foreach (var voice in voices)
        {
            CreateVoiceButton(voice);
        }
        
        UpdateSelectedVoiceDisplay();
        gameObject.SetActive(true);

        // Rebuild layout next frame so items position correctly
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

    // Ensure each item cooperates with Vertical Layout Group
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
            texts[1].text = $"{voice.gender} • {voice.accent}";
            texts[2].text = voice.description;
        }

        // Determine main highlight image (prefer root image)
        Image highlightImage = buttonObj.GetComponent<Image>();
        if (highlightImage == null)
        {
            // fallback to first child Image
            highlightImage = buttonObj.GetComponentInChildren<Image>(true);
        }

        if (highlightImage == null)
        {
            Debug.LogWarning($"⚠️ No Image found to highlight in prefab '{buttonObj.name}'. Selection will still work.");
        }

        // Highlight if currently selected
        if (voice.voiceId == currentSelectedVoiceId && highlightImage != null)
        {
            highlightImage.color = new Color(0.3f, 0.6f, 1f, 0.5f);
        }
        else if (highlightImage != null)
        {
            highlightImage.color = Color.white;
        }

        // Add selection listener to ALL buttons in this row to be robust
        var allButtons = buttonObj.GetComponentsInChildren<Button>(true);
        foreach (var b in allButtons)
        {
            b.onClick.AddListener(() => {
                Debug.Log($"Voice row clicked → {voice.voiceName} ({voice.voiceId})");
                // Play the local preview MP3 for this voice (if mapped in VoicePreviewPlayer)
                var player = VoicePreviewPlayer.Instance;
                if (player != null)
                {
                    player.PlayPreview(voice.voiceId, voice.voiceName);
                }
                else
                {
                    Debug.LogWarning("⚠️ VoicePreviewPlayer.Instance is null. Add a VoicePreviewPlayer to the scene and assign clips.");
                }
                SelectVoice(voice.voiceId, highlightImage);
            });
        }
    }
    
    // Find the correct parent to place buttons under (ScrollRect.content or a layout container)
    private RectTransform ResolveButtonsParent()
    {
        // If user dragged the Scroll View object, prefer its ScrollRect.content
        if (voiceButtonsParent != null)
        {
            var sr = voiceButtonsParent.GetComponentInParent<ScrollRect>();
            if (sr != null && sr.content != null)
                return sr.content;

            // Or find a VerticalLayoutGroup in children (commonly on Content)
            var layout = voiceButtonsParent.GetComponentInChildren<VerticalLayoutGroup>(true);
            if (layout != null)
                return layout.transform as RectTransform;

            // If the assigned transform itself has a layout, use it
            var ownLayout = voiceButtonsParent.GetComponent<VerticalLayoutGroup>();
            if (ownLayout != null)
                return voiceButtonsParent as RectTransform;
        }

        // Fallback to our own transform
        return transform as RectTransform;
    }
    

    
    void SelectVoice(string voiceId, Image buttonImage)
    {
        currentSelectedVoiceId = voiceId;
        UpdateSelectedVoiceDisplay();
        
        // Reset all button colors
        var parent = buttonsParentRT != null ? buttonsParentRT : voiceButtonsParent;
        foreach (Transform child in parent)
        {
            Button btn = child.GetComponentInChildren<Button>(true);
            if (btn != null && btn.TryGetComponent(out Image img))
            {
                img.color = Color.white;
            }
        }
        
        // Highlight selected
        if (buttonImage != null)
            buttonImage.color = new Color(0.3f, 0.6f, 1f, 0.5f);
    }
    
    void UpdateSelectedVoiceDisplay()
    {
        var voice = VoiceLibrary.GetVoiceById(currentSelectedVoiceId);
        if (selectedVoiceText != null && voice != null)
        {
            selectedVoiceText.text = $"Selected: {voice.voiceName}";
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
        yield return null; // wait a frame so children exist
        if (buttonsParentRT != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(buttonsParentRT);
        }
    }
}
