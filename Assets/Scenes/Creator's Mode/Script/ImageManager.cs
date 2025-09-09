using UnityEngine;

public static class ImageStorage
{
    public static Texture2D UploadedTexture; // Temporary storage for currently uploaded image
    
    // Store multiple story backgrounds (saved when "Next" is clicked)
    public static Texture2D[] StoryBackgrounds = new Texture2D[6]; // For 6 stories
    
    // Track which stories have backgrounds
    public static bool[] HasBackground = new bool[6];
    
    // Current story being edited (set when creating/editing a story)
    public static int CurrentStoryIndex = -1;
    
    // Method to save current uploaded image to specific story (called on "Next" click)
    public static void SaveCurrentImageToStory()
    {
        if (CurrentStoryIndex >= 0 && CurrentStoryIndex < 6 && UploadedTexture != null)
        {
            StoryBackgrounds[CurrentStoryIndex] = UploadedTexture;
            HasBackground[CurrentStoryIndex] = true;
            Debug.Log($"Background saved for Story {CurrentStoryIndex + 1}");
        }
    }
    
    // Method to get background for specific story
    public static Texture2D GetBackgroundForStory(int storyIndex)
    {
        if (storyIndex >= 0 && storyIndex < 6 && HasBackground[storyIndex])
        {
            return StoryBackgrounds[storyIndex];
        }
        return null;
    }
    
    // Check if current story has a saved background
    public static bool CurrentStoryHasBackground()
    {
        return CurrentStoryIndex >= 0 && CurrentStoryIndex < 6 && HasBackground[CurrentStoryIndex];
    }
}