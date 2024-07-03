namespace Common.Cutscenes.Commands
{
    using System;
    using System.Collections;
    using UnityEngine;

    [Serializable]
    public sealed class PlayAnimation : ICinematicCommand
    {
        [SerializeField] AwaitableAnimation m_Animation;

        [SerializeField] bool m_ShouldWaitEnd = default;
        public bool ShouldWaitEnd => m_ShouldWaitEnd;

        public void Execute() => m_Animation.Execute();
        public IEnumerator ExecuteAwaitable()
        {
            yield return m_Animation.ExecuteAwaitable();
        }
    }
}