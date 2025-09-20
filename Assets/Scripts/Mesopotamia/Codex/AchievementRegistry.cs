using System.Collections.Generic;

public static class AchievementRegistry
{
    public static readonly Dictionary<string, AchievementModel> AchievementsById = new()
    {
        { "AC001", new AchievementModel { Id = "AC001", Name = "Scribe", Title = "First Scribe", Description = "Correctly answer your first recall challenge in the Sumerian Chapter" } },
        { "AC002", new AchievementModel { Id = "AC002", Name = "Master", Title = "Master of Cuneiform", Description = "Get all Sumerian recall challenges correct without any mistake." } },
        { "AC003", new AchievementModel { Id = "AC003", Name = "Rise", Title = "Rise of an Empire", Description = "Meet Sargon I" } },
        { "AC004", new AchievementModel { Id = "AC004", Name = "Strategist", Title = "Strategist of Akkad", Description = "Finish all recall challenges in the Akkadian Civilization" } },
        { "AC005", new AchievementModel { Id = "AC005", Name = "Keeper", Title = "Keeper of Stories", Description = "Finish the Babylonian Civilization with at least 2 hearts" } },
        { "AC006", new AchievementModel { Id = "AC006", Name = "Fear", Title = "Fear and Fire", Description = "Complete the 1st Assyrian challenge without losing hearts" } },
        { "AC007", new AchievementModel { Id = "AC007", Name = "Guardian", Title = "Guardian of Knowledge", Description = "Visit Ashurbanipal's library" } },
    };

    // For quick lookup by name (so your existing code won’t break)
    public static readonly Dictionary<string, AchievementModel> AchievementsByName =
        new Dictionary<string, AchievementModel>();

    static AchievementRegistry()
    {
        foreach (var kvp in AchievementsById)
        {
            AchievementsByName[kvp.Value.Name] = kvp.Value;
        }
    }
}
