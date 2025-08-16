using System;

[Serializable]
public class DialogueModel
{
    public string dialogueId { get; set; }
    public string sceneId { get; set; }
    public string speaker { get; set; }
    public string text { get; set; }
    public string emotion { get; set; }

    public DialogueModel() { }

    public DialogueModel(string dialogueId, string sceneId, string speaker, string text, string emotion)
    {
        this.dialogueId = dialogueId;
        this.sceneId = sceneId;
        this.speaker = speaker;
        this.text = text;
        this.emotion = emotion;
    }
}
