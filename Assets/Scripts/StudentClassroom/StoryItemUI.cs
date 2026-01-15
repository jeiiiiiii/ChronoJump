using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Component for individual story items in the student classroom
/// Handles the display and interaction of published stories
/// </summary>
public class StoryItemUI : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI dateText;
    public Button playButton;        
    public Button lockedButton;      
    public Image statusIcon;         

    [Header("Visual Settings")]
    public Color availableColor = Color.green;
    public Color defaultColor = Color.white;

    private PublishedStory storyData;
    private System.Action<PublishedStory> onPlayCallback;

    /// <summary>
    /// Set up the story item with data and callback
    /// </summary>
    public void SetupStory(PublishedStory story, System.Action<PublishedStory> playCallback)
    {
        storyData = story;
        onPlayCallback = playCallback;

        // Update UI elements
        if (titleText != null)
            titleText.text = story.storyTitle;

        if (dateText != null)
            dateText.text = $"Published: {story.publishDate}";

        // Set up button states - show unlocked, hide locked
        SetupButtonStates(true); // true = story is playable

        if (statusIcon != null)
        {
            statusIcon.color = availableColor;
        }

        Debug.Log($"Story item setup: {story.storyTitle}");
    }

    /// <summary>
    /// Configure which button is visible and active
    /// </summary>
    private void SetupButtonStates(bool isPlayable)
    {
        if (isPlayable)
        {
            // Show unlocked button
            if (playButton != null)
            {
                playButton.gameObject.SetActive(true);
                playButton.interactable = true;
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(() => onPlayCallback?.Invoke(storyData));

                // Update button text
                TextMeshProUGUI buttonText = playButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "START";
                }
            }

            // Hide locked button
            if (lockedButton != null)
            {
                lockedButton.gameObject.SetActive(false);
            }
        }
        else
        {
            // Show locked button
            if (lockedButton != null)
            {
                lockedButton.gameObject.SetActive(true);
                lockedButton.interactable = false;

                // Update locked button text
                TextMeshProUGUI buttonText = lockedButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "LOCKED";
                }
            }

            // Hide unlocked button
            if (playButton != null)
            {
                playButton.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Handle play button click (for testing/manual calls)
    /// </summary>
    public void OnPlayButtonClicked()
    {
        if (storyData != null && onPlayCallback != null)
        {
            Debug.Log($"Playing story: {storyData.storyTitle}");
            onPlayCallback.Invoke(storyData);
        }
    }

    /// <summary>
    /// Public method to change story state if needed
    /// </summary>
    public void SetStoryPlayable(bool isPlayable)
    {
        SetupButtonStates(isPlayable);
    }

    /// <summary>
    /// Clean up when destroyed
    /// </summary>
    private void OnDestroy()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
        }
    }
}