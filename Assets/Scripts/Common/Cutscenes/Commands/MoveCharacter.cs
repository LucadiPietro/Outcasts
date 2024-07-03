namespace Common.Cutscenes.Commands
{
    using Minigames.GameGo;
    using System;
    using System.Collections;
    using UnityEngine;

    [Serializable]
    public sealed class MoveCharacter : ICinematicCommand
    {
        [SerializeField] Movable m_Character;
        [SerializeField] MoveType m_WalkType;
        [SerializeField] Vector2 m_Destination;
        [SerializeField] bool m_ShouldWaitEnd = default;
        public bool ShouldWaitEnd => m_ShouldWaitEnd;

        public void Execute()
        {
            m_Character.MoveTo(m_Destination, m_WalkType);
        }
        public IEnumerator ExecuteAwaitable()
        {
            yield return m_Character.MoveToAwaitable(m_Destination, m_WalkType);
        }
    }
}