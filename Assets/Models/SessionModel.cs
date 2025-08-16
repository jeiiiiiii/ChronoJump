using System;

[Serializable]
public class SessionModel
{
    public string sessionId { get; set; }
    public string userId { get; set; }
    public string token { get; set; }
    public DateTime loginAt { get; set; }
    public DateTime expiresAt { get; set; }

    public SessionModel() { }

    public SessionModel(string sessionId, string userId, string token, DateTime loginAt, DateTime expiresAt)
    {
        this.sessionId = sessionId;
        this.userId = userId;
        this.token = token;
        this.loginAt = loginAt;
        this.expiresAt = expiresAt;
    }
}
