using System.Collections.Generic;
using UnityEngine;

public static class DialogueStorage
{
    public static List<DialogueLine> dialogues = new List<DialogueLine>();

    public static void AddDialogue(string name, string text)
    {
        dialogues.Add(new DialogueLine(name, text));
    }

    public static void ClearDialogues()
    {
        dialogues.Clear();
    }

    public static void DeleteDialogue(int index)
    {
        if (index >= 0 && index < dialogues.Count)
            dialogues.RemoveAt(index);
    }

    public static void EditDialogue(int index, string newName, string newText)
    {
        if (index >= 0 && index < dialogues.Count)
        {
            dialogues[index].characterName = newName;
            dialogues[index].dialogueText = newText;
        }
    }
}
