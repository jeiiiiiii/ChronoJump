using UnityEngine;
using UnityEngine.UI;

public class CharacterSetter : MonoBehaviour
{
    public RawImage characterImageOne;
    public RawImage characterImageTwo;

    void Start()
    {
        string userRole = PlayerPrefs.GetString("UserRole", "student");

        // ✅ FIX: Student mode uses StudentPrefs, Teacher mode uses StoryManager
        if (userRole.ToLower() == "student")
        {
            LoadStudentStoryCharacters();
        }
        else
        {
            LoadTeacherStoryCharacters();
        }
    }

    private void LoadStudentStoryCharacters()
    {
        // Load from StudentPrefs for student mode
        string storyJson = StudentPrefs.GetString("CurrentStoryData", "");
        StoryData story = null;

        if (!string.IsNullOrEmpty(storyJson))
        {
            try
            {
                story = JsonUtility.FromJson<StoryData>(storyJson);
                Debug.Log($"✅ CharacterSetter: Loaded STUDENT story from StudentPrefs - {story?.storyTitle}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Error loading student story from StudentPrefs: {ex.Message}");
            }
        }

        LoadCharacters(story, "STUDENT");
    }

    private void LoadTeacherStoryCharacters()
    {
        // Load from StoryManager for teacher mode
        StoryData story = StoryManager.Instance?.currentStory;
        Debug.Log($"✅ CharacterSetter: Using TEACHER StoryManager - {story?.storyTitle}");

        LoadCharacters(story, "TEACHER");
    }

    private void LoadCharacters(StoryData story, string mode)
    {
        if (story == null)
        {
            Debug.LogWarning($"❌ No current story found for {mode} CharacterSetter.");
            return;
        }

        // ✅ Character 1
        if (!string.IsNullOrEmpty(story.character1Path))
        {
            Debug.Log($"🔍 Attempting to load {mode} Character 1 from: {story.character1Path}");
            Texture2D tex1 = ImageStorage.LoadImage(story.character1Path);
            if (tex1 != null)
            {
                SetCharacterImage(characterImageOne, tex1);
                Debug.Log($"✅ SUCCESS: Loaded {mode} character 1 from: {story.character1Path}");
            }
            else
            {
                Debug.LogWarning($"❌ FAILED: Could not load {mode} character 1 from: {story.character1Path}");
                characterImageOne.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning($"ℹ️ No character1Path specified in {mode} story data");
            characterImageOne.gameObject.SetActive(false);
        }

        // ✅ Character 2
        if (!string.IsNullOrEmpty(story.character2Path))
        {
            Debug.Log($"🔍 Attempting to load {mode} Character 2 from: {story.character2Path}");
            Texture2D tex2 = ImageStorage.LoadImage(story.character2Path);
            if (tex2 != null)
            {
                SetCharacterImage(characterImageTwo, tex2);
                Debug.Log($"✅ SUCCESS: Loaded {mode} character 2 from: {story.character2Path}");
            }
            else
            {
                Debug.LogWarning($"❌ FAILED: Could not load {mode} character 2 from: {story.character2Path}");
                characterImageTwo.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning($"ℹ️ No character2Path specified in {mode} story data");
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
