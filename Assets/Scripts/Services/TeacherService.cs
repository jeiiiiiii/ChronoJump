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

    // Main method that handles both userId and teacherId
    public async void GetTeacherData(string userIdOrTeacherId, Action<TeacherModel> callback)
    {
        try
        {
            if (Application.isPlaying == false) return;

            // First, try direct document lookup (assumes it's a teacherId)
            var teacherDocSnapshot = await _firebaseService.DB
                .Collection("teachers")
                .Document(userIdOrTeacherId)
                .GetSnapshotAsync();

            if (Application.isPlaying == false) return;

            // If document doesn't exist, try querying by userId field
            if (!teacherDocSnapshot.Exists)
            {
                Debug.Log($"No teacher document with ID: {userIdOrTeacherId}, trying userId query...");
                teacherDocSnapshot = await GetTeacherDocumentByUserId(userIdOrTeacherId);

                if (teacherDocSnapshot == null || !teacherDocSnapshot.Exists)
                {
                    if (Application.isPlaying)
                        Debug.LogError("No teacher document found with userId: " + userIdOrTeacherId);
                    callback?.Invoke(null);
                    return;
                }
            }

            if (Application.isPlaying)
                LogTeacherDocumentFields(teacherDocSnapshot);

            // Use the document ID (teacherId) to get classes
            string teacherId = teacherDocSnapshot.Id;
            var classCodes = await GetTeacherClassCodes(teacherId);

            if (Application.isPlaying == false) return;

            var teacherData = MapDocumentToTeacher(teacherDocSnapshot, classCodes);
            callback?.Invoke(teacherData);
        }
        catch (Exception ex)
        {
            if (Application.isPlaying)
                Debug.LogError($"Error getting teacher data: {ex.Message}");
            callback?.Invoke(null);
        }
    }

    private async Task<DocumentSnapshot> GetTeacherDocumentByUserId(string userId)
    {
        Query teachQuery = _firebaseService.DB.Collection("teachers")
            .WhereEqualTo("userId", userId)
            .Limit(1);

        QuerySnapshot teachQuerySnapshot = await teachQuery.GetSnapshotAsync();

        return teachQuerySnapshot.Count > 0 ? teachQuerySnapshot.Documents.FirstOrDefault() : null;
    }

    // CHANGED: Filter out removed classes
    private async Task<Dictionary<string, List<string>>> GetTeacherClassCodes(string teacherId)
    {
        Dictionary<string, List<string>> classCodes = new Dictionary<string, List<string>>();

        // NEW: Add filter for isRemoved = false
        Query classQuery = _firebaseService.DB.Collection("classes")
            .WhereEqualTo("teachId", teacherId)
            .WhereEqualTo("isRemoved", false);  // Only get non-removed classes

        QuerySnapshot querySnapshot = await classQuery.GetSnapshotAsync();

        foreach (DocumentSnapshot document in querySnapshot.Documents)
        {
            string classCode = document.GetValue<string>("classCode");
            string className = document.GetValue<string>("className");
            string classLevel = document.GetValue<string>("classLevel");
            classCodes[classCode] = new List<string> { classLevel, className };
        }

        Debug.Log($"[TeacherService] Found {classCodes.Count} active classes for teacher {teacherId}");

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
