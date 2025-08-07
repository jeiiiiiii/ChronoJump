using System;

[Serializable]
public class StudentModel
{
    public string studId { get; set; }
    public string userId { get; set; }
    public string studName { get; set; }

    public StudentModel() { }

    public StudentModel(string studId, string userId, string studName)
    {
        this.studId = studId;
        this.userId = userId;
        this.studName = studName;
    }
}
