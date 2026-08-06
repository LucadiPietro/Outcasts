namespace Common.Cutscenes.Commands
{
    using NaughtyAttributes;
    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Waits for the configured duration in scaled game time.
    /// </summary>
    [Serializable]
    [AddTypeMenu("Pause")]
    public sealed class Pause : ICinematicCommand
    {
        [SerializeField, Tooltip("In Seconds"), AllowNesting]
        float m_Duration;

        public bool ShouldWaitEnd => true;

        public void Execute()
        {
        }

        public IEnumerator ExecuteAwaitable()
        {
            float duration = Mathf.Max(0f, m_Duration);
            if (duration > 0f)
            {
                yield return new WaitForSeconds(duration);
            }
        }

        public void FastForward()
        {
        }
    }
}
