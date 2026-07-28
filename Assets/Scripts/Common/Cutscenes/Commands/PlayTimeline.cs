namespace Common.Cutscenes.Commands
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Playables;

    [Serializable]
    [AddTypeMenu("PlayTimeline")]
    public sealed class PlayTimeline : ICinematicCommand
    {
        [SerializeField] PlayableDirector m_TimelineInstance;
        [SerializeField] bool m_ShouldWaitEnd = true;
        public bool ShouldWaitEnd => m_ShouldWaitEnd;

        public void Execute()
        {
            m_TimelineInstance.Play();
        }

        public IEnumerator ExecuteAwaitable()
        {
            Execute();
            yield return new WaitForSeconds((float)m_TimelineInstance.duration);
        }
        public void FastForward()
        {
            m_TimelineInstance.time = m_TimelineInstance.duration;
        }
    }
}
