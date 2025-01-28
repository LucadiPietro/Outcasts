namespace Common
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Common;
    using NaughtyAttributes;
    using UnityEditor.SceneManagement;
    using UnityEditor.Experimental.GraphView;

    public class PartyFollow : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Transform m_Target;
        [SerializeField] CharacterView m_View;
        [SerializeField] PlayableMovement m_MainCharacter;
        [SerializeField] Animator m_Animator;

        [Header("Capabilities")]
        [SerializeField] float m_MaxTargetDistance = .1f;
        [SerializeField] bool m_CanCrouch;
        [SerializeField] float m_WalkSpeed = 4f;
        [SerializeField] float m_RunSpeed = 7f;
        [SerializeField, ShowIf(nameof(m_CanCrouch))] float m_CrouchSpeed = 1.5f;


        bool m_CanFollow = true;

        bool m_IsMoving = false;
        bool m_IsRunning = false;
        bool m_IsCrouching = false;

        private void Start()
        {

        }

        private void Update()
        {
            bool closeToTarget = Vector2.Distance(transform.position, m_Target.position) <= m_MaxTargetDistance;

            Vector2 direction = Vector2.zero;

            if (m_Target != null && !closeToTarget && m_CanFollow)
            {
                m_IsMoving = true;
                m_IsRunning = m_MainCharacter.moveType == MoveType.Run;


                direction = (m_Target.position - transform.position).normalized;

                m_View.IsCrouching = false;
                m_View.LookAtDirection(direction);

                float speed = m_IsRunning ? m_RunSpeed : m_WalkSpeed;
                transform.Translate(direction * speed * Time.deltaTime);
            }
            else
            {
                m_IsMoving = false;

                m_View.LookAtPosition(m_Target.position);
            }

            m_View.IsMoving = m_IsMoving;
            m_View.IsRunning = m_IsRunning;
        }

        public void StartFollowing()
        {
            m_CanFollow = true;
        }

        public void StopFollowing()
        {
            m_CanFollow = false;
        }
    }
}