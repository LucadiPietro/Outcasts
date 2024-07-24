namespace Common.Cutscenes.Commands
{
    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Makes a Movable character Walk/Run/Crouch until it reaches a target Transform
    /// </summary>
    [Serializable]
    [AddTypeMenu("Play Animation")]
    public sealed class PlayAnimation : ICinematicCommand
    {
        [SerializeField] AwaitableAnimation m_Animation;

        [SerializeField] bool m_ShouldWaitEnd = true;
        public bool ShouldWaitEnd => m_ShouldWaitEnd;

        public void Execute() => m_Animation.Execute();
        public IEnumerator ExecuteAwaitable()
        {
            yield return m_Animation.ExecuteAwaitable();
        }
        public void FastForward() { }
    }
}