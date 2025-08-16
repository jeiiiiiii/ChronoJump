using System;

[Serializable]
public class TeacherModel
{
    public string teachId { get; set; }
    public string userId { get; set; }
    public string teachName { get; set; }

    public TeacherModel() { }

    public TeacherModel(string teachId, string userId, string teachName)
    {
        this.teachId = teachId;
        this.userId = userId;
        this.teachName = teachName;
    }
}
