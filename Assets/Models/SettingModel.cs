using System;

[Serializable]
public class SettingsModel
{
    public string settingId { get; set; }  
    public string userId { get; set; }  
    public float soundVolume { get; set; }  
    public float musicVolume { get; set; }   
    public string displayMode { get; set; } 
    public string language { get; set; } 

    public SettingsModel() { }

    public SettingsModel(string settingId, string userId, float soundVolume, float musicVolume, string displayMode, string language)
    {
        this.settingId = settingId;
        this.userId = userId;
        this.soundVolume = soundVolume;
        this.musicVolume = musicVolume;
        this.displayMode = displayMode;
        this.language = language;
    }
}
