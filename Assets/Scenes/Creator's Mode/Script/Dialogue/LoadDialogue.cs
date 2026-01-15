using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;
using System.Linq;
using System.IO;
using System;

public class DialoguePlayer : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public Button nextButton;
    public Button backButton;
    public RawImage backgroundImage;
    public RawImage character1Image;
    public RawImage character2Image;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public Button skipAudioButton;
    public GameObject audioPlayingIndicator;

    private int currentIndex = 0;
    private List<DialogueLine> dialogues;
    private StoryData currentStory;
    private bool isPlayingAudio = false;

    void Start()
    {
        Debug.Log("🎬 DialoguePlayer Started");
        Debug.Log($"📊 Data source: {DialogueStorage.GetDataSourceInfo()}");

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        LoadStoryData();
        dialogues = DialogueStorage.GetAllDialogues();

        if (dialogues.Count > 0)
        {
            bool hasVoicesFromFirebase = dialogues.All(d =>
                !string.IsNullOrEmpty(d.selectedVoiceId) ||
                string.IsNullOrEmpty(d.selectedVoiceId)
            );

            if (hasVoicesFromFirebase)
            {
                Debug.Log("✅ Voices already loaded from Firebase - skipping TeacherPrefs load");
                for (int i = 0; i < dialogues.Count; i++)
                {
                    var dialogue = dialogues[i];
                    if (string.IsNullOrEmpty(dialogue.selectedVoiceId))
                    {
                        Debug.Log($"🔇 Dialogue {i} '{dialogue.characterName}' - No voice selected");
                    }
                    else
                    {
                        var voice = VoiceLibrary.GetVoiceById(dialogue.selectedVoiceId);
                        Debug.Log($"🎤 Dialogue {i}: '{dialogue.characterName}' → {voice.voiceName}");
                    }
                }
            }
            else
            {
                Debug.Log("🎤 Loading voice assignments from persistent storage...");
                DialogueStorage.LoadAllVoices();
            }

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

        if (skipAudioButton != null)
        {
            skipAudioButton.onClick.AddListener(SkipAudio);
            skipAudioButton.gameObject.SetActive(false);
        }

        if (audioPlayingIndicator != null)
        {
            audioPlayingIndicator.SetActive(false);
        }

        LoadStoryAssets();
        LoadQuizQuestions();
    }

    void ShowDialogue(int index)
    {
        if (dialogues == null || index >= dialogues.Count) return;

        var dialogue = dialogues[index];
        Debug.Log($"💬 Showing dialogue {index + 1}/{dialogues.Count}: {dialogue.characterName}");

        dialogueText.text = $"{dialogue.characterName}: {dialogue.dialogueText}";

        StartCoroutine(LoadAudioForDialogue(dialogue, index));
    }

    // ✅ FIXED: Properly handle S3 URLs with sanitized filenames
    IEnumerator LoadAudioForDialogue(DialogueLine dialogue, int dialogueIndex)
    {
        if (string.IsNullOrEmpty(dialogue.selectedVoiceId) || VoiceLibrary.IsNoVoice(dialogue.selectedVoiceId))
        {
            Debug.Log($"🔇 Dialogue {dialogueIndex}: '{dialogue.characterName}' - No voice, skipping");
            yield break;
        }

        var voice = VoiceLibrary.GetVoiceById(dialogue.selectedVoiceId);
        Debug.Log($"🔍 Looking for audio: '{dialogue.characterName}' - {voice.voiceName}");

        // ========== STEP 1: CHECK LOCAL CACHE ==========
        string localPath = FindLocalAudioFile(dialogue, dialogueIndex);

        if (!string.IsNullOrEmpty(localPath) && File.Exists(localPath))
        {
            Debug.Log($"✅ Found local audio: {Path.GetFileName(localPath)}");
            dialogue.audioFilePath = localPath;
            dialogue.hasAudio = true;

            StartCoroutine(PlayDialogueAudio(dialogue));
            yield break;
        }

        // ========== STEP 2: TRY TO DOWNLOAD FROM S3 ==========
        if (!string.IsNullOrEmpty(dialogue.audioStoragePath))
        {
            // ✅ FIX: Sanitize the S3 URL to ensure spaces are replaced with underscores
            string sanitizedS3Url = SanitizeS3Url(dialogue.audioStoragePath);

            Debug.Log($"☁️ Local not found, downloading from S3");
            Debug.Log($"   Original URL: {dialogue.audioStoragePath}");
            Debug.Log($"   Sanitized URL: {sanitizedS3Url}");

            if (S3StorageService.Instance == null || !S3StorageService.Instance.IsReady)
            {
                Debug.LogWarning($"⚠️ S3 service not available");
                yield break;
            }

            if (audioPlayingIndicator != null)
            {
                audioPlayingIndicator.SetActive(true);
            }

            // ✅ Use the sanitized URL for download
            var downloadTask = S3StorageService.Instance.DownloadVoiceAudio(sanitizedS3Url);

            while (!downloadTask.IsCompleted)
            {
                yield return null;
            }

            byte[] audioData = downloadTask.Result;

            if (audioData != null && audioData.Length > 0)
            {
                Debug.Log($"✅ Downloaded {audioData.Length} bytes from S3");

                // ========== STEP 3: SAVE TO LOCAL CACHE ==========
                string cachedPath = SaveAudioToLocalCache(audioData, dialogue, dialogueIndex);

                if (!string.IsNullOrEmpty(cachedPath))
                {
                    Debug.Log($"💾 Cached audio: {cachedPath}");
                    dialogue.audioFilePath = cachedPath;
                    dialogue.hasAudio = true;

                    if (audioPlayingIndicator != null)
                    {
                        audioPlayingIndicator.SetActive(false);
                    }

                    StartCoroutine(PlayDialogueAudio(dialogue));
                }
            }
            else
            {
                Debug.LogError($"❌ Failed to download from S3 - received empty data");
                if (audioPlayingIndicator != null)
                {
                    audioPlayingIndicator.SetActive(false);
                }
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ No audio available for '{dialogue.characterName}'");
            Debug.LogWarning($"   No local file and no S3 URL");
        }
    }

    // ✅ NEW: Sanitize S3 URL to replace spaces with underscores
    string SanitizeS3Url(string s3Url)
    {
        if (string.IsNullOrEmpty(s3Url)) return s3Url;

        // Get the last part of the URL (the filename)
        int lastSlashIndex = s3Url.LastIndexOf('/');
        if (lastSlashIndex == -1) return s3Url;

        string baseUrl = s3Url.Substring(0, lastSlashIndex + 1);
        string fileName = s3Url.Substring(lastSlashIndex + 1);

        // Replace spaces with underscores in the filename
        string sanitizedFileName = fileName.Replace(" ", "_");

        // URL encode the filename to handle other special characters
        sanitizedFileName = UnityWebRequest.EscapeURL(sanitizedFileName)
            .Replace("%2F", "/")  // Don't encode forward slashes
            .Replace("%3A", ":");  // Don't encode colons

        return baseUrl + sanitizedFileName;
    }

    string SaveAudioToLocalCache(byte[] audioData, DialogueLine dialogue, int dialogueIndex)
    {
        try
        {
            string teacherId = GetTeacherId();
            int storyIndex = GetStoryIndex();
            string audioDir = Path.Combine(
                Application.persistentDataPath,
                teacherId,
                $"story_{storyIndex}",
                "audio"
            );

            if (!Directory.Exists(audioDir))
            {
                Directory.CreateDirectory(audioDir);
            }

            // ✅ Use sanitized filename that matches what was uploaded
            string sanitizedName = SanitizeFileName(dialogue.characterName);
            var voice = VoiceLibrary.GetVoiceById(dialogue.selectedVoiceId);
            string sanitizedVoiceName = SanitizeFileName(voice.voiceName);

            string fileName = $"dialogue_{dialogueIndex}_{sanitizedName}_{sanitizedVoiceName}.mp3";
            string filePath = Path.Combine(audioDir, fileName);

            File.WriteAllBytes(filePath, audioData);

            Debug.Log($"💾 Saved to cache: {fileName}");
            return filePath;
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Failed to cache audio: {ex.Message}");
            return null;
        }
    }

    string FindLocalAudioFile(DialogueLine dialogue, int dialogueIndex)
    {
        try
        {
            string teacherId = GetTeacherId();
            int storyIndex = GetStoryIndex();
            string audioDir = Path.Combine(
                Application.persistentDataPath,
                teacherId,
                $"story_{storyIndex}",
                "audio"
            );

            if (!Directory.Exists(audioDir))
            {
                return null;
            }

            string sanitizedName = SanitizeFileName(dialogue.characterName);
            var voice = VoiceLibrary.GetVoiceById(dialogue.selectedVoiceId);
            string sanitizedVoiceName = SanitizeFileName(voice.voiceName);

            // ✅ Try exact match with sanitized names
            string exactPattern = $"dialogue_{dialogueIndex}_{sanitizedName}_{sanitizedVoiceName}.mp3";
            string exactPath = Path.Combine(audioDir, exactPattern);

            if (File.Exists(exactPath))
            {
                Debug.Log($"✅ Found exact match: {exactPattern}");
                return exactPath;
            }

            // Fallback: Search with wildcards
            string[] files = Directory.GetFiles(audioDir, $"dialogue_{dialogueIndex}_*.mp3");

            if (files.Length > 0)
            {
                Array.Sort(files);
                string latestFile = files[files.Length - 1];
                Debug.Log($"✅ Found fallback: {Path.GetFileName(latestFile)}");
                return latestFile;
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Error finding audio: {ex.Message}");
            return null;
        }
    }

    string SanitizeFileName(string fileName)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }

        // Replace spaces and special characters
        fileName = fileName.Replace(' ', '_');
        fileName = fileName.Replace('#', '_');
        fileName = fileName.Replace('%', '_');
        fileName = fileName.Replace('&', '_');

        return fileName;
    }

    string GetTeacherId()
    {
        string userRole = PlayerPrefs.GetString("UserRole", "student");

        if (userRole.ToLower() == "student")
        {
            string storyJson = StudentPrefs.GetString("CurrentStoryData", "");
            if (!string.IsNullOrEmpty(storyJson))
            {
                try
                {
                    var studentStory = JsonUtility.FromJson<StoryData>(storyJson);
                    if (!string.IsNullOrEmpty(studentStory.backgroundPath))
                    {
                        // ✅ Check if it's an S3 URL
                        if (studentStory.backgroundPath.StartsWith("http"))
                        {
                            // Extract teacher ID from S3 URL
                            // URL format: https://jei-cj.s3.ap-southeast-1.amazonaws.com/images/TEACHER_ID/story_X/...
                            string[] urlParts = studentStory.backgroundPath.Split('/');

                            // Find "images" in the URL, teacher ID is right after it
                            for (int i = 0; i < urlParts.Length - 1; i++)
                            {
                                if (urlParts[i] == "images")
                                {
                                    string teacherId = urlParts[i + 1];
                                    Debug.Log($"✅ Extracted teacher ID from S3 URL: {teacherId}");
                                    return teacherId;
                                }
                            }
                        }
                        else
                        {
                            // Local path format: TEACHER_ID/story_X/...
                            string[] pathParts = studentStory.backgroundPath.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                            if (pathParts.Length > 0)
                            {
                                Debug.Log($"✅ Extracted teacher ID from local path: {pathParts[0]}");
                                return pathParts[0];
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Could not extract teacher ID: {ex.Message}");
                }
            }
        }

        if (StoryManager.Instance != null && StoryManager.Instance.IsCurrentUserTeacher())
        {
            return StoryManager.Instance.GetCurrentTeacherId();
        }

        return TeacherPrefs.GetString("CurrentTeachId", "default");
    }


    int GetStoryIndex()
    {
        var story = StoryManager.Instance?.GetCurrentStory();
        if (story != null && story.storyIndex >= 0)
        {
            return story.storyIndex;
        }

        string storyJson = StudentPrefs.GetString("CurrentStoryData", "");
        if (!string.IsNullOrEmpty(storyJson))
        {
            try
            {
                var studentStory = JsonUtility.FromJson<StoryData>(storyJson);

                if (studentStory.storyIndex < 0 && !string.IsNullOrEmpty(studentStory.backgroundPath))
                {
                    string[] pathParts = studentStory.backgroundPath.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    if (pathParts.Length > 1 && pathParts[1].StartsWith("story_"))
                    {
                        string indexStr = pathParts[1].Replace("story_", "");
                        if (int.TryParse(indexStr, out int extractedIndex))
                        {
                            Debug.Log($"✅ Extracted story index from path: {extractedIndex}");
                            return extractedIndex;
                        }
                    }
                }

                if (studentStory.storyIndex >= 0)
                {
                    return studentStory.storyIndex;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Could not extract story index: {ex.Message}");
            }
        }

        return 0;
    }

    IEnumerator PlayDialogueAudio(DialogueLine dialogue)
    {
        isPlayingAudio = true;

        if (skipAudioButton != null)
            skipAudioButton.gameObject.SetActive(true);
        if (audioPlayingIndicator != null)
            audioPlayingIndicator.SetActive(true);

        nextButton.interactable = false;
        backButton.interactable = false;

        string url = "file://" + dialogue.audioFilePath;

        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                audioSource.clip = clip;
                audioSource.Play();

                Debug.Log($"🔊 Playing audio for: {dialogue.characterName}");

                while (audioSource.isPlaying && isPlayingAudio)
                {
                    yield return null;
                }

                Debug.Log($"✅ Finished playing audio for: {dialogue.characterName}");
            }
            else
            {
                Debug.LogError($"❌ Failed to load audio: {request.error}");
            }
        }

        nextButton.interactable = true;
        backButton.interactable = true;

        if (skipAudioButton != null)
            skipAudioButton.gameObject.SetActive(false);
        if (audioPlayingIndicator != null)
            audioPlayingIndicator.SetActive(false);

        isPlayingAudio = false;
    }

    void SkipAudio()
    {
        if (isPlayingAudio && audioSource.isPlaying)
        {
            audioSource.Stop();
            isPlayingAudio = false;
            Debug.Log("⏭️ Audio skipped");
        }
    }

    void NextDialogue()
    {
        if (isPlayingAudio)
        {
            SkipAudio();
        }

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
            Debug.Log("🎓 All dialogues completed, ready for quiz");
        }
    }

    public void PreviousDialogue()
    {
        if (isPlayingAudio)
        {
            SkipAudio();
        }

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

    private void LoadStoryData()
    {
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
            catch (Exception ex)
            {
                Debug.LogError($"❌ Error loading story data: {ex.Message}");
            }
        }
        else
        {
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

        if (!string.IsNullOrEmpty(currentStory.backgroundPath))
        {
            StartCoroutine(LoadBackgroundImage(currentStory.backgroundPath));
        }
        else
        {
            Debug.Log("ℹ️ No background path specified");
            SetDefaultBackground();
        }

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

        if (ImageStorage.IsS3Url(imagePath))
        {
            Debug.Log($"🌐 Background is S3 URL, downloading: {imagePath}");
            SetDownloadPlaceholder(backgroundImage, "Background");

            var downloadTask = ImageStorage.LoadImageAsync(imagePath);
            yield return new WaitUntil(() => downloadTask.IsCompleted);

            if (downloadTask.Result != null)
            {
                ApplyBackgroundTexture(downloadTask.Result);
                Debug.Log($"✅ Successfully loaded background from S3");
                yield break;
            }
            else
            {
                Debug.LogError($"❌ Failed to download background from S3");
                SetDefaultBackground();
                yield break;
            }
        }

        Texture2D localTexture = ImageStorage.LoadImage(imagePath);
        if (localTexture != null)
        {
            Debug.Log($"✅ Loaded background from local storage");
            ApplyBackgroundTexture(localTexture);
            yield break;
        }

        if (IsFirebaseStoragePath(imagePath))
        {
            Debug.Log($"🌐 Background is a Firebase Storage path");
            SetDownloadPlaceholder(backgroundImage, "Background");
            yield return StartCoroutine(DownloadImageFromFirebase(imagePath, backgroundImage, true));
        }
        else
        {
            Debug.Log($"⚠️ Background path not found locally");
            SetDefaultBackground();
        }
    }

    private IEnumerator LoadCharacterImage(string imagePath, RawImage characterImage, int characterNumber)
    {
        Debug.Log($"👤 Attempting to load character {characterNumber}: {imagePath}");

        if (ImageStorage.IsS3Url(imagePath))
        {
            Debug.Log($"🌐 Character {characterNumber} is S3 URL, downloading");
            SetDownloadPlaceholder(characterImage, $"Character {characterNumber}");

            var downloadTask = ImageStorage.LoadImageAsync(imagePath);
            yield return new WaitUntil(() => downloadTask.IsCompleted);

            if (downloadTask.Result != null)
            {
                ApplyCharacterTexture(characterImage, downloadTask.Result);
                Debug.Log($"✅ Successfully loaded character {characterNumber} from S3");
                yield break;
            }
            else
            {
                Debug.LogError($"❌ Failed to download character {characterNumber} from S3");
                characterImage.gameObject.SetActive(false);
                yield break;
            }
        }

        Texture2D localTexture = ImageStorage.LoadImage(imagePath);
        if (localTexture != null)
        {
            Debug.Log($"✅ Loaded character {characterNumber} from local storage");
            ApplyCharacterTexture(characterImage, localTexture);
            yield break;
        }

        if (IsFirebaseStoragePath(imagePath))
        {
            Debug.Log($"🌐 Character {characterNumber} is a Firebase Storage path");
            SetDownloadPlaceholder(characterImage, $"Character {characterNumber}");
            yield return StartCoroutine(DownloadImageFromFirebase(imagePath, characterImage, false));
        }
        else
        {
            Debug.Log($"⚠️ Character {characterNumber} path not found locally");
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
        yield return new WaitForSeconds(0.5f);

        Texture2D placeholderTexture = CreatePlaceholderTexture(isBackground ? "Downloading Background..." : "Downloading Character...");

        if (isBackground)
        {
            ApplyBackgroundTexture(placeholderTexture);
        }
        else
        {
            ApplyCharacterTexture(targetImage, placeholderTexture);
        }

        Debug.Log($"🔥 Download placeholder set for: {firebasePath}");
    }

    private Texture2D CreatePlaceholderTexture(string message)
    {
        Texture2D texture = new Texture2D(256, 256);
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
        Texture2D defaultBg = new Texture2D(1, 1);
        defaultBg.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.2f));
        defaultBg.Apply();

        ApplyBackgroundTexture(defaultBg);
        Debug.Log("🎨 Applied default background");
    }

    private void LoadQuizQuestions()
    {
        string storyJson = StudentPrefs.GetString("CurrentStoryData", "");
        if (!string.IsNullOrEmpty(storyJson))
        {
            try
            {
                var studentStory = JsonUtility.FromJson<StoryData>(storyJson);
                if (studentStory != null && studentStory.quizQuestions != null)
                {
                    AddQuiz.quizQuestions = studentStory.quizQuestions;
                    Debug.Log($"✅ Loaded {studentStory.quizQuestions.Count} questions from StudentPrefs");
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Error loading quiz questions: {ex.Message}");
            }
        }

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
                Debug.Log($"✅ Loaded {story.quizQuestions?.Count ?? 0} questions from StoryManager");
            }
            else
            {
                Debug.LogWarning("⚠️ No quiz questions available");
            }
        }
    }
}
