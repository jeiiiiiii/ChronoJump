using Firebase.Firestore;

[FirestoreData]
public class StudentProgressModel
{
    [FirestoreDocumentId] public string studId { get; set; }
    [FirestoreProperty] public DocumentReference currentStory { get; set; }
    [FirestoreProperty] public string overallScore { get; set; }
    [FirestoreProperty] public string successRate { get; set; }
    [FirestoreProperty] public bool isRemoved { get; set; }
    [FirestoreProperty] public Timestamp dateUpdated { get; set; }
}

