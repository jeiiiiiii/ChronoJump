using UnityEngine;
using UnityEngine.UI;

public class CharacterSetter : MonoBehaviour
{
    public RawImage characterImageOne;
    public RawImage characterImageTwo;

    void Start()
    {
        // Show character 1 if uploaded
        if (ImageStorage.uploadedTexture1 != null)
        {
            characterImageOne.texture = ImageStorage.uploadedTexture1;
            AspectRatioFitter fitter = characterImageOne.GetComponent<AspectRatioFitter>();
            if (fitter != null)
                fitter.aspectRatio = (float)ImageStorage.uploadedTexture1.width / ImageStorage.uploadedTexture1.height;
        }

        // Show character 2 if uploaded
        if (ImageStorage.uploadedTexture2 != null)
        {
            characterImageTwo.texture = ImageStorage.uploadedTexture2;
            AspectRatioFitter fitter = characterImageTwo.GetComponent<AspectRatioFitter>();
            if (fitter != null)
                fitter.aspectRatio = (float)ImageStorage.uploadedTexture2.width / ImageStorage.uploadedTexture2.height;
        }
    }
}
