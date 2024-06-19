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

        bool m_IsActive;
        public bool IsActive => m_IsActive;

        /// <summary>
        /// Makes the view Active or Inactive (Active means it's the current button to press)
        /// </summary>
        public void SetActive(bool isActive, float animationDuration = 0)
        {
            bool isImmediate = animationDuration == 0;
            m_IsActive = isActive;

            m_UiElement.DOKill();
            // Inactive buttons are smaller
            var size = GetSize(isActive);
            if (isImmediate)
            {
                m_UiElement.preferredWidth = size.x;
                m_UiElement.preferredHeight = size.y;
            }
            else m_UiElement.DOPreferredSize(size, animationDuration);
        }

        const float kPreviewScale = 0.6f;
        const float kSize = 150f;
        public static float FullSize => kSize;
        public static float ReducedSize => kSize * kPreviewScale;

        Vector2 GetSize(bool isActive)
        {
            var result = new Vector2(kSize, kSize);
            if (!isActive) result *= kPreviewScale;
            return result;
        }

        void OnEnable() => SetActive(false);
        void OnDisable() => SetActive(true);
    }
}
