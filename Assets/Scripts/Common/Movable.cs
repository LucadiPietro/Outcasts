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
        [SerializeField] float m_DashSpeed = 20f;
        [SerializeField, ShowIf(nameof(m_CanCrouch))] float m_CrouchSpeed = 1.5f;

        [SerializeField] AIDestinationSetter m_DestinationSetter;

        public bool CanCrouch => m_CanCrouch;

        [SerializeField] AIPath m_Agent;
        [SerializeField] CharacterView m_View;

        bool m_IsMoving = false;
        public void MoveTo(Vector2 destination, Vector2? lookAt = null, MoveType walkType = MoveType.Walk, bool croucheAtTheEnd = false)
        {
            StartCoroutine(MoveToAwaitable(destination, lookAt, walkType, croucheAtTheEnd));
        }

        public IEnumerator MoveToAwaitable(Vector2 destination, Vector2? lookAt = null, MoveType walkType = MoveType.Walk, bool croucheAtTheEnd = false)
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
            else
            {
                m_View.IsCrouching = false;
                yield return FixAnimatorBugAwaitable();
            }

            m_View.IsMoving = true;
            m_View.IsRunning = walkType == MoveType.Run;
            m_View.IsDashing = walkType == MoveType.Dash || walkType == MoveType.Run;

            while (!m_Agent.reachedDestination)
            {
                m_View.LookAtDirection(m_Agent.velocity);
                
                //OVERRIDE LOOKAT PER FAR GIRARE PERSONAGGIO NELLA DIREZIONE GIUSTA
                //lookAt = m_Agent.position + m_Agent.velocity; 
                
                yield return null;
            }

            m_View.IsMoving = false;
            if (CanCrouch)
            {
                yield return FixAnimatorBugAwaitable();
                m_View.IsCrouching = croucheAtTheEnd;
            }
            m_View.IsRunning = false;

            m_Agent.maxSpeed = previousMaxSpeed;
            m_IsMoving = false;

            //if lookat = 0 allora impostalo come la linea commentata su

            if (lookAt.HasValue) m_View.LookAtPosition(lookAt.Value);
        }

        public void StartFollowing(Transform target)
        {
            if(TryGetComponent<PartyFollow>(out PartyFollow p))
            {
                p.enabled = true;
            }
        }
        
        public void StopFollowing()
        {
            if (TryGetComponent<PartyFollow>(out PartyFollow p))
            {
                p.enabled = false;
            }
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
                case MoveType.Dash: return m_DashSpeed;
                case MoveType.Walk:
                default: return m_WalkSpeed;
            }
        }
    }
}
