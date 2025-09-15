using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance;

    // Main list of stories
    public List<StoryData> allStories = new List<StoryData>();

    // ✅ Alias so other scripts using .stories will still work
    public List<StoryData> stories => allStories;

    public int currentStoryIndex = -1;   // index of the story being edited/viewed

    public StoryData currentStory
    {
        get => GetCurrentStory();
        set
        {
            if (value == null)
            {
                currentStoryIndex = -1;
                return;
            }

            int idx = allStories.IndexOf(value);
            if (idx >= 0)
            {
                currentStoryIndex = idx;
            }
            else
            {
                allStories.Add(value);
                currentStoryIndex = allStories.Count - 1;
            }
        }
    }
    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadStories(); // auto-load at startup
            Debug.Log("✅ StoryManager initialized and stories loaded.");
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("StoryManager");
            Instance = go.AddComponent<StoryManager>();
            DontDestroyOnLoad(go);
            Debug.Log("✅ StoryManager auto-created.");
        }
    }

    // --- Helpers ---
    public StoryData GetCurrentStory()
    {
        if (currentStoryIndex >= 0 && currentStoryIndex < allStories.Count)
            return allStories[currentStoryIndex];
        return null;
    }

    public bool SetCurrentStoryByIndex(int index)
    {
        if (index >= 0 && index < allStories.Count)
        {
            currentStoryIndex = index;
            Debug.Log($"📖 Current story set by index: {allStories[index].storyTitle}");
            return true;
        }
        Debug.LogWarning($"SetCurrentStoryByIndex failed: index {index} out of range (count={allStories.Count})");
        return false;
    }

    // ✅ Explicit setter for story objects
    public void SetCurrentStory(StoryData story)
    {
        if (story == null)
        {
            Debug.LogError("⚠ Tried to set a null story as current!");
            currentStoryIndex = -1;
            return;
        }

        int idx = allStories.IndexOf(story);
        if (idx >= 0)
        {
            currentStoryIndex = idx;
        }
        else
        {
            allStories.Add(story);
            currentStoryIndex = allStories.Count - 1;
        }

        Debug.Log($"📖 Current story set to: {story.storyTitle}");
    }

    // ✅ Uses parameterless StoryData constructor now
    public StoryData CreateNewStory(string title = null)
    {
        var s = new StoryData();

        if (!string.IsNullOrEmpty(title))
            s.storyTitle = title;
        else
            s.storyTitle = "";

        allStories.Add(s);
        currentStoryIndex = allStories.Count - 1;

        Debug.Log($"✅ Created new story: '{(string.IsNullOrEmpty(s.storyTitle) ? "<empty>" : s.storyTitle)}' (Index: {currentStoryIndex})");
        return s;
    }

    // ✅ Debug method to check current state
    public void DebugCurrentState()
    {
        Debug.Log("🔍 === StoryManager Debug Info ===");
        Debug.Log($"🔍 Total Stories: {allStories.Count}");
        Debug.Log($"🔍 Current Story Index: {currentStoryIndex}");
        Debug.Log($"🔍 Current Story: {(currentStory != null ? currentStory.storyTitle : "NULL")}");
        
        for (int i = 0; i < allStories.Count; i++)
        {
            Debug.Log($"🔍 Story {i}: {allStories[i].storyTitle}");
        }
        Debug.Log("🔍 === End Debug Info ===");
    }

    // ✅ Auto-select first story if none is selected
    public bool EnsureCurrentStoryExists()
    {
        if (currentStory == null && allStories.Count > 0)
        {
            SetCurrentStoryByIndex(0);
            Debug.Log($"⚠️ No current story set, auto-selected: {currentStory.storyTitle}");
            return true;
        }
        return currentStory != null;
    }

    // --- SAVE & LOAD STORIES (JSON) ---
    public void SaveStories()
    {
        string json = JsonUtility.ToJson(new StoryListWrapper { stories = allStories }, true);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "stories.json"), json);
        Debug.Log("✅ Stories saved to " + Path.Combine(Application.persistentDataPath, "stories.json"));
    }

    public void LoadStories()
    {
        string path = Path.Combine(Application.persistentDataPath, "stories.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            StoryListWrapper wrapper = JsonUtility.FromJson<StoryListWrapper>(json);
            allStories = wrapper.stories ?? new List<StoryData>();
            Debug.Log($"✅ Loaded {allStories.Count} stories");
        }
        else
        {
            allStories = new List<StoryData>();
            Debug.Log("⚠️ No saved stories found, starting fresh.");
        }
    }

    [System.Serializable]
    private class StoryListWrapper
    {
        public List<StoryData> stories;
    }

    // --- BACKGROUND HANDLING ---
    public string SaveBackground(Texture2D tex, string storyId = null)
    {
        string folder = Path.Combine(Application.persistentDataPath, "Backgrounds");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        var story = GetCurrentStory();
        string id = storyId ?? story?.storyId ?? System.Guid.NewGuid().ToString();

        string filePath = Path.Combine(folder, id + ".png");
        File.WriteAllBytes(filePath, tex.EncodeToPNG());

        if (story != null)
            story.backgroundPath = filePath;

        Debug.Log("✅ Background saved to: " + filePath);
        return filePath;
    }

    public Texture2D LoadBackground(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            Debug.LogWarning("⚠️ Background not found: " + filePath);
            return null;
        }

        byte[] bytes = File.ReadAllBytes(filePath);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        return tex;
    }
}