using System;

[Serializable]
public class StudentAchievementModel
{
    public string studId { get; set; }
    public string achievementId { get; set; }

    public StudentAchievementModel() { }

    public StudentAchievementModel(string studId, string achievementId)
    {
        this.studId = studId;
        this.achievementId = achievementId;
    }
}
