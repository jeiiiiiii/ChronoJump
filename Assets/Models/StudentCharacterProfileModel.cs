using System;

[Serializable]
public class StudentCharacterProfileModel
{
    public string studId { get; set; }
    public string charProfId { get; set; }

    public StudentCharacterProfileModel() { }

    public StudentCharacterProfileModel(string studId, string charProfId)
    {
        this.studId = studId;
        this.charProfId = charProfId;
    }
}
