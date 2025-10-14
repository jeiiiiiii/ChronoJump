using UnityEngine;
using UnityEngine.SceneManagement;

public class GameScene : MonoBehaviour
{
    void Start()
    {
        // ✅ FIXED: Only clear student data if teacher is viewing a story from StoryManager
        // Don't clear if teacher is properly viewing their own story
        string userRole = PlayerPrefs.GetString("UserRole", "student");

        if (userRole.ToLower() == "teacher")
        {
            // Only clear student data if StoryManager has a valid current story
            // This means teacher is viewing their own story, so student data should not interfere
            if (StoryManager.Instance?.currentStory != null)
            {
                string studentStoryData = StudentPrefs.GetString("CurrentStoryData", "");
                if (!string.IsNullOrEmpty(studentStoryData))
                {
                    Debug.LogWarning("⚠️ Teacher viewing story but student data exists - clearing student data");
                    // ✅ FIX: Only clear the full story data, NOT the SelectedStoryID
                    ClearStudentStoryDataButKeepStoryId();
                }
            }
        }
    }

    public void MainMenu()
    {
        string userRole = PlayerPrefs.GetString("UserRole", "student");
        Debug.Log("=== MainMenu Called ===");
        Debug.Log("UserRole from PlayerPrefs: " + userRole);

        // ✅ FIXED: Only clear student data if the current user is actually a student
        if (userRole.ToLower() == "student")
        {
            // ✅ FIX: Don't clear SelectedStoryID - we need it for quiz tracking!
            ClearStudentStoryDataButKeepStoryId();
        }

        // Check if user is a teacher or student and load appropriate scene
        if (IsTeacher())
        {
            Debug.Log("User is Teacher - Loading Creator'sModeScene");
            SceneManager.LoadScene("Creator'sModeScene");
        }
        else
        {
            Debug.Log("User is Student - Loading Classroom");
            SceneManager.LoadScene("Classroom");
        }
    }

    // ✅ FIXED: Only handle back button cleanup for students
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // Android back button
        {
            string userRole = PlayerPrefs.GetString("UserRole", "student");
            if (userRole.ToLower() == "student")
            {
                // ✅ FIX: Don't clear SelectedStoryID
                ClearStudentStoryDataButKeepStoryId();
            }

            // Navigate to appropriate scene
            if (IsTeacher())
            {
                SceneManager.LoadScene("Creator'sModeScene");
            }
            else
            {
                SceneManager.LoadScene("Classroom");
            }
        }
    }

    // ✅ FIXED: Only clear on destruction for students, but keep StoryID
    void OnDestroy()
    {
        string userRole = PlayerPrefs.GetString("UserRole", "student");
        if (userRole.ToLower() == "student")
        {
            // ✅ FIX: Don't clear SelectedStoryID - we need it for quiz!
            ClearStudentStoryDataButKeepStoryId();
            Debug.Log("🧹 Cleared student data on GameScene destruction (kept StoryID)");
        }
    }

    private bool IsTeacher()
    {
        string userRole = PlayerPrefs.GetString("UserRole", "student");
        return userRole.ToLower() == "teacher";
    }

    private void ClearStudentStoryData()
    {
        StudentPrefs.DeleteKey("CurrentStoryData");
        StudentPrefs.DeleteKey("SelectedStoryID");
        StudentPrefs.DeleteKey("SelectedStoryTitle");
        StudentPrefs.DeleteKey("PlayingFromClass");
        StudentPrefs.Save();
        Debug.Log("🧹 Cleared ALL student story data");
    }

    // ✅ NEW: Only clear the data that causes persistence issues, keep StoryID for quiz
    private void ClearStudentStoryDataButKeepStoryId()
    {
        // Save the StoryID first
        string savedStoryId = StudentPrefs.GetString("SelectedStoryID", "");
        string savedStoryTitle = StudentPrefs.GetString("SelectedStoryTitle", "");
        string savedClassCode = StudentPrefs.GetString("PlayingFromClass", "");

        // Clear the main story data that causes persistence
        StudentPrefs.DeleteKey("CurrentStoryData");
        StudentPrefs.Save();

        Debug.Log("🧹 Cleared CurrentStoryData but kept StoryID for quiz tracking");

        // Optional: You might want to clear these after quiz is completed
        // But for now, let's keep them until we're sure quiz is done
    }
}
