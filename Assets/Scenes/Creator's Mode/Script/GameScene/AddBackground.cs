using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundSetter : MonoBehaviour
{
    public RawImage backgroundImage;

    void Start()
    {
        string userRole = PlayerPrefs.GetString("UserRole", "student");

        // ✅ FIX: Use coroutines for both student and teacher modes
        if (userRole.ToLower() == "student")
        {
            StartCoroutine(LoadStudentStoryBackground());
        }
        else
        {
            StartCoroutine(LoadTeacherStoryBackground());
        }
    }

    private IEnumerator LoadStudentStoryBackground()
    {
        // Load from StudentPrefs for student mode
        string storyJson = StudentPrefs.GetString("CurrentStoryData", "");
        if (string.IsNullOrEmpty(storyJson))
        {
            ApplyFallbackBackground();
            yield break;
        }

        StoryData studentStory = null;

        // ✅ FIX: Move try-catch outside of yield statements
        try
        {
            studentStory = JsonUtility.FromJson<StoryData>(storyJson);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Error loading student story background: {ex.Message}");
            ApplyFallbackBackground();
            yield break;
        }

        if (studentStory == null || string.IsNullOrEmpty(studentStory.backgroundPath))
        {
            ApplyFallbackBackground();
            yield break;
        }

        // ✅ Handle S3 URLs with async loading
        if (ImageStorage.IsS3Url(studentStory.backgroundPath))
        {
            Debug.Log($"🌐 Student background is S3 URL, downloading: {studentStory.backgroundPath}");

            var downloadTask = ImageStorage.LoadImageAsync(studentStory.backgroundPath);
            yield return new WaitUntil(() => downloadTask.IsCompleted);

            if (downloadTask.Result != null)
            {
                ApplyTexture(downloadTask.Result);
                Debug.Log($"✅ Loaded STUDENT background from S3: {studentStory.storyTitle}");
            }
            else
            {
                Debug.LogError($"❌ Failed to download STUDENT background from S3");
                ApplyFallbackBackground();
            }
        }
        else
        {
            // Handle local files
            Texture2D savedBackground = ImageStorage.LoadImage(studentStory.backgroundPath);
            if (savedBackground != null)
            {
                ApplyTexture(savedBackground);
                Debug.Log($"📖 Loaded STUDENT background for story '{studentStory.storyTitle}'");
            }
            else
            {
                Debug.LogWarning($"❌ STUDENT background not found locally: {studentStory.backgroundPath}");
                ApplyFallbackBackground();
            }
        }
    }

    private IEnumerator LoadTeacherStoryBackground()
    {
        StoryData currentStory = StoryManager.Instance?.currentStory;

        if (currentStory == null)
        {
            Debug.LogWarning("⚠ No current story found in StoryManager.");
            ApplyFallbackBackground();
            yield break;
        }

        // 1. If story already has a saved background path → load from disk
        if (!string.IsNullOrEmpty(currentStory.backgroundPath))
        {
            // ✅ Handle S3 URLs for teachers too
            if (ImageStorage.IsS3Url(currentStory.backgroundPath))
            {
                Debug.Log($"🌐 Teacher background is S3 URL: {currentStory.backgroundPath}");

                var downloadTask = ImageStorage.LoadImageAsync(currentStory.backgroundPath);
                yield return new WaitUntil(() => downloadTask.IsCompleted);

                if (downloadTask.Result != null)
                {
                    ApplyTexture(downloadTask.Result);
                    Debug.Log($"✅ Loaded TEACHER background from S3: {currentStory.storyTitle}");
                    yield break;
                }
            }
            else
            {
                Texture2D savedBackground = ImageStorage.LoadImage(currentStory.backgroundPath);
                if (savedBackground != null)
                {
                    ApplyTexture(savedBackground);
                    Debug.Log($"📖 Loaded TEACHER background for story '{currentStory.storyTitle}' from: {currentStory.backgroundPath}");
                    yield break;
                }
            }
        }

        // 2. If no saved path but user uploaded during session → just apply (don't save)
        if (ImageStorage.UploadedTexture != null)
        {
            ApplyTexture(ImageStorage.UploadedTexture);
            Debug.Log($"🖼 Applied temporary uploaded background for story '{currentStory.storyTitle}'");
        }
        else
        {
            ApplyFallbackBackground();
        }
    }

    private void ApplyFallbackBackground()
    {
        // Fallback for both student and teacher modes
        if (ImageStorage.UploadedTexture != null)
        {
            ApplyTexture(ImageStorage.UploadedTexture);
            Debug.Log("🖼 Applied temporary uploaded background");
        }
        else
        {
            Debug.LogWarning("⚠️ No background found, using default");
            // You could set a default background texture here if needed
        }
    }

    private void ApplyTexture(Texture2D tex)
    {
        if (tex == null)
        {
            Debug.LogWarning("⚠️ Attempted to apply null texture");
            return;
        }

        backgroundImage.texture = tex;

        AspectRatioFitter fitter = backgroundImage.GetComponent<AspectRatioFitter>();
        if (fitter != null)
        {
            fitter.aspectRatio = (float)tex.width / tex.height;
        }

        backgroundImage.gameObject.SetActive(true);
    }
}
