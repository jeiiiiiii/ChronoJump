using System;

[Serializable]
public class AuditLogModel
{
    public string logId { get; set; }
    public string userId { get; set; }
    public string action { get; set; }
    public DateTime timeStamp { get; set; }

    public AuditLogModel() { }

    public AuditLogModel(string logId, string userId, string action, DateTime timeStamp)
    {
        this.logId = logId;
        this.userId = userId;
        this.action = action;
        this.timeStamp = timeStamp;
    }
}
