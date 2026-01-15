using Firebase.Firestore;

[FirestoreData]
public class StudentLeaderboardModel
{
    [FirestoreDocumentId] public string docId { get; set; } 
    [FirestoreProperty] public string studId { get; set; } 
    [FirestoreProperty] public string classCode { get; set; }
    [FirestoreProperty] public string displayName { get; set; }
    [FirestoreProperty] public string overallScore { get; set; }
    [FirestoreProperty] public bool isRemoved { get; set; }
    [FirestoreProperty] public Timestamp dateUpdated { get; set; }
}