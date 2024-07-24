namespace Common
{
    using NaughtyAttributes;
    using Pathfinding;
    using System;
    using System.Collections;
    using UnityEngine;

    public sealed class Movable : MonoBehaviour
    {
        [Header("Capabilities")]
        [SerializeField] bool m_CanCrouch;
        [SerializeField] float m_WalkSpeed = 4f;
        [SerializeField] float m_RunSpeed = 7f;
        [SerializeField, ShowIf(nameof(m_CanCrouch))] float m_CrouchSpeed = 1.5f;

        public bool CanCrouch => m_CanCrouch;

        [SerializeField] AIPath m_Agent;
        [SerializeField] CharacterView m_View;

        bool m_IsMoving = false;
        public void MoveTo(Vector2 destination, Vector2? lookAt = null, MoveType walkType = MoveType.Walk)
        {
            StartCoroutine(MoveToAwaitable(destination, lookAt, walkType));
        }

        public IEnumerator MoveToAwaitable(Vector2 destination, Vector2? lookAt = null, MoveType walkType = MoveType.Walk)
        {
            if (m_IsMoving) throw new InvalidOperationException($"Character {gameObject.name} cannot start moving because it already is");

            m_IsMoving = true;
            float previousMaxSpeed = m_Agent.maxSpeed;
            m_Agent.maxSpeed = GetMaxSpeed(walkType);
            m_Agent.destination = destination;

            if (walkType == MoveType.Crouch)
            {
                if (CanCrouch)
                {
                    m_View.IsCrouching = true;
                    yield return FixAnimatorBugAwaitable();
                }
                else throw new InvalidOperationException($"Character {gameObject.name} cannot crouch. You should add the crouch animation to its Animator and then check the {nameof(CanCrouch)} checkbox on the {nameof(Movable)} component");
            }

            m_View.IsRunning = walkType == MoveType.Run;
            m_View.IsMoving = true;

            while (!m_Agent.reachedDestination)
            {
                m_View.LookAtDirection(m_Agent.velocity);
                yield return null;
            }

            m_View.IsMoving = false;
            if (CanCrouch)
            {
                yield return FixAnimatorBugAwaitable();
                m_View.IsCrouching = false;
            }
            m_View.IsRunning = false;

            m_Agent.maxSpeed = previousMaxSpeed;
            m_IsMoving = false;

            if (lookAt.HasValue) m_View.LookAtPosition(lookAt.Value);
        }

        /// <summary>
        /// This is needed because of a bug in the Animator conditions. If IsCrouching and IsMoving are set together, it transitions to the normal movement
        /// </summary>
        IEnumerator FixAnimatorBugAwaitable()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }

        float GetMaxSpeed(MoveType walkType = MoveType.Walk)
        {
            switch (walkType)
            {
                case MoveType.Run: return m_RunSpeed;
                case MoveType.Crouch: return m_CrouchSpeed;

                case MoveType.Walk:
                default: return m_WalkSpeed;
            }
        }
    }
}
