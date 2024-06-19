namespace Minigames.GameGo
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Manager controlling the minigame. It can be used to start a new game here and listen to events to know when it's over
    /// </summary>
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] GuardBrain m_Guard;

        StealingGameDefinition m_GameDefinition;
        /// <summary>
        /// Definition of the current game being run
        /// </summary>
        public StealingGameDefinition GameDefinition => m_GameDefinition;

        GameState m_GameState = GameState.Waiting;
        /// <summary>
        /// State of the current or the last game played
        /// </summary>
        public GameState GameState => m_GameState;

        /// <summary>
        /// Starts a new game. You should listen to the GameWon or GameLost event if you need to know when the game has stopped
        /// </summary>
        public void StartGame(StealingGameDefinition gameData)
        {
            m_GameDefinition = gameData;

            // Characters
            m_Guard.StartPattern(gameData.GuardPattern);

            // Setup initial state
            m_RemainingTime = gameData.TotalTime;
            m_CurrentActionIndex = 0;
            m_GameState = GameState.Running;
            enabled = true;
            OnGameStarted();
        }

        #region Game Logic
        /// <summary>
        /// Performs the action and, based on the success, transitions to the next game state
        /// </summary>
        /// <returns>A summary of what happen during this action</returns>
        public ActionSummary PerformAction(StealAction actionPerformed)
        {
            var result = new ActionSummary(ActionToPerform, actionPerformed, m_Guard.IsFocusedOnStealer);

            // If you pressed anything while the guard is alert, you lose
            if (m_Guard.IsFocusedOnStealer) Lose(FailReason.CaughtByGuard);
            else // The guard is looking away
            {
                // Correct action
                if (ActionToPerform == actionPerformed) AdvanceGame();
                // Wrong action
                else Penalize();
            }

            OnActionPerformed(result);
            return result;
        }

        void AdvanceGame()
        {
            if (CompletedAllActions) Win();
            else m_CurrentActionIndex++;
        }
        void Win()
        {
            StopGame();
            m_GameState = GameState.Won;
            OnGameWon();
        }

        void Lose(FailReason reason)
        {
            StopGame();
            m_GameState = GameState.Lost;
            OnGameLost(reason);
        }
        void StopGame()
        {
            m_GameDefinition = null;
            m_Guard.StopPattern();
            enabled = false;
        }

        // Index for the current action to perform, inside the list of all actions
        int m_CurrentActionIndex;
        // Current action to perform
        StealAction ActionToPerform => m_GameDefinition.ActionsToPerform[m_CurrentActionIndex];
        // True if the player performed all actions correctly
        bool CompletedAllActions => m_CurrentActionIndex == m_GameDefinition.ActionsToPerform.Count - 1;
        #endregion

        #region TimeManagement
        float m_RemainingTime;
        /// <summary>
        /// How much time remains. If the player doesn't win before this time, the game is lost
        /// </summary>
        public float RemainingTime => m_RemainingTime;

        void Update() => DeductTime(Time.deltaTime);

        // Deduct time as a penalty for mistakes
        void Penalize() => DeductTime(m_GameDefinition.PenaltyForMistakes);
        // Deducts from the RemainingTime and if it reaches 0, the game is lost
        void DeductTime(float time)
        {
            m_RemainingTime = Mathf.Max(m_RemainingTime - time, 0);
            if (m_RemainingTime == 0) Lose(FailReason.OutOfTime);
        }
        #endregion

        #region Events
        Action m_GameStarted;
        public event Action GameStarted
        {
            add { m_GameStarted += value; }
            remove { m_GameStarted -= value; }
        }
        void OnGameStarted()
        {
            if (m_GameStarted != null) m_GameStarted();
        }

        Action<ActionSummary> m_ActionPerformed;
        public event Action<ActionSummary> ActionPerformed
        {
            add { m_ActionPerformed += value; }
            remove { m_ActionPerformed -= value; }
        }
        void OnActionPerformed(ActionSummary result)
        {
            if (m_ActionPerformed != null) m_ActionPerformed(result);
        }

        Action m_GameWon;
        public event Action GameWon
        {
            add { m_GameWon += value; }
            remove { m_GameWon -= value; }
        }
        void OnGameWon()
        {
            if (m_GameWon != null) m_GameWon();
        }

        Action<FailReason> m_GameLost;
        public event Action<FailReason> GameLost
        {
            add { m_GameLost += value; }
            remove { m_GameLost -= value; }
        }
        void OnGameLost(FailReason reason)
        {
            if (m_GameLost != null) m_GameLost(reason);
        }
        #endregion
    }
    public enum GameState
    {
        /// <summary>
        /// This only happens before the first game is started
        /// </summary>
        Waiting = -1,
        /// <summary>
        /// The game is currently in progress
        /// </summary>
        Running,
        /// <summary>
        /// The game is over and the player won
        /// </summary>
        Won,
        /// <summary>
        /// The game is over and the player lost
        /// </summary>
        Lost,
    }

    public enum StealAction
    {
        Up,
        Left,
        Down,
        Right,
    }
    /// <summary>
    /// The reason why the game was lost
    /// </summary>
    public enum FailReason
    {
        CaughtByGuard,
        OutOfTime,
    }

    [Serializable]
    public struct ActionSummary
    {
        [SerializeField] StealAction m_ActionToPerform;
        /// <summary>
        /// Action the player had to perform
        /// </summary>
        public StealAction ActionToPerform => m_ActionToPerform;

        [SerializeField] StealAction m_ActionPerformed;
        /// <summary>
        /// Action the player actually performed
        /// </summary>
        public StealAction ActionPerformed => m_ActionPerformed;

        [SerializeField] bool m_WasGuardAlerted;
        /// <summary>
        /// True if the guard was alerted during the action
        /// </summary>
        public bool WasGuardAlerted => m_WasGuardAlerted;

        public ActionSummary(StealAction actionToPerform, StealAction actionPerformed, bool wasGuardAlerted)
        {
            m_ActionToPerform = actionToPerform;
            m_ActionPerformed = actionPerformed;
            m_WasGuardAlerted = wasGuardAlerted;
        }
    }
}
