using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ClipboardButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Button clipboardButton;
    public Image clipboardImage;
    public TextMeshProUGUI feedbackText; // Optional: for "Copied!" message
    
    [Header("Visual Effects")]
    public float clickScaleEffect = 0.9f;
    public float effectDuration = 0.2f;
    public Color clickTintColor = Color.white;
    public Color hoverTintColor = Color.gray; // new
    public float hoverDuration = 0.2f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
    
    [Header("Feedback Settings")]
    public bool showFeedbackText = true;
    public float feedbackDuration = 2f;
    
    [Header("Debug")]
    public string testClassCode = "TEST123"; // For testing in editor
    
    private Vector3 _originalScale;
    private Color _originalColor;
    private string _currentClassCode;
    private Color _originalTextColor;
    
    private Coroutine _currentClickCoroutine;
    private Coroutine _currentHoverCoroutine;

    private void Awake()
    {
        // Validate references
        if (clipboardButton == null)
        {
            clipboardButton = GetComponent<Button>();
            if (clipboardButton == null)
            {
                Debug.LogError("ClipboardButton: No Button component found!");
                return;
            }
        }

        if (clipboardImage == null)
        {
            clipboardImage = GetComponent<Image>();
            if (clipboardImage == null)
            {
                Debug.LogError("ClipboardButton: No Image component found!");
                return;
            }
        }

        // Set up button listener
        clipboardButton.onClick.AddListener(OnClipboardClick);

        // Store original values
        _originalScale = clipboardImage.transform.localScale;
        _originalColor = clipboardImage.color;

        // Store original text color and hide feedback text initially
        if (feedbackText != null)
        {
            _originalTextColor = feedbackText.color;
            feedbackText.gameObject.SetActive(false);
        }

        // Only set test class code in editor if testClassCode is not empty
        if (Application.isEditor && !string.IsNullOrEmpty(testClassCode) && string.IsNullOrEmpty(_currentClassCode))
        {
            _currentClassCode = testClassCode;
        }
    }
    
    public void SetClassCode(string classCode)
    {
        _currentClassCode = classCode;
        Debug.Log($"Class code set to: {_currentClassCode}");
    }
    
    private void OnClipboardClick()
    {
        Debug.Log("Clipboard button clicked!");
        
        if (string.IsNullOrEmpty(_currentClassCode))
        {
            Debug.LogWarning("No class code to copy! Call SetClassCode() first.");
            
            // Show error feedback
            if (showFeedbackText && feedbackText != null)
            {
                StartCoroutine(ShowErrorFeedback());
            }
            return;
        }
        
        // Copy to clipboard
        bool copySuccess = CopyToClipboard(_currentClassCode);
        
        if (copySuccess)
        {
            // Play visual effects
            if (_currentClickCoroutine != null)
                StopCoroutine(_currentClickCoroutine); 

            _currentClickCoroutine = StartCoroutine(PlayClickEffect());

            // Show success feedback
            if (showFeedbackText && feedbackText != null)
            {
                StartCoroutine(ShowSuccessFeedback());
            }
            
            Debug.Log($"Class code copied to clipboard: {_currentClassCode}");
        }
        else
        {
            Debug.LogError("Failed to copy to clipboard!");
            
            // Show error feedback
            if (showFeedbackText && feedbackText != null)
            {
                StartCoroutine(ShowErrorFeedback());
            }
        }
    }
    
    private bool CopyToClipboard(string text)
    {
        try
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            GUIUtility.systemCopyBuffer = text;
            return true;
#elif UNITY_ANDROID
            GUIUtility.systemCopyBuffer = text;
            return true;
#elif UNITY_IOS
            GUIUtility.systemCopyBuffer = text;
            return true;
#else
            Debug.LogWarning("Clipboard not supported on this platform");
            return false;
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error copying to clipboard: {e.Message}");
            return false;
        }
    }
    
    private IEnumerator PlayClickEffect()
    {
        if (clipboardImage == null) yield break;
        
        float elapsedTime = 0f;
        
        // Scale down and tint
        while (elapsedTime < effectDuration)
        {
            float progress = elapsedTime / effectDuration;
            float curveValue = scaleCurve.Evaluate(progress);
            
            float currentScale = Mathf.Lerp(1f, clickScaleEffect, curveValue);
            clipboardImage.transform.localScale = _originalScale * currentScale;
            
            clipboardImage.color = Color.Lerp(_originalColor, clickTintColor, curveValue);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Scale back up and restore color
        elapsedTime = 0f;
        while (elapsedTime < effectDuration)
        {
            float progress = elapsedTime / effectDuration;
            float curveValue = scaleCurve.Evaluate(1f - progress);
            
            float currentScale = Mathf.Lerp(1f, clickScaleEffect, curveValue);
            clipboardImage.transform.localScale = _originalScale * currentScale;
            
            clipboardImage.color = Color.Lerp(_originalColor, clickTintColor, curveValue);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        clipboardImage.transform.localScale = _originalScale;
        clipboardImage.color = _originalColor;
    }
    
    private IEnumerator ShowSuccessFeedback()
    {
        yield return StartCoroutine(ShowFeedbackMessage("Copied!", new Color(0.2f, 0.8f, 0.4f, 1f)));
    }
    
    private IEnumerator ShowErrorFeedback()
    {
        yield return StartCoroutine(ShowFeedbackMessage("No code set!", Color.red));
    }
    
    private IEnumerator ShowFeedbackMessage(string message, Color messageColor)
    {
        if (feedbackText == null) yield break;
        
        feedbackText.text = message;
        Color baseColor = messageColor;
        baseColor.a = 1f;
        feedbackText.gameObject.SetActive(true);

        float fadeTime = 0.3f;
        float elapsedTime = 0f;

        // Fade in
        while (elapsedTime < fadeTime)
        {
            float alpha = elapsedTime / fadeTime;
            feedbackText.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        feedbackText.color = baseColor;

        // Wait
        yield return new WaitForSeconds(feedbackDuration - (fadeTime * 2));

        // Fade out
        elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            float alpha = 1f - (elapsedTime / fadeTime);
            feedbackText.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        feedbackText.gameObject.SetActive(false);
        feedbackText.color = _originalTextColor;
    }
    
    // Hover logic
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_currentHoverCoroutine != null) StopCoroutine(_currentHoverCoroutine);
        _currentHoverCoroutine = StartCoroutine(HoverEffect(true));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_currentHoverCoroutine != null) StopCoroutine(_currentHoverCoroutine);
        _currentHoverCoroutine = StartCoroutine(HoverEffect(false));
    }

    private IEnumerator HoverEffect(bool entering)
    {
        if (clipboardImage == null) yield break;

        float elapsed = 0f;
        Color startColor = clipboardImage.color;
        Color targetColor = entering ? hoverTintColor : _originalColor;

        while (elapsed < hoverDuration)
        {
            clipboardImage.color = Color.Lerp(startColor, targetColor, elapsed / hoverDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        clipboardImage.color = targetColor;
    }

    // Public method for testing
    [ContextMenu("Test Copy")]
    public void TestCopy()
    {
        OnClipboardClick();
    }
    
    private void OnDestroy()
    {
        if (_currentHoverCoroutine != null)
            StopCoroutine(_currentHoverCoroutine);
        if (_currentClickCoroutine != null)
            StopCoroutine(_currentClickCoroutine);
            
        if (clipboardButton != null)
        {
            clipboardButton.onClick.RemoveListener(OnClipboardClick);
        }
    }
    
    private void OnValidate()
    {
        if (clipboardButton == null)
            clipboardButton = GetComponent<Button>();
        if (clipboardImage == null)
            clipboardImage = GetComponent<Image>();
    }
}
