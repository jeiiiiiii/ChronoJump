public static class GameState
{
    private static int _hearts = 3;
    private static int _score = 0;

    public static int hearts
    {
        get => _hearts;
        set
        {
            _hearts = value;
            GameProgressManager.Instance?.SetHearts(_hearts);
        }
    }

    public static int score
    {
        get => _score;
        set
        {
            _score = value;
            GameProgressManager.Instance?.SetScore(_score);
        }
    }

    public static void Initialize()
    {
        var gp = GameProgressManager.Instance?.CurrentStudentState?.GameProgress;
        var sp = GameProgressManager.Instance?.CurrentStudentState?.Progress;

        if (gp != null)
        {
            _hearts = gp.currentHearts;
        }

        if (sp != null && int.TryParse(sp.overallScore, out int parsedScore))
        {
            _score = parsedScore;
        }
        else
        {
            _score = 0;
        }

        // Stay synced if hearts are modified via GameProgressManager
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.OnHeartsChanged += newHearts => _hearts = newHearts;
        }
    }

    public static void ResetHearts() => hearts = 3;
    public static void ResetScore() => score = 0;
}
