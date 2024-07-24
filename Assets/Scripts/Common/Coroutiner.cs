namespace Common
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Thanks to this class, you can start and stop coroutines statically without needing a reference to a MonoBehaviour.
    /// Just call Coroutiner.Start and Coroutiner.Stop.
    /// 
    /// Only use this at runtime!
    /// </summary>
    public sealed class Coroutiner : MonoBehaviour
    {
        /// <summary>
        /// Starts a coroutine and returns a reference to it
        /// </summary>
        public static Coroutine Start(IEnumerator enumerator) => Instance.StartCoroutine(enumerator);
        /// <summary>
        /// Stops a coroutine that was started by the Coroutiner
        /// </summary>
        public static void Stop(Coroutine coroutine) => Instance.StopCoroutine(coroutine);

        static Coroutiner s_Instance = default;
        static Coroutiner Instance
        {
            get
            {
                if (s_IsQuitting) return null;

                if (s_Instance == null) s_Instance = FindObjectOfType<Coroutiner>();
                if (s_Instance == null)
                {
                    var gameObject = new GameObject("Coroutiner");
                    s_Instance = gameObject.AddComponent<Coroutiner>();
                    DontDestroyOnLoad(gameObject);
                }
                return s_Instance;
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            s_Instance = null;
            s_IsQuitting = false;
        }
        static bool s_IsQuitting = false;
        void OnApplicationQuit() => s_IsQuitting = true;
    }
}
