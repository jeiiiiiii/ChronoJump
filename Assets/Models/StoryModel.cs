using System;

[Serializable]
public class StoryModel
{
    public string storyId;
    public string codexId;
    public string title;
    public string description;

    public StoryModel() { }

    public StoryModel(string storyId, string codexId, string title, string description)
    {
        this.storyId = storyId;
        this.codexId = codexId;
        this.title = title;
        this.description = description;
    }
}
