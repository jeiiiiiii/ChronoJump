using Firebase.Firestore;
using System.Collections.Generic;

[FirestoreData]
public class TeacherModel
{
    [FirestoreProperty] public string userId { get; set; }
    [FirestoreProperty] public string teachId { get; set; }
    [FirestoreProperty] public string teachFirstName { get; set; }
    [FirestoreProperty] public string teachLastName { get; set; }
    [FirestoreProperty] public string title { get; set; }
    [FirestoreProperty] public string teachProfileIcon { get; set; }

    public Dictionary<string, List<string>> classCode { get; set; }
}
