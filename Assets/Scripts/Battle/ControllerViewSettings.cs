namespace Outcasts.Battle
{
    using System;
    using UnityEngine;

    [CreateAssetMenu(fileName = "ControllerViewSettings", menuName = "Outcasts/Controller View Settings")]
    public sealed class ControllerViewSettings : ScriptableObject
    {
        #region Dpad
        [Header("Dpad Buttons")]

        [SerializeField] InputButton m_DpadRight;
        public InputButton DpadRight => m_DpadRight;

        [SerializeField] InputButton m_DpadUp;
        public InputButton DpadUp => m_DpadUp;

        [SerializeField] InputButton m_DpadLeft;
        public InputButton DpadLeft => m_DpadLeft;

        [SerializeField] InputButton m_DpadDown;
        public InputButton DpadDown => m_DpadDown;
        #endregion

        #region FaceButtons
        [Header("Face Buttons")]

        [SerializeField] InputButton m_East;
        public InputButton East => m_East;

        [SerializeField] InputButton m_North;
        public InputButton North => m_North;

        [SerializeField] InputButton m_West;
        public InputButton West => m_West;

        [SerializeField] InputButton m_South;
        public InputButton South => m_South;
        #endregion
    }

    [Serializable]
    public struct InputButton
    {
        [SerializeField] Sprite m_Symbol;
        public Sprite Symbol => m_Symbol;

        [SerializeField] Color m_MainColor;
        public Color MainColor => m_MainColor;

        // We can add HighlightColor and ShadedColor, if needed
    }
}
