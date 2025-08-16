using System;

[Serializable]
public class AIInteractionModel
{
    public string interactionId { get; set; }
    public string studId { get; set; }
    public string message { get; set; }
    public string response { get; set; }
    public DateTime timeStamp { get; set; }

    public AIInteractionModel() { }

    public AIInteractionModel(string interactionId, string studId, string message, string response, DateTime timeStamp)
    {
        this.interactionId = interactionId;
        this.studId = studId;
        this.message = message;
        this.response = response;
        this.timeStamp = timeStamp;
    }
}
