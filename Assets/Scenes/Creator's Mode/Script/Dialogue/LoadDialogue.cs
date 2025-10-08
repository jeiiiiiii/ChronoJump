using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class DialoguePlayer : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public Button nextButton;
    public Button backButton;
    public RawImage backgroundImage;
    public RawImage character1Image;
    public RawImage character2Image;

    private int currentIndex = 0;
    private List<DialogueLine> dialogues;
    private StoryData currentStory;

    void Start()
    {
        Debug.Log("🎬 DialoguePlayer Started");
        Debug.Log($"📊 Data source: {DialogueStorage.GetDataSourceInfo()}");

        // Load story data first
        LoadStoryData();

        // Load dialogues from DialogueStorage
        dialogues = DialogueStorage.GetAllDialogues();

        if (dialogues.Count > 0)
        {
            ShowDialogue(0);
            Debug.Log($"✅ Loaded {dialogues.Count} dialogues for playback");
        }
        else
        {
            Debug.LogError("❌ No dialogues found in DialogueStorage!");
            dialogueText.text = "No story content available.";
            return;
        }

        nextButton.onClick.AddListener(NextDialogue);
        backButton.onClick.AddListener(PreviousDialogue); 

        // Load background and character images
        LoadStoryAssets();

        // Load quizzes for the quiz scene
        LoadQuizQuestions();
    }

    private void LoadStoryData()
    {
        // Try to load from StudentPrefs first (student mode)
        string storyJson = StudentPrefs.GetString("CurrentStoryData", "");
        if (!string.IsNullOrEmpty(storyJson))
        {
            try
            {
                currentStory = JsonUtility.FromJson<StoryData>(storyJson);
                if (currentStory != null)
                {
                    Debug.Log($"✅ Loaded story: {currentStory.storyTitle}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Error loading story data: {ex.Message}");
            }
        }
        else
        {
            // Fallback to StoryManager (teacher mode)
            currentStory = StoryManager.Instance?.GetCurrentStory();
        }
    }

    private void LoadStoryAssets()
    {
        if (currentStory == null)
        {
            Debug.LogWarning("⚠️ No story data available for loading assets");
            return;
        }

        // Load background
        if (!string.IsNullOrEmpty(currentStory.backgroundPath))
        {
            StartCoroutine(LoadBackgroundImage(currentStory.backgroundPath));
        }
        else
        {
            Debug.Log("ℹ️ No background path specified");
            SetDefaultBackground();
        }

        // Load character images
        if (!string.IsNullOrEmpty(currentStory.character1Path))
        {
            StartCoroutine(LoadCharacterImage(currentStory.character1Path, character1Image, 1));
        }
        else
        {
            character1Image.gameObject.SetActive(false);
            Debug.Log("ℹ️ No character 1 path specified");
        }

        if (!string.IsNullOrEmpty(currentStory.character2Path))
        {
            StartCoroutine(LoadCharacterImage(currentStory.character2Path, character2Image, 2));
        }
        else
        {
            character2Image.gameObject.SetActive(false);
            Debug.Log("ℹ️ No character 2 path specified");
        }
    }

    private IEnumerator LoadBackgroundImage(string imagePath)
    {
        Debug.Log($"🖼️ Attempting to load background: {imagePath}");

        // First try: Check if it's a local file path that exists
        Texture2D localTexture = ImageStorage.LoadImage(imagePath);
        if (localTexture != null)
        {
            Debug.Log($"✅ Loaded background from local storage: {imagePath}");
            ApplyBackgroundTexture(localTexture);
            yield break;
        }

        // Second try: Check if it's a Firebase Storage path (starts with gs:// or https://)
        if (IsFirebaseStoragePath(imagePath))
        {
            Debug.Log($"🌐 Background is a Firebase Storage path, will download: {imagePath}");

            // TODO: Implement Firebase Storage download here
            // For now, set a placeholder and mark for download
            SetDownloadPlaceholder(backgroundImage, "Background");
            yield return StartCoroutine(DownloadImageFromFirebase(imagePath, backgroundImage, true));
        }
        else
        {
            // Third try: It might be a relative path that doesn't exist locally
            Debug.Log($"⚠️ Background path not found locally and not a Firebase URL: {imagePath}");
            SetDefaultBackground();
        }
    }

    private IEnumerator LoadCharacterImage(string imagePath, RawImage characterImage, int characterNumber)
    {
        Debug.Log($"👤 Attempting to load character {characterNumber}: {imagePath}");

        // First try: Check if it's a local file path that exists
        Texture2D localTexture = ImageStorage.LoadImage(imagePath);
        if (localTexture != null)
        {
            Debug.Log($"✅ Loaded character {characterNumber} from local storage: {imagePath}");
            ApplyCharacterTexture(characterImage, localTexture);
            yield break;
        }

        // Second try: Check if it's a Firebase Storage path
        if (IsFirebaseStoragePath(imagePath))
        {
            Debug.Log($"🌐 Character {characterNumber} is a Firebase Storage path, will download: {imagePath}");

            // TODO: Implement Firebase Storage download here
            // For now, set a placeholder and mark for download
            SetDownloadPlaceholder(characterImage, $"Character {characterNumber}");
            yield return StartCoroutine(DownloadImageFromFirebase(imagePath, characterImage, false));
        }
        else
        {
            Debug.Log($"⚠️ Character {characterNumber} path not found locally: {imagePath}");
            characterImage.gameObject.SetActive(false);
        }
    }

    private bool IsFirebaseStoragePath(string path)
    {
        return !string.IsNullOrEmpty(path) &&
               (path.StartsWith("gs://") || path.StartsWith("https://firebasestorage.googleapis.com/"));
    }

    private IEnumerator DownloadImageFromFirebase(string firebasePath, RawImage targetImage, bool isBackground)
    {
        Debug.Log($"⬇️ Starting download from Firebase: {firebasePath}");

        // TODO: Implement actual Firebase Storage download
        // This is a placeholder for the download implementation

        // Simulate download delay
        yield return new WaitForSeconds(0.5f);

        // For now, we'll create a placeholder texture
        Texture2D placeholderTexture = CreatePlaceholderTexture(isBackground ? "Downloading Background..." : "Downloading Character...");

        if (isBackground)
        {
            ApplyBackgroundTexture(placeholderTexture);
        }
        else
        {
            ApplyCharacterTexture(targetImage, placeholderTexture);
        }

        Debug.Log($"📥 Download placeholder set for: {firebasePath}");

        // Note: When you implement the actual download, you should:
        // 1. Download the image from Firebase Storage
        // 2. Convert it to Texture2D
        // 3. Save it locally using ImageStorage.SaveImage()
        // 4. Update the story data with the local path
        // 5. Apply the actual texture to the UI
    }

    private Texture2D CreatePlaceholderTexture(string message)
    {
        Texture2D texture = new Texture2D(256, 256);

        // Create a simple colored placeholder
        Color[] pixels = new Color[256 * 256];
        Color bgColor = new Color(0.2f, 0.2f, 0.3f, 1f);

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = bgColor;
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return texture;
    }

    private void SetDownloadPlaceholder(RawImage image, string type)
    {
        Texture2D placeholder = CreatePlaceholderTexture($"Downloading {type}...");
        image.texture = placeholder;
        image.gameObject.SetActive(true);

        // Add aspect ratio fitter if needed
        AspectRatioFitter fitter = image.GetComponent<AspectRatioFitter>();
        if (fitter != null)
        {
            fitter.aspectRatio = 1f;
        }
    }

    private void ApplyBackgroundTexture(Texture2D texture)
    {
        backgroundImage.texture = texture;
        backgroundImage.gameObject.SetActive(true);

        AspectRatioFitter fitter = backgroundImage.GetComponent<AspectRatioFitter>();
        if (fitter != null)
        {
            fitter.aspectRatio = (float)texture.width / texture.height;
        }
    }

    private void ApplyCharacterTexture(RawImage characterImage, Texture2D texture)
    {
        characterImage.texture = texture;
        characterImage.gameObject.SetActive(true);

        AspectRatioFitter fitter = characterImage.GetComponent<AspectRatioFitter>();
        if (fitter != null)
        {
            fitter.aspectRatio = (float)texture.width / texture.height;
        }
    }

    private void SetDefaultBackground()
    {
        // Create a simple default background
        Texture2D defaultBg = new Texture2D(1, 1);
        defaultBg.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.2f));
        defaultBg.Apply();

        ApplyBackgroundTexture(defaultBg);
        Debug.Log("🎨 Applied default background");
    }

    private void LoadQuizQuestions()
    {
        // Try to load from StudentPrefs first (student mode)
        string storyJson = StudentPrefs.GetString("CurrentStoryData", "");
        if (!string.IsNullOrEmpty(storyJson))
        {
            try
            {
                var studentStory = JsonUtility.FromJson<StoryData>(storyJson);
                if (studentStory != null && studentStory.quizQuestions != null)
                {
                    AddQuiz.quizQuestions = studentStory.quizQuestions;
                    Debug.Log($"✅ Loaded {studentStory.quizQuestions.Count} questions from StudentPrefs for quiz");
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Error loading quiz questions: {ex.Message}");
            }
        }

        // Fallback to currentStory or StoryManager
        if (currentStory != null && currentStory.quizQuestions != null)
        {
            AddQuiz.quizQuestions = currentStory.quizQuestions;
            Debug.Log($"✅ Loaded {currentStory.quizQuestions.Count} questions for quiz");
        }
        else
        {
            var story = StoryManager.Instance?.GetCurrentStory();
            if (story != null)
            {
                AddQuiz.quizQuestions = story.quizQuestions;
                Debug.Log($"✅ Loaded {story.quizQuestions?.Count ?? 0} questions from StoryManager for quiz");
            }
            else
            {
                Debug.LogWarning("⚠️ No quiz questions available for quiz scene");
            }
        }
    }

    void ShowDialogue(int index)
    {
        if (dialogues == null || index >= dialogues.Count) return;

        var dialogue = dialogues[index];
        Debug.Log($"💬 Showing dialogue {index + 1}/{dialogues.Count}: {dialogue.characterName} - {dialogue.dialogueText}");

        dialogueText.text = $"{dialogue.characterName}: {dialogue.dialogueText}";

        UpdateCharacterImages(dialogue.characterName);
    }

    void UpdateCharacterImages(string characterName)
    {
        // Simple character visibility logic - enhance based on your needs
        // This could show/hide characters based on who's speaking
        if (character1Image != null && character1Image.gameObject.activeInHierarchy)
        {
            // You could add logic here to highlight the speaking character
        }
    }

    void NextDialogue()
    {
        currentIndex++;
        if (currentIndex < dialogues.Count)
        {
            ShowDialogue(currentIndex);
        }
        else
        {
            dialogueText.text = "Tapos na ang iyong paglalakbay! Maghanda para sa pagsusulit.";
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(QuizTime);

            Debug.Log("🏁 All dialogues completed, ready for quiz");
        }
    }

    public void PreviousDialogue()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            ShowDialogue(currentIndex);
        }
    }

    public void QuizTime()
    {
        Debug.Log("🎯 Moving to quiz scene");
        SceneManager.LoadScene("QuizTime");
    }
}
