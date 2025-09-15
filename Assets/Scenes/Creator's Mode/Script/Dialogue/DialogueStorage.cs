using System.Collections.Generic;
using UnityEngine;

public static class DialogueStorage
{
    // Convenience: get dialogues for the current story
    private static List<DialogueLine> GetStoryDialogues()
    {
        var story = StoryManager.Instance.GetCurrentStory();
        if (story == null)
        {
            Debug.LogWarning("⚠️ No current story set in StoryManager.");
            return null;
        }

        if (story.dialogues == null)
            story.dialogues = new List<DialogueLine>();

        return story.dialogues;
    }

    public static void AddDialogue(string name, string text)
    {
        var dialogues = GetStoryDialogues();
        if (dialogues == null) return;

        dialogues.Add(new DialogueLine(name, text));
    }

    public static void ClearDialogues()
    {
        var dialogues = GetStoryDialogues();
        if (dialogues == null) return;

        dialogues.Clear();
    }

    public static void DeleteDialogue(int index)
    {
        var dialogues = GetStoryDialogues();
        if (dialogues == null) return;

        if (index >= 0 && index < dialogues.Count)
            dialogues.RemoveAt(index);
    }

    public static void EditDialogue(int index, string newName, string newText)
    {
        var dialogues = GetStoryDialogues();
        if (dialogues == null) return;

        if (index >= 0 && index < dialogues.Count)
        {
            dialogues[index].characterName = newName;
            dialogues[index].dialogueText = newText;
        }
    }

    public static List<DialogueLine> GetAllDialogues()
    {
        var dialogues = GetStoryDialogues();
        return dialogues ?? new List<DialogueLine>();
    }
}
