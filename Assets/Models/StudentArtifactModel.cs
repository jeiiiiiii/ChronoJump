using System;

[Serializable]
public class StudentArtifactModel
{
    public string studId { get; set; }
    public string artifactId { get; set; }

    public StudentArtifactModel() { }

    public StudentArtifactModel(string studId, string artifactId)
    {
        this.studId = studId;
        this.artifactId = artifactId;
    }
}
