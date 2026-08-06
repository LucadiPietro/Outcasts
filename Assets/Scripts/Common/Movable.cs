namespace Common
{
    using NaughtyAttributes;
    using Pathfinding;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Reliable movement facade for A* Pathfinding characters.
    /// New movement requests safely supersede older ones and always restore animation
    /// and agent state when completed, interrupted, disabled, or timed out.
    /// </summary>
    public sealed class Movable : MonoBehaviour
    {
        [Header("Capabilities")]
        [SerializeField] bool m_CanCrouch;
        [SerializeField] float m_WalkSpeed = 4f;
        [SerializeField] float m_RunSpeed = 7f;
        [SerializeField] float m_DashSpeed = 20f;
        [SerializeField, ShowIf(nameof(m_CanCrouch))] float m_CrouchSpeed = 1.5f;

        [Header("References")]
        [SerializeField] AIDestinationSetter m_DestinationSetter;
        [SerializeField] AIPath m_Agent;
        [SerializeField] CharacterView m_View;

        const float kVelocityEpsilon = 0.0001f;
        const float kMinimumTimeout = 5f;
        const float kTimeoutPadding = 8f;

        bool m_IsMoving;
        int m_MovementVersion;
        float m_DefaultMaxSpeed;

        public bool CanCrouch => m_CanCrouch;
        public bool IsMoving => m_IsMoving;

        /// <summary>
        /// Caches the authored default speed used after every cutscene movement.
        /// </summary>
        void Awake()
        {
            if (m_Agent != null)
            {
                m_DefaultMaxSpeed = m_Agent.maxSpeed;
            }
        }

        /// <summary>
        /// Cancels active movement and returns the character to an idle state when
        /// the component is disabled.
        /// </summary>
        void OnDisable()
        {
            StopMoving();
        }

        /// <summary>
        /// Starts a movement request without waiting for it.
        /// </summary>
        public void MoveTo(
            Vector2 destination,
            Vector2? lookAt = null,
            MoveType walkType = MoveType.Walk,
            bool croucheAtTheEnd = false)
        {
            StartCoroutine(MoveToAwaitable(destination, lookAt, walkType, croucheAtTheEnd));
        }

        /// <summary>
        /// Moves to a destination and waits until the A* agent reaches it. A newer
        /// request cancels this one cleanly instead of throwing an exception.
        /// </summary>
        public IEnumerator MoveToAwaitable(
            Vector2 destination,
            Vector2? lookAt = null,
            MoveType walkType = MoveType.Walk,
            bool croucheAtTheEnd = false)
        {
            if (!ValidateReferences())
            {
                yield break;
            }

            if (walkType == MoveType.Crouch && !CanCrouch)
            {
                Debug.LogError($"Character '{name}' cannot crouch with its current configuration.", this);
                yield break;
            }

            int movementVersion = BeginMovement(walkType);
            float speed = Mathf.Max(0.01f, GetMaxSpeed(walkType));
            float distance = Vector2.Distance(m_Agent.position, destination);
            float timeout = Mathf.Max(kMinimumTimeout, distance / speed + kTimeoutPadding);
            float elapsed = 0f;

            m_Agent.destination = destination;

            if (walkType == MoveType.Crouch)
            {
                m_View.IsCrouching = true;
                yield return FixAnimatorTransitionAwaitable();
            }
            else
            {
                m_View.IsCrouching = false;
                yield return FixAnimatorTransitionAwaitable();
            }

            if (movementVersion != m_MovementVersion)
            {
                yield break;
            }

            m_View.IsMoving = true;
            m_View.IsRunning = walkType == MoveType.Run;
            m_View.IsDashing = walkType == MoveType.Dash;

            while (movementVersion == m_MovementVersion && !m_Agent.reachedDestination)
            {
                Vector3 velocity = m_Agent.velocity;
                if (velocity.sqrMagnitude > kVelocityEpsilon)
                {
                    m_View.LookAtDirection(velocity);
                }

                elapsed += Time.deltaTime;
                if (elapsed >= timeout)
                {
                    Debug.LogWarning(
                        $"Movement for '{name}' timed out after {timeout:0.##} seconds. " +
                        "The path may be unreachable or the graph may need rescanning.",
                        this);
                    break;
                }

                yield return null;
            }

            if (movementVersion != m_MovementVersion)
            {
                yield break;
            }

            FinishMovement(croucheAtTheEnd);

            if (lookAt.HasValue &&
                ((Vector2)lookAt.Value - destination).sqrMagnitude > kVelocityEpsilon)
            {
                m_View.LookAtPosition(lookAt.Value);
            }
        }

        /// <summary>
        /// Immediately places the character at a cutscene destination and resets
        /// movement state. Used by fast-forward.
        /// </summary>
        public void TeleportTo(Vector2 destination, Vector2? lookAt = null)
        {
            StopMoving();

            if (m_Agent != null)
            {
                m_Agent.Teleport(destination, true);
            }
            else
            {
                transform.position = new Vector3(destination.x, destination.y, transform.position.z);
            }

            if (m_View != null && lookAt.HasValue)
            {
                m_View.LookAtPosition(lookAt.Value);
            }
        }

        /// <summary>
        /// Enables the existing party-follow component.
        /// </summary>
        public void StartFollowing(Transform target)
        {
            if (TryGetComponent(out PartyFollow partyFollow))
            {
                partyFollow.enabled = true;
            }
        }

        /// <summary>
        /// Disables the existing party-follow component.
        /// </summary>
        public void StopFollowing()
        {
            if (TryGetComponent(out PartyFollow partyFollow))
            {
                partyFollow.enabled = false;
            }
        }

        /// <summary>
        /// Cancels the active movement and restores authored agent/animation state.
        /// </summary>
        public void StopMoving()
        {
            m_MovementVersion++;
            m_IsMoving = false;

            if (m_Agent != null)
            {
                m_Agent.maxSpeed = m_DefaultMaxSpeed;
                m_Agent.destination = m_Agent.position;
            }

            ApplyIdleAnimation(crouched: false);
        }

        /// <summary>
        /// Starts a new versioned movement request.
        /// </summary>
        int BeginMovement(MoveType moveType)
        {
            m_MovementVersion++;
            m_IsMoving = true;

            if (m_Agent != null)
            {
                m_Agent.maxSpeed = GetMaxSpeed(moveType);
            }

            ApplyIdleAnimation(crouched: false);
            return m_MovementVersion;
        }

        /// <summary>
        /// Restores state after a successful movement.
        /// </summary>
        void FinishMovement(bool crouched)
        {
            m_IsMoving = false;

            if (m_Agent != null)
            {
                m_Agent.maxSpeed = m_DefaultMaxSpeed;
            }

            ApplyIdleAnimation(crouched);
        }

        /// <summary>
        /// Applies a coherent set of animator flags.
        /// </summary>
        void ApplyIdleAnimation(bool crouched)
        {
            if (m_View == null)
            {
                return;
            }

            m_View.IsMoving = false;
            m_View.IsRunning = false;
            m_View.IsDashing = false;
            m_View.IsCrouching = m_CanCrouch && crouched;
        }

        /// <summary>
        /// Gives the Animator one rendered frame to settle mutually exclusive flags.
        /// </summary>
        IEnumerator FixAnimatorTransitionAwaitable()
        {
            yield return new WaitForEndOfFrame();
        }

        /// <summary>
        /// Maps authored movement types to their configured speeds.
        /// </summary>
        float GetMaxSpeed(MoveType moveType)
        {
            switch (moveType)
            {
                case MoveType.Run:
                    return m_RunSpeed;
                case MoveType.Crouch:
                    return m_CrouchSpeed;
                case MoveType.Dash:
                    return m_DashSpeed;
                case MoveType.Walk:
                default:
                    return m_WalkSpeed;
            }
        }

        /// <summary>
        /// Verifies required scene references before starting a command.
        /// </summary>
        bool ValidateReferences()
        {
            if (m_Agent == null || m_View == null)
            {
                Debug.LogError(
                    $"Movable '{name}' requires both an AIPath agent and CharacterView.",
                    this);
                return false;
            }

            return true;
        }
    }
}
