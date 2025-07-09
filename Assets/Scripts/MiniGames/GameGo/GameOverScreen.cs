namespace Outcasts.Minigames.GameGo
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// A screen with a text message and a restart button
    /// </summary>
    public sealed class GameOverScreen : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI m_FeedbackLabel;
        [SerializeField] Image m_Background;
        [SerializeField] Color m_WinColor;
        [SerializeField] Color m_FailColor;

        GameState m_State;
        public GameState State => m_State;

        public void Show(GameState gameState, string message = "")
        {
            m_State = gameState;

            m_FeedbackLabel.text = message;
            m_FeedbackLabel.gameObject.SetActive(!string.IsNullOrEmpty(message));

            switch (gameState)
            {
                case GameState.Won:
                    m_Background.color = m_WinColor;
                    break;
                case GameState.Lost:
                    m_Background.color = m_FailColor;
                    break;
                default:
                    break;
            }

            gameObject.SetActive(true);
        }
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
