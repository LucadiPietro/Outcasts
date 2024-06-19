namespace Minigames.GameGo
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Stores all the data necessary for the minigame
    /// </summary>
    [CreateAssetMenu(fileName = "StealingGameDefinition", menuName = "Minigames/Game Go/StealingGameDefinition")]
    public sealed class StealingGameDefinition : ScriptableObject
    {
        [SerializeField] GuardPattern m_GuardPattern;
        /// <summary>
        /// The instructions for the guard to follow
        /// </summary>
        public GuardPattern GuardPattern => m_GuardPattern;

        [SerializeField] List<StealAction> m_ActionsToPerform;
        /// <summary>
        /// The list of actions to perform. If you perform all of these correctly, you win the game
        /// </summary>
        public IReadOnlyList<StealAction> ActionsToPerform => m_ActionsToPerform;

        [SerializeField] float m_TotalTime = 60f;
        /// <summary>
        /// If you don't perform all the actions during this time, you lose the game
        /// </summary>
        public float TotalTime => m_TotalTime;


        [SerializeField] float m_PenaltyForMistakes = 5f;
        /// <summary>
        /// When you make a mistake, this much time is removed from the time it remains
        /// </summary>
        public float PenaltyForMistakes => m_PenaltyForMistakes;
    }
}
