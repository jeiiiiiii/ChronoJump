using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VoiceProfile
{
    public string voiceName;
    public string voiceId;
    public string description;
    public string gender; // "Male", "Female", "Neutral"
    public string accent; // "American", "British", etc.
    
    public VoiceProfile(string name, string id, string desc, string gender, string accent)
    {
        this.voiceName = name;
        this.voiceId = id;
        this.description = desc;
        this.gender = gender;
        this.accent = accent;
    }
}