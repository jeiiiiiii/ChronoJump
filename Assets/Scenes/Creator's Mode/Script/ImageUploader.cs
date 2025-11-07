using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Threading.Tasks;
using SFB;
using TMPro;
using System;

public class ImageUploader : MonoBehaviour
{
    [Header("Character Previews")]
    public RawImage previewImage1; // left character
    public RawImage previewImage2; // right character

    [Header("Background Preview")]
    public RawImage previewBackground; // background image slot

    [Header("Upload Status")]
    public TMP_Text uploadStatusText; // Optional: show upload progress

    private bool isUploading = false;
    private Texture2D lastUploadedTexture1;
    private Texture2D lastUploadedTexture2;
    private Texture2D lastUploadedBackground;

    private void Start()
    {
        LoadExistingImages();
    }

    /// <summary>
    /// Load existing images - prefers local cache, downloads from S3 only if needed
    /// </summary>
    private async void LoadExistingImages()
    {
        var story = StoryManager.Instance.currentStory;
        if (story == null) return;

        // ✅ Load background - check local cache first
        if (!string.IsNullOrEmpty(story.backgroundPath))
        {
            Texture2D tex = await LoadImageWithCache(story.backgroundPath, story.storyIndex, ImageStorage.IMAGE_TYPE_BACKGROUND);
            if (tex != null)
            {
                previewBackground.texture = tex;
                FixAspectRatio(previewBackground, tex);
            }
        }

        // ✅ Load character 1 - check local cache first
        if (!string.IsNullOrEmpty(story.character1Path))
        {
            Texture2D tex1 = await LoadImageWithCache(story.character1Path, story.storyIndex, ImageStorage.IMAGE_TYPE_CHARACTER1);
            if (tex1 != null)
            {
                previewImage1.texture = tex1;
                FixAspectRatio(previewImage1, tex1);
            }
        }

        // ✅ Load character 2 - check local cache first
        if (!string.IsNullOrEmpty(story.character2Path))
        {
            Texture2D tex2 = await LoadImageWithCache(story.character2Path, story.storyIndex, ImageStorage.IMAGE_TYPE_CHARACTER2);
            if (tex2 != null)
            {
                previewImage2.texture = tex2;
                FixAspectRatio(previewImage2, tex2);
            }
        }
    }

    /// <summary>
    /// Check if a path is an S3 URL
    /// </summary>
    private bool IsS3Url(string path)
    {
        return !string.IsNullOrEmpty(path) &&
               (path.StartsWith("https://") || path.StartsWith("http://")) &&
               path.Contains("s3") &&
               path.Contains("amazonaws.com");
    }

    /// <summary>
    /// Load image with smart caching - prefers local files, downloads from S3 only if missing
    /// </summary>
    /// <summary>
    /// Load image with smart caching - prefers local files, downloads from S3 only if missing
    /// </summary>
    private async Task<Texture2D> LoadImageWithCache(string path, int storyIndex, string imageType)
    {
        if (string.IsNullOrEmpty(path)) return null;

        // If it's a local path, try to load directly
        if (!IsS3Url(path))
        {
            Texture2D localTex = ImageStorage.LoadImage(path);
            if (localTex != null)
            {
                Debug.Log($"✅ Loaded from local path: {path}");
                return localTex;
            }
            return null;
        }

        // It's an S3 URL - use async loading which handles caching automatically
        Debug.Log($"⬇️ Loading from S3 (with auto-cache): {path}");
        UpdateStatus("Loading image...");

        Texture2D downloadedTex = await ImageStorage.LoadImageAsync(path);
        if (downloadedTex != null)
        {
            Debug.Log($"✅ Loaded from S3: {path}");
            UpdateStatus("");
            return downloadedTex;
        }

        Debug.LogError($"❌ Failed to load from S3: {path}");
        UpdateStatus("Load failed");
        return null;
    }


    // Upload for first character slot
    public void UploadImage1()
    {
        if (!isUploading)
            _ = UploadAndSetImageAsync(1, previewImage1);
    }

    // Upload for second character slot
    public void UploadImage2()
    {
        if (!isUploading)
            _ = UploadAndSetImageAsync(2, previewImage2);
    }

    // Upload for background slot
    public void UploadBackground()
    {
        if (!isUploading)
            _ = UploadAndSetImageAsync(3, previewBackground);
    }

    /// <summary>
    /// Upload image to S3 AND save locally, then update Firebase with the URL
    /// </summary>
    private async Task UploadAndSetImageAsync(int slot, RawImage targetPreview)
    {
        // ✅ Check if StoryManager is ready
        if (!ImageStorage.IsReady())
        {
            Debug.LogError("❌ Cannot upload image - StoryManager is not initialized yet");
            UpdateStatus("Error: StoryManager not ready");
            return;
        }

        // ✅ Check if S3 service is ready
        if (!S3StorageService.Instance.IsReady)
        {
            Debug.LogError("❌ S3 service not initialized");
            UpdateStatus("Error: S3 service not ready");
            return;
        }

        var extensions = new[] {
            new ExtensionFilter("Image files", "jpg", "jpeg", "png", "bmp", "gif")
        };

        var paths = StandaloneFileBrowser.OpenFilePanel("Select Image", "", extensions, false);

        if (paths.Length > 0 && File.Exists(paths[0]))
        {
            isUploading = true;
            UpdateStatus("Loading image...");

            try
            {
                // ========== FILE VALIDATION ==========
                string selectedPath = paths[0];

                // 1. Check if file exists
                if (!File.Exists(selectedPath))
                {
                    Debug.LogError($"❌ File not found: {selectedPath}");
                    UpdateStatus("Error: File not found");
                    return;
                }

                // 2. Validate file format using ValidationManager
                var formatValidation = ValidateFileFormat(selectedPath);
                if (!formatValidation.isValid)
                {
                    Debug.LogError($"❌ Invalid file format: {formatValidation.message}");
                    ValidationManager.Instance.ShowWarning(
                        "Invalid File Format",
                        formatValidation.message,
                        null,
                        null
                    );
                    return;
                }

                // 3. Validate file size using ValidationManager
                var sizeValidation = ValidateFileSize(selectedPath);
                if (!sizeValidation.isValid)
                {
                    Debug.LogError($"❌ File too large: {sizeValidation.message}");
                    ValidationManager.Instance.ShowWarning(
                        "File Too Large",
                        sizeValidation.message,
                        null,
                        null
                    );
                    return;
                }

                // Load image from file
                byte[] fileData = File.ReadAllBytes(selectedPath);
                Texture2D tex = new Texture2D(2, 2);

                if (!tex.LoadImage(fileData))
                {
                    Debug.LogError("❌ Failed to load image. Please select a valid file.");
                    ValidationManager.Instance.ShowWarning(
                        "Invalid Image File",
                        "The selected file appears to be corrupted or is not a valid image.\n\nPlease select a different file.",
                        null,
                        null
                    );
                    return;
                }

                // Clean up old texture if exists
                if (slot == 1 && lastUploadedTexture1 != null)
                    Destroy(lastUploadedTexture1);
                else if (slot == 2 && lastUploadedTexture2 != null)
                    Destroy(lastUploadedTexture2);
                else if (slot == 3 && lastUploadedBackground != null)
                    Destroy(lastUploadedBackground);

                // Show preview immediately
                targetPreview.texture = tex;

                // Store texture reference
                if (slot == 1)
                    lastUploadedTexture1 = tex;
                else if (slot == 2)
                    lastUploadedTexture2 = tex;
                else if (slot == 3)
                    lastUploadedBackground = tex;

                FixAspectRatio(targetPreview, tex);

                // Get story info
                var story = StoryManager.Instance.currentStory;
                if (story == null)
                {
                    Debug.LogError("❌ No current story");
                    UpdateStatus("Error: No current story");
                    return;
                }

                string teacherId = StoryManager.Instance.GetCurrentTeacherId();
                int storyIndex = story.storyIndex;

                // ========== SAVE LOCALLY FIRST ==========
                UpdateStatus("Saving locally...");
                string localPath = SaveTextureLocally(tex, storyIndex,
                    slot == 1 ? ImageStorage.IMAGE_TYPE_CHARACTER1 :
                    slot == 2 ? ImageStorage.IMAGE_TYPE_CHARACTER2 :
                    ImageStorage.IMAGE_TYPE_BACKGROUND);

                // ========== UPLOAD TO S3 ==========
                UpdateStatus("Uploading to cloud...");
                string s3Url = null;

                if (slot == 1)
                {
                    s3Url = await S3StorageService.Instance.UploadCharacter1Image(tex, teacherId, storyIndex);
                    if (!string.IsNullOrEmpty(s3Url))
                    {
                        story.character1Path = s3Url; // Store S3 URL as primary
                        ImageStorage.uploadedTexture1 = tex;
                    }
                }
                else if (slot == 2)
                {
                    s3Url = await S3StorageService.Instance.UploadCharacter2Image(tex, teacherId, storyIndex);
                    if (!string.IsNullOrEmpty(s3Url))
                    {
                        story.character2Path = s3Url; // Store S3 URL as primary
                        ImageStorage.uploadedTexture2 = tex;
                    }
                }
                else if (slot == 3)
                {
                    s3Url = await S3StorageService.Instance.UploadBackgroundImage(tex, teacherId, storyIndex);
                    if (!string.IsNullOrEmpty(s3Url))
                    {
                        story.backgroundPath = s3Url; // Store S3 URL as primary
                        ImageStorage.UploadedTexture = tex;
                    }
                }

                if (!string.IsNullOrEmpty(s3Url))
                {
                    // Save to local storage as backup
                    StoryManager.Instance.SaveStories();

                    UpdateStatus("✅ Upload successful!");
                    Debug.Log($"✅ Image uploaded to S3: {s3Url}");
                    Debug.Log($"✅ Image saved locally: {localPath}");

                    // Clear status after 3 seconds
                    await Task.Delay(3000);
                    UpdateStatus("");
                }
                else
                {
                    // If S3 upload fails, fall back to local path
                    if (slot == 1) story.character1Path = localPath;
                    else if (slot == 2) story.character2Path = localPath;
                    else if (slot == 3) story.backgroundPath = localPath;

                    StoryManager.Instance.SaveStories();
                    UpdateStatus("✅ Saved locally (cloud upload failed)");
                    Debug.LogWarning($"⚠️ S3 upload failed, using local path: {localPath}");

                    // Clear status after 3 seconds
                    await Task.Delay(3000);
                    UpdateStatus("");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Upload error: {ex.Message}");
                UpdateStatus($"Error: {ex.Message}");
            }
            finally
            {
                isUploading = false;
            }
        }
    }

    /// <summary>
    /// Validate file format
    /// </summary>
    private ValidationResult ValidateFileFormat(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLower();
        if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".bmp" && extension != ".gif")
        {
            return new ValidationResult
            {
                isValid = false,
                message = $"Unsupported file format: {extension}\n\nPlease select a valid image file (JPG, PNG, BMP, GIF)."
            };
        }

        return new ValidationResult { isValid = true };
    }

    /// <summary>
    /// Validate file size
    /// </summary>
    private ValidationResult ValidateFileSize(string filePath)
    {
        try
        {
            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > 10 * 1024 * 1024) // 10MB
            {
                double sizeInMB = fileInfo.Length / (1024.0 * 1024.0);
                return new ValidationResult
                {
                    isValid = false,
                    message = $"File is too large: {sizeInMB:F1}MB\n\nMaximum allowed size is 10MB. Please select a smaller image."
                };
            }

            return new ValidationResult { isValid = true };
        }
        catch
        {
            return new ValidationResult
            {
                isValid = false,
                message = "Unable to read file size. Please select a different file."
            };
        }
    }

    /// <summary>
    /// Save texture locally and return the relative path
    /// </summary>
    private string SaveTextureLocally(Texture2D texture, int storyIndex, string imageType)
    {
        string teachId = StoryManager.Instance.GetCurrentTeacherId();
        if (string.IsNullOrEmpty(teachId)) teachId = "default";
        string safeTeachId = teachId.Replace("/", "_").Replace("\\", "_");

        // Create relative path structure: teacherId/story_{index}/{imageType}.png
        string relativeDir = Path.Combine(safeTeachId, $"story_{storyIndex}");
        string relativePath = Path.Combine(relativeDir, $"{imageType}.png");

        // Convert to absolute path for saving
        string absolutePath = Path.Combine(Application.persistentDataPath, relativePath);

        // Ensure directory exists
        string absoluteDir = Path.GetDirectoryName(absolutePath);
        if (!Directory.Exists(absoluteDir))
        {
            Directory.CreateDirectory(absoluteDir);
        }

        // Save the file
        File.WriteAllBytes(absolutePath, texture.EncodeToPNG());

        Debug.Log($"✅ Saved locally: {relativePath}");
        return relativePath;
    }

    /// <summary>
    /// Fix aspect ratio for image display
    /// </summary>
    private void FixAspectRatio(RawImage image, Texture2D texture)
    {
        if (image == null || texture == null) return;

        AspectRatioFitter fitter = image.GetComponent<AspectRatioFitter>();
        if (fitter != null)
        {
            fitter.aspectRatio = (float)texture.width / texture.height;
        }
    }

    /// <summary>
    /// Update status text
    /// </summary>
    private void UpdateStatus(string message)
    {
        if (uploadStatusText != null)
        {
            uploadStatusText.text = message;
        }
        Debug.Log($"📊 Upload Status: {message}");
    }
}
