namespace Common.Cutscenes.Commands
{
    using System;
    using System.Collections;
    using UnityEngine;

    [Serializable]
    [AddTypeMenu("Unity Event")]
    public sealed class UnityEvent : ICinematicCommand
    {
        public bool ShouldWaitEnd => false;

        [SerializeField] UnityEngine.Events.UnityEvent m_Executed;

        public void Execute() => m_Executed.Invoke();
        public IEnumerator ExecuteAwaitable()
        {
            Execute();
            yield break;
        }
        public void FastForward() => Execute();
    }
}
