namespace Common.Cutscenes.Commands
{
    using Cinemachine;
    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Activates a Cinemachine camera
    /// </summary>
    [Serializable]
    [AddTypeMenu("ActivateCamera")]
    public sealed class ActivateCamera : ICinematicCommand
    {
        [SerializeField] CinemachineVirtualCamera m_Camera;
        [SerializeField] bool m_ShouldWaitEnd = true;
        public bool ShouldWaitEnd => m_ShouldWaitEnd;

        public void Execute()
        {
            CinemachineHelper.Instance.SwitchToCamera(m_Camera);
        }
        public IEnumerator ExecuteAwaitable()
        {
            yield return CinemachineHelper.Instance.SwitchToCameraAwaitable(m_Camera);
        }

        // TODO: Implement a proper cut to the new camera
        public void FastForward() => Execute();
    }
}
