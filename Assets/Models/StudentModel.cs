using System;
using System.Collections.Generic;

[Serializable]
public class StudentModel
{
    public string studId { get; set; }
    public string userId { get; set; }
    public string studName { get; set; }
    public string teachId { get; set; }
    public string classCode { get; set; }
    public string studProfilePic { get; set; }
    public Dictionary<string, string> progress { get; set; }
    

    public StudentModel() { }

    public StudentModel(string studId, string userId, string teachId, string classCode, string studName, string studProfilePic)
    {
        this.studId = studId;
        this.userId = userId;
        this.teachId = teachId;
        this.classCode = classCode;
        this.studName = studName;
        this.studProfilePic = studProfilePic;
    }
}
