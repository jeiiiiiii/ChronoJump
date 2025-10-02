using Firebase.Firestore;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

public class StudentService
{
    private readonly IFirebaseService _firebaseService;

    public StudentService(IFirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    public void GetStudentsInClass(string classCode, Action<List<StudentModel>> callback)
    {
        _firebaseService.DB.Collection("students")
            .WhereEqualTo("classCode", classCode)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("Failed to get students: " + task.Exception);
                    callback(null);
                    return;
                }

                QuerySnapshot snapshot = task.Result;
                ProcessStudentDocuments(snapshot, callback);
            });
    }

    private void ProcessStudentDocuments(QuerySnapshot snapshot, Action<List<StudentModel>> callback)
    {
        List<StudentModel> students = new List<StudentModel>();

        if (snapshot.Count() == 0)
        {
            callback(students);
            return;
        }

        int remaining = snapshot.Count();

        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            string studId = document.Id;
            StudentModel student = MapDocumentToStudent(document);

            GetStudentProgress(studId, progress =>
            {
                student.studentProgress = progress ?? new Dictionary<string, object>();
                students.Add(student);
                remaining--;

                if (remaining <= 0)
                {
                    callback(students);
                }
            });
        }
    }

    private StudentModel MapDocumentToStudent(DocumentSnapshot document)
    {
        return new StudentModel
        {
            studId = document.Id,
            teachId = GetFieldValue(document, "teachId"),
            userId = GetFieldValue(document, "userId"),
            studName = GetFieldValue(document, "studName"),
            classCode = GetFieldValue(document, "classCode"),
            isRemoved = document.ContainsField("isRemoved") ? document.GetValue<bool>("isRemoved") : false,
        };
    }

    private async void GetStudentProgress(string studId, Action<Dictionary<string, object>> callback)
    {
        try
        {
            // Get student progress document
            var studentProgressSnapshot = await _firebaseService.DB.Collection("studentProgress").Document(studId).GetSnapshotAsync();

            if (!studentProgressSnapshot.Exists)
            {
                Debug.LogWarning($"No progress found for student {studId}");
                callback(null);
                return;
            }

            var progress = await ProcessProgressDocuments(studentProgressSnapshot);
            callback(progress);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[GetStudentProgress] Failed to get student progress: {ex}");
            callback(null);
        }
    }

    public void GetStudentDataByUserId(string userId, Action<StudentModel> callback)
    {
        Debug.Log($"Fetching student data for userId: {userId}");

        _firebaseService.DB.Collection("students")
            .WhereEqualTo("userId", userId)
            .WhereEqualTo("isRemoved", false)
            .Limit(1)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("Failed to get student data: " + task.Exception);
                    callback(null);
                    return;
                }

                QuerySnapshot snapshot = task.Result;

                if (snapshot.Count > 0)
                {
                    DocumentSnapshot document = snapshot.Documents.First();
                    StudentModel student = MapDocumentToStudent(document);
                    callback(student);
                }
                else
                {
                    Debug.LogWarning($"No student found for userId: {userId}");
                    callback(null);
                }
            });
    }

    private async Task<Dictionary<string, object>> ProcessProgressDocuments(DocumentSnapshot studentProgressDoc)
    {
        Dictionary<string, object> progress = new Dictionary<string, object>();
        var studentData = studentProgressDoc.ToDictionary();

        // Process each field in the student progress document
        foreach (var kv in studentData)
        {
            // Check if this field is a currentStory reference
            if (kv.Key == "currentStory" && kv.Value is DocumentReference storyRef)
            {
                try
                {
                    // Get the referenced story document
                    var storySnapshot = await storyRef.GetSnapshotAsync();
                    if (storySnapshot.Exists)
                    {
                        var storyFields = new Dictionary<string, object>();
                        var storyData = storySnapshot.ToDictionary();

                        // Add all story fields except the chapter reference
                        foreach (var field in storyData)
                        {
                            if (field.Key != "chapter")
                            {
                                storyFields[field.Key] = field.Value;
                            }
                        }

                        // Handle chapter reference if it exists
                        if (storyData.ContainsKey("chapter") && storyData["chapter"] is DocumentReference chapterRef)
                        {
                            try
                            {
                                var chapterSnapshot = await chapterRef.GetSnapshotAsync();
                                if (chapterSnapshot.Exists)
                                {
                                    var chapterFields = new Dictionary<string, object>();
                                    foreach (var chapterField in chapterSnapshot.ToDictionary())
                                    {
                                        chapterFields[chapterField.Key] = chapterField.Value;
                                    }
                                    storyFields["chapter"] = chapterFields;
                                }
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogError($"Failed to get chapter reference: {ex}");
                            }
                        }

                        progress["currentStory"] = storyFields;
                    }
                    else
                    {
                        progress[kv.Key] = kv.Value;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to get story reference: {ex}");
                    progress[kv.Key] = kv.Value;
                }
            }
            else
            {
                // Add non-reference fields directly
                progress[kv.Key] = kv.Value;
            }
        }

        return progress;
    }

    private string GetFieldValue(DocumentSnapshot document, string fieldName)
    {
        return document.ContainsField(fieldName) ? document.GetValue<string>(fieldName) : "";
    }

    public void GetStudentLeaderboard(string classCode, Action<List<LeaderboardStudentModel>> callback)
    {
        Debug.Log($"[GetStudentLeaderboard] Starting for class: {classCode}");

        _firebaseService.DB.Collection("studentLeaderboards")
            .WhereEqualTo("classCode", classCode)
            .OrderByDescending("overallScore")
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("Failed to get student leaderboard: " + task.Exception);
                    callback(null);
                    return;
                }

                QuerySnapshot snapshot = task.Result;
                List<LeaderboardStudentModel> students = new List<LeaderboardStudentModel>();

                Debug.Log($"[GetStudentLeaderboard] Found {snapshot.Documents.Count()} students");

                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    var data = document.ToDictionary();

                    string studId = document.Id;
                    string displayName = data.ContainsKey("displayName") ? data["displayName"]?.ToString() : "Unknown Student";
                    string classCodeFromDoc = data.ContainsKey("classCode") ? data["classCode"]?.ToString() : "";
                    bool isRemoved = data.ContainsKey("isRemoved") && data["isRemoved"] is bool b && b;

                    int overallScore = 0;
                    if (data.ContainsKey("overallScore"))
                    {
                        if (data["overallScore"] is long longScore)
                            overallScore = (int)longScore;
                        else if (data["overallScore"] is int intScore)
                            overallScore = intScore;
                        else if (int.TryParse(data["overallScore"]?.ToString(), out int parsedScore))
                            overallScore = parsedScore;
                    }

                    students.Add(new LeaderboardStudentModel(studId, displayName, overallScore, classCodeFromDoc, isRemoved));
                }

                callback(students);
            });
    }

}