using System;
using System.Collections.Generic;

[Serializable]
public class TeacherModel
{
    public string teachId { get; set; }
    public string userId { get; set; }
    public string teachFirstName { get; set; }
    public string teachLastName { get; set; }
    public string title { get; set; }
    public string teachProfileIcon { get; set; }
    public Dictionary<string, List<string>> classCode { get; set; }

    public TeacherModel() { }

    public TeacherModel(string teachId, string userId, string teachFirstName, string teachLastName, string title, string teachProfileIcon, Dictionary<string, List<string>> classCode)
    {
        this.teachId = teachId;
        this.userId = userId;
        this.teachFirstName = teachFirstName;
        this.teachLastName = teachLastName;
        this.title = title;
        this.teachProfileIcon = teachProfileIcon;
        this.classCode = classCode;
    }
}
