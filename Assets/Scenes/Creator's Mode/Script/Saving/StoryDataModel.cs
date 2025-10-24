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
    public int storyIndex;
    public int storyVersion = 1;


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
        storyIndex = -1;
        storyVersion = 1;
    }
}