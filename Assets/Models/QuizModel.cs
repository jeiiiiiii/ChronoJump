using System;

[Serializable]
public class QuizModel
{
    public string quizId;
    public string quizTitle;
    public int duration;

    public QuizModel() { }
    
    public QuizModel(string quizId, string quizTitle, int duration)
    {
        this.quizId = quizId;
        this.quizTitle = quizTitle;
        this.duration = duration;
    }
}
