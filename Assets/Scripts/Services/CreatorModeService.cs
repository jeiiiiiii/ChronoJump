using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;
using System.Linq;

public class CreatorModeService
{
    private readonly IFirebaseService _firebaseService;

    public CreatorModeService(IFirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    // === FIRESTORE CRUD OPERATIONS ===

    public async Task<bool> SaveStoryToFirestore(StoryData story)
{
    try
    {
        if (_firebaseService?.CurrentUser == null)
        {
            Debug.LogError("❌ No user logged in, cannot save to Firestore");
            return false;
        }

        // Get teacher ID
        string teachId = await GetCurrentTeachId();
        if (string.IsNullOrEmpty(teachId))
        {
            Debug.LogError("❌ No teacher ID found, cannot save story");
            return false;
        }

        // ✅ USE THE STORY'S BUILT-IN INDEX INSTEAD OF CALCULATING
        int storyIndex = story.storyIndex;
        if (storyIndex < 0)
        {
            Debug.LogWarning($"⚠️ Story '{story.storyTitle}' has invalid index: {storyIndex}. Using list position.");
            storyIndex = StoryManager.Instance.allStories.IndexOf(story);
            if (storyIndex == -1) 
            {
                storyIndex = StoryManager.Instance.allStories.Count;
            }
        }

        // ✅ Map to Firestore model WITH index (now 3 parameters)
        var firestoreStory = MapToFirestoreStory(story, teachId, storyIndex);

        // Save main story document
        var storyRef = _firebaseService.DB
            .Collection("createdStories")
            .Document(story.storyId);
        
        await storyRef.SetAsync(firestoreStory);

        // Save dialogues subcollection
        await SaveDialoguesToFirestore(story.storyId, story.dialogues);

        // Save questions subcollection  
        await SaveQuestionsToFirestore(story.storyId, story.quizQuestions);

        Debug.Log($"✅ Story '{story.storyTitle}' saved to Firestore successfully with index: {storyIndex}");
        return true;
    }
    catch (Exception ex)
    {
        Debug.LogError($"❌ Failed to save story to Firestore: {ex.Message}");
        return false;
    }
}

    public async Task<bool> LoadStoriesFromFirestore()
    {
        try
        {
            if (_firebaseService?.CurrentUser == null)
            {
                Debug.LogError("❌ No user logged in, cannot load from Firestore");
                return false;
            }

            // Get teacher ID
            string teachId = await GetCurrentTeachId();
            if (string.IsNullOrEmpty(teachId))
            {
                Debug.LogError("❌ No teacher ID found, cannot load stories");
                return false;
            }

            // Query stories for this teacher
            var storiesQuery = _firebaseService.DB
                .Collection("createdStories")
                .WhereEqualTo("teachId", teachId);
            
            var snapshot = await storiesQuery.GetSnapshotAsync();

            var loadedStories = new List<StoryData>();
            
            // FIXED: Use ToList() to convert IEnumerable to List for indexing
            var documents = snapshot.Documents.ToList();
            foreach (var storyDoc in documents)
            {
                var firestoreStory = storyDoc.ConvertTo<StoryDataFirestore>();
                
                // Load dialogues and questions
                var dialogues = await LoadDialoguesFromFirestore(storyDoc.Id);
                var questions = await LoadQuestionsFromFirestore(storyDoc.Id);
                
                // Map back to Unity model
                var unityStory = MapToUnityStory(firestoreStory, dialogues, questions);
                loadedStories.Add(unityStory);
            }

            // Update StoryManager's stories list
            if (StoryManager.Instance != null)
            {
                StoryManager.Instance.allStories = loadedStories;
                Debug.Log($"✅ Loaded {loadedStories.Count} stories from Firestore");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Failed to load stories from Firestore: {ex.Message}");
            return false;
        }
    }

    // === SUBCOLLECTION OPERATIONS ===

    private async Task SaveDialoguesToFirestore(string storyId, List<DialogueLine> dialogues)
    {
        if (dialogues == null) return;

        var dialoguesRef = _firebaseService.DB
            .Collection("createdStories")
            .Document(storyId)
            .Collection("dialogues");

        // Clear existing dialogues
        var existingDialogues = await dialoguesRef.GetSnapshotAsync();
        // FIXED: Convert to List before using in foreach
        var existingDocs = existingDialogues.Documents.ToList();
        foreach (var doc in existingDocs)
        {
            await doc.Reference.DeleteAsync();
        }

        // Save new dialogues
        for (int i = 0; i < dialogues.Count; i++)
        {
            var dialogueFirestore = MapToFirestoreDialogue(dialogues[i], i);
            await dialoguesRef.AddAsync(dialogueFirestore);
        }
    }

    private async Task SaveQuestionsToFirestore(string storyId, List<Question> questions)
    {
        if (questions == null) return;

        var questionsRef = _firebaseService.DB
            .Collection("createdStories")
            .Document(storyId)
            .Collection("questions");

        // Clear existing questions
        var existingQuestions = await questionsRef.GetSnapshotAsync();
        // FIXED: Convert to List before using in foreach
        var existingDocs = existingQuestions.Documents.ToList();
        foreach (var doc in existingDocs)
        {
            await doc.Reference.DeleteAsync();
        }

        // Save new questions
        for (int i = 0; i < questions.Count; i++)
        {
            var questionFirestore = MapToFirestoreQuestion(questions[i], i);
            await questionsRef.AddAsync(questionFirestore);
        }
    }

    private async Task<List<DialogueLine>> LoadDialoguesFromFirestore(string storyId)
    {
        var dialogues = new List<DialogueLine>();
        
        try
        {
            var dialoguesSnapshot = await _firebaseService.DB
                .Collection("createdStories")
                .Document(storyId)
                .Collection("dialogues")
                .OrderBy("orderIndex")
                .GetSnapshotAsync();

            // FIXED: Convert to List before iterating
            var dialogueDocs = dialoguesSnapshot.Documents.ToList();
            foreach (var dialogueDoc in dialogueDocs)
            {
                var dialogueFirestore = dialogueDoc.ConvertTo<DialogueLineFirestore>();
                dialogues.Add(MapToUnityDialogue(dialogueFirestore));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Failed to load dialogues: {ex.Message}");
        }

        return dialogues;
    }

    private async Task<List<Question>> LoadQuestionsFromFirestore(string storyId)
    {
        var questions = new List<Question>();
        
        try
        {
            var questionsSnapshot = await _firebaseService.DB
                .Collection("createdStories")
                .Document(storyId)
                .Collection("questions")
                .GetSnapshotAsync();

            // FIXED: Convert to List before iterating
            var questionDocs = questionsSnapshot.Documents.ToList();
            foreach (var questionDoc in questionDocs)
            {
                var questionFirestore = questionDoc.ConvertTo<QuestionFirestore>();
                questions.Add(MapToUnityQuestion(questionFirestore));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Failed to load questions: {ex.Message}");
        }

        return questions;
    }

    // === TEACHER ID RESOLUTION ===

    private async Task<string> GetCurrentTeachId()
    {
        // Try to get from StoryManager first
        if (StoryManager.Instance != null && StoryManager.Instance.IsCurrentUserTeacher())
        {
            return StoryManager.Instance.GetCurrentTeacherId();
        }

        // Fallback: Query Firestore for teacher data
        if (_firebaseService?.CurrentUser != null)
        {
            var teacherQuery = _firebaseService.DB
                .Collection("teachers")
                .WhereEqualTo("userId", _firebaseService.CurrentUser.UserId)
                .Limit(1);
            
            var snapshot = await teacherQuery.GetSnapshotAsync();
            // FIXED: Use Count property and FirstOrDefault()
            if (snapshot.Count > 0)
            {
                var teacherDoc = snapshot.Documents.FirstOrDefault();
                return teacherDoc?.Id; // teachId is the document ID
            }
        }

        return null;
    }

    // === MAPPING METHODS ===

    private StoryDataFirestore MapToFirestoreStory(StoryData unityStory, string teachId, int storyIndex)
{
    return new StoryDataFirestore
    {
        storyId = unityStory.storyId,
        title = unityStory.storyTitle,
        description = unityStory.storyDescription,
        backgroundUrl = unityStory.backgroundPath,
        character1Url = unityStory.character1Path,
        character2Url = unityStory.character2Path,
        teachId = teachId,
        createdAt = string.IsNullOrEmpty(unityStory.createdAt) ? 
            Timestamp.GetCurrentTimestamp() : 
            ConvertToTimestamp(unityStory.createdAt),
        updatedAt = Timestamp.GetCurrentTimestamp(),
        isPublished = unityStory.assignedClasses?.Count > 0,
        assignedClasses = unityStory.assignedClasses ?? new List<string>(),
        
        // ✅ ADD THE INDEX PARAMETER
        storyIndex = storyIndex
    };
}

private StoryData MapToUnityStory(StoryDataFirestore firestoreStory, 
                                List<DialogueLine> dialogues, 
                                List<Question> questions)
{
    return new StoryData
    {
        storyId = firestoreStory.storyId,
        storyTitle = firestoreStory.title,
        storyDescription = firestoreStory.description,
        backgroundPath = firestoreStory.backgroundUrl,
        character1Path = firestoreStory.character1Url,
        character2Path = firestoreStory.character2Url,
        assignedClasses = firestoreStory.assignedClasses ?? new List<string>(),
        dialogues = dialogues,
        quizQuestions = questions,
        
        // ✅ MAP INDEX BACK TO UNITY
        storyIndex = firestoreStory.storyIndex
    };
}

    private DialogueLineFirestore MapToFirestoreDialogue(DialogueLine unityDialogue, int orderIndex)
    {
        return new DialogueLineFirestore
        {
            characterName = unityDialogue.characterName,
            dialogueText = unityDialogue.dialogueText,
            orderIndex = orderIndex
        };
    }

    private DialogueLine MapToUnityDialogue(DialogueLineFirestore firestoreDialogue)
    {
        return new DialogueLine(
            firestoreDialogue.characterName,
            firestoreDialogue.dialogueText
        );
    }

    private QuestionFirestore MapToFirestoreQuestion(Question unityQuestion, int orderIndex)
    {
        return new QuestionFirestore
        {
            questionText = unityQuestion.questionText,
            choices = new List<string>(unityQuestion.choices),
            correctAnswerIndex = unityQuestion.correctAnswerIndex,
            orderIndex = orderIndex
        };
    }

    private Question MapToUnityQuestion(QuestionFirestore firestoreQuestion)
    {
        return new Question(
            firestoreQuestion.questionText,
            firestoreQuestion.choices.ToArray(),
            firestoreQuestion.correctAnswerIndex
        );
    }

    // === UTILITY METHODS ===

    private Timestamp ConvertToTimestamp(string dateString)
    {
        if (DateTime.TryParse(dateString, out DateTime date))
        {
            return Timestamp.FromDateTime(date);
        }
        return Timestamp.GetCurrentTimestamp();
    }

    private string ConvertFromTimestamp(Timestamp timestamp)
    {
        return timestamp.ToDateTime().ToString();
    }
}