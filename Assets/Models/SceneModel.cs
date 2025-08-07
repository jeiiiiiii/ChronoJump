[Serializable]
public class SceneModel
{
    public string sceneId;
    public string chaptId;
    public string bgImage;
    public string sceneCoordinate;

    public SceneModel() { }

    public SceneModel(string sceneId, string chaptId, string bgImage, string sceneCoordinate)
    {
        this.sceneId = sceneId;
        this.chaptId = chaptId;
        this.bgImage = bgImage;
        this.sceneCoordinate = sceneCoordinate;
    }
}
