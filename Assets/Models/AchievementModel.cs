using System;

[Serializable]
public class AchievementModel
{
    public string achievementId { get; set; }
    public string title { get; set; }
    public string description { get; set; }

    public AchievementModel() { }

    public AchievementModel(string achievementId, string title, string description)
    {
        this.achievementId = achievementId;
        this.title = title;
        this.description = description;
    }
}
