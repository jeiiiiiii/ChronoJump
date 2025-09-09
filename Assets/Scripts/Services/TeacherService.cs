using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class TeacherService
{
    private readonly IFirebaseService _firebaseService;

    public TeacherService(IFirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    public async void GetTeacherData(string userId, Action<TeacherModel> callback)
    {
        try
        {
            var teacherDoc = await GetTeacherDocument(userId);
            if (teacherDoc == null)
            {
                Debug.LogWarning("No teacher data found for userId: " + userId);
                callback(null);
                return;
            }

            LogTeacherDocumentFields(teacherDoc);
            
            var classCodes = await GetTeacherClassCodes(teacherDoc.Id);
            var teacherData = MapDocumentToTeacher(teacherDoc, classCodes);
            
            callback(teacherData);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error getting teacher data: {ex.Message}");
            callback(null);
        }
    }

    private async Task<DocumentSnapshot> GetTeacherDocument(string userId)
    {
        Query teachQuery = _firebaseService.DB.Collection("teachers")
            .WhereEqualTo("userId", userId)
            .Limit(1);
            
        QuerySnapshot teachQuerySnapshot = await teachQuery.GetSnapshotAsync();
        
        return teachQuerySnapshot.Count > 0 ? teachQuerySnapshot.Documents.FirstOrDefault() : null;
    }

    private async Task<Dictionary<string, List<string>>> GetTeacherClassCodes(string teacherId)
    {
        Dictionary<string, List<string>> classCodes = new Dictionary<string, List<string>>();
        
        Query classQuery = _firebaseService.DB.Collection("classes")
            .WhereEqualTo("teachId", teacherId);
            
        QuerySnapshot querySnapshot = await classQuery.GetSnapshotAsync();

        foreach (DocumentSnapshot document in querySnapshot.Documents)
        {
            string classCode = document.GetValue<string>("classCode");
            string className = document.GetValue<string>("className");
            string classLevel = document.GetValue<string>("classLevel");
            classCodes[classCode] = new List<string> { classLevel, className };
        }

        return classCodes;
    }

    private TeacherModel MapDocumentToTeacher(DocumentSnapshot teacherDoc, Dictionary<string, List<string>> classCodes)
    {
        return new TeacherModel
        {
            userId = GetFieldValue(teacherDoc, "userId"),
            teachId = teacherDoc.Id,
            teachFirstName = GetFieldValue(teacherDoc, "teachFirstName"),
            teachLastName = GetFieldValue(teacherDoc, "teachLastName"),
            title = GetFieldValue(teacherDoc, "title"),
            teachProfileIcon = GetFieldValue(teacherDoc, "teachProfilePic"),
            classCode = classCodes
        };
    }

    private string GetFieldValue(DocumentSnapshot document, string fieldName)
    {
        return document.ContainsField(fieldName) ? document.GetValue<string>(fieldName) : "";
    }

    private void LogTeacherDocumentFields(DocumentSnapshot teacherDoc)
    {
        Debug.Log($"Teacher Document ID: {teacherDoc.Id}");
        foreach (var field in teacherDoc.ToDictionary())
        {
            Debug.Log($"Field: {field.Key}, Value: {field.Value}");
        }
    }
}