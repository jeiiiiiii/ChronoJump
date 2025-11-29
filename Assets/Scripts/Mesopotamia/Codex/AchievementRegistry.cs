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
        //Indus Achievements
        Register(new AchievementModel {
            Id = "AC008", Name = "Grid", Title = "Grid Master",
            Description = "Correctly answer your first recall challenge in the Indus chapter",
            Type = "achievement"
        });
        Register(new AchievementModel {
            Id = "AC009", Name = "Craftsman", Title = "Master Craftsman",
            Description = "Learned how to craft, Correctly answer for the Recall Challenge",
            Type = "achievement"
        });
        // China Achievements
        Register(new AchievementModel {
            Id = "AC010", Name = "Fortress", Title = "Fortress of Nature",
            Description = "Flood Control of the ancient times? Learn about Natural Barriers",
            Type = "achievement"
        });
        Register(new AchievementModel {
            Id = "AC011", Name = "River", Title = "River of Sorrow and Hope",
            Description = "Finish the all the Recall Challenges of the Yellow River",
            Type = "achievement"
        });
        // Egypt Achievements
        Register(new AchievementModel {
            Id = "AC012", Name = "Scholar", Title = "Papyrus Scholar",
            Description = "Finish the 3rd Scene of the “Ilog Nile” Story",
            Type = "achievement"
        });
        Register(new AchievementModel {
            Id = "AC013", Name = "Civilizations", Title = "River Civilizations Master",
            Description = "Finished all the Chapters and Stories! Congratulations!",
            Type = "achievement"
        });

        // --- Artifacts ---
        Register(new AchievementModel {
            Id = "AR001", Name = "Tablet", Title = "Tablet of Destiny",
            Description = "Discover the Tablet artifact",
            Type = "artifact"
        });
        Register(new AchievementModel {
            Id = "AR002", Name = "Clay", Title = "Clay Seal of Mohenjo-daro",
            Description = "Obtain the Clay Seal of Mohenjo-daro",
            Type = "artifact"
        });
        Register(new AchievementModel {
            Id = "AR003", Name = "Jar", Title = "Jar of Yellow Silt",
            Description = "Uncover the Jar of Yellow Silt",
            Type = "artifact"
        });
        Register(new AchievementModel {
            Id = "AR004", Name = "Papyrus", Title = "Papyrus of the Nile",
            Description = "Equip the Papyrus of the Nile",
            Type = "artifact"
        });
    }

    private static void Register(AchievementModel model)
    {
        AchievementsById[model.Id] = model;
        AchievementsByName[model.Name] = model;
    }
}
