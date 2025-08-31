using System.Collections.Generic;

[System.Serializable]
public class DashboardState
{
    public TeacherModel teacherData;
    public string selectedClassCode;
    public string selectedClassName;
    public List<StudentModel> currentStudents;
    
    public bool HasClasses => teacherData?.classCode != null && teacherData.classCode.Count > 0;
}