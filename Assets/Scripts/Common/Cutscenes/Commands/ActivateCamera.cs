namespace Common.Cutscenes.Commands
{
    using Cinemachine;
    using System;
    using System.Collections;
    using UnityEngine;

    [Serializable]
    public sealed class ActivateCamera : ICinematicCommand
    {
        [SerializeField] CinemachineVirtualCamera m_Camera;
        [SerializeField] bool m_ShouldWaitEnd = default;
        public bool ShouldWaitEnd => m_ShouldWaitEnd;

        public void Execute()
        {
            CinemachineHelper.Instance.SwitchToCamera(m_Camera);
        }
        public IEnumerator ExecuteAwaitable()
        {
            yield return CinemachineHelper.Instance.SwitchToCameraAwaitable(m_Camera);
        }
    }
}