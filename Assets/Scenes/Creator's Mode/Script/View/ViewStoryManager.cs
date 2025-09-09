using UnityEngine;
using UnityEngine.UI;

public class ViewStoriesBackgroundManager : MonoBehaviour
{
    [System.Serializable]
    public class StorySlot
    {
        public RawImage backgroundImage;
    }
    
    [SerializeField] private StorySlot[] storySlots = new StorySlot[6];
    [SerializeField] private GameObject storyActionPopup; // The Edit/View/Delete popup
    
    void Start()
    {
        UpdateAllStoryBackgrounds();
        
        // Make sure popup is initially hidden
        if (storyActionPopup != null)
        {
            storyActionPopup.SetActive(false);
        }
    }
    
    void UpdateAllStoryBackgrounds()
    {
        for (int i = 0; i < 6; i++)
        {
            UpdateStorySlot(i);
        }
    }

    void UpdateStorySlot(int index)
    {
        if (index < 0 || index >= storySlots.Length) return;

        var slot = storySlots[index];
        Texture2D background = ImageStorage.GetBackgroundForStory(index);
        bool hasBackground = ImageStorage.HasBackground[index];

        if (hasBackground && background != null)
        {
            // Story has content - show background
            slot.backgroundImage.texture = background;
            slot.backgroundImage.gameObject.SetActive(true);


            // Adjust aspect ratio
            AspectRatioFitter fitter = slot.backgroundImage.GetComponent<AspectRatioFitter>();
            if (fitter != null)
            {
                fitter.aspectRatio = (float)background.width / background.height;
            }
        }
        else
        {
            // Story is empty - show empty indicator
            slot.backgroundImage.gameObject.SetActive(false);


        }
    }
    
    public void RefreshBackgrounds()
    {
        UpdateAllStoryBackgrounds();
    }
    
    void OnEnable()
    {
        UpdateAllStoryBackgrounds();
    }
}