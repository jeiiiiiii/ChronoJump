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
}
