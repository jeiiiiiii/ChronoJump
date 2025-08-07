using System;

[Serializable]
public class LeaderboardEntryModel
{
    public string entryId { get; set; }
    public string leaderboardId { get; set; }
    public string studId { get; set; }
    public float score { get; set; }
    public int rank { get; set; }

    public LeaderboardEntryModel() { }

    public LeaderboardEntryModel(string entryId, string leaderboardId, string studId, float score, int rank)
    {
        this.entryId = entryId;
        this.leaderboardId = leaderboardId;
        this.studId = studId;
        this.score = score;
        this.rank = rank;
    }
}
