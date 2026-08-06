namespace Common.Cutscenes.Commands
{
    using Cinemachine;
    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Activates a Cinemachine virtual camera through the scene camera coordinator.
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
            CinemachineHelper helper = CinemachineHelper.Instance;
            if (helper == null || m_Camera == null)
            {
                Debug.LogWarning("ActivateCamera skipped because its helper or camera is missing.");
                return;
            }

            helper.SwitchToCamera(m_Camera);
        }

        public IEnumerator ExecuteAwaitable()
        {
            CinemachineHelper helper = CinemachineHelper.Instance;
            if (helper == null || m_Camera == null)
            {
                Debug.LogWarning("ActivateCamera skipped because its helper or camera is missing.");
                yield break;
            }

            yield return helper.SwitchToCameraAwaitable(m_Camera);
        }

        public void FastForward()
        {
            CinemachineHelper helper = CinemachineHelper.Instance;
            if (helper != null && m_Camera != null)
            {
                helper.CutToCamera(m_Camera);
            }
        }
    }
}
