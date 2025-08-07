using System;

[Serializable]
public class ChoiceModel
{
    public string choiceId { get; set; }
    public string sceneId { get; set; }
    public string text { get; set; }
    public string consequence { get; set; }

    public ChoiceModel() { }

    public ChoiceModel(string choiceId, string sceneId, string text, string consequence)
    {
        this.choiceId = choiceId;
        this.sceneId = sceneId;
        this.text = text;
        this.consequence = consequence;
    }
}
