using System;

[Serializable]
public class ProgressTrackerModel
{
    public string trackerId { get; set; }
    public string studId { get; set; }
    public string currentSceneId { get; set; }
    public bool completionStatus { get; set; }

    public ProgressTrackerModel() { }

    public ProgressTrackerModel(string trackerId, string studId, string currentSceneId, bool completionStatus)
    {
        this.trackerId = trackerId;
        this.studId = studId;
        this.currentSceneId = currentSceneId;
        this.completionStatus = completionStatus;
    }
}
