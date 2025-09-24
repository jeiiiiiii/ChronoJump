using System.Collections.Generic;

public static class AchievementRegistry
{
    public static readonly Dictionary<string, AchievementModel> AchievementsById = new();
    public static readonly Dictionary<string, AchievementModel> AchievementsByName = new();

    static AchievementRegistry()
    {
        // --- Achievements ---
        Register(new AchievementModel {
            Id = "AC001", Name = "Scribe", Title = "First Scribe",
            Description = "Correctly answer your first recall challenge in the Sumerian Chapter",
            Type = "achievement"
        });
        Register(new AchievementModel {
            Id = "AC002", Name = "Master", Title = "Master of Cuneiform",
            Description = "Get all Sumerian recall challenges correct without any mistake.",
            Type = "achievement"
        });
        Register(new AchievementModel {
            Id = "AC003", Name = "Rise", Title = "Rise of an Empire",
            Description = "Meet Sargon I",
            Type = "achievement"
        });
        Register(new AchievementModel {
            Id = "AC004", Name = "Strategist", Title = "Strategist of Akkad",
            Description = "Finish all recall challenges in the Akkadian Civilization",
            Type = "achievement"
        });
        Register(new AchievementModel {
            Id = "AC005", Name = "Keeper", Title = "Keeper of Stories",
            Description = "Finish the Babylonian Civilization with at least 2 hearts",
            Type = "achievement"
        });
        Register(new AchievementModel {
            Id = "AC006", Name = "Fear", Title = "Fear and Fire",
            Description = "Complete the 1st Assyrian challenge without losing hearts",
            Type = "achievement"
        });
        Register(new AchievementModel {
            Id = "AC007", Name = "Guardian", Title = "Guardian of Knowledge",
            Description = "Visit Ashurbanipal's library",
            Type = "achievement"
        });

        // --- Artifacts ---
        Register(new AchievementModel {
            Id = "AR001", Name = "Tablet", Title = "Tablet of Destiny",
            Description = "Discover the Tablet artifact",
            Type = "artifact"
        });
        Register(new AchievementModel {
            Id = "AR002", Name = "Sword", Title = "Sword of Kings",
            Description = "Obtain the Sword artifact",
            Type = "artifact"
        });
        Register(new AchievementModel {
            Id = "AR003", Name = "Stone", Title = "Stone of Wisdom",
            Description = "Uncover the Stone artifact",
            Type = "artifact"
        });
        Register(new AchievementModel {
            Id = "AR004", Name = "Belt", Title = "Belt of Power",
            Description = "Equip the Belt artifact",
            Type = "artifact"
        });
    }

    private static void Register(AchievementModel model)
    {
        AchievementsById[model.Id] = model;
        AchievementsByName[model.Name] = model;
    }
}
