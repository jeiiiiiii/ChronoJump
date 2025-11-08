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

    [Header("Button State Management")]
    public Button nextButton; // Assign in inspector
    public GameObject loadingSpinner; // Optional: visual indicator

    private bool _isSavingLocal = false;
    private bool _isSavingCloud = false;
    private bool _hasLocalSaveFailed = false;
    private bool _hasCloudSaveFailed = false;

    // Call this when starting any upload
    private void StartUploadProcess()
    {
        _isSavingLocal = true;
        _isSavingCloud = true;
        _hasLocalSaveFailed = false;
        _hasCloudSaveFailed = false;
        UpdateNextButtonState();
    }

    // Call this when local save completes
    private void CompleteLocalSave(bool success)
    {
        _isSavingLocal = false;
        _hasLocalSaveFailed = !success;
        UpdateNextButtonState();
    }

    // Call this when cloud save completes
    private void CompleteCloudSave(bool success)
    {
        _isSavingCloud = false;
        _hasCloudSaveFailed = !success;
        UpdateNextButtonState();
    }

    private void UpdateNextButtonState()
    {
        if (nextButton == null) return;

        bool canProceed = !_isSavingLocal && !_isSavingCloud && !_hasLocalSaveFailed;

        nextButton.interactable = canProceed;

        // Show/hide loading spinner
        if (loadingSpinner != null)
        {
            loadingSpinner.SetActive(_isSavingLocal || _isSavingCloud);
        }

        Debug.Log($"🔄 Next Button State: Local={_isSavingLocal}, Cloud={_isSavingCloud}, CanProceed={canProceed}");
    }

    private void Start()
    {
        LoadExistingImages();
    }

    /// <summary>
    /// Load existing images - prefers local cache, downloads from S3 only if needed
    /// </summary>
    private async void LoadExistingImages()
    {
        var story = StoryManager.Instance?.currentStory;
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

    private async Task UploadAndSetImageAsync(int slot, RawImage targetPreview)
    {
        // ✅ Start the upload process - disable next button
        StartUploadProcess();

        try
        {
            // ✅ Check if StoryManager is ready
            if (StoryManager.Instance == null)
            {
                Debug.LogError("❌ Cannot upload image - StoryManager is not initialized yet");
                UpdateStatus("Error: StoryManager not ready");
                CompleteLocalSave(false);
                CompleteCloudSave(false);
                return;
            }

            // ✅ Check if ValidationManager is ready
            if (ValidationManager.Instance == null)
            {
                Debug.LogError("❌ ValidationManager not found");
                UpdateStatus("Error: Validation system not ready");
                CompleteLocalSave(false);
                CompleteCloudSave(false);
                return;
            }

            // ✅ Check if S3 service is ready
            if (S3StorageService.Instance == null || !S3StorageService.Instance.IsReady)
            {
                Debug.LogError("❌ S3 service not initialized");
                UpdateStatus("Error: S3 service not ready");
                CompleteLocalSave(false);
                CompleteCloudSave(false);
                return;
            }

            var extensions = new[] {
                new ExtensionFilter("Image files", "jpg", "jpeg", "png", "bmp")
            };

            var paths = StandaloneFileBrowser.OpenFilePanel("Select Image", "", extensions, false);

            if (paths.Length == 0 || string.IsNullOrEmpty(paths[0]) || !File.Exists(paths[0]))
            {
                Debug.Log("ℹ️ No file selected or file doesn't exist");
                CompleteLocalSave(false);
                CompleteCloudSave(false);
                return;
            }

            isUploading = true;
            UpdateStatus("Loading image...");

            // ========== FILE VALIDATION ==========
            string selectedPath = paths[0];

            // 1. Check if file exists
            if (!File.Exists(selectedPath))
            {
                Debug.LogError($"❌ File not found: {selectedPath}");
                UpdateStatus("");
                ValidationManager.Instance.ShowWarning(
                    "File Not Found",
                    "The selected file could not be found.",
                    null,
                    null
                );
                CompleteLocalSave(false);
                CompleteCloudSave(false);
                isUploading = false;
                return;
            }

            // 2. Validate file format using ValidationManager
            var formatValidation = ValidateFileFormat(selectedPath);
            if (!formatValidation.isValid)
            {
                Debug.LogError($"❌ Invalid file format: {formatValidation.message}");
                UpdateStatus("");
                ValidationManager.Instance.ShowWarning(
                    "Invalid File Format",
                    formatValidation.message,
                    null,
                    null
                );
                CompleteLocalSave(false);
                CompleteCloudSave(false);
                isUploading = false;
                return;
            }

            // 3. Validate file size using ValidationManager
            var sizeValidation = ValidateFileSize(selectedPath);
            if (!sizeValidation.isValid)
            {
                Debug.LogError($"❌ File too large: {sizeValidation.message}");
                UpdateStatus("");
                ValidationManager.Instance.ShowWarning(
                    "File Too Large",
                    sizeValidation.message,
                    null,
                    null
                );
                CompleteLocalSave(false);
                CompleteCloudSave(false);
                isUploading = false;
                return;
            }

            // ✅ SAFE IMAGE LOADING WITH PROPER ERROR HANDLING
            Texture2D tex = null;
            bool loadingFailed = false;
            string errorTitle = "";
            string errorMessage = "";

            try
            {
                // Load image from file with additional safety checks
                byte[] fileData = File.ReadAllBytes(selectedPath);

                if (fileData == null || fileData.Length == 0)
                {
                    Debug.LogError("❌ File is empty or could not be read");
                    loadingFailed = true;
                    errorTitle = "Invalid Image File";
                    errorMessage = "The selected file appears to be empty or corrupted.\n\nPlease select a different file.";
                }
                else
                {
                    tex = new Texture2D(2, 2);

                    // Try to load the image data with additional validation
                    bool loadSuccess = false;
                    try
                    {
                        loadSuccess = tex.LoadImage(fileData);
                    }
                    catch (System.Exception innerEx)
                    {
                        Debug.LogError($"❌ LoadImage failed: {innerEx.Message}");
                        loadSuccess = false;
                    }

                    if (!loadSuccess || tex == null || tex.width <= 0 || tex.height <= 0)
                    {
                        Debug.LogError("❌ Failed to load image. The file may be corrupted or not a valid image format.");
                        loadingFailed = true;
                        errorTitle = "Invalid Image File";
                        errorMessage = "The selected file appears to be corrupted or is not a valid image.\n\nPlease select a different file.";

                        // DON'T destroy the texture here - just null it out
                        // Destroying it immediately might cause issues with Unity's internal state
                        tex = null;
                    }
                    else
                    {
                        Debug.Log($"✅ Image loaded successfully: {tex.width}x{tex.height}");
                    }
                }
            }
            catch (System.Exception loadEx)
            {
                Debug.LogError($"❌ Error loading image file: {loadEx.Message}");
                loadingFailed = true;
                errorTitle = "File Read Error";
                errorMessage = "Could not read the selected file.\n\nPlease select a different file.";

                // Just null it out - don't destroy
                tex = null;
            }

            // Handle any loading failures
            if (loadingFailed)
            {
                Debug.Log("🛑 ABOUT TO CLEANUP - Before UpdateStatus");
                UpdateStatus("");
                Debug.Log("🛑 ABOUT TO CLEANUP - After UpdateStatus, before CompleteLocalSave");
                CompleteLocalSave(false);
                Debug.Log("🛑 ABOUT TO CLEANUP - After CompleteLocalSave, before CompleteCloudSave");
                CompleteCloudSave(false);
                Debug.Log("🛑 ABOUT TO CLEANUP - After CompleteCloudSave, before isUploading");
                isUploading = false;
                Debug.Log("🛑 ABOUT TO SHOW WARNING");

                // Show warning LAST, after everything is cleaned up
                if (ValidationManager.Instance != null)
                {
                    ValidationManager.Instance.ShowWarning(errorTitle, errorMessage, null, null);
                }
                Debug.Log("🛑 AFTER SHOWING WARNING - ABOUT TO RETURN");
                return;
            }

            // Final safety check - this should never trigger if loadingFailed works correctly
            if (tex == null || tex.width <= 0 || tex.height <= 0)
            {
                Debug.LogError("❌ CRITICAL: Texture is invalid but loadingFailed was not set!");
                tex = null; // Just null it, don't destroy
                UpdateStatus("");
                CompleteLocalSave(false);
                CompleteCloudSave(false);
                isUploading = false;

                if (ValidationManager.Instance != null)
                {
                    ValidationManager.Instance.ShowWarning(
                        "Invalid Image File",
                        "Failed to load the image. Please select a different file.",
                        null,
                        null
                    );
                }
                return;
            }

            // Clean up old texture if exists
            try
            {
                if (slot == 1 && lastUploadedTexture1 != null)
                    DestroyImmediate(lastUploadedTexture1);
                else if (slot == 2 && lastUploadedTexture2 != null)
                    DestroyImmediate(lastUploadedTexture2);
                else if (slot == 3 && lastUploadedBackground != null)
                    DestroyImmediate(lastUploadedBackground);
            }
            catch (System.Exception destroyEx)
            {
                Debug.LogWarning($"⚠️ Error cleaning up old textures: {destroyEx.Message}");
                // Continue anyway - this is not critical
            }

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
                DestroyImmediate(tex);
                CompleteLocalSave(false);
                CompleteCloudSave(false);
                isUploading = false;
                return;
            }

            string teacherId = StoryManager.Instance.GetCurrentTeacherId();
            int storyIndex = story.storyIndex;

            // ========== SAVE LOCALLY FIRST ==========
            UpdateStatus("Saving locally...");
            string localPath = null;

            try
            {
                localPath = SaveTextureLocally(tex, storyIndex,
                    slot == 1 ? ImageStorage.IMAGE_TYPE_CHARACTER1 :
                    slot == 2 ? ImageStorage.IMAGE_TYPE_CHARACTER2 :
                    ImageStorage.IMAGE_TYPE_BACKGROUND);

                // ✅ Local save completed
                CompleteLocalSave(true);
                Debug.Log("✅ Local save completed");
            }
            catch (System.Exception localSaveEx)
            {
                Debug.LogError($"❌ Local save failed: {localSaveEx.Message}");
                UpdateStatus("");
                ValidationManager.Instance.ShowWarning(
                    "Save Failed",
                    "Could not save the image locally.\n\nPlease try again.",
                    null,
                    null
                );
                DestroyImmediate(tex);
                CompleteLocalSave(false);
                CompleteCloudSave(false);
                isUploading = false;
                return;
            }

            // ========== UPLOAD TO S3 ==========
            UpdateStatus("Uploading to cloud...");
            string s3Url = null;

            try
            {
                if (slot == 1)
                {
                    s3Url = await S3StorageService.Instance.UploadCharacter1Image(tex, teacherId, storyIndex);
                    if (!string.IsNullOrEmpty(s3Url))
                    {
                        story.character1Path = s3Url; // Store S3 URL as primary
                    }
                }
                else if (slot == 2)
                {
                    s3Url = await S3StorageService.Instance.UploadCharacter2Image(tex, teacherId, storyIndex);
                    if (!string.IsNullOrEmpty(s3Url))
                    {
                        story.character2Path = s3Url; // Store S3 URL as primary
                    }
                }
                else if (slot == 3)
                {
                    s3Url = await S3StorageService.Instance.UploadBackgroundImage(tex, teacherId, storyIndex);
                    if (!string.IsNullOrEmpty(s3Url))
                    {
                        story.backgroundPath = s3Url; // Store S3 URL as primary
                    }
                }

                if (!string.IsNullOrEmpty(s3Url))
                {
                    // Save to local storage as backup
                    StoryManager.Instance.SaveStories();

                    CompleteCloudSave(true);
                    UpdateStatus("✅ Upload successful!");
                    Debug.Log($"✅ Image uploaded to S3: {s3Url}");
                    Debug.Log($"✅ Image saved locally: {localPath}");
                }
                else
                {
                    // If S3 upload fails, fall back to local path
                    if (slot == 1) story.character1Path = localPath;
                    else if (slot == 2) story.character2Path = localPath;
                    else if (slot == 3) story.backgroundPath = localPath;

                    StoryManager.Instance.SaveStories();
                    CompleteCloudSave(false);
                    UpdateStatus("✅ Saved locally (cloud upload failed)");
                    Debug.LogWarning($"⚠️ S3 upload failed, using local path: {localPath}");
                }
            }
            catch (System.Exception uploadEx)
            {
                Debug.LogError($"❌ S3 upload error: {uploadEx.Message}");

                // Fall back to local path on upload failure
                if (slot == 1) story.character1Path = localPath;
                else if (slot == 2) story.character2Path = localPath;
                else if (slot == 3) story.backgroundPath = localPath;

                StoryManager.Instance.SaveStories();
                CompleteCloudSave(false);
                UpdateStatus("✅ Saved locally (cloud upload failed)");
                Debug.LogWarning($"⚠️ S3 upload failed, using local path: {localPath}");
            }

            // Clear status after 3 seconds
            await Task.Delay(3000);
            UpdateStatus("");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Upload process error: {ex.Message}");
            UpdateStatus("");

            // ✅ Mark both saves as failed
            CompleteLocalSave(false);
            CompleteCloudSave(false);

            // Show user-friendly error message
            if (ValidationManager.Instance != null)
            {
                ValidationManager.Instance.ShowWarning(
                    "Upload Failed",
                    "An error occurred while uploading the image.\n\nPlease try again.",
                    null,
                    null
                );
            }
        }
        finally
        {
            isUploading = false;
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
