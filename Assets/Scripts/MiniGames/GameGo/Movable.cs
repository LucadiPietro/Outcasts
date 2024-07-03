namespace Minigames.GameGo
{
    using NaughtyAttributes;
    using Pathfinding;
    using System;
    using System.Collections;
    using System.Collections.Generic;
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
        public void MoveTo(Vector2 destination, MoveType walkType = MoveType.Walk)
        {
            StartCoroutine(MoveToAwaitable(destination, walkType));
        }
        public IEnumerator MoveToAwaitable(Vector2 destination, MoveType walkType = MoveType.Walk)
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
    public enum MoveType
    {
        Walk,
        Run,
        Crouch,
    }

    public abstract class ObjectCollection<T> : MonoBehaviour where T : UnityEngine.Object
    {
        [SerializeField] List<T> m_List;
        public IReadOnlyList<T> List => m_List;

        public void Add(T item)
        {
            m_List.Add(item);
            OnAdded(item);
        }
        public bool Remove(T item)
        {
            bool wasRemoved = m_List.Remove(item);
            if (wasRemoved) OnRemoved(item);
            return wasRemoved;
        }

        Action<T> m_Added;
        public event Action<T> Added
        {
            add { m_Added += value; }
            remove { m_Added -= value; }
        }
        void OnAdded(T item)
        {
            if (m_Added != null) m_Added(item);
        }

        Action<T> m_Removed;
        public event Action<T> Removed
        {
            add { m_Removed += value; }
            remove { m_Removed -= value; }
        }
        void OnRemoved(T item)
        {
            if (m_Removed != null) m_Removed(item);
        }
    }
}
