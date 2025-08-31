using Firebase.Firestore;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

public class StudentService
{
    private readonly IFirebaseService _firebaseService;

    public StudentService(IFirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    public void GetStudentsInClass(string classCode, Action<List<StudentModel>> callback)
    {
        Debug.Log($"Fetching students in class: {classCode}");
        
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

        if (snapshot.Count == 0)
        {
            callback(students);
            return;
        }

        int remaining = snapshot.Count;

        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            string studId = document.Id;
            StudentModel student = MapDocumentToStudent(document);

            GetStudentProgress(studId, progress =>
            {
                student.progress = progress ?? new Dictionary<string, string>();
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
            studProfilePic = GetFieldValue(document, "studProfilePic"),
            classCode = GetFieldValue(document, "classCode")
        };
    }

    private void GetStudentProgress(string studId, Action<Dictionary<string, string>> callback)
    {
        _firebaseService.DB.Collection("students").Document(studId).Collection("progress")
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("Failed to get student progress: " + task.Exception);
                    callback(null);
                    return;
                }

                QuerySnapshot snapshot = task.Result;
                Dictionary<string, string> progress = ProcessProgressDocuments(snapshot);
                callback(progress);
            });
    }

    private Dictionary<string, string> ProcessProgressDocuments(QuerySnapshot snapshot)
    {
        Dictionary<string, string> progress = new Dictionary<string, string>();

        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            string level = GetFieldValue(document, "level");
            string status = GetFieldValue(document, "status");
            
            if (!string.IsNullOrEmpty(level))
            {
                progress[level] = status;
            }
        }

        return progress;
    }

    private string GetFieldValue(DocumentSnapshot document, string fieldName)
    {
        return document.ContainsField(fieldName) ? document.GetValue<string>(fieldName) : "";
    }
}