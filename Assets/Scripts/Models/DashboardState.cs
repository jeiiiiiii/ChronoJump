using System.Collections.Generic;

[System.Serializable]
public class DashboardState
{
    public TeacherModel teacherData;
    public string selectedClassCode;
    public string selectedClassName;
    public List<StudentModel> currentStudents;

    public Dictionary<string, List<StudentModel>> cachedStudents = new Dictionary<string, List<StudentModel>>();
    public Dictionary<string, List<LeaderboardStudentModel>> cachedLeaderboards = new Dictionary<string, List<LeaderboardStudentModel>>();

    public bool HasClasses => teacherData?.classCode != null && teacherData.classCode.Count > 0;
}
