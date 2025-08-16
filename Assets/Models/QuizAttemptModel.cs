using System;

[Serializable]
public class QuizAttemptModel
{
    public string attemptId { get; set; }
    public string studId { get; set; }
    public string quizId { get; set; }
    public float score { get; set; }
    public float timeTaken { get; set; }
    public DateTime submittedAt { get; set; }

    public QuizAttemptModel() { }

    public QuizAttemptModel(string attemptId, string studId, string quizId, float score, float timeTaken, DateTime submittedAt)
    {
        this.attemptId = attemptId;
        this.studId = studId;
        this.quizId = quizId;
        this.score = score;
        this.timeTaken = timeTaken;
        this.submittedAt = submittedAt;
    }
}
