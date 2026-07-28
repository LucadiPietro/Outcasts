namespace Outcasts.Minigames.GameGo
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Describes the guard behavior
    /// </summary>
    [Serializable]
    public sealed class GuardPattern
    {
        [Tooltip("During even states the guard is Alerted; During odd ones, he's distracted")]
        [SerializeField] List<State> m_States;
        public IReadOnlyList<State> States => m_States;

        public GuardPattern(IEnumerable<State> states)
        {
            m_States = new List<State>(states);
        }

        [Tooltip("Adds or subtracts a random duration (smaller than this value) to the PauseTime of each state")]
        [SerializeField, Range(0, 1)] float m_MaxVariation;
        public float MaxVariation => m_MaxVariation;



        /// <summary>
        /// The guard alternates between 2 states: Alerted and Distracted. This data stores how long it takes to transition to the current state and how long it lasts
        /// </summary>
        [Serializable]
        public struct State
        {
            public State(float moveTime, float pauseTime)
            {
                m_MoveTime = moveTime;
                m_PauseTime = pauseTime;
            }

            [Tooltip("How long it takes to transition to the current state")]
            [SerializeField] float m_MoveTime;
            public float MoveTime => m_MoveTime;

            [Tooltip("How long the current state lasts")]
            [SerializeField] float m_PauseTime;
            public float PauseTime => m_PauseTime;
        }
    }
}
