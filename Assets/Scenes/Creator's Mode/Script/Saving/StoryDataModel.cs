using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StoryData
{
    public string storyId;          // unique ID
    public string storyTitle;       // display title
    public string backgroundPath;   // saved background image path

    public List<DialogueLine> dialogues;
    public List<Question> quizQuestions;

    // ✅ Required for JsonUtility (empty constructor)
    public StoryData()
    {
        storyId = System.Guid.NewGuid().ToString();
        storyTitle = "Untitled Story";
        backgroundPath = "";
        dialogues = new List<DialogueLine>();
        quizQuestions = new List<Question>();
    }

    // ✅ Optional constructor if you want to initialize quickly
    public StoryData(string id, string title)
    {
        storyId = string.IsNullOrEmpty(id) ? System.Guid.NewGuid().ToString() : id;
        storyTitle = string.IsNullOrEmpty(title) ? "Untitled Story" : title;
        backgroundPath = "";
        dialogues = new List<DialogueLine>();
        quizQuestions = new List<Question>();
    }
}
