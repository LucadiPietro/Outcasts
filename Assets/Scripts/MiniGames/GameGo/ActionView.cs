namespace Minigames.GameGo
{
    using DG.Tweening;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Represents a button to press during the minigame
    /// </summary>
    public sealed class ActionView : MonoBehaviour
    {
        [SerializeField] StealAction m_Action;
        public StealAction Action
        {
            get => m_Action;
            set => m_Action = value;
        }

        [SerializeField] LayoutElement m_UiElement;
        [SerializeField] CanvasGroup m_Canvas;

        /// <summary>
        /// Shows a feedback for a successful steal
        /// </summary>
        public void ShowSuccess()
        {
            m_Canvas.alpha = 0f;
            // TODO: Implement VFX
        }
        /// <summary>
        /// Shows a feedback for a mistake in stealing
        /// </summary>
        public void ShowError()
        {
            // TODO: Implement VFX
        }

        /// <summary>
        /// Makes the view bigger or smaller based on the sizePercentage; 0 is the smallest, 1 is full size
        /// </summary>
        public void SetSize(float sizePercentage, float animationDuration = 0)
        {
            sizePercentage = Mathf.Clamp01(sizePercentage);

            bool isImmediate = animationDuration == 0;

            m_UiElement.DOKill();
            // Inactive buttons are smaller
            var size = GetSize(sizePercentage);
            if (isImmediate)
            {
                m_UiElement.preferredWidth = size.x;
                m_UiElement.preferredHeight = size.y;
            }
            else m_UiElement.DOPreferredSize(size, animationDuration);
        }

        const float kPreviewScale = 0.5f;
        const float kSize = 150f;
        public static float FullSize => kSize;
        public static float ReducedSize => kSize * kPreviewScale;

        Vector2 GetSize(float sizePercentage)
        {
            float size = kSize * Mathf.Lerp(kPreviewScale, 1f, sizePercentage);
            return new Vector2(size, size);
        }

        void OnEnable() => SetSize(0);
        void OnDisable() => SetSize(1);
    }
}
