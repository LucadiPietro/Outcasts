namespace Common.Cutscenes
{
    using Cinemachine;
    using System.Collections;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Simplifies Cinemachine's interface so that every inactive camera has the same low priority and only one active camera with higher priority exists.
    /// You can call SwitchToCamera or StartCoroutine(SwitchToCameraAwaitable) as much as you like and this helper will make sure only the last call is active.
    /// If a user is yielding SwitchToCameraAwaitable and SwitchToCamera is called again, the previous routine will return early as if it's canceled by the new one.
    /// </summary>
    public sealed class CinemachineHelper : MonoBehaviour
    {
        [SerializeField] int m_ActiveCameraPriority = 100;
        [SerializeField] int m_InactiveCameraPriority = 10;

        void OnEnable()
        {
            // Currently active camera is the one with highest priority
            m_ActiveCamera = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None).OrderByDescending(x => x.Priority).FirstOrDefault();
        }

        /// <summary>
        /// Ensures the targetCamera becomes active if it isn't already
        /// </summary>
        public void SwitchToCamera(ICinemachineCamera targetCamera)
        {
            StartCoroutine(SwitchToCameraAwaitable(targetCamera));
        }

        int m_CurrentBlendId = -1;
        /// <summary>
        /// Ensures the targetCamera becomes active if it isn't already and yields until the camera blending is complete
        /// </summary>
        public IEnumerator SwitchToCameraAwaitable(ICinemachineCamera targetCamera)
        {

            // We assign an increasing blendId to each blend
            // This is because a blend can be interrupted by the request of a new blend
            // If this happens, we'll be able to compare IDs and if the current blend is not the latest, we can interrupt it early
            int blendId = ++m_CurrentBlendId;

            if (m_ActiveCamera == targetCamera) yield break;

            // If there's a previous blend happening, we wait for its completion before proceeding
            if (m_ActiveCamera != null && m_IsBlending)
            {
                //Debug.Log($"Blend to {targetCamera.Name} waiting until previous blend is canceled...");
                m_ShouldCancelBlend = true;
                yield return new WaitWhile(() => m_IsBlending);

                //Debug.Log($"Blend to {targetCamera.Name} can resume as the previous blend has been canceled");

                // If this is not the latest blend request, we return early as there can only ever be one camera considered ACTIVE
                if (blendId != m_CurrentBlendId)
                {
                    //Debug.Log($"Blend to {targetCamera.Name} is not the latest one anymore, so it will stop (currentId: {blendId}; latestId = {m_CurrentBlendId}).");
                    yield break;
                }
            }

            //Debug.Log($"Blend to {targetCamera.Name} starts");
            m_IsBlending = true;

            ICinemachineCamera oldCamera = null;
            // Switch camera priorities
            if (m_ActiveCamera != null)
            {
                oldCamera = m_ActiveCamera;
                m_ActiveCamera.Priority = m_InactiveCameraPriority;
            }
            m_ActiveCamera = targetCamera;
            if (m_ActiveCamera != null) m_ActiveCamera.Priority = m_ActiveCameraPriority;

            // This is needed because cinemachine's brain is only reliable after a LateUpdate
            yield return new WaitForEndOfFrame();

            float elapsed = 0f;
            float blendDuration = GetBlendDuration(oldCamera, targetCamera);

            // Either wait until the blend has been canceled or to the end of its duration
            while (true)
            {
                // If the current blend is canceled by a new one, return early
                if (m_ShouldCancelBlend)
                {
                    //Debug.Log($"Blend to {targetCamera.Name} canceled");
                    m_ShouldCancelBlend = false;
                    break;
                }

                // If the entire blend duration has passed, we break out of the loop to return
                if (elapsed < blendDuration)
                {
                    yield return null;
                    elapsed += Time.deltaTime;
                }
                else
                {
                    // This is needed because cinemachine's brain is only reliable after a LateUpdate
                    yield return new WaitForEndOfFrame();

                    break;
                }
            }

            //Debug.Log($"Blend to {targetCamera.Name} over");
            m_IsBlending = false;
        }

        /// <summary>
        /// Same as SwitchToCamera but callable from events
        /// </summary>
        public void SwitchToCameraExposed(CinemachineVirtualCamera targetCamera)
        {
            SwitchToCameraAwaitable(targetCamera);
        }

        float GetBlendDuration(ICinemachineCamera from, ICinemachineCamera to)
        {
            if (Brain.m_CustomBlends == null) return Brain.m_DefaultBlend.BlendTime;

            string fromName = (from != null) ? from.Name : CinemachineBlenderSettings.kBlendFromAnyCameraLabel;
            string toName = (to != null) ? to.Name : CinemachineBlenderSettings.kBlendFromAnyCameraLabel;
            float blendDuration = Brain.m_CustomBlends.GetBlendForVirtualCameras(fromName, toName, Brain.m_DefaultBlend).BlendTime;
            return blendDuration;
        }

        ICinemachineCamera m_ActiveCamera;

        bool m_IsBlending = false;
        bool m_ShouldCancelBlend = false;

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

        static readonly WaitWhile kWaitForCompletion = new WaitWhile(() => Brain.IsBlending || Instance.m_ShouldCancelBlend);
    }
}