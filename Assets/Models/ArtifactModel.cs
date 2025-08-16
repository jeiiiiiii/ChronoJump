using System;

[Serializable]
public class ArtifactModel
{
    public string artifactId { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string historicalContext {get; set; }
    public string image { get; set; }

    public ArtifactModel() { }

    public ArtifactModel(string artifactId, string name, string description, string historicalContext, string image)
    {
        this.artifactId = artifactId;
        this.name = name;
        this.description = description;
        this.historicalContext = historicalContext;
        this.image = image;
    }
}
