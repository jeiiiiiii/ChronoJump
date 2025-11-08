using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CharacterSetter : MonoBehaviour
{
    public RawImage characterImageOne;
    public RawImage characterImageTwo;

    void Start()
    {
        string userRole = PlayerPrefs.GetString("UserRole", "student");

        // ✅ FIX: Use coroutines for both student and teacher modes
        if (userRole.ToLower() == "student")
        {
            StartCoroutine(LoadStudentStoryCharacters());
        }
        else
        {
            StartCoroutine(LoadTeacherStoryCharacters());
        }
    }

    private IEnumerator LoadStudentStoryCharacters()
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

        yield return StartCoroutine(LoadCharactersAsync(story, "STUDENT"));
    }

    private IEnumerator LoadTeacherStoryCharacters()
    {
        // Load from StoryManager for teacher mode
        StoryData story = StoryManager.Instance?.currentStory;
        Debug.Log($"✅ CharacterSetter: Using TEACHER StoryManager - {story?.storyTitle}");

        yield return StartCoroutine(LoadCharactersAsync(story, "TEACHER"));
    }

    private IEnumerator LoadCharactersAsync(StoryData story, string mode)
    {
        if (story == null)
        {
            Debug.LogWarning($"❌ No current story found for {mode} CharacterSetter.");
            // Hide both character holders if no story
            characterImageOne.gameObject.SetActive(false);
            characterImageTwo.gameObject.SetActive(false);
            yield break;
        }

        bool hasCharacter1 = !string.IsNullOrEmpty(story.character1Path);
        bool hasCharacter2 = !string.IsNullOrEmpty(story.character2Path);

        Debug.Log($"🎭 Character Setup - {mode}: Char1={hasCharacter1}, Char2={hasCharacter2}");

        // ✅ SOLUTION 1: Handle single-character stories properly
        if (hasCharacter1 && !hasCharacter2)
        {
            // Single character story - load character 1 and hide character 2
            yield return StartCoroutine(LoadSingleCharacter(story.character1Path, characterImageOne, 1, mode));
            characterImageTwo.gameObject.SetActive(false);
            Debug.Log($"ℹ️ Single-character story detected - hiding character 2 holder");
        }
        else if (!hasCharacter1 && hasCharacter2)
        {
            // Single character story - load character 2 and hide character 1
            yield return StartCoroutine(LoadSingleCharacter(story.character2Path, characterImageTwo, 2, mode));
            characterImageOne.gameObject.SetActive(false);
            Debug.Log($"ℹ️ Single-character story detected - hiding character 1 holder");
        }
        else if (hasCharacter1 && hasCharacter2)
        {
            // Two-character story - load both characters
            yield return StartCoroutine(LoadSingleCharacter(story.character1Path, characterImageOne, 1, mode));
            yield return StartCoroutine(LoadSingleCharacter(story.character2Path, characterImageTwo, 2, mode));
            Debug.Log($"ℹ️ Two-character story detected - showing both character holders");
        }
        else
        {
            // No characters - hide both holders
            characterImageOne.gameObject.SetActive(false);
            characterImageTwo.gameObject.SetActive(false);
            Debug.LogWarning($"ℹ️ No character paths specified in {mode} story data - hiding both character holders");
        }
    }

    private IEnumerator LoadSingleCharacter(string characterPath, RawImage characterHolder, int characterIndex, string mode)
    {
        if (string.IsNullOrEmpty(characterPath))
        {
            Debug.LogWarning($"❌ No path specified for {mode} character {characterIndex}");
            characterHolder.gameObject.SetActive(false);
            yield break;
        }

        Debug.Log($"🔍 Attempting to load {mode} Character {characterIndex} from: {characterPath}");

        Texture2D characterTexture = null;

        if (ImageStorage.IsS3Url(characterPath))
        {
            // Handle S3 URLs with async download
            var downloadTask = ImageStorage.LoadImageAsync(characterPath);
            yield return new WaitUntil(() => downloadTask.IsCompleted);

            characterTexture = downloadTask.Result;

            if (characterTexture != null)
            {
                Debug.Log($"✅ SUCCESS: Loaded {mode} character {characterIndex} from S3: {characterPath}");
            }
            else
            {
                Debug.LogWarning($"❌ FAILED: Could not load {mode} character {characterIndex} from S3: {characterPath}");
            }
        }
        else
        {
            // Handle local files
            characterTexture = ImageStorage.LoadImage(characterPath);
            if (characterTexture != null)
            {
                Debug.Log($"✅ SUCCESS: Loaded {mode} character {characterIndex} from local: {characterPath}");
            }
            else
            {
                Debug.LogWarning($"❌ FAILED: Could not load {mode} character {characterIndex} from local: {characterPath}");
            }
        }

        // Set the character image or hide if failed
        if (characterTexture != null)
        {
            SetCharacterImage(characterHolder, characterTexture);
        }
        else
        {
            characterHolder.gameObject.SetActive(false);
            Debug.LogWarning($"❌ Failed to load texture for {mode} character {characterIndex}");
        }
    }

    private void SetCharacterImage(RawImage image, Texture2D texture)
    {
        if (texture == null)
        {
            Debug.LogWarning("⚠️ Attempted to set null texture to character image");
            image.gameObject.SetActive(false);
            return;
        }

        image.texture = texture;

        // Fix aspect ratio
        var fitter = image.GetComponent<AspectRatioFitter>();
        if (fitter != null)
        {
            fitter.aspectRatio = (float)texture.width / texture.height;
            Debug.Log($"📐 Set aspect ratio for character: {texture.width}x{texture.height} = {fitter.aspectRatio:F2}");
        }

        image.gameObject.SetActive(true);
        Debug.Log($"👤 Character image set successfully: {texture.width}x{texture.height}");
    }
}
