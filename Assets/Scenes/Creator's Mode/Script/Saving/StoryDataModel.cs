using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StoryData
{
    public string storyId;          // unique ID
    public string storyTitle;       // display title
    public string storyDescription;  // Description
    public string backgroundPath;   // saved background image path

    public List<DialogueLine> dialogues;
    public List<Question> quizQuestions;

    public StoryData()
    {
        storyId = System.Guid.NewGuid().ToString();
        storyTitle = "";
        storyDescription = "";
        backgroundPath = "";
        dialogues = new List<DialogueLine>();
        quizQuestions = new List<Question>();
    }
}
