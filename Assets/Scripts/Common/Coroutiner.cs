namespace Common
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Lightweight persistent coroutine host for serializable commands that are not
    /// MonoBehaviours. It is recreated safely after domain reloads and scene changes.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Coroutiner : MonoBehaviour
    {
        static Coroutiner s_Instance;
        static bool s_IsQuitting;

        /// <summary>
        /// Starts a routine on the persistent host. Returns null during shutdown or
        /// when the supplied enumerator is null.
        /// </summary>
        public static Coroutine Start(IEnumerator enumerator)
        {
            if (enumerator == null || s_IsQuitting)
            {
                return null;
            }

            Coroutiner instance = Instance;
            return instance != null ? instance.StartCoroutine(enumerator) : null;
        }

        /// <summary>
        /// Stops a routine previously started by this host.
        /// </summary>
        public static void Stop(Coroutine coroutine)
        {
            if (coroutine == null || s_IsQuitting || s_Instance == null)
            {
                return;
            }

            s_Instance.StopCoroutine(coroutine);
        }

        /// <summary>
        /// Clears static state when entering play mode without a domain reload.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            s_Instance = null;
            s_IsQuitting = false;
        }

        /// <summary>
        /// Enforces a single persistent host instance.
        /// </summary>
        void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Clears the singleton only when this object owns it.
        /// </summary>
        void OnDestroy()
        {
            if (s_Instance == this)
            {
                s_Instance = null;
            }
        }

        void OnApplicationQuit()
        {
            s_IsQuitting = true;
        }

        static Coroutiner Instance
        {
            get
            {
                if (s_IsQuitting)
                {
                    return null;
                }

                if (s_Instance == null)
                {
                    s_Instance = FindObjectOfType<Coroutiner>();
                }

                if (s_Instance == null)
                {
                    var host = new GameObject("[Runtime] Coroutiner");
                    s_Instance = host.AddComponent<Coroutiner>();
                }

                return s_Instance;
            }
        }
    }
}
