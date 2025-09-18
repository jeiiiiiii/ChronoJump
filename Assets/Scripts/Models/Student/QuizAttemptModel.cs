using Firebase.Firestore;

[FirestoreData]
public class QuizAttemptModel
{
    [FirestoreDocumentId] public string attemptId { get; set; }
    [FirestoreProperty] public string quizId { get; set; }
    [FirestoreProperty] public int score { get; set; }
    [FirestoreProperty] public Timestamp attemptDate { get; set; }
    [FirestoreProperty] public bool isRemoved { get; set; }
}
