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
}
