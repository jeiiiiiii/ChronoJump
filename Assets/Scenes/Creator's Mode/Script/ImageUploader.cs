using UnityEngine;
using UnityEngine.UI;
using System.IO;
using SFB;

public class ImageUploader : MonoBehaviour
{
    [Header("Character Previews")]
    public RawImage previewImage1; // left character
    public RawImage previewImage2; // right character

    [Header("Background Preview")]
    public RawImage previewBackground; // background image slot

    private Texture2D lastUploadedTexture1;
    private Texture2D lastUploadedTexture2;
    private Texture2D lastUploadedBackground;

private void Start()
{
    var story = StoryManager.Instance.currentStory;
    if (story == null) return;

    // ✅ Load background if path exists
    if (!string.IsNullOrEmpty(story.backgroundPath))
    {
        Texture2D tex = ImageStorage.LoadImage(story.backgroundPath);
        if (tex != null)
        {
            previewBackground.texture = tex;

            // Fix aspect ratio so it displays properly
            AspectRatioFitter fitter = previewBackground.GetComponent<AspectRatioFitter>();
            if (fitter != null)
                fitter.aspectRatio = (float)tex.width / tex.height;
        }
    }

    // TODO: If you want characters to also persist, add similar logic here:
    if (!string.IsNullOrEmpty(story.character1Path))
    {
        Texture2D tex1 = ImageStorage.LoadImage(story.character1Path);
        if (tex1 != null)
        {
            previewImage1.texture = tex1;
            var fitter = previewImage1.GetComponent<AspectRatioFitter>();
            if (fitter != null)
                fitter.aspectRatio = (float)tex1.width / tex1.height;
        }
    }

    if (!string.IsNullOrEmpty(story.character2Path))
    {
        Texture2D tex2 = ImageStorage.LoadImage(story.character2Path);
        if (tex2 != null)
        {
            previewImage2.texture = tex2;
            var fitter = previewImage2.GetComponent<AspectRatioFitter>();
            if (fitter != null)
                fitter.aspectRatio = (float)tex2.width / tex2.height;
        }
    }

    // // ✅ Load dialogues
    // DialogueStorage.dialogues = story.dialogues;

    // ✅ Load quizzes
    // AddQuiz.quizQuestions = story.quizQuestions;
}



    // Upload for first character slot
    public void UploadImage1()
    {
        UploadAndSetImage(ref lastUploadedTexture1, previewImage1, 1);
    }

    // Upload for second character slot
    public void UploadImage2()
    {
        UploadAndSetImage(ref lastUploadedTexture2, previewImage2, 2);
    }

    // Upload for background slot
    public void UploadBackground()
    {
        UploadAndSetImage(ref ImageStorage.UploadedTexture, previewBackground, 3);
        // Save into current story slot - THIS IS THE ONLY SAVE THAT SHOULD HAPPEN
        ImageStorage.SaveCurrentImageToStory();
    }  



    private void UploadAndSetImage(ref Texture2D lastTexture, RawImage targetPreview, int slot)
{
    // ✅ CHECK IF STORYMANAGER IS READY BEFORE UPLOADING
    if (!ImageStorage.IsReady())
    {
        Debug.LogError("❌ Cannot upload image - StoryManager is not initialized yet");
        return;
    }

    var extensions = new[] {
        new ExtensionFilter("Image files", "jpg", "jpeg", "png", "bmp", "gif")
    };

    var paths = StandaloneFileBrowser.OpenFilePanel("Select Image", "", extensions, false);

    if (paths.Length > 0 && File.Exists(paths[0]))
    {
        byte[] fileData = File.ReadAllBytes(paths[0]);

        Texture2D tex = new Texture2D(2, 2);
        if (tex.LoadImage(fileData))
        {
            if (lastTexture != null)
                Destroy(lastTexture);

            // Assign to preview
            targetPreview.texture = tex;
            lastTexture = tex;

            // ✅ Save globally
            if (slot == 1)
                ImageStorage.uploadedTexture1 = tex;
            else if (slot == 2)
                ImageStorage.uploadedTexture2 = tex;
            else if (slot == 3)
                ImageStorage.UploadedTexture = tex;

            // Save into current story
            ImageStorage.SaveCurrentImageToStory();

            // Fix aspect ratio
            AspectRatioFitter fitter = targetPreview.GetComponent<AspectRatioFitter>();
            if (fitter != null)
                fitter.aspectRatio = (float)tex.width / tex.height;

            Debug.Log($"✅ Image uploaded to {targetPreview.name} and saved to story");
        }
        else
        {
            Debug.LogError("❌ Failed to load image. Please select a valid file.");
        }
    }
}

    
}