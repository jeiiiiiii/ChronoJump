using UnityEngine;
using UnityEngine.UI;

public class CharacterSetter : MonoBehaviour
{
    public RawImage characterImageOne;
    public RawImage characterImageTwo;

    void Start()
    {
        // ✅ FIRST: Try to load from StudentPrefs (Student Mode)
        string storyJson = StudentPrefs.GetString("CurrentStoryData", "");
        StoryData story = null;

        if (!string.IsNullOrEmpty(storyJson))
        {
            try
            {
                story = JsonUtility.FromJson<StoryData>(storyJson);
                Debug.Log($"✅ CharacterSetter: Loaded story from StudentPrefs - {story?.storyTitle}");
                Debug.Log($"✅ Character1 Path from StudentPrefs: {story?.character1Path}");
                Debug.Log($"✅ Character2 Path from StudentPrefs: {story?.character2Path}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Error loading story from StudentPrefs: {ex.Message}");
            }
        }

        // ✅ SECOND: Fallback to StoryManager (Teacher Mode)
        if (story == null)
        {
            story = StoryManager.Instance?.currentStory;
            Debug.Log($"✅ CharacterSetter: Using StoryManager - {story?.storyTitle}");
            Debug.Log($"✅ Character1 Path from StoryManager: {story?.character1Path}");
            Debug.Log($"✅ Character2 Path from StoryManager: {story?.character2Path}");
        }

        if (story == null)
        {
            Debug.LogWarning("❌ No current story found for CharacterSetter.");
            return;
        }

        // ✅ Character 1
        if (!string.IsNullOrEmpty(story.character1Path))
        {
            Debug.Log($"🔍 Attempting to load Character 1 from: {story.character1Path}");
            Texture2D tex1 = ImageStorage.LoadImage(story.character1Path);
            if (tex1 != null)
            {
                SetCharacterImage(characterImageOne, tex1);
                Debug.Log($"✅ SUCCESS: Loaded character 1 from: {story.character1Path}");
            }
            else
            {
                Debug.LogWarning($"❌ FAILED: Could not load character 1 from: {story.character1Path}");

                // Check if it might be a Firebase URL that needs downloading
                if (story.character1Path.StartsWith("gs://") || story.character1Path.StartsWith("https://"))
                {
                    Debug.Log($"🌐 Character 1 appears to be a Firebase URL, needs download: {story.character1Path}");
                }

                characterImageOne.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("ℹ️ No character1Path specified in story data");
            characterImageOne.gameObject.SetActive(false);
        }

        // ✅ Character 2
        if (!string.IsNullOrEmpty(story.character2Path))
        {
            Debug.Log($"🔍 Attempting to load Character 2 from: {story.character2Path}");
            Texture2D tex2 = ImageStorage.LoadImage(story.character2Path);
            if (tex2 != null)
            {
                SetCharacterImage(characterImageTwo, tex2);
                Debug.Log($"✅ SUCCESS: Loaded character 2 from: {story.character2Path}");
            }
            else
            {
                Debug.LogWarning($"❌ FAILED: Could not load character 2 from: {story.character2Path}");

                // Check if it might be a Firebase URL that needs downloading
                if (story.character2Path.StartsWith("gs://") || story.character2Path.StartsWith("https://"))
                {
                    Debug.Log($"🌐 Character 2 appears to be a Firebase URL, needs download: {story.character2Path}");
                }

                characterImageTwo.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("ℹ️ No character2Path specified in story data");
            characterImageTwo.gameObject.SetActive(false);
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