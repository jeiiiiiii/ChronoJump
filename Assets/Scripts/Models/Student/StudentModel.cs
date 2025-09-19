using Firebase.Firestore;
using System.Collections.Generic;

[FirestoreData]
public class StudentModel
{
    [FirestoreDocumentId] public string studId { get; set; }
    [FirestoreProperty] public string teachId { get; set; }
    [FirestoreProperty] public string userId { get; set; }
    [FirestoreProperty] public string studName { get; set; }
    [FirestoreProperty] public string classCode { get; set; }
    [FirestoreProperty] public bool isRemoved { get; set; }

    public Dictionary<string, object> studentProgress { get; set; }
}
