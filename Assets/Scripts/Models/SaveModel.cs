using Firebase.Firestore;
using System.Collections.Generic;

[FirestoreData]
public class SaveData
{
    [FirestoreProperty] public string studentId { get; set; }
    [FirestoreProperty] public string currentScene { get; set; }
    [FirestoreProperty] public int dialogueIndex { get; set; }
    [FirestoreProperty] public string timestamp { get; set; }

    [FirestoreProperty] public GameProgressModel gameProgress { get; set; }

    public SaveData() {}

    public SaveData(string studentId, string scene, int dialogue, GameProgressModel progress)
    {
        this.studentId = studentId;
        this.currentScene = scene;
        this.dialogueIndex = dialogue;
        this.timestamp = System.DateTime.Now.ToString("yyyy-MM-dd h:mm:tt");
        this.gameProgress = progress;
    }
}
