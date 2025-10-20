using System.Collections.Generic;
using UnityEngine;

public static class VoiceLibrary
{
    public static List<VoiceProfile> GetAvailableVoices()
    {
        return new List<VoiceProfile>
        {
            new VoiceProfile(
                "Select Voice...",
                "",
                "Choose a voice for text-to-speech",
                "",
                ""
            ),
            new VoiceProfile(
                "Rachel",
                "21m00Tcm4TlvDq8ikWAM",
                "Clear, friendly female voice",
                "Female",
                "American"
            ),
            new VoiceProfile(
                "Adam",
                "pNInz6obpgDQGcFmaJgB",
                "Deep, confident male voice",
                "Male",
                "American"
            ),
            new VoiceProfile(
                "Sara",
                "EXAVITQu4vr4xnSDxMaL",
                "Soft, gentle female voice",
                "Female",
                "American"
            ),
            new VoiceProfile(
                "Antoni",
                "ErXwobaYiN019PkySvjV",
                "Calm, well-spoken male voice",
                "Male",
                "American"
            ),
            new VoiceProfile(
                "Elli",
                "MF3mGyEYCl7XYWbV9V6O",
                "Young, energetic female voice",
                "Female",
                "American"
            ),
            new VoiceProfile(
                "Josh",
                "TxGEqnHWrfWFTfGW9XjX",
                "Professional, clear male voice",
                "Male",
                "American"
            )
        };
    }

    public static VoiceProfile GetVoiceById(string voiceId)
    {
        var voices = GetAvailableVoices();

        // Handle empty voice ID
        if (string.IsNullOrEmpty(voiceId))
        {
            return voices[0]; // Return "Select Voice..." option
        }

        return voices.Find(v => v.voiceId == voiceId) ?? voices[0]; // Default to "Select Voice..."
    }

    public static VoiceProfile GetDefaultVoice()
    {
        return GetAvailableVoices()[0]; // "Select Voice..." (empty)
    }

    public static VoiceProfile GetEmptyVoice()
    {
        return GetAvailableVoices()[0]; // "Select Voice..." (empty)
    }
}
