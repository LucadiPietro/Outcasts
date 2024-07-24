namespace Common.Cutscenes.Commands
{
    using NaughtyAttributes;
    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Waits the specified amount of time
    /// </summary>
    [Serializable]
    [AddTypeMenu("Pause")]
    public sealed class Pause : ICinematicCommand
    {
        [SerializeField, Tooltip("In Seconds"), AllowNesting] float m_Duration;
        public bool ShouldWaitEnd => true;

        // A non-awaitable pause does nothing
        public void Execute() { }
        public IEnumerator ExecuteAwaitable()
        {
            yield return new WaitForSeconds(m_Duration);
        }
        public void FastForward() { }
    }
}