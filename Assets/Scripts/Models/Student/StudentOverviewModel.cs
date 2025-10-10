using System;
using System.Collections.Generic;

[System.Serializable]
public class StudentOverviewData
{
    public string studentId;
    public string studentName;
    public string classCode;
    public DateTime lastUpdated;
    public Dictionary<string, ChapterOverview> chapters = new Dictionary<string, ChapterOverview>();
}

[System.Serializable]
public class ChapterOverview
{
    public string chapterId;
    public string chapterTitle;
    public string chapterName;
    public List<StoryProgress> stories = new List<StoryProgress>();
}

[System.Serializable]
public class StoryProgress
{
    public string storyId;
    public string storyTitle;
    public string storyName;
    public List<QuizAttempt> quizAttempts = new List<QuizAttempt>();
}

[System.Serializable]
public class QuizAttempt
{
    public string attemptId;
    public int attemptNumber;
    public bool isPassed;
    public string quizId;
    public int score;
    public DateTime dateCompleted;
    public string remarks;
    public string remarksColor;
}