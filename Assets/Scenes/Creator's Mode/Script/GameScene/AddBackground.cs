using UnityEngine;
using UnityEngine.UI;

public class BackgroundSetter : MonoBehaviour
{
    public RawImage backgroundImage;

    void Start()
    {
        string userRole = PlayerPrefs.GetString("UserRole", "student");

        // ✅ FIX: Student mode uses StudentPrefs, Teacher mode uses StoryManager
        if (userRole.ToLower() == "student")
        {
            LoadStudentStoryBackground();
        }
        else
        {
            LoadTeacherStoryBackground();
        }
    }

    private void LoadStudentStoryBackground()
    {
        // Load from StudentPrefs for student mode
        string storyJson = StudentPrefs.GetString("CurrentStoryData", "");
        if (!string.IsNullOrEmpty(storyJson))
        {
            try
            {
                StoryData studentStory = JsonUtility.FromJson<StoryData>(storyJson);
                if (studentStory != null && !string.IsNullOrEmpty(studentStory.backgroundPath))
                {
                    Texture2D savedBackground = ImageStorage.LoadImage(studentStory.backgroundPath);
                    if (savedBackground != null)
                    {
                        ApplyTexture(savedBackground);
                        Debug.Log($"📖 Loaded STUDENT background for story '{studentStory.storyTitle}'");
                        return;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Error loading student story background: {ex.Message}");
            }
        }

        // Fallback for student mode
        if (ImageStorage.UploadedTexture != null)
        {
            ApplyTexture(ImageStorage.UploadedTexture);
            Debug.Log("🖼 Applied temporary uploaded background for student");
        }
        else
        {
            Debug.LogWarning("⚠️ No background found for student story");
        }
    }

    private void LoadTeacherStoryBackground()
    {
        StoryData CurrentStory = StoryManager.Instance?.currentStory;

        if (CurrentStory == null)
        {
            Debug.LogWarning("⚠ No current story found in StoryManager.");
            return;
        }

        // 1. If story already has a saved background path → load from disk
        if (!string.IsNullOrEmpty(CurrentStory.backgroundPath))
        {
            Texture2D savedBackground = ImageStorage.LoadImage(CurrentStory.backgroundPath);
            if (savedBackground != null)
            {
                ApplyTexture(savedBackground);
                Debug.Log($"📖 Loaded TEACHER background for story '{CurrentStory.storyTitle}' from: {CurrentStory.backgroundPath}");
                return;
            }
        }

        // 2. If no saved path but user uploaded during session → just apply (don't save)
        if (ImageStorage.UploadedTexture != null)
        {
            ApplyTexture(ImageStorage.UploadedTexture);
            Debug.Log($"🖼 Applied temporary uploaded background for story '{CurrentStory.storyTitle}'");
        }
    }

    private void ApplyTexture(Texture2D tex)
    {
        backgroundImage.texture = tex;

        AspectRatioFitter fitter = backgroundImage.GetComponent<AspectRatioFitter>();
        if (fitter != null)
        {
            fitter.aspectRatio = (float)tex.width / tex.height;
        }
    }
}
