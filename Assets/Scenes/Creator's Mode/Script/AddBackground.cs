using UnityEngine;
using UnityEngine.UI;

public class BackgroundSetter : MonoBehaviour
{
    public RawImage backgroundImage;

    void Start()
    {
        if (ImageStorage.UploadedTexture != null)
        {
            backgroundImage.texture = ImageStorage.UploadedTexture;

            AspectRatioFitter fitter = backgroundImage.GetComponent<AspectRatioFitter>();
            if (fitter != null)
                fitter.aspectRatio = (float)ImageStorage.UploadedTexture.width /
                                    ImageStorage.UploadedTexture.height;
        }
    }
}
