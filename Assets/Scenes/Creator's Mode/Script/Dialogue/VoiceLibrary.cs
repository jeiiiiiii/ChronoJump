using System.Collections.Generic;
using UnityEngine;

public static class VoiceLibrary
{
    // ✅ NEW: Special voice profile for "No Voice" option
    private static VoiceProfile noVoiceProfile = new VoiceProfile(
        "No Voice",
        "", // Empty voice ID
        "No voice will be used for this dialogue",
        "None",
        "None"
    );

    public static List<VoiceProfile> GetAvailableVoices()
    {
        return new List<VoiceProfile>
        {
            new VoiceProfile(
                "Tatang Luis Gonzales",
                "s2eGmuqaEBjTQPWzMapp",
                "Matandang lalaking may malalim na boses",
                "Male",
                "Filipino"
            ),
            new VoiceProfile(
                "Sebastian",
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
                "Nanay Avelina Gonzales",
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

    // ✅ FIXED: Return null or no-voice profile instead of defaulting to first voice
    public static VoiceProfile GetVoiceById(string voiceId)
    {
        // Handle empty/null voice ID - return the "No Voice" profile
        if (string.IsNullOrEmpty(voiceId))
        {
            return noVoiceProfile;
        }

        var voices = GetAvailableVoices();
        var foundVoice = voices.Find(v => v.voiceId == voiceId);

        // If voice ID not found, return no-voice profile instead of defaulting
        if (foundVoice == null)
        {
            Debug.LogWarning($"⚠️ Voice ID '{voiceId}' not found in library. Returning 'No Voice'.");
            return noVoiceProfile;
        }

        return foundVoice;
    }

    public static VoiceProfile GetDefaultVoice()
    {
        return GetAvailableVoices()[0]; // Tatang Luis
    }

    // ✅ NEW: Check if a voice ID represents "no voice"
    public static bool IsNoVoice(string voiceId)
    {
        return string.IsNullOrEmpty(voiceId);
    }

    // ✅ NEW: Get the no-voice profile
    public static VoiceProfile GetNoVoiceProfile()
    {
        return noVoiceProfile;
    }
}
