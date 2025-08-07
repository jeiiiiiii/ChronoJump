using System;

[Serializable]
public class GalleryItemModel
{
    public string galleryItemId { get; set; }
    public string galleryId { get; set; }
    public string image { get; set; }
    public string caption { get; set; }

    public GalleryItemModel() { }

    public GalleryItemModel(string galleryItemId, string galleryId, string image, string caption)
    {
        this.galleryItemId = galleryItemId;
        this.galleryId = galleryId;
        this.image = image;
        this.caption = caption;
    }
}
