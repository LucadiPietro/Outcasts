namespace Common.Cutscenes.Commands
{
    using System;
    using System.Collections;
    using UnityEngine;

    [Serializable]
    [AddTypeMenu("Unity Event")]
    public sealed class UnityEvent : ICinematicCommand
    {
        [SerializeField] UnityEngine.Events.UnityEvent m_Executed =
            new UnityEngine.Events.UnityEvent();

        public bool ShouldWaitEnd => false;

        public void Execute()
        {
            m_Executed?.Invoke();
        }

        public IEnumerator ExecuteAwaitable()
        {
            Execute();
            yield break;
        }

        public void FastForward()
        {
            Execute();
        }
    }
}
