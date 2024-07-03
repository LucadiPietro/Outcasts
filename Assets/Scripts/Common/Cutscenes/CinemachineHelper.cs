namespace Common.Cutscenes
{
    using Cinemachine;
    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Simplifies Cinemachine's interface so that every inactive camera has the same low priority and only one active camera with higher priority exists
    /// </summary>
    public sealed class CinemachineHelper : MonoBehaviour
    {
        [SerializeField] int m_ActiveCameraPriority = 100;
        [SerializeField] int m_InactiveCameraPriority = 10;

        /// <summary>
        /// Ensures the targetCamera becomes active if it isn't already
        /// </summary>
        public void SwitchToCamera(ICinemachineCamera targetCamera)
        {
            if (targetCamera == m_ActiveCamera) return;
            if (m_ActiveCamera != null && Brain.IsBlending) throw new InvalidOperationException($"You can't Switch to a different camera while a camera blend is already in progress");

            if (m_ActiveCamera != null) m_ActiveCamera.Priority = m_InactiveCameraPriority;
            m_ActiveCamera = targetCamera;
            if (m_ActiveCamera != null) m_ActiveCamera.Priority = m_ActiveCameraPriority;
        }
        /// <summary>
        /// Ensures the targetCamera becomes active if it isn't already and yields until the camera blending is complete
        /// </summary>
        public IEnumerator SwitchToCameraAwaitable(ICinemachineCamera targetCamera)
        {
            if (m_ActiveCamera == targetCamera) yield break;
            SwitchToCamera(targetCamera);

            // This is needed because Brain.IsBlending is only reliable after LateUpdate
            yield return new WaitForEndOfFrame();

            yield return kWaitForCameraBlend;

            // This is needed because Brain.IsBlending is only reliable after LateUpdate
            yield return new WaitForEndOfFrame();
        }
        ICinemachineCamera m_ActiveCamera;

        static CinemachineHelper s_Instance = default;
        public static CinemachineHelper Instance
        {
            get
            {
                if (s_Instance == null) s_Instance = FindObjectOfType<CinemachineHelper>();
                return s_Instance;
            }
        }
        void Awake()
        {
            s_Instance = this;
            m_ActiveCamera = Brain.ActiveVirtualCamera;
        }

        static CinemachineBrain m_Brain;
        static CinemachineBrain Brain
        {
            get
            {
                if (m_Brain == null) m_Brain = Camera.main.GetComponent<CinemachineBrain>();
                return m_Brain;
            }
        }
        static readonly WaitWhile kWaitForCameraBlend = new WaitWhile(() => Brain.IsBlending);
    }
}