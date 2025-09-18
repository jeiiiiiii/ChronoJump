using System;
using Firebase.Firestore;

[FirestoreData]
public class SaveData
{
    [FirestoreProperty] public string studentId { get; set; }
    [FirestoreProperty] public string currentScene { get; set; }
    [FirestoreProperty] public int dialogueIndex { get; set; }
    [FirestoreProperty] public string timestamp { get; set; }

    [FirestoreProperty] public GameProgressModel gameProgress { get; set; }

    public SaveData() {}

    public SaveData(string studId, string scene, int dialogue, GameProgressModel progress)
    {
        studentId = studId;
        currentScene = scene;
        dialogueIndex = dialogue;
        gameProgress = progress;
        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
