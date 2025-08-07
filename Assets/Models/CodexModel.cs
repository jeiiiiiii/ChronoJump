using System;

[Serializable]
public class CodexModel
{
    public string codexId { get; set; }
    public string studId { get; set; }

    public CodexModel() { }

    public CodexModel(string codexId, string studId)
    {
        this.codexId = codexId;
        this.studId = studId;
    }
}
