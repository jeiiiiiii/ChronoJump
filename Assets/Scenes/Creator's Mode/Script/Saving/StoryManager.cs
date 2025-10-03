using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Firebase.Firestore;

[System.Serializable]
public class PublishedStory
{
    public string storyId;
    public string storyTitle;
    public string classCode;
    public string className;
    public string publishDate;
    
    public PublishedStory(StoryData story, string classCode, string className)
    {
        this.storyId = story.storyId;
        this.storyTitle = story.storyTitle;
        this.classCode = classCode;
        this.className = className;
        this.publishDate = System.DateTime.Now.ToString("MMM dd, yyyy");
    }
}

public class StoryManager : MonoBehaviour
{
    // Main list of stories
    public List<StoryData> allStories = new List<StoryData>();
    public List<PublishedStory> publishedStories = new List<PublishedStory>();
    public List<StoryData> stories => allStories;
    public int currentStoryIndex = -1;

    // NEW: Firebase integration
    private CreatorModeService _creatorModeService;
    public bool UseFirestore { get; private set; } = false;
    public bool IsFirebaseReady => FirebaseManager.Instance != null &&
                                  FirebaseManager.Instance.CurrentUser != null;

    // NEW: Teacher-specific storage
    private string _currentTeachId;
    private TeacherModel _currentTeacher;

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

    private static StoryManager _instance;
    public static StoryManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to find existing instance in scene
                _instance = FindFirstObjectByType<StoryManager>();
                
                if (_instance == null)
                {
                    // Create new instance
                    GameObject go = new GameObject("StoryManager");
                    _instance = go.AddComponent<StoryManager>();
                    DontDestroyOnLoad(go);
                    
                    // Initialize
                    _instance.InitializeFirebaseIntegration();
                    _instance.LoadStories();
                    Debug.Log("✅ StoryManager created and initialized");
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeFirebaseIntegration();
            LoadStories();
            Debug.Log("✅ StoryManager initialized and stories loaded.");
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"⚠️ Destroying duplicate StoryManager. Existing: {_instance.gameObject.name}, New: {gameObject.name}");
            Destroy(gameObject);
        }
    }

    // NEW: Initialize Firebase integration
    private void InitializeFirebaseIntegration()
    {
        if (FirebaseManager.Instance != null)
        {
            _creatorModeService = new CreatorModeService(FirebaseManager.Instance.GetFirebaseService());
            UseFirestore = true;
            Debug.Log("✅ StoryManager: Firestore mode enabled");

            // Load teacher data if user is logged in
            if (FirebaseManager.Instance.CurrentUser != null)
            {
                LoadTeacherData();
            }
        }
        else
        {
            Debug.Log("ℹ️ StoryManager: Local JSON mode (Firebase not available)");
        }
    }

    private static void AutoCreate()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("StoryManager");
            _instance = go.AddComponent<StoryManager>();
            DontDestroyOnLoad(go);
            Debug.Log("✅ StoryManager auto-created.");
        }
    }

    // NEW: Teacher-specific methods
    private void LoadTeacherData()
    {
        if (FirebaseManager.Instance.CurrentUser == null) return;

        FirebaseManager.Instance.GetTeacherData(FirebaseManager.Instance.CurrentUser.UserId, teacher =>
        {
            if (teacher != null)
            {
                _currentTeacher = teacher;
                _currentTeachId = teacher.teachId;
                Debug.Log($"✅ Teacher data loaded: {teacher.teachFirstName} {teacher.teachLastName} (ID: {teacher.teachId})");

                // Update TeacherPrefs
                TeacherPrefs.SetString("CurrentTeachId", _currentTeachId);
                TeacherPrefs.Save();

                // Reload stories for this teacher
                LoadStories();
            }
            else
            {
                Debug.LogWarning("⚠️ No teacher data found for current user");
            }
        });
    }

    public string GetCurrentTeacherId()
    {
        // Priority 1: Use loaded teacher data
        if (!string.IsNullOrEmpty(_currentTeachId))
        {
            return _currentTeachId;
        }

        // Priority 2: Use TeacherPrefs
        if (TeacherPrefs.HasKey("CurrentTeachId"))
        {
            return TeacherPrefs.GetString("CurrentTeachId");
        }

        // Priority 3: Fallback to Firebase user ID
        if (FirebaseManager.Instance?.CurrentUserData?.role == "teacher" &&
            FirebaseManager.Instance.CurrentUser != null)
        {
            return FirebaseManager.Instance.CurrentUser.UserId;
        }

        return "default";
    }

    // NEW: Get teacher-specific base directory
    private string GetTeacherBaseDirectory()
    {
        string teachId = GetCurrentTeacherId();
        string safeTeachId = string.IsNullOrEmpty(teachId) ? "default" : teachId.Replace("/", "_").Replace("\\", "_");
        return Path.Combine(Application.persistentDataPath, safeTeachId);
    }

    public bool IsCurrentUserTeacher()
    {
        return FirebaseManager.Instance?.CurrentUserData?.role == "teacher" ||
               !string.IsNullOrEmpty(_currentTeachId);
    }

    public TeacherModel GetCurrentTeacher()
    {
        return _currentTeacher;
    }

    public void SetCurrentTeacher(string teachId)
    {
        if (!string.IsNullOrEmpty(teachId))
        {
            _currentTeachId = teachId;
            TeacherPrefs.SetString("CurrentTeachId", teachId);
            TeacherPrefs.Save();
            Debug.Log($"✅ Current teacher set to: {teachId}");

            // Reload stories for this teacher
            LoadStories();
        }
    }

    public void ClearCurrentTeacher()
    {
        _currentTeachId = null;
        _currentTeacher = null;
        TeacherPrefs.DeleteKey("CurrentTeachId");
        TeacherPrefs.Save();
        allStories.Clear();
        currentStoryIndex = -1;
        Debug.Log("✅ Teacher data cleared");
    }

    // --- EXISTING METHODS (UPDATED FOR TEACHER-SPECIFIC STORAGE) ---
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

    public StoryData CreateNewStory(string title = null)
    {
        var s = new StoryData();

        if (!string.IsNullOrEmpty(title))
            s.storyTitle = title;
        else
            s.storyTitle = "";

        allStories.Add(s);
        currentStoryIndex = allStories.Count - 1;

        SaveStories(); // ✅ Save immediately

        Debug.Log($"✅ Created new story: '{(string.IsNullOrEmpty(s.storyTitle) ? "<empty>" : s.storyTitle)}' (Index: {currentStoryIndex})");
        return s;
    }

    public void DebugCurrentState()
    {
        Debug.Log("🔍 === StoryManager Debug Info ===");
        Debug.Log($"🔍 Total Stories: {allStories.Count}");
        Debug.Log($"🔍 Current Story Index: {currentStoryIndex}");
        Debug.Log($"🔍 Current Story: {(currentStory != null ? currentStory.storyTitle : "NULL")}");
        Debug.Log($"🔍 Current Teacher ID: {GetCurrentTeacherId()}");
        Debug.Log($"🔍 Using Firestore: {UseFirestore}");
        Debug.Log($"🔍 Teacher Directory: {GetTeacherBaseDirectory()}");

        for (int i = 0; i < allStories.Count; i++)
        {
            Debug.Log($"🔍 Story {i}: {allStories[i].storyTitle}");
        }
        Debug.Log("🔍 === End Debug Info ===");
    }

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

    // --- UPDATED SAVE & LOAD STORIES (TEACHER-SPECIFIC) ---
    public async void SaveStories()
{
    // Always save locally for backup
    SaveStoriesLocal();
    
    Debug.Log("💾 Stories saved locally - Firestore save requires explicit 'Save & Publish'");
    
    // ❌ REMOVED: The automatic Firestore save
    // Only save to Firestore when explicitly called via SaveCurrentStoryToFirestore()
}

    public async void LoadStories()
    {
        // Try Firestore first if available
        if (UseFirestore && _creatorModeService != null)
        {
            bool success = await _creatorModeService.LoadStoriesFromFirestore();
            if (success)
            {
                Debug.Log("✅ Stories loaded from Firestore");
                LoadPublishedStories(); // Load published stories separately
                return;
            }
        }

        // Fallback to local loading
        LoadStoriesLocal();
        LoadPublishedStories();
    }

    // NEW: Teacher-specific local storage methods with proper directory structure
    private string GetTeacherStoriesFilePath()
    {
        string baseDir = GetTeacherBaseDirectory();
        string teacherStoriesDir = Path.Combine(baseDir, "TeacherStories");

        if (!Directory.Exists(teacherStoriesDir))
        {
            Directory.CreateDirectory(teacherStoriesDir);
        }

        return Path.Combine(teacherStoriesDir, "stories.json");
    }

    private string GetTeacherPublishedStoriesFilePath()
    {
        string baseDir = GetTeacherBaseDirectory();
        string teacherStoriesDir = Path.Combine(baseDir, "TeacherStories");

        if (!Directory.Exists(teacherStoriesDir))
        {
            Directory.CreateDirectory(teacherStoriesDir);
        }

        return Path.Combine(teacherStoriesDir, "published_stories.json");
    }

    private void SaveStoriesLocal()
    {
        string filePath = GetTeacherStoriesFilePath();
        string json = JsonUtility.ToJson(new StoryListWrapper { stories = allStories }, true);
        File.WriteAllText(filePath, json);
        Debug.Log($"✅ Stories saved locally for teacher: {GetCurrentTeacherId()}");
        Debug.Log($"📁 File: {filePath}");
    }

    private void LoadStoriesLocal()
    {
        string filePath = GetTeacherStoriesFilePath();
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            StoryListWrapper wrapper = JsonUtility.FromJson<StoryListWrapper>(json);
            allStories = wrapper.stories ?? new List<StoryData>();
            Debug.Log($"Loaded {allStories.Count} stories for teacher: {GetCurrentTeacherId()}");
            Debug.Log($"📁 File: {filePath}");
        }
        else
        {
            allStories = new List<StoryData>();
            Debug.Log($"No saved stories found for teacher: {GetCurrentTeacherId()}, starting fresh.");
            Debug.Log($"📁 Expected file: {filePath}");
        }
    }

    [System.Serializable]
    private class StoryListWrapper
    {
        public List<StoryData> stories;
    }

    // --- UPDATED BACKGROUND HANDLING (TEACHER-SPECIFIC) ---
    public string SaveBackground(Texture2D tex, string storyId = null)
    {
        // Get teacher-specific background directory
        string baseDir = GetTeacherBaseDirectory();
        string backgroundsDir = Path.Combine(baseDir, "Backgrounds");
        
        if (!Directory.Exists(backgroundsDir))
        {
            Directory.CreateDirectory(backgroundsDir);
        }

        var story = GetCurrentStory();
        string id = storyId ?? story?.storyId ?? System.Guid.NewGuid().ToString();

        string filePath = Path.Combine(backgroundsDir, id + ".png");
        File.WriteAllBytes(filePath, tex.EncodeToPNG());

        if (story != null)
        {
            story.backgroundPath = filePath;
            SaveStories();
        }

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

    // --- PUBLISHED STORIES (UPDATED FOR TEACHER-SPECIFIC STORAGE) ---
    public bool PublishStory(StoryData story, string classCode, string className)
    {
        // Prevent duplicate by storyId and classCode
        var existing = publishedStories.Find(p => p.storyId == story.storyId && p.classCode == classCode);
        if (existing != null)
        {
            Debug.Log($"Story '{story.storyTitle}' is already published to class {className}");
            return false;
        }

        // Prevent duplicate by title and classCode (optional)
        var existingTitle = publishedStories.Find(p => p.storyTitle == story.storyTitle && p.classCode == classCode);
        if (existingTitle != null)
        {
            Debug.Log($"A story with the title '{story.storyTitle}' is already published to class {className}");
            return false;
        }

        var publishedStory = new PublishedStory(story, classCode, className);
        publishedStories.Add(publishedStory);
        SavePublishedStories();

        Debug.Log($"Published story '{story.storyTitle}' to class {className}");
        return true;
    }

    public void DeletePublishedStory(string storyId, string classCode)
    {
        publishedStories.RemoveAll(p => p.storyId == storyId && p.classCode == classCode);
        SavePublishedStories();
        Debug.Log($"Deleted published story with ID: {storyId}");
    }

    public List<PublishedStory> GetPublishedStoriesForClass(string classCode)
    {
        return publishedStories.FindAll(p => p.classCode == classCode);
    }

    private void SavePublishedStories()
    {
        string filePath = GetTeacherPublishedStoriesFilePath();
        string json = JsonUtility.ToJson(new PublishedStoryWrapper { publishedStories = publishedStories }, true);
        File.WriteAllText(filePath, json);
        Debug.Log($"Published stories saved for teacher: {GetCurrentTeacherId()}");
        Debug.Log($"📁 File: {filePath}");
    }

    private void LoadPublishedStories()
    {
        string filePath = GetTeacherPublishedStoriesFilePath();
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            PublishedStoryWrapper wrapper = JsonUtility.FromJson<PublishedStoryWrapper>(json);
            publishedStories = wrapper.publishedStories ?? new List<PublishedStory>();
            Debug.Log($"Loaded {publishedStories.Count} published stories for teacher: {GetCurrentTeacherId()}");
            Debug.Log($"📁 File: {filePath}");
        }
        else
        {
            publishedStories = new List<PublishedStory>();
            Debug.Log($"No published stories found for teacher: {GetCurrentTeacherId()}");
            Debug.Log($"📁 Expected file: {filePath}");
        }
    }

    [System.Serializable]
    private class PublishedStoryWrapper
    {
        public List<PublishedStory> publishedStories;
    }

    // NEW: Explicit Firestore operations for manual control
    public async void SaveCurrentStoryToFirestore()
    {
        if (!IsFirebaseReady || currentStory == null) return;

        try
        {
            bool success = await _creatorModeService.SaveStoryToFirestore(currentStory);
            if (success)
            {
                Debug.Log($"✅ Story '{currentStory.storyTitle}' saved to Firestore");
            }
            else
            {
                Debug.LogError($"❌ Failed to save story to Firestore");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to save story to Firestore: {ex.Message}");
        }
    }

    public async void LoadStoriesFromFirestore()
    {
        if (!IsFirebaseReady) return;

        try
        {
            bool success = await _creatorModeService.LoadStoriesFromFirestore();
            if (success)
            {
                Debug.Log("✅ Stories loaded from Firestore");
            }
            else
            {
                Debug.LogError("❌ Failed to load stories from Firestore");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to load stories from Firestore: {ex.Message}");
        }
    }

    // NEW: Debug method to show file structure
    [ContextMenu("Debug File Structure")]
    public void DebugFileStructure()
    {
        string baseDir = GetTeacherBaseDirectory();
        Debug.Log($"📁 TEACHER FILE STRUCTURE FOR: {GetCurrentTeacherId()}");
        Debug.Log($"📁 Base Directory: {baseDir}");

        if (Directory.Exists(baseDir))
        {
            string[] directories = Directory.GetDirectories(baseDir);
            foreach (string dir in directories)
            {
                Debug.Log($"   📂 {Path.GetFileName(dir)}/");
                string[] files = Directory.GetFiles(dir);
                foreach (string file in files)
                {
                    Debug.Log($"      📄 {Path.GetFileName(file)}");
                }
            }
        }
        else
        {
            Debug.Log("   📁 Directory does not exist yet");
        }
    }
}