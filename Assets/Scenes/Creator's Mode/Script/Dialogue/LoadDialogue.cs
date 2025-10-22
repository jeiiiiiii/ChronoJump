using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;

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
    public Button skipAudioButton; // Optional: skip to next dialogue while audio is playing
    public GameObject audioPlayingIndicator; // Optional: visual indicator
    
    private int currentIndex = 0;
    private List<DialogueLine> dialogues;
    private StoryData currentStory;
    private bool isPlayingAudio = false;

    // LoadDialogue.cs - FIXED Start() method
    // Replace your existing Start() method with this:

    void Start()
    {
        Debug.Log("🎬 DialoguePlayer Started");
        Debug.Log($"📊 Data source: {DialogueStorage.GetDataSourceInfo()}");

        // Setup audio source
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // ✅ Load story data first
        LoadStoryData();

        // ✅ CRITICAL FIX: Load voices BEFORE getting dialogues
        // This ensures voice IDs are populated from TeacherPrefs
        Debug.Log("🎤 Loading voice assignments from persistent storage...");
        DialogueStorage.LoadAllVoices();

        // ✅ Now get the dialogues (they should have voice IDs loaded)
        dialogues = DialogueStorage.GetAllDialogues();

        if (dialogues.Count > 0)
        {
            ShowDialogue(0);
            Debug.Log($"✅ Loaded {dialogues.Count} dialogues for playback");

            // ✅ FIXED: Debug: Verify voice assignments
            bool allVoicesValid = true;
            for (int i = 0; i < dialogues.Count; i++)
            {
                var dialogue = dialogues[i];

                // ✅ FIXED: Handle empty voice IDs properly
                if (string.IsNullOrEmpty(dialogue.selectedVoiceId))
                {
                    // This is VALID - means "No Voice" was intentionally selected
                    Debug.Log($"🔇 Dialogue {i} '{dialogue.characterName}' - No voice selected (will skip TTS)");
                    // DON'T assign default voice - keep it empty!
                }
                else
                {
                    // Validate that non-empty voice IDs exist in library
                    var voice = VoiceLibrary.GetVoiceById(dialogue.selectedVoiceId);
                    if (voice == null || VoiceLibrary.IsNoVoice(voice.voiceId))
                    {
                        Debug.LogWarning($"⚠️ Dialogue {i} '{dialogue.characterName}' has invalid voice ID: {dialogue.selectedVoiceId}");
                        allVoicesValid = false;

                        // Optional: You could set it to empty here if you want to auto-fix invalid IDs
                        // dialogue.selectedVoiceId = "";
                    }
                    else
                    {
                        Debug.Log($"🎤 Dialogue {i}: '{dialogue.characterName}' → {voice.voiceName} (ID: {dialogue.selectedVoiceId})");
                    }
                }
            }


            if (!allVoicesValid)
            {
                Debug.LogWarning("⚠️ Some dialogues had invalid voices - they will be treated as 'No Voice'");
                // Save to persist any corrections
                DialogueStorage.LoadAllVoices();
            }

        }
        else
        {
            Debug.LogError("❌ No dialogues found in DialogueStorage!");
            dialogueText.text = "No story content available.";
            return;
        }

        // Setup buttons
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



    // LoadDialogue.cs - FIXED ShowDialogue method
    // Replace your existing ShowDialogue method with this:

    void ShowDialogue(int index)
    {
        if (dialogues == null || index >= dialogues.Count) return;

        var dialogue = dialogues[index];
        Debug.Log($"💬 Showing dialogue {index + 1}/{dialogues.Count}: {dialogue.characterName} - {dialogue.dialogueText}");

        dialogueText.text = $"{dialogue.characterName}: {dialogue.dialogueText}";
        UpdateCharacterImages(dialogue.characterName);

        // ✅ CRITICAL FIX: Find the audio file using the correct voice
        var voice = VoiceLibrary.GetVoiceById(dialogue.selectedVoiceId);
        Debug.Log($"🔍 Looking for audio - Character: '{dialogue.characterName}', Voice: {voice.voiceName}, Index: {index}");

        // Try to find existing audio file
        string audioPath = FindAudioFile(dialogue, index);

        if (!string.IsNullOrEmpty(audioPath) && System.IO.File.Exists(audioPath))
        {
            dialogue.audioFilePath = audioPath;
            dialogue.hasAudio = true;
            Debug.Log($"✅ Found audio file: {audioPath}");

            // Play the audio
            StartCoroutine(PlayDialogueAudio(dialogue));
        }
        else
        {
            Debug.LogWarning($"⚠️ No audio found for dialogue: {dialogue.characterName}");
            Debug.LogWarning($"   Expected voice: {voice.voiceName} (ID: {dialogue.selectedVoiceId})");
            Debug.LogWarning($"   Searched path pattern: dialogue_{index}_{SanitizeFileName(dialogue.characterName)}_{voice.voiceName}_*.mp3");
        }
    }


    // ✅ NEW: Find audio file for a specific dialogue
    string FindAudioFile(DialogueLine dialogue, int dialogueIndex)
    {
        // ✅ FIXED: If no voice is selected, don't look for audio files
        if (string.IsNullOrEmpty(dialogue.selectedVoiceId) || VoiceLibrary.IsNoVoice(dialogue.selectedVoiceId))
        {
            Debug.Log($"🔇 Dialogue {dialogueIndex}: '{dialogue.characterName}' - No voice selected, skipping audio search");
            return null;
        }
        
        try
        {
            // Get the audio directory
            string teacherId = GetTeacherId();
            int storyIndex = GetStoryIndex();
            string audioDir = System.IO.Path.Combine(
                Application.persistentDataPath,
                teacherId,
                $"story_{storyIndex}",
                "audio"
            );

            if (!System.IO.Directory.Exists(audioDir))
            {
                Debug.LogWarning($"⚠️ Audio directory not found: {audioDir}");
                return null;
            }

            string sanitizedName = SanitizeFileName(dialogue.characterName);
            var voice = VoiceLibrary.GetVoiceById(dialogue.selectedVoiceId);

            // Look for files matching the pattern: dialogue_{index}_{character}_{voice}_*.mp3
            string searchPattern = $"dialogue_{dialogueIndex}_{sanitizedName}_{voice.voiceName}_*.mp3";
            Debug.Log($"🔍 Searching in: {audioDir}");
            Debug.Log($"🔍 Pattern: {searchPattern}");

            string[] files = System.IO.Directory.GetFiles(audioDir, searchPattern);

            if (files.Length > 0)
            {
                // Return the most recent file
                System.Array.Sort(files);
                string latestFile = files[files.Length - 1];
                Debug.Log($"✅ Found {files.Length} matching file(s), using: {System.IO.Path.GetFileName(latestFile)}");
                return latestFile;
            }

            // ✅ FALLBACK 1: Try searching for any file with this character name and dialogue index
            string fallbackPattern = $"dialogue_{dialogueIndex}_{sanitizedName}_*.mp3";
            Debug.Log($"🔍 Trying fallback pattern: {fallbackPattern}");

            files = System.IO.Directory.GetFiles(audioDir, fallbackPattern);
            if (files.Length > 0)
            {
                System.Array.Sort(files);
                string latestFile = files[files.Length - 1];
                Debug.LogWarning($"⚠️ Using fallback audio (voice mismatch): {System.IO.Path.GetFileName(latestFile)}");
                return latestFile;
            }

            // ✅ FALLBACK 2: Try any file with this dialogue index
            string indexPattern = $"dialogue_{dialogueIndex}_*.mp3";
            Debug.Log($"🔍 Trying index-only pattern: {indexPattern}");

            files = System.IO.Directory.GetFiles(audioDir, indexPattern);
            if (files.Length > 0)
            {
                System.Array.Sort(files);
                string latestFile = files[files.Length - 1];
                Debug.LogWarning($"⚠️ Using index-only fallback: {System.IO.Path.GetFileName(latestFile)}");
                return latestFile;
            }

            Debug.LogWarning($"❌ No audio file found for any pattern in: {audioDir}");
            return null;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Error finding audio file: {ex.Message}");
            return null;
        }
    }


string SanitizeFileName(string fileName)
{
    foreach (char c in System.IO.Path.GetInvalidFileNameChars())
    {
        fileName = fileName.Replace(c, '_');
    }
    return fileName;
}

string GetTeacherId()
{
    // Check current user role
    string userRole = PlayerPrefs.GetString("UserRole", "student");
    
    if (userRole.ToLower() == "student")
    {
        // For students, extract from story data
        string storyJson = StudentPrefs.GetString("CurrentStoryData", "");
        if (!string.IsNullOrEmpty(storyJson))
        {
            try
            {
                var studentStory = JsonUtility.FromJson<StoryData>(storyJson);
                if (!string.IsNullOrEmpty(studentStory.backgroundPath))
                {
                    string[] pathParts = studentStory.backgroundPath.Split(System.IO.Path.DirectorySeparatorChar);
                    if (pathParts.Length > 0)
                    {
                        return pathParts[0]; // First part is teacher ID
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Could not extract teacher ID: {ex.Message}");
            }
        }
    }
    
    // For teachers or fallback
    if (StoryManager.Instance != null && StoryManager.Instance.IsCurrentUserTeacher())
    {
        return StoryManager.Instance.GetCurrentTeacherId();
    }
    
    return TeacherPrefs.GetString("CurrentTeachId", "default");
}

    int GetStoryIndex()
    {
        // Try from current story
        var story = StoryManager.Instance?.GetCurrentStory();
        if (story != null && story.storyIndex >= 0)
        {
            return story.storyIndex;
        }

        // ✅ CRITICAL FIX: For students, extract from story data
        string storyJson = StudentPrefs.GetString("CurrentStoryData", "");
        if (!string.IsNullOrEmpty(storyJson))
        {
            try
            {
                var studentStory = JsonUtility.FromJson<StoryData>(storyJson);

                // ✅ FIX: If storyIndex is -1, extract from path
                if (studentStory.storyIndex < 0 && !string.IsNullOrEmpty(studentStory.backgroundPath))
                {
                    // Extract from path: "3U2VTjs7ng/story_0/background.png"
                    string[] pathParts = studentStory.backgroundPath.Split(new char[] { '/', '\\' }, System.StringSplitOptions.RemoveEmptyEntries);
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

                // Return the stored index if valid
                if (studentStory.storyIndex >= 0)
                {
                    return studentStory.storyIndex;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Could not extract story index: {ex.Message}");
            }
        }

        return 0;
    }

    IEnumerator PlayDialogueAudio(DialogueLine dialogue)
    {
        isPlayingAudio = true;
        
        // Show audio playing indicator
        if (skipAudioButton != null)
            skipAudioButton.gameObject.SetActive(true);
        if (audioPlayingIndicator != null)
            audioPlayingIndicator.SetActive(true);

        // Disable next/back buttons while audio plays (optional)
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

                // Wait for audio to finish or be skipped
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

        // Re-enable buttons
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
        // Stop current audio if playing
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
            Debug.Log("🏁 All dialogues completed, ready for quiz");
        }
    }

    public void PreviousDialogue()
    {
        // Stop current audio if playing
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

    // Keep all your existing methods below
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
            catch (System.Exception ex)
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

        Texture2D localTexture = ImageStorage.LoadImage(imagePath);
        if (localTexture != null)
        {
            Debug.Log($"✅ Loaded background from local storage: {imagePath}");
            ApplyBackgroundTexture(localTexture);
            yield break;
        }

        if (IsFirebaseStoragePath(imagePath))
        {
            Debug.Log($"🌐 Background is a Firebase Storage path, will download: {imagePath}");
            SetDownloadPlaceholder(backgroundImage, "Background");
            yield return StartCoroutine(DownloadImageFromFirebase(imagePath, backgroundImage, true));
        }
        else
        {
            Debug.Log($"⚠️ Background path not found locally and not a Firebase URL: {imagePath}");
            SetDefaultBackground();
        }
    }

    private IEnumerator LoadCharacterImage(string imagePath, RawImage characterImage, int characterNumber)
    {
        Debug.Log($"👤 Attempting to load character {characterNumber}: {imagePath}");

        Texture2D localTexture = ImageStorage.LoadImage(imagePath);
        if (localTexture != null)
        {
            Debug.Log($"✅ Loaded character {characterNumber} from local storage: {imagePath}");
            ApplyCharacterTexture(characterImage, localTexture);
            yield break;
        }

        if (IsFirebaseStoragePath(imagePath))
        {
            Debug.Log($"🌐 Character {characterNumber} is a Firebase Storage path, will download: {imagePath}");
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

        Debug.Log($"📥 Download placeholder set for: {firebasePath}");
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
                    Debug.Log($"✅ Loaded {studentStory.quizQuestions.Count} questions from StudentPrefs for quiz");
                    return;
                }
            }
            catch (System.Exception ex)
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
                Debug.Log($"✅ Loaded {story.quizQuestions?.Count ?? 0} questions from StoryManager for quiz");
            }
            else
            {
                Debug.LogWarning("⚠️ No quiz questions available for quiz scene");
            }
        }
    }

    void UpdateCharacterImages(string characterName)
    {
        // Simple character visibility logic - enhance based on your needs
        if (character1Image != null && character1Image.gameObject.activeInHierarchy)
        {
            // You could add logic here to highlight the speaking character
        }
    }
}