using Firebase.Firestore;
using System;
using System.Collections.Generic;

[FirestoreData]
public class StoryDataFirestore
{
    [FirestoreDocumentId] public string storyId { get; set; }  
    [FirestoreProperty] public string title { get; set; }
    [FirestoreProperty] public string description { get; set; }
    [FirestoreProperty] public string backgroundUrl { get; set; }
    [FirestoreProperty] public string character1Url { get; set; }
    [FirestoreProperty] public string character2Url { get; set; }
    [FirestoreProperty] public string teachId { get; set; }
    [FirestoreProperty] public Timestamp createdAt { get; set; }
    [FirestoreProperty] public Timestamp updatedAt { get; set; }
    [FirestoreProperty] public bool isPublished { get; set; }
    [FirestoreProperty] public List<string> assignedClasses { get; set; }
    [FirestoreProperty] public int storyIndex { get; set; }
}

[FirestoreData]
public class QuestionFirestore
{
    [FirestoreDocumentId] public string questionId { get; set; }
    [FirestoreProperty] public string questionText { get; set; }
    [FirestoreProperty] public List<string> choices { get; set; }
    [FirestoreProperty] public int correctAnswerIndex { get; set; }
    [FirestoreProperty] public int orderIndex { get; set; }
}


[FirestoreData]
public class DialogueLineFirestore
{
    [FirestoreProperty] public string characterName { get; set; }
    [FirestoreProperty] public string dialogueText { get; set; }
    [FirestoreProperty] public string selectedVoiceId { get; set; }
    [FirestoreProperty] public int orderIndex { get; set; }

    // ✅ Audio fields for voice narration (simplified)
    [FirestoreProperty] public string audioFilePath { get; set; }
    [FirestoreProperty] public bool hasAudio { get; set; }
    [FirestoreProperty] public string audioFileName { get; set; }
    [FirestoreProperty] public string audioStoragePath { get; set; }

    public DialogueLineFirestore()
    {
        characterName = "";
        dialogueText = "";
        selectedVoiceId = "";
        orderIndex = 0;
        audioFilePath = "";
        hasAudio = false;
        audioFileName = "";
        audioStoragePath = "";
    }

    public DialogueLineFirestore(string name, string text, string voiceId, int index)
    {
        characterName = name;
        dialogueText = text;
        selectedVoiceId = voiceId ?? "";
        orderIndex = index;
        audioFilePath = "";
        hasAudio = false;
        audioFileName = "";
        audioStoragePath = "";
    }
}
