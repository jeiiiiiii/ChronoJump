using UnityEngine;
using UnityEngine.UI;
using System.IO;
using SFB;

public class ImageUploader : MonoBehaviour
{
    public RawImage previewImage;

    public void UploadImage()
    {
        var extensions = new[] {
            new ExtensionFilter("JPEG files", "jpg", "jpeg")
        };

        var paths = StandaloneFileBrowser.OpenFilePanel("Select JPEG Image", "", extensions, false);

        if (paths.Length > 0 && File.Exists(paths[0]))
        {
            byte[] fileData = File.ReadAllBytes(paths[0]);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);

            previewImage.texture = tex;

            // Update AspectRatioFitter to match image
            AspectRatioFitter fitter = previewImage.GetComponent<AspectRatioFitter>();
            if (fitter != null)
                fitter.aspectRatio = (float)tex.width / tex.height;
        }
    }
}
