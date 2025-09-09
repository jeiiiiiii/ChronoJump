using System.Collections.Generic;

public interface IStudentLeaderboardView
{
    void ShowStudentLeaderboard(List<LeaderboardStudentModel> students);
    void AddStudentToLeaderboard(LeaderboardStudentModel student, int ranking);
    void ClearLeaderboard();
}