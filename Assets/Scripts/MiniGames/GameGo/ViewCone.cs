namespace Minigames.GameGo
{
    using DG.Tweening;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// You can orient this towards a target, change it's appearance and test if a point is inside the cone
    /// </summary>
    public sealed class ViewCone : MonoBehaviour
    {
        [SerializeField] Image m_Image;
        [SerializeField] RectTransform m_Pivot;

        /// <summary>
        /// Use this to change the angle width, the radius and the color of the cone
        /// </summary>
        public void ApplySettings(float viewAngle, float viewRadius, Color color)
        {
            m_ViewAngle = viewAngle;
            m_ViewRadius = viewRadius;
            m_Color = color;

            UpdateView(0.2f);
        }

        [SerializeField] float m_ViewRadius;
        public float ViewRadius => m_ViewRadius;

        [SerializeField] float m_ViewAngle;
        public float ViewAngle => m_ViewAngle;

        [SerializeField] Color m_Color;
        public Color Color => m_Color;

        Vector2 m_Forward;
        /// <summary>
        /// Controls the direction of the cone
        /// </summary>
        public Vector2 Forward
        {
            get { return m_Forward; }
            set
            {
                m_Forward = value;

                float forwardAngle = Vector2.SignedAngle(Vector2.right, Forward);
                m_Pivot.localRotation = Quaternion.Euler(0, 0, forwardAngle);
            }
        }

        /// <summary>
        /// Returns true if the given point is inside the view cone
        /// </summary>
        public bool IsVisible(Vector2 targetPoint)
        {
            Vector2 delta = targetPoint - (Vector2)m_Pivot.position;
            float angleToTarget = Vector2.Angle(Forward, delta);

            // The ViewAngle is divided by 2 because angleToTarget is not signed
            // If ViewAngle is 60 degrees, we see 30 degrees to the left and 30 degrees to the right
            bool isInFront = angleToTarget <= ViewAngle * 0.5f;
            bool isWithinRange = delta.magnitude <= ViewRadius;

            return isInFront && isWithinRange;
        }

        RectTransform m_ImageTransform;
        RectTransform ImageTransform
        {
            get
            {
                if (m_ImageTransform == null) m_ImageTransform = m_Image.transform as RectTransform;
                return m_ImageTransform;
            }
        }

        float m_CurrentViewAngle;
        float GetViewAngle() => m_CurrentViewAngle;
        void SetViewAngle(float value)
        {
            m_CurrentViewAngle = value;
            ImageTransform.localRotation = Quaternion.Euler(0, 0, -90f + (m_CurrentViewAngle * 0.5f));
            m_Image.fillAmount = m_CurrentViewAngle / 360f;
        }

        float m_CurrentRadius;
        float GetRadius() => m_CurrentRadius;
        void SetRadius(float value)
        {
            m_CurrentRadius = value;
            ImageTransform.sizeDelta = new Vector2(m_CurrentRadius, m_CurrentRadius) * 2f;
        }

        void UpdateView(float animationDuration = -1)
        {
            bool isImmediate = animationDuration <= 0;
            if (isImmediate)
            {
                SetViewAngle(ViewAngle);
                SetRadius(ViewRadius);
                m_Image.color = Color;
            }
            else
            {
                DOTween.Kill(this);
                DOTween.To(GetViewAngle, SetViewAngle, ViewAngle, animationDuration).SetTarget(this);
                DOTween.To(GetRadius, SetRadius, ViewRadius, animationDuration).SetTarget(this);
                m_Image.DOColor(Color, animationDuration).SetTarget(this);
            }
        }

#if UNITY_EDITOR
        void OnValidate() => UpdateView();
#endif
    }
}
