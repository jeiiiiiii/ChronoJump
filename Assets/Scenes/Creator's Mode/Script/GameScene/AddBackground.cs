using UnityEngine;
using UnityEngine.UI;

public class BackgroundSetter : MonoBehaviour
{
    public RawImage backgroundImage;
    
    void Start()
    {
        // First, check if current story already has a saved background
        if (ImageStorage.CurrentStoryIndex >= 0)
        {
            Texture2D savedBackground = ImageStorage.GetBackgroundForStory(ImageStorage.CurrentStoryIndex);
            if (savedBackground != null)
            {
                // Load the previously saved background for this story
                backgroundImage.texture = savedBackground;
                ImageStorage.UploadedTexture = savedBackground; // Set as current for editing
                
                AspectRatioFitter fitter = backgroundImage.GetComponent<AspectRatioFitter>();
                if (fitter != null)
                {
                    fitter.aspectRatio = (float)savedBackground.width / savedBackground.height;
                }
                return;
            }
        }
        
        // If no saved background, check for newly uploaded texture
        if (ImageStorage.UploadedTexture != null)
        {
            backgroundImage.texture = ImageStorage.UploadedTexture;
            
            AspectRatioFitter fitter = backgroundImage.GetComponent<AspectRatioFitter>();
            if (fitter != null)
            {
                fitter.aspectRatio = (float)ImageStorage.UploadedTexture.width / ImageStorage.UploadedTexture.height;
            }
        }
    }
}