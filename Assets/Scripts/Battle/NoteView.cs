namespace Outcasts.Battle
{
    using DG.Tweening;
    using LemonGames;
    using Outcasts.Battle.View;
    using UnityEngine;
    using UnityEngine.UI;

    public sealed class NoteView : MonoBehaviour
    {
        [SerializeField] Image m_Symbol;

        [SerializeField] FeedbackView m_FeedbackPrefab;
        void SpawnFeedback(NoteResult result)
        {
            if (result == NoteResult.Deactivated) return;

            var feedback = Instantiate(m_FeedbackPrefab, transform.position, Quaternion.identity, transform.parent);
            feedback.SetResult(result);
        }

        Note m_Note;
        public Note Note
        {
            get { return m_Note; }
            set 
            {
                m_Note = value;
                m_Symbol.sprite = BattleViewSystem.Instance.GetButtonData(value.Type, value.Lane).Symbol;
            }
        }

        void OnEnable()
        {
            if(BattleViewSystem.Instance.Horizontal) m_Line.transform.rotation = Quaternion.Euler(0, 0, 90);
        }

        [SerializeField] AudioSource m_ClapSfx;

        Vector3 m_StartPosition;
        Vector3 m_TargetPosition;
        double m_StartTime;
        [SerializeField] Image m_Line;
        public Vector3 TargetPosition => m_TargetPosition;

        public void SetDestination(Vector3 targetPosition, double travelTime)
        {
            m_StartPosition = transform.position;
            m_TargetPosition = targetPosition;

            m_StartTime = Note.Time - travelTime;
        }

        public void Destroy(NoteResult result)
        {
            SpawnFeedback(result);
            m_ClapSfx.Play();
            transform.DOScale(1.5f, 0.2f).SetEase(Ease.OutCubic);
            m_Symbol.DOFade(0, 0.2f).SetEase(Ease.OutCubic).OnComplete(DestroySilently);
            enabled = false;
        }
        void DestroySilently() => Destroy(gameObject);

        void Update()
        {
            double now = BattleManager.Instance.Elapsed;

            // NormalizedTime is 0 when the note is spawned and 1 when it reaches its target
            // Note it can be bigger than 1 when a note goes beyond its target
            float normalizedTime = (float)((now - m_StartTime) / (Note.Time - m_StartTime));
            // We don't want normalizedTime to be negative though, as notes whould never show ahead of the start position
            // This could happen for held notes where the end-note is spawned together with the start-note way before its normal spawn time
            if (normalizedTime < 0) normalizedTime = 0;

            // LerpUnclamped to allow the note to go beyond its target
            transform.position = Vector3.LerpUnclamped(m_StartPosition, m_TargetPosition, normalizedTime);

            // From start to halfway: alpha = 0
            // From halfway to end: alpha ramps up to 0.5f
            float alpha = Mathf.Max((Mathf.Clamp01(normalizedTime) - 0.5f), 0f);
            m_Line.color = BattleViewSystem.Instance.GetButtonData(Note.Type, Note.Lane).MainColor.With(a: alpha);

            // Self destroy after 1 second from hitting the target
            if(now > Note.Time + 1f) DestroySilently();
        }
    }
}
