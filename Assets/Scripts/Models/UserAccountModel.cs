using Firebase.Firestore;

[FirestoreData]
public class UserAccountModel
{
    [FirestoreProperty] public string userId { get; set; }
    [FirestoreProperty] public string displayName { get; set; }
    [FirestoreProperty] public string email { get; set; }
    [FirestoreProperty] public string role { get; set; }
    [FirestoreProperty] public Timestamp dateCreated { get; set; }
}
