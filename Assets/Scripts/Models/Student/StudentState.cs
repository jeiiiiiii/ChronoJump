using System.Collections.Generic;
using Firebase.Firestore;

public class StudentState
{
    // Core identifiers
    public string StudentId { get; private set; }  
    public string UserId { get; private set; }   

    // Models
    public StudentModel Identity { get; private set; }
    public StudentProgressModel Progress { get; private set; }
    public StudentLeaderboardModel Leaderboard { get; private set; }
    public GameProgressModel GameProgress { get; private set; }
    public List<QuizAttemptModel> QuizAttempts { get; private set; }

    // Constructor
    public StudentState(string studentId, string userId, StudentModel identity)
    {
        StudentId = studentId;
        UserId = userId;
        Identity = identity;
        QuizAttempts = new List<QuizAttemptModel>();

        // Initialize Progress
        if (identity.studentProgress != null)
        {
            object referenceObj = null;
            identity.studentProgress.TryGetValue("currentStory", out referenceObj);

            Progress = new StudentProgressModel
            {
                currentStory = referenceObj as DocumentReference,
                overallScore = identity.studentProgress.ContainsKey("overallScore")
                    ? identity.studentProgress["overallScore"] as string
                    : "0",
                successRate = identity.studentProgress.ContainsKey("successRate")
                    ? identity.studentProgress["successRate"] as string
                    : "0%",
                isRemoved = identity.studentProgress.ContainsKey("isRemoved")
                    ? (bool)identity.studentProgress["isRemoved"]
                    : false,
                dateUpdated = identity.studentProgress.ContainsKey("dateUpdated") && identity.studentProgress["dateUpdated"] != null
                    ? (Timestamp)identity.studentProgress["dateUpdated"]
                    : Timestamp.GetCurrentTimestamp()
            };
        }
        else
        {
            Progress = new StudentProgressModel
            {
                currentStory = null,
                overallScore = "0",
                successRate = "0%",
                isRemoved = false,
                dateUpdated = Timestamp.GetCurrentTimestamp()
            };
        }

        GameProgress = new GameProgressModel
        {
            currentHearts = 3,
            unlockedChapters = new List<string>(),
            unlockedStories = new List<string>(),
            unlockedAchievements = new List<string>(),
            unlockedArtifacts = new List<string>(),
            lastUpdated = Timestamp.GetCurrentTimestamp(),
            isRemoved = false
        };
    }


    // Setters for updating state
    public void SetLeaderboard(StudentLeaderboardModel leaderboard) => Leaderboard = leaderboard;
    public void SetGameProgress(GameProgressModel gameProgress) => GameProgress = gameProgress;
    public void SetQuizAttempts(List<QuizAttemptModel> attempts) => QuizAttempts = attempts;
    public void SetProgress(StudentProgressModel progress) => Progress = progress;

    // Convenience getters
    public string GetCurrentStoryId() => Progress?.currentStory?.Id;
}
