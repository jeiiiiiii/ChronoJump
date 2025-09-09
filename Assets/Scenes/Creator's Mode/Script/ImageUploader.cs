using UnityEngine;
using UnityEngine.UI;
using System.IO;
using SFB;

public class ImageUploader : MonoBehaviour
{
    public RawImage previewImage;

    // Keep track of the last runtime texture to safely clean it up
    private Texture2D lastUploadedTexture;

    public void UploadImage()
    {
        var extensions = new[] {
            new ExtensionFilter("Image files", "jpg", "jpeg", "png", "bmp", "gif")
        };

        var paths = StandaloneFileBrowser.OpenFilePanel("Select Image", "", extensions, false);

        if (paths.Length > 0 && File.Exists(paths[0]))
        {
            byte[] fileData = File.ReadAllBytes(paths[0]);

            // Create new texture for this upload
            Texture2D tex = new Texture2D(2, 2);
            if (tex.LoadImage(fileData))
            {
                // ✅ Clean up previously uploaded texture (only if it was runtime-created)
                if (lastUploadedTexture != null)
                {
                    Destroy(lastUploadedTexture);
                }

                // Apply new texture
                previewImage.texture = tex;

                // Store globally for next scene
                ImageStorage.UploadedTexture = tex;

                // Keep reference for safe cleanup next time
                lastUploadedTexture = tex;

                // Adjust aspect ratio if using AspectRatioFitter
                AspectRatioFitter fitter = previewImage.GetComponent<AspectRatioFitter>();
                if (fitter != null)
                    fitter.aspectRatio = (float)tex.width / tex.height;

                Debug.Log("Image uploaded successfully.");
            }
            else
            {
                Debug.LogError("Failed to load image. Please select a valid image file.");
            }
        }
    }
}
