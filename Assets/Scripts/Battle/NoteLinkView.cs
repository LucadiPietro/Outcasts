namespace Outcasts.Battle
{
    using LemonGames;
    using Outcasts.Battle.View;
    using UnityEngine;

    public sealed class NoteLinkView : MonoBehaviour
    {
        NoteView m_StartView;
        NoteView m_EndView;

        Note m_StartNote;
        Note m_EndNote;
        public void SetViews(NoteView start, NoteView end)
        {
            m_StartNote = start.Note;
            m_EndNote = end.Note;

            m_StartView = start;
            m_EndView = end;

            if (BattleViewSystem.Instance.Horizontal)
            {
                if (m_StartNote.Type == NoteType.Attack)
                {
                    m_Border.pivot = new Vector2(1, 0.5f);
                    m_Border.anchoredPosition = new Vector2(EndTransform.sizeDelta.x * 0.5f, 0);
                }
                else
                {
                    m_Border.pivot = new Vector2(0, 0.5f);
                    m_Border.anchoredPosition = new Vector2(-EndTransform.sizeDelta.y * 0.5f, 0);
                }
            }
            else
            {
                if (m_StartNote.Type == NoteType.Attack)
                {
                    m_Border.pivot = new Vector2(0.5f, 1f);
                    m_Border.anchoredPosition = new Vector2(0, EndTransform.sizeDelta.y * 0.5f);
                }
                else
                {
                    m_Border.pivot = new Vector2(0.5f, 0f);
                    m_Border.anchoredPosition = new Vector2(0, -EndTransform.sizeDelta.y * 0.5f);
                }
            }
        }

        public void Destroy() => Destroy(gameObject);

        RectTransform StartTransform => m_StartView != null ? m_StartView.transform as RectTransform : null;
        RectTransform EndTransform => m_EndView != null ? m_EndView.transform as RectTransform : null;

        [SerializeField] RectTransform m_Border;
        void LateUpdate()
        {
            if(StartTransform != null)
            {
                transform.position = StartTransform.position;
            }
            else
            {
                transform.position = m_EndView.TargetPosition;
            }

            if (BattleViewSystem.Instance.Horizontal)
            {
                var size = m_Border.sizeDelta.With(x: Mathf.Abs(m_Border.anchoredPosition.x * 2f) + Mathf.Abs(transform.position.x - EndTransform.position.x));
                m_Border.sizeDelta = size;
            }
            else
            {
                var size = m_Border.sizeDelta.With(y: Mathf.Abs(m_Border.anchoredPosition.y * 2f) + Mathf.Abs(transform.position.y - EndTransform.position.y));
                m_Border.sizeDelta = size;
            }
        }
    }
}
