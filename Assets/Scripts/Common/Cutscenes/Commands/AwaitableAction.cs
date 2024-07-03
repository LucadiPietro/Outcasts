namespace Common.Cutscenes.Commands
{
    using System;
    using System.Collections;
    using UnityEngine;

    [Serializable]
    public sealed class AwaitableAction : ICinematicCommand
    {
        [SerializeField] AwaitableActionBase m_Action;

        [SerializeField] bool m_ShouldWaitEnd = default;
        public bool ShouldWaitEnd => m_ShouldWaitEnd;

        public void Execute() => m_Action.Execute();
        public IEnumerator ExecuteAwaitable()
        {
            yield return m_Action.ExecuteAwaitable();
        }
    }
}