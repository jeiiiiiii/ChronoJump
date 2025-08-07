using System;

[Serializable]
public class QuestionModel
{
    public string questionId { get; set; }
    public string quizId { get; set; }
    public string text { get; set; }
    public string[] options { get; set; } 
    public string correctAnswer { get; set; }

    public QuestionModel() { }

    public QuestionModel(string questionId, string quizId, string text, string[] options, string correctAnswer)
    {
        this.questionId = questionId;
        this.quizId = quizId;
        this.text = text;
        this.options = options ?? new string[0];
        this.correctAnswer = correctAnswer;
    }   
}
