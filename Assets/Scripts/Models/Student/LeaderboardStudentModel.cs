using NUnit.Framework;

[System.Serializable]
public class LeaderboardStudentModel
{
    public string studId;
    public string displayName;
    public int overallScore;
    public string classCode;
    public bool isRemoved;

    public LeaderboardStudentModel(string studId, string displayName, int overallScore, string classCode, bool isRemoved)
    {
        this.studId = studId;
        this.displayName = displayName;
        this.overallScore = overallScore;
        this.classCode = classCode;
        this.isRemoved = isRemoved;
    }
}