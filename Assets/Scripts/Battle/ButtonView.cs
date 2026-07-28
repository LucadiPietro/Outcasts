namespace Outcasts.Battle
{
    using UnityEngine;
    using UnityEngine.UI;

    namespace View
    {
        public sealed class ButtonView : MonoBehaviour
        {
            [SerializeField] Image m_Image;

            InputButton m_Button;
            public InputButton Button
            {
                get { return m_Button; }
                set 
                {
                    m_Button = value;
                    m_Image.sprite = value.Symbol;
                }
            }
        }
    }
}
