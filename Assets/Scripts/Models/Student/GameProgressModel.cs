using Firebase.Firestore;
using System.Collections.Generic;

[FirestoreData]
public class GameProgressModel
{
    [FirestoreDocumentId] public string studId { get; set; } 
    
    // Collections/arrays you mentioned as nested/unlocked items
    [FirestoreProperty] public List<string> unlockedChapters { get; set; }
    [FirestoreProperty] public List<string> unlockedStories { get; set; }
    [FirestoreProperty] public List<string> unlockedCivilizations { get; set; }
    [FirestoreProperty] public List<string> unlockedAchievements { get; set; }
    [FirestoreProperty] public List<string> unlockedArtifacts { get; set; }
    [FirestoreProperty] public Dictionary<string, object> unlockedCodex { get; set; } 

    [FirestoreProperty] public int currentHearts { get; set; }
    [FirestoreProperty] public Timestamp lastUpdated { get; set; }
    [FirestoreProperty] public bool isRemoved { get; set; }
}