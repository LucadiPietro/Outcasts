using Common.Dialogues;
using System.Collections.Generic;
using UnityEngine;

public class RetroCastleDialogueManager : MonoBehaviour
{
    public DialogueController dialogueController;
    [SerializeField] List<DialogueData> m_Dialogues;

    public void StartDialague(int index)
    {
        var element = m_Dialogues[index];
        dialogueController.PlayDialogue(element);
    }
}