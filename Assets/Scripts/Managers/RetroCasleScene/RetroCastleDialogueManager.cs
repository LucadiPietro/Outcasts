using Common.Dialogues;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Safe scene-level entry point for retro castle dialogue events.
/// </summary>
public sealed class RetroCastleDialogueManager : MonoBehaviour
{
    public DialogueController dialogueController;
    [SerializeField] List<DialogueData> m_Dialogues = new List<DialogueData>();

    /// <summary>
    /// Starts a dialogue by index after validating the controller and list entry.
    /// </summary>
    public void StartDialogue(int index)
    {
        if (dialogueController == null)
        {
            Debug.LogWarning("RetroCastleDialogueManager has no DialogueController.", this);
            return;
        }

        if (m_Dialogues == null || index < 0 || index >= m_Dialogues.Count)
        {
            Debug.LogWarning($"Invalid retro castle dialogue index: {index}.", this);
            return;
        }

        DialogueData dialogue = m_Dialogues[index];
        if (dialogue == null)
        {
            Debug.LogWarning($"Retro castle dialogue {index} is null.", this);
            return;
        }

        dialogueController.PlayDialogue(dialogue);
    }

    /// <summary>
    /// Backward-compatible alias retained for existing UnityEvent bindings.
    /// </summary>
    public void StartDialague(int index)
    {
        StartDialogue(index);
    }
}
