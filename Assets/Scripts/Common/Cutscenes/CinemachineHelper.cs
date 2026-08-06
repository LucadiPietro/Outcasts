namespace Common.Cutscenes
{
    using Cinemachine;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Centralizes camera priority changes and makes interrupted blends deterministic.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CinemachineHelper : MonoBehaviour
    {
        [SerializeField] int m_ActiveCameraPriority = 100;
        [SerializeField] int m_InactiveCameraPriority = 10;

        static CinemachineHelper s_Instance;

        CinemachineBrain m_Brain;
        ICinemachineCamera m_ActiveCamera;
        CinemachineVirtualCamera[] m_SceneCameras;
        int m_SwitchRequestId;

        public static CinemachineHelper Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = FindObjectOfType<CinemachineHelper>();
                }

                return s_Instance;
            }
        }

        /// <summary>
        /// Initializes singleton state and camera caches.
        /// </summary>
        void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Debug.LogWarning("Duplicate CinemachineHelper disabled.", this);
                enabled = false;
                return;
            }

            s_Instance = this;
            RefreshCache();
        }

        /// <summary>
        /// Refreshes the active camera after scene objects become enabled.
        /// </summary>
        void OnEnable()
        {
            RefreshCache();
        }

        /// <summary>
        /// Clears the static reference when this instance is destroyed.
        /// </summary>
        void OnDestroy()
        {
            if (s_Instance == this)
            {
                s_Instance = null;
            }
        }

        /// <summary>
        /// Switches camera without waiting for the blend.
        /// </summary>
        public void SwitchToCamera(ICinemachineCamera targetCamera)
        {
            StartCoroutine(SwitchToCameraAwaitable(targetCamera));
        }

        /// <summary>
        /// Switches camera and waits until the blend has completed or a newer request
        /// supersedes this one.
        /// </summary>
        public IEnumerator SwitchToCameraAwaitable(ICinemachineCamera targetCamera)
        {
            if (targetCamera == null)
            {
                Debug.LogWarning("Camera switch ignored because the target is null.", this);
                yield break;
            }

            RefreshBrainIfNeeded();

            int requestId = ++m_SwitchRequestId;
            ICinemachineCamera previousCamera = m_ActiveCamera;

            if (previousCamera == targetCamera &&
                (m_Brain == null || m_Brain.ActiveVirtualCamera == targetCamera))
            {
                yield break;
            }

            ApplyPriorities(targetCamera);
            m_ActiveCamera = targetCamera;

            // Cinemachine resolves priority changes during LateUpdate.
            yield return new WaitForEndOfFrame();

            if (m_Brain == null)
            {
                yield break;
            }

            float expectedDuration = GetBlendDuration(previousCamera, targetCamera);
            float timeout = Mathf.Max(1f, expectedDuration + 2f);
            float elapsed = 0f;

            while (requestId == m_SwitchRequestId && elapsed < timeout)
            {
                bool targetIsLive = m_Brain.ActiveVirtualCamera == targetCamera;
                if (targetIsLive && !m_Brain.IsBlending)
                {
                    break;
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (requestId == m_SwitchRequestId && elapsed >= timeout)
            {
                Debug.LogWarning(
                    $"Camera blend to '{targetCamera.Name}' exceeded the {timeout:0.##}s safety timeout.",
                    this);
            }
        }

        /// <summary>
        /// Event-friendly camera switch entry point.
        /// </summary>
        public void SwitchToCameraExposed(CinemachineVirtualCamera targetCamera)
        {
            SwitchToCamera(targetCamera);
        }

        /// <summary>
        /// Applies the target priority immediately. Used by cutscene fast-forward.
        /// </summary>
        public void CutToCamera(ICinemachineCamera targetCamera)
        {
            if (targetCamera == null)
            {
                return;
            }

            ++m_SwitchRequestId;
            ApplyPriorities(targetCamera);
            m_ActiveCamera = targetCamera;
        }

        /// <summary>
        /// Rebuilds scene-level camera and brain references.
        /// </summary>
        void RefreshCache()
        {
            m_SceneCameras = FindObjectsOfType<CinemachineVirtualCamera>(includeInactive: true);
            RefreshBrainIfNeeded();

            if (m_Brain != null && m_Brain.ActiveVirtualCamera != null)
            {
                m_ActiveCamera = m_Brain.ActiveVirtualCamera;
            }
            else
            {
                m_ActiveCamera = FindHighestPriorityCamera();
            }

            if (m_ActiveCamera != null)
            {
                ApplyPriorities(m_ActiveCamera);
            }
        }

        /// <summary>
        /// Resolves the CinemachineBrain from the tagged main camera.
        /// </summary>
        void RefreshBrainIfNeeded()
        {
            if (m_Brain != null)
            {
                return;
            }

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                m_Brain = mainCamera.GetComponent<CinemachineBrain>();
            }
        }

        /// <summary>
        /// Ensures that exactly one virtual camera owns the active priority.
        /// </summary>
        void ApplyPriorities(ICinemachineCamera targetCamera)
        {
            if (m_SceneCameras == null || m_SceneCameras.Length == 0)
            {
                m_SceneCameras = FindObjectsOfType<CinemachineVirtualCamera>(includeInactive: true);
            }

            for (int i = 0; i < m_SceneCameras.Length; i++)
            {
                CinemachineVirtualCamera camera = m_SceneCameras[i];
                if (camera == null)
                {
                    continue;
                }

                camera.Priority = ReferenceEquals(camera, targetCamera)
                    ? m_ActiveCameraPriority
                    : m_InactiveCameraPriority;
            }

            // Supports other ICinemachineCamera implementations without affecting the
            // virtual-camera normalization above.
            targetCamera.Priority = m_ActiveCameraPriority;
        }

        /// <summary>
        /// Finds the current priority winner when the brain is not ready yet.
        /// </summary>
        ICinemachineCamera FindHighestPriorityCamera()
        {
            ICinemachineCamera best = null;
            int bestPriority = int.MinValue;

            if (m_SceneCameras == null)
            {
                return null;
            }

            for (int i = 0; i < m_SceneCameras.Length; i++)
            {
                CinemachineVirtualCamera camera = m_SceneCameras[i];
                if (camera != null && camera.Priority > bestPriority)
                {
                    best = camera;
                    bestPriority = camera.Priority;
                }
            }

            return best;
        }

        /// <summary>
        /// Returns the configured Cinemachine blend duration for a camera pair.
        /// </summary>
        float GetBlendDuration(ICinemachineCamera from, ICinemachineCamera to)
        {
            if (m_Brain == null)
            {
                return 0f;
            }

            if (m_Brain.m_CustomBlends == null)
            {
                return m_Brain.m_DefaultBlend.BlendTime;
            }

            string fromName = from != null
                ? from.Name
                : CinemachineBlenderSettings.kBlendFromAnyCameraLabel;
            string toName = to != null
                ? to.Name
                : CinemachineBlenderSettings.kBlendFromAnyCameraLabel;

            return m_Brain.m_CustomBlends
                .GetBlendForVirtualCameras(fromName, toName, m_Brain.m_DefaultBlend)
                .BlendTime;
        }
    }
}
