using Firebase.Firestore;
using System.Collections.Generic;

[FirestoreData]
public class StudentModel
{
    [FirestoreProperty] public string studId { get; set; }
    [FirestoreProperty] public string teachId { get; set; }
    [FirestoreProperty] public string userId { get; set; }
    [FirestoreProperty] public string studName { get; set; }
    [FirestoreProperty] public string studProfilePic { get; set; }
    [FirestoreProperty] public string classCode { get; set; }

    public Dictionary<string, object> studentProgress { get; set; }
}
