using System.Collections.Generic;
using UnityEngine;

public static class VoiceLibrary
{
    public static List<VoiceProfile> GetAvailableVoices()
    {
        return new List<VoiceProfile>
        {
            new VoiceProfile(
                "Tatang Luis Gonzales", // Choose
                "s2eGmuqaEBjTQPWzMapp",
                "Matandang lalaking may malalim na boses",
                "Male",
                "Filipino"
            ),
            new VoiceProfile(
                "Sebastian", // Choose
                "p7cwnUviDFhhX9y8sG2Q",
                "Batang boses na masigla at masayahin",
                "Male",
                "Filipino"
            ),
            new VoiceProfile(
                "Daniel",
                "onwK4e9ZLuTAKqWW03F9",
                "Lalaking boses na may malinis na bigkas",
                "Male",
                "Filipino"
            ),
            new VoiceProfile(
                "Nanay Avelina Gonzales", // Choose
                "HXiggO6rHDAxWaFMzhB7",
                "Matandang babae na may malambing na boses",
                "Female",
                "Filipino"
            ),
            new VoiceProfile(
                "Charlotte", 
                "XB0fDUnXU5powFXDhCwa",
                "Boses babae na pwedeng gamitin pang kwento",
                "Female",
                "Filipino"
            ),
            new VoiceProfile(
                "Aria", 
                "9BWtsMINqrJLrRacOk9x",
                "Pang-teenage na babae na may malinis na bigkas",
                "Female",
                "Filipino"
            )
        };
    }
    
    public static VoiceProfile GetVoiceById(string voiceId)
    {
        var voices = GetAvailableVoices();
        return voices.Find(v => v.voiceId == voiceId) ?? voices[0]; // Default to Rachel
    }
    
    public static VoiceProfile GetDefaultVoice()
    {
        return GetAvailableVoices()[0]; // Rachel
    }
}