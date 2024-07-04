namespace Common.Cutscenes.Commands
{
    using Common.Dialogues;
    using System;
    using System.Collections;
    using UnityEngine;

    [Serializable]
    public sealed class PlayDialogue : ICinematicCommand
    {
        [SerializeField] DialogueData m_Dialogue;
        [SerializeField] bool m_ShouldWaitEnd = default;
        public bool ShouldWaitEnd => m_ShouldWaitEnd;

        public void Execute()
        {
            DialogueController.instance.PlayDialogue(m_Dialogue);
        }
        public IEnumerator ExecuteAwaitable()
        {
            yield return DialogueController.instance.PlayDialogueAwaitable(m_Dialogue);

        }
    }
}