using UnityEngine;
using UnityEngine.UI;

public class CharacterSetter : MonoBehaviour
{
    public RawImage characterImageOne;
    public RawImage characterImageTwo;

    void Start()
    {
        var story = StoryManager.Instance.currentStory;
        if (story == null)
        {
            Debug.LogWarning("❌ No current story found for CharacterSetter.");
            return;
        }

        // ✅ Character 1
        if (ImageStorage.uploadedTexture1 != null)
        {
            SetCharacterImage(characterImageOne, ImageStorage.uploadedTexture1);
        }
        else if (!string.IsNullOrEmpty(story.character1Path))
        {
            Texture2D tex1 = ImageStorage.LoadImage(story.character1Path);
            if (tex1 != null)
            {
                ImageStorage.uploadedTexture1 = tex1; // keep in memory
                SetCharacterImage(characterImageOne, tex1);
            }
        }

        // ✅ Character 2
        if (ImageStorage.uploadedTexture2 != null)
        {
            SetCharacterImage(characterImageTwo, ImageStorage.uploadedTexture2);
        }
        else if (!string.IsNullOrEmpty(story.character2Path))
        {
            Texture2D tex2 = ImageStorage.LoadImage(story.character2Path);
            if (tex2 != null)
            {
                ImageStorage.uploadedTexture2 = tex2; // keep in memory
                SetCharacterImage(characterImageTwo, tex2);
            }
        }
    }

    private void SetCharacterImage(RawImage image, Texture2D texture)
    {
        image.texture = texture;
        var fitter = image.GetComponent<AspectRatioFitter>();
        if (fitter != null)
            fitter.aspectRatio = (float)texture.width / texture.height;

        image.gameObject.SetActive(true);
    }
}
