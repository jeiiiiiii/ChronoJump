using System;

[Serializable]
public class CharacterProfileModel
{
    public string charProfId { get; set; }
    public string charRole { get; set; }
    public string charImage { get; set; }
    public string charBio { get; set; }

    public CharacterProfileModel() { }

    public CharacterProfileModel(string charProfId, string charRole, string charImage, string charBio)
    {
        this.charProfId = charProfId;
        this.charRole = charRole;
        this.charImage = charImage;
        this.charBio = charBio;
    }
}
