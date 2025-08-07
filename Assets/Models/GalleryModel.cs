using System;

[Serializable]
public class GalleryModel
{
    public string galleryId { get; set; }
    public string studId { get; set; }

    public GalleryModel() { }

    public GalleryModel(string galleryId, string studId)
    {
        this.galleryId = galleryId;
        this.studId = studId;
    }
}
