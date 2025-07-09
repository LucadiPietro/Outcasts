namespace Outcasts.Battle
{
    using DG.Tweening;
    using System;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Timeline;

    public sealed class FeedbackView : MonoBehaviour
    {
        [Serializable]
        struct FeedbackData
        {
            public string Text;
            public Material Material;
        }

        [SerializeField] TMP_Text m_Label;
        [SerializeField] SDictionary<NoteResult, FeedbackData> m_ResultToFeedbacks;

        [SerializeField] AnimationSettings m_Settings;

        Sequence m_DestroySequence;
        public void SetResult(NoteResult result)
        {
            var data = m_ResultToFeedbacks[result];
            m_Label.text = data.Text;
            m_Label.fontSharedMaterial = data.Material;

            m_Label.alpha = 0f;
            transform.localScale = Vector3.zero;

            m_DestroySequence?.Kill();
            m_DestroySequence = DOTween.Sequence();
            m_DestroySequence
                // Fade In
                .Append(transform.DOScale(1, m_Settings.FadeInDuration).SetEase(m_Settings.FadeInScaleCurve))
                .Join(m_Label.DOFade(1, m_Settings.FadeOutDuration * 0.5f))
                // Await
                .AppendInterval(m_Settings.AwaitDuration)
                // FadeOut
                .Append(transform.DOScale(1.5f, m_Settings.FadeOutDuration))
                .Join(m_Label.DOFade(0, m_Settings.FadeOutDuration))
                .OnComplete(() => Destroy(gameObject));
        }

        [Serializable]
        struct AnimationSettings
        {
            public float FadeInDuration;
            public AnimationCurve FadeInScaleCurve;
            public float AwaitDuration;
            public float FadeOutDuration;
            public AnimationCurve FadeOutScaleCurve;
        }
    }
}
