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

        // ✅ Character 1 - UPDATED to use ImageStorage.LoadImage with relative paths
        if (!string.IsNullOrEmpty(story.character1Path))
        {
            Texture2D tex1 = ImageStorage.LoadImage(story.character1Path);
            if (tex1 != null)
            {
                SetCharacterImage(characterImageOne, tex1);
                Debug.Log($"✅ Loaded character 1 from relative path: {story.character1Path}");
            }
            else
            {
                Debug.LogWarning($"❌ Failed to load character 1 from: {story.character1Path}");
                characterImageOne.gameObject.SetActive(false);
            }
        }
        else if (ImageStorage.uploadedTexture1 != null)
        {
            // Use temporary uploaded image only if no saved path
            SetCharacterImage(characterImageOne, ImageStorage.uploadedTexture1);
            Debug.Log("✅ Using temporary uploaded character 1");
        }
        else
        {
            characterImageOne.gameObject.SetActive(false); // hide if none
            Debug.Log("ℹ️ No character 1 image available");
        }

        // ✅ Character 2 - UPDATED to use ImageStorage.LoadImage with relative paths
        if (!string.IsNullOrEmpty(story.character2Path))
        {
            Texture2D tex2 = ImageStorage.LoadImage(story.character2Path);
            if (tex2 != null)
            {
                SetCharacterImage(characterImageTwo, tex2);
                Debug.Log($"✅ Loaded character 2 from relative path: {story.character2Path}");
            }
            else
            {
                Debug.LogWarning($"❌ Failed to load character 2 from: {story.character2Path}");
                characterImageTwo.gameObject.SetActive(false);
            }
        }
        else if (ImageStorage.uploadedTexture2 != null)
        {
            SetCharacterImage(characterImageTwo, ImageStorage.uploadedTexture2);
            Debug.Log("✅ Using temporary uploaded character 2");
        }
        else
        {
            characterImageTwo.gameObject.SetActive(false); // hide if none
            Debug.Log("ℹ️ No character 2 image available");
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
