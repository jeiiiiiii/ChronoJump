using System.Collections.Generic;

[System.Serializable]
public class StoryData
{
    public string storyId;
    public string storyTitle;
    public string storyDescription;
    public string backgroundPath;
    public string character1Path;
    public string character2Path;
    public List<DialogueLine> dialogues;
    public List<Question> quizQuestions;
    
    // NEW: For Firestore integration
    public List<string> assignedClasses;
    public string createdAt;
    public string updatedAt;
    
    // ✅ NEW: Add index for both Unity and Firestore
    public int storyIndex;

    public StoryData()
    {
        storyId = System.Guid.NewGuid().ToString();
        storyTitle = "";
        storyDescription = "";
        backgroundPath = "";
        character1Path = "";
        character2Path = "";
        dialogues = new List<DialogueLine>();
        quizQuestions = new List<Question>();
        assignedClasses = new List<string>();
        createdAt = System.DateTime.Now.ToString();
        updatedAt = System.DateTime.Now.ToString();
        storyIndex = -1; // Default to -1 (will be set when added to list)
    }
}