using System;

[Serializable]
public class LeaderboardModel
{
    public string leaderboardId { get; set; }
    public string name { get; set; }
    public string description { get; set; }

    public LeaderboardModel() { }

    public LeaderboardModel(string leaderboardId, string name, string description)
    {
        this.leaderboardId = leaderboardId;
        this.name = name;
        this.description = description;
    }
}
