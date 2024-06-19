namespace Minigames.GameGo
{
    using UnityEngine;

    /// <summary>
    /// Controls the entirety of the GameGoScene. Now it's just a test class, later it will coordinate the minigame with dialogues, cutscenes, etc
    /// </summary>
    public sealed class SceneManager : MonoBehaviour
    {
        [SerializeField] StealingGameDefinition m_GameDefinition;
        [SerializeField] GameManager m_Manager;

        [SerializeField] GameOverScreen m_GameOverScreen;

        // On Start, start a minigame
        void Start()
        {
            m_Manager.GameWon += ShowWin;
            m_Manager.GameLost += ShowLost;
            m_Manager.StartGame(m_GameDefinition);
        }
        void ShowWin() => m_GameOverScreen.Show(GameState.Won, "You Won!");
        void ShowLost(FailReason reason) => m_GameOverScreen.Show(GameState.Lost, GetFeedbackMessage(reason));

        // Returns a text message with the reason why you lost the game.
        // TODO: Replace with a string database lookup
        string GetFeedbackMessage(FailReason reason)
        {
            switch (reason)
            {
                case FailReason.CaughtByGuard: return "You got caught!";
                case FailReason.OutOfTime: return "Time's up";
                default: return "";
            }
        }

        // Called by inspector events
        public void Restart()
        {
            m_GameOverScreen.Hide();
            m_Manager.StartGame(m_GameDefinition);
        }
    }
}
