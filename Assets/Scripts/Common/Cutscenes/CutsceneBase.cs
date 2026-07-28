namespace Common.Cutscenes
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Events;

    public class CutsceneBase : MonoBehaviour
    {
        [SerializeField] bool m_AllowPlayingMultipleTimes = false;
        [SerializeField] bool m_PlayOnStart = false;
        void Start()
        {
            if (m_PlayOnStart) Play();
        }

        protected virtual string kPrefKey => GetType().Name;
        public string CinematicKey => kPrefKey;

        // TODO: Implement using a persistent save state, right now it always returns false
        public bool WasPlayed
        {
            get => false;
            private set
            {
                //Debug.LogWarning($"{nameof(WasPlayed)} is not implemented yet");
            }
        }

        public void Play() => StartCoroutine(PlayAwaitable());
        protected void StopPlaying()
        {
            if (m_IsPlaying)
            {
                if (m_SequenceCoroutine != null)
                {
                    StopCoroutine(m_SequenceCoroutine);
                    m_SequenceCoroutine = null;
                }

                m_IsPlaying = false;
                //CutsceneEvent.Trigger(this, CutsceneEventType.Finished);
                OnFinished();
            }
        }

        public virtual void FastForward() { }

        Coroutine m_SequenceCoroutine = default;
        bool m_IsPlaying = false;
        public IEnumerator PlayAwaitable()
        {
            if (CanBePlayed)
            {
                m_IsPlaying = true;
                //CutsceneEvent.Trigger(this, CutsceneEventType.Started);
                OnStarted();
                m_SequenceCoroutine = StartCoroutine(Sequence());
                yield return m_SequenceCoroutine;
                WasPlayed = true;
                StopPlaying();
            }
            else OnSkipped();
        }
        protected virtual bool CanBePlayed => (!WasPlayed && !m_IsPlaying) || m_AllowPlayingMultipleTimes;
        protected virtual IEnumerator Sequence() { yield break; }

        [SerializeField] UnityEvent m_Started;
        public event UnityAction Started
        {
            add { m_Started.AddListener(value); }
            remove { m_Started.RemoveListener(value); }
        }
        void OnStarted()
        {
            m_Started.Invoke();
        }

        [SerializeField] UnityEvent m_Finished = default;
        public event UnityAction Finished
        {
            add { m_Finished.AddListener(value); }
            remove { m_Finished.RemoveListener(value); }
        }
        void OnFinished()
        {
            m_Finished.Invoke();
        }

        [SerializeField] UnityEvent m_Skipped = default;
        public event UnityAction Skipped
        {
            add { m_Skipped.AddListener(value); }
            remove { m_Skipped.RemoveListener(value); }
        }
        void OnSkipped()
        {
            m_Skipped.Invoke();
        }

#if UNITY_EDITOR
        [ContextMenu("ClearPlayCount")]
        void ClearPlayCount() => WasPlayed = false;
#endif
    }

    //public struct CutsceneEvent
    //{
    //    public CutsceneBase Cutscene;
    //    public CutsceneEventType Type;
    //    public CutsceneEvent(CutsceneBase cutscene, CutsceneEventType newType)
    //    {
    //        Cutscene = cutscene;
    //        Type = newType;
    //    }
    //    static CutsceneEvent e;
    //    public static void Trigger(CutsceneBase cutscene, CutsceneEventType newType)
    //    {
    //        e.Cutscene = cutscene;
    //        e.Type = newType;
    //        GameEventManager.TriggerEvent(e);
    //    }
    //}
    //public enum CutsceneEventType
    //{
    //    Started,
    //    Finished,
    //}
}
