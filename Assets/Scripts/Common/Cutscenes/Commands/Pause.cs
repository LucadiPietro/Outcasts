namespace Common.Cutscenes.Commands
{
    using System;
    using System.Collections;
    using UnityEngine;

    [Serializable]
    public sealed class Pause : ICinematicCommand
    {
        [SerializeField] float m_Duration;
        public bool ShouldWaitEnd => true;

        // A non-awaitable pause does nothing
        public void Execute() { }
        public IEnumerator ExecuteAwaitable()
        {
            yield return new WaitForSeconds(m_Duration);
        }
    }
}