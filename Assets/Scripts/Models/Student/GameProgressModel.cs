using Firebase.Firestore;
using System;
using System.Collections.Generic;

[FirestoreData]
public class GameProgressModel
{
    [FirestoreDocumentId] public string studId { get; set; } 

    // Collections
    [FirestoreProperty] public List<string> unlockedChapters { get; set; }
    [FirestoreProperty] public List<string> unlockedStories { get; set; }
    [FirestoreProperty] public List<string> unlockedCivilizations { get; set; }
    [FirestoreProperty] public List<string> unlockedAchievements { get; set; }
    [FirestoreProperty] public List<string> unlockedArtifacts { get; set; }
    [FirestoreProperty] public Dictionary<string, object> unlockedCodex { get; set; } 

    // Game state
    [FirestoreProperty] public int currentHearts { get; set; }
    [FirestoreProperty] public Timestamp lastUpdated { get; set; }
    [FirestoreProperty] public bool isRemoved { get; set; }

    public GameProgressModel()
    {
        unlockedChapters = new List<string>();
        unlockedStories = new List<string>();
        unlockedCivilizations = new List<string>();
        unlockedAchievements = new List<string>();
        unlockedArtifacts = new List<string>();
        unlockedCodex = new Dictionary<string, object>();
        currentHearts = 3;
        lastUpdated = Timestamp.GetCurrentTimestamp();
        isRemoved = false;
    }
}


