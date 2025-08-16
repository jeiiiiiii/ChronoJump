using System;
using System.Collections.Generic;
using UnityEditor.SearchService;

[Serializable]
public class ChapterModel
{
    public string chaptId { get; set; }
    public string storyId { get; set; }
    public string chaptTitle { get; set; }
    public string chaptDescript { get; set; }
    public List<SceneModel> scenes { get; set; }

    public ChapterModel() { }

    public ChapterModel(string chaptId, string storyId, string chaptTitle, string chaptDescript, List<SceneModel> scenes)
    {
        this.chaptId = chaptId;
        this.storyId = storyId;
        this.chaptTitle = chaptTitle;
        this.chaptDescript = chaptDescript;
        this.scenes = scenes ?? new List<SceneModel>();
    }
}
