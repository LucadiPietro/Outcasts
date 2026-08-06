namespace Common.Cutscenes
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// Owns the lifecycle of a cutscene playback.
    /// A component can run only one playback at a time and emits each lifecycle
    /// event at most once for every accepted execution.
    /// </summary>
    public class CutsceneBase : MonoBehaviour
    {
        [Header("Playback")]
        [SerializeField] bool m_AllowPlayingMultipleTimes = false;
        [SerializeField] bool m_PlayOnStart = false;

        [Header("Events")]
        [SerializeField] UnityEvent m_Started = new UnityEvent();
        [SerializeField] UnityEvent m_Finished = new UnityEvent();
        [SerializeField] UnityEvent m_Skipped = new UnityEvent();

        Coroutine m_PlaybackCoroutine;
        bool m_IsPlaying;
        bool m_WasPlayed;
        int m_PlaybackVersion;

        protected virtual string kPrefKey => GetType().Name;

        public string CinematicKey => kPrefKey;
        public bool IsPlaying => m_IsPlaying;
        public bool WasPlayed => m_WasPlayed;

        public event UnityAction Started
        {
            add => m_Started.AddListener(value);
            remove => m_Started.RemoveListener(value);
        }

        public event UnityAction Finished
        {
            add => m_Finished.AddListener(value);
            remove => m_Finished.RemoveListener(value);
        }

        public event UnityAction Skipped
        {
            add => m_Skipped.AddListener(value);
            remove => m_Skipped.RemoveListener(value);
        }

        void Start()
        {
            if (m_PlayOnStart)
            {
                Play();
            }
        }

        void OnDisable()
        {
            Cancel();
        }

        /// <summary>
        /// Starts the cutscene without waiting for it.
        /// </summary>
        public void Play()
        {
            TryStartPlayback(out _);
        }

        /// <summary>
        /// Starts the cutscene and waits for that exact playback to end.
        /// </summary>
        public IEnumerator PlayAwaitable()
        {
            if (!TryStartPlayback(out int playbackVersion))
            {
                yield break;
            }

            while (m_IsPlaying && playbackVersion == m_PlaybackVersion)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Cancels the current playback without marking it as completed.
        /// </summary>
        public void Cancel()
        {
            CancelPlayback(invokeFinished: false);
        }

        /// <summary>
        /// Derived cutscenes apply their final state here.
        /// </summary>
        public virtual void FastForward()
        {
        }

        /// <summary>
        /// Stops playback and treats the interruption as a completed cutscene.
        /// Kept protected for compatibility with existing derived classes.
        /// </summary>
        protected void StopPlaying()
        {
            CancelPlayback(invokeFinished: true);
        }

        protected void CancelPlayback(bool invokeFinished)
        {
            if (!m_IsPlaying && m_PlaybackCoroutine == null)
            {
                return;
            }

            m_PlaybackVersion++;

            if (m_PlaybackCoroutine != null)
            {
                StopCoroutine(m_PlaybackCoroutine);
                m_PlaybackCoroutine = null;
            }

            bool wasPlaying = m_IsPlaying;
            m_IsPlaying = false;

            if (invokeFinished && wasPlaying)
            {
                m_WasPlayed = true;
                m_Finished?.Invoke();
            }
        }

        /// <summary>
        /// Completes a fast-forward operation without emitting Finished twice.
        /// </summary>
        protected void CompleteImmediately()
        {
            bool shouldNotify = m_IsPlaying || !m_WasPlayed;

            m_PlaybackVersion++;

            if (m_PlaybackCoroutine != null)
            {
                StopCoroutine(m_PlaybackCoroutine);
                m_PlaybackCoroutine = null;
            }

            m_IsPlaying = false;
            m_WasPlayed = true;

            if (shouldNotify)
            {
                m_Finished?.Invoke();
            }
        }

        protected virtual IEnumerator Sequence()
        {
            yield break;
        }

        [ContextMenu("Clear Play Count")]
        void ClearPlayCount()
        {
            m_WasPlayed = false;
        }

        bool TryStartPlayback(out int playbackVersion)
        {
            playbackVersion = m_PlaybackVersion;

            if (!CanBePlayed)
            {
                m_Skipped?.Invoke();
                return false;
            }

            m_IsPlaying = true;
            playbackVersion = ++m_PlaybackVersion;
            m_Started?.Invoke();
            m_PlaybackCoroutine = StartCoroutine(RunSequence(playbackVersion));
            return true;
        }

        IEnumerator RunSequence(int playbackVersion)
        {
            IEnumerator sequence = null;

            try
            {
                sequence = Sequence();

                if (sequence != null)
                {
                    while (true)
                    {
                        bool hasNext;

                        try
                        {
                            hasNext = sequence.MoveNext();
                        }
                        catch (Exception exception)
                        {
                            Debug.LogException(exception, this);
                            break;
                        }

                        if (!hasNext)
                        {
                            break;
                        }

                        yield return sequence.Current;
                    }
                }
            }
            finally
            {
                (sequence as IDisposable)?.Dispose();
            }

            if (!m_IsPlaying || playbackVersion != m_PlaybackVersion)
            {
                yield break;
            }

            m_PlaybackCoroutine = null;
            m_IsPlaying = false;
            m_WasPlayed = true;
            m_Finished?.Invoke();
        }

        bool CanBePlayed =>
            !m_IsPlaying && (m_AllowPlayingMultipleTimes || !m_WasPlayed);
    }
}
