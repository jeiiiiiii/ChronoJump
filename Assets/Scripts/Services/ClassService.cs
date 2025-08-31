using Firebase.Firestore;
using Firebase.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ClassService
{
    private readonly IFirebaseService _firebaseService;
    private readonly TeacherService _teacherService;

    public ClassService(IFirebaseService firebaseService, TeacherService teacherService)
    {
        _firebaseService = firebaseService;
        _teacherService = teacherService;
    }

    public void CreateClass(string className, string classLevel, Action<bool, string> callback)
    {
        if (_firebaseService.CurrentUser == null)
        {
            Debug.LogError("No current user signed in.");
            callback(false, "No user signed in.");
            return;
        }

        _teacherService.GetTeacherData(_firebaseService.CurrentUser.UserId, teacherData =>
        {
            if (teacherData == null)
            {
                Debug.LogError("No teacher data found for current user.");
                callback(false, "No teacher data found.");
                return;
            }

            CreateClassWithTeacherData(className, classLevel, teacherData.teachId, callback);
        });
    }

    private void CreateClassWithTeacherData(string className, string classLevel, string teacherId, Action<bool, string> callback)
    {
        GenerateUniqueClassCode().ContinueWithOnMainThread(codeTask =>
        {
            if (codeTask.IsCanceled || codeTask.IsFaulted)
            {
                Debug.LogError("Failed to generate class code: " + codeTask.Exception);
                callback(false, "Failed to generate class code. Please try again.");
                return;
            }

            string classCode = codeTask.Result;
            SaveNewClass(className, classLevel, classCode, teacherId, callback);
        });
    }

    private void SaveNewClass(string className, string classLevel, string classCode, string teacherId, Action<bool, string> callback)
    {
        var newClass = new
        {
            className = className,
            classLevel = classLevel,
            classCode = classCode,
            teachId = teacherId,
            dateCreated = Timestamp.GetCurrentTimestamp()
        };

        _firebaseService.DB.Collection("classes").AddAsync(newClass)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("Failed to create class: " + task.Exception);
                    callback(false, "Failed to create class. Please try again.");
                    return;
                }

                Debug.Log($"Class {className} created successfully with code: {classCode}");
                callback(true, classCode);
            });
    }

    public async Task<string> GenerateUniqueClassCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new System.Random();
        string classCode;
        bool exists = true;

        do
        {
            classCode = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            exists = await DoesClassCodeExist(classCode);
        }
        while (exists);

        return classCode;
    }

    private async Task<bool> DoesClassCodeExist(string classCode)
    {
        QuerySnapshot snapshot = await _firebaseService.DB.Collection("classes")
            .WhereEqualTo("classCode", classCode)
            .Limit(1)
            .GetSnapshotAsync();

        return snapshot.Count > 0;
    }
}