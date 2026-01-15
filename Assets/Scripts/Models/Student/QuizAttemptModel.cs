using Firebase.Firestore;
using System;

[FirestoreData]
public class QuizAttemptModel
{
    [FirestoreProperty] public string quizId { get; set; }
    [FirestoreProperty] public int attemptNumber { get; set; }
    [FirestoreProperty] public int score { get; set; }
    [FirestoreProperty] public bool isPassed { get; set; }
    [FirestoreProperty] public Timestamp dateCompleted { get; set; }

    public QuizAttemptModel() {}

    public QuizAttemptModel(string quizId, int attemptNum, int score, bool isPassed)
    {
        this.quizId = quizId;
        this.attemptNumber = attemptNum;
        this.score = score;
        this.isPassed = isPassed;
        this.dateCompleted = Timestamp.GetCurrentTimestamp();
    }
    
    // Correct property for local time display
    public DateTime LocalDateCompleted 
    {
        get 
        {
            // Convert Firebase Timestamp to DateTime and then to local time
            return dateCompleted.ToDateTime().ToLocalTime();
        }
    }
    
    // Optional: Property for formatted display string
    public string LocalDateDisplayString 
    {
        get 
        {
            return LocalDateCompleted.ToString("MMM dd, yyyy 'at' hh:mm tt");
        }
    }
}