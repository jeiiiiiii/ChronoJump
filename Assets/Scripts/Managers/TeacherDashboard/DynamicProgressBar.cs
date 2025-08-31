using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DynamicProgressBar : MonoBehaviour
{
    [Header("Progress Bar Components")]
    public Image fillImage;         
    public TextMeshProUGUI percentageText;  
    
    [Header("Progress Bar Settings")]
    [Range(0f, 100f)]
    public float currentProgress = 57f;
    
    [Header("Animation Settings")]
    public bool animateProgress = true;
    public float animationSpeed = 50f; 
    
    [Header("Colors")]
    public Color fillColor = new Color(0.2f, 0.8f, 0.4f, 1f); 
    
    private float targetProgress;
    private float displayedProgress;
    
    void Start()
    {
        // Initialize the progress bar
        if (fillImage != null)
            fillImage.color = fillColor;
            
        targetProgress = currentProgress;
        displayedProgress = animateProgress ? 0f : currentProgress;
        
        UpdateProgressBar();
    }
    
    void Update()
    {
        // Handle animation
        if (animateProgress && Mathf.Abs(displayedProgress - targetProgress) > 0.01f)
        {
            displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, animationSpeed * Time.deltaTime);
            UpdateProgressBar();
        }
        
        // Update target progress if the public field changes (for testing in inspector)
        if (!Mathf.Approximately(targetProgress, currentProgress))
        {
            SetProgress(currentProgress);
        }
    }
    
    /// <summary>
    /// Set the progress bar to a specific percentage (0-100)
    /// </summary>
    /// <param name="percentage">Progress percentage (0-100)</param>
    public void SetProgress(float percentage)
    {
        targetProgress = Mathf.Clamp(percentage, 0f, 100f);
        currentProgress = targetProgress;
        
        if (!animateProgress)
        {
            displayedProgress = targetProgress;
            UpdateProgressBar();
        }
    }
    
    /// <summary>
    /// Add to the current progress
    /// </summary>
    /// <param name="amount">Amount to add to progress</param>
    public void AddProgress(float amount)
    {
        SetProgress(targetProgress + amount);
    }
    
    /// <summary>
    /// Set progress as a normalized value (0-1)
    /// </summary>
    /// <param name="normalizedValue">Progress as 0-1 value</param>
    public void SetProgressNormalized(float normalizedValue)
    {
        SetProgress(normalizedValue * 100f);
    }
    
    private void UpdateProgressBar()
    {
        float normalizedProgress = displayedProgress / 100f;
        
        // Update fill amount
        if (fillImage != null)
            fillImage.fillAmount = normalizedProgress;
        
        // Update percentage text
        if (percentageText != null)
            percentageText.text = Mathf.RoundToInt(displayedProgress) + "%";
    }
    
    /// <summary>
    /// Get current progress percentage
    /// </summary>
    public float GetProgress()
    {
        return displayedProgress;
    }
    
    /// <summary>
    /// Check if progress bar is currently animating
    /// </summary>
    public bool IsAnimating()
    {
        return animateProgress && !Mathf.Approximately(displayedProgress, targetProgress);
    }
    
    /// <summary>
    /// Reset progress to 0
    /// </summary>
    public void ResetProgress()
    {
        SetProgress(0f);
    }
    
    /// <summary>
    /// Fill progress to 100%
    /// </summary>
    public void FillProgress()
    {
        SetProgress(100f);
    }
}