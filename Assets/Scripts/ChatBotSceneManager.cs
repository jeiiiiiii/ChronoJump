using UnityEngine;
using UnityEngine.SceneManagement;

public class ChatBotSceneManager : MonoBehaviour
{
    [Header("Scene Names")]
    public string chatBotSceneName = "ChatBotScene"; // Name of your AI chatbot scene
    
    private static string previousSceneName; // Stores which scene we came from
    
    void Start()
    {
        // Store the current scene name when this script starts
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log("Current scene: " + currentScene);
    }
    
    // Call this method when clicking the "Open ChatBot" button
    public void OpenChatBot()
    {
        // Remember which scene we're coming from
        previousSceneName = SceneManager.GetActiveScene().name;
        Debug.Log("Switching from " + previousSceneName + " to " + chatBotSceneName);
        
        // Load the chatbot scene
        SceneManager.LoadScene(chatBotSceneName);
    }
    
    // Call this method when clicking the "Close/X" button in the chatbot scene
    public void CloseChatBot()
    {
        if (string.IsNullOrEmpty(previousSceneName))
        {
            Debug.LogWarning("No previous scene stored! Loading default scene...");
            // If no previous scene is stored, load a default scene
            SceneManager.LoadScene(0); // Loads the first scene in build settings
        }
        else
        {
            Debug.Log("Returning to " + previousSceneName);
            SceneManager.LoadScene(previousSceneName);
        }
    }
    
    // Alternative: Go to a specific scene (if you know the exact scene name)
    public void GoToScene(string sceneName)
    {
        previousSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(sceneName);
    }
    
    // Get the name of the previous scene (useful for debugging)
    public string GetPreviousSceneName()
    {
        return previousSceneName;
    }
}