namespace Common
{
    using Common.Cutscenes.Commands;
    using LemonGames;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Base class for controlling the visual appearance of characters
    /// </summary>
    public class CharacterView : MonoBehaviour
    {
        [Tooltip("This animator is a reference to the one in the \"Sprite\" object, child of this one")]
        [SerializeField] Animator m_Animator;

        public Animator Animator => m_Animator;

        readonly int kDirectionX = Animator.StringToHash("idle_x_input");
        readonly int kDirectionY = Animator.StringToHash("idle_y_input");
        readonly int kMoveDirectionX = Animator.StringToHash("x_Input");
        readonly int kMoveDirectionY = Animator.StringToHash("y_Input");
        readonly int kIsMoving = Animator.StringToHash("Movement");
        readonly int kIsRunning = Animator.StringToHash("isRun");
        readonly int kIsCrouching = Animator.StringToHash("isCrouch");

        Vector2 m_Forward = Vector2.down;
        /// <summary>
        /// This is the normalized direction where the character is facing
        /// </summary>
        public Vector2 Forward => m_Forward;
        /// <summary>
        /// This is the angle where the character is facing where 0° => Right; 90° => Up; 180° => Left; 270° => Down; etc...
        /// </summary>
        public Angle ForwardAngle => Angle.FromPosition(Forward);

        /// <summary>
        /// Rotates the character towards the given world-space position
        /// </summary>
        public void LookAtPosition(Vector3 targetPosition) => LookAtDirection(targetPosition - transform.position);
        /// <summary>
        /// Rotates the character towards the given direction angle where 0° => Right; 90° => Up; 180° => Left; 270° => Down; etc...
        /// </summary>
        public void LookAtAngle(Angle angle) => LookAtDirection(angle.ToPosition());

        ///<summary>
        ///Rotates the character based on the orientation given
        /// </summary>
        public void LookAtOrientation(Orientation orientation)
        {
            Vector2 direction = Vector2.zero;

            switch (orientation)
            {
                case Orientation.N: direction = Vector2.up; break;
                case Orientation.NE: direction = Vector2.up + Vector2.right; break;
                case Orientation.E: direction = Vector2.right; break;
                case Orientation.SE: direction = Vector2.down + Vector2.right; break;
                case Orientation.S: direction = Vector2.down; break;
                case Orientation.SW: direction = Vector2.down + Vector2.left; break;
                case Orientation.W: direction = Vector2.left; break;
                case Orientation.NW: direction = Vector2.up + Vector2.left; break;
                case Orientation.None: direction = Vector2.zero; break;
            }

            LookAtDirection(direction);
        }

        /// <summary>
        /// Rotates the character towards the given world-space direction
        /// </summary>
        public void LookAtDirection(Vector2 direction)
        {
            //Debug.Log(direction);


            if (Mathf.Approximately(direction.sqrMagnitude, 0f)) return;
            direction.Normalize();

            m_Forward = direction;
            
            Debug.DrawRay(transform.position, direction * 2f, Color.red, 0.1f);


            // Atan2 is positive in the top two quadrants (0 -> pi) and negative in the bottom two (-pi -> 0)
            var angleToTarget = Mathf.Atan2(direction.y, direction.x);
            // Convert the angle so it's in range 0, 2pi
            if (angleToTarget < 0f) angleToTarget += Mathf.PI * 2f;

            // An octant is 1/8 th of the plane, which is 1/8th of 2pi
            float octantSize = Mathf.PI / 4f;

            // Shift the angle half of an octant
            float angleShift = octantSize * 0.5f;
            if (angleToTarget < (2f * Mathf.PI) - angleShift) angleToTarget += angleShift;
            else angleToTarget -= (2f * Mathf.PI) - angleShift;

            // This determines in which octant the character is facing
            // 0 => right
            // 1 => top-right
            // 2 => top
            // 3 => top-left
            // etc
            int octant = Mathf.FloorToInt(angleToTarget / octantSize);

            // Discretize the direction
            direction = new Vector2(Mathf.Cos(octant * octantSize), Mathf.Sin(octant * octantSize));
            // Make it so that x and y can only be -1, 0 or +1
            direction.x = Mathf.RoundToInt(direction.x);
            direction.y = Mathf.RoundToInt(direction.y);


            m_Animator.SetFloat(kDirectionX, direction.x);
            m_Animator.SetFloat(kDirectionY, direction.y);
            m_Animator.SetFloat(kMoveDirectionX, direction.x);
            m_Animator.SetFloat(kMoveDirectionY, direction.y);
            
            Vector2 finalDirection = new Vector2(
                m_Animator.GetFloat(kDirectionX), 
                m_Animator.GetFloat(kDirectionY)
            );
            Debug.DrawRay(transform.position, finalDirection * 2f, Color.blue, 0.1f);
    
            // Verifica se la direzione è opposta
            float dot = Vector2.Dot(direction.normalized, finalDirection.normalized);
            if (dot < 0) {
                Debug.LogWarning("Direzione invertita rilevata! Angolo: " + 
                                 Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            }
        }

        bool m_IsMoving = false;
        public bool IsMoving
        {
            get => m_IsMoving;
            set
            {
                m_IsMoving = value;
                m_Animator.SetBool(kIsMoving, value);
            }
        }

        bool m_IsRunning = false;
        public bool IsRunning
        {
            get => m_IsRunning;
            set
            {
                m_IsRunning = value;
                m_Animator.SetBool(kIsRunning, value);
            }
        }

        bool m_IsCrouching = false;
        public bool IsCrouching
        {
            get => m_IsCrouching;
            set
            {
                m_IsCrouching = value;
                m_Animator.SetBool(kIsCrouching, value);
            }
        }

        bool m_IsDashing = false;
        public bool IsDashing
        {
            get => m_IsDashing;
            set
            {
                m_IsDashing = value;
                m_Animator.SetBool(kIsRunning, value);
            }
        }
    }
    public sealed class StealerView : MonoBehaviour
    {
        readonly int kSteal = Animator.StringToHash("Steal");

        [SerializeField] Animator m_Animator;

        bool m_IsStealing = false;
        bool IsStealing() => m_IsStealing;
        public void Steal()
        {
            m_Animator.SetTrigger(kSteal);
            m_IsStealing = true;
        }
        public void StopStealing()
        {
            if (!m_IsStealing) return;
            m_IsStealing = false;
        }

        WaitWhile kWaitWhileStealing;
        void Awake()
        {
            kWaitWhileStealing = new WaitWhile(IsStealing);
        }
        public IEnumerator StealAwaitable()
        {
            Steal();
            yield return kWaitWhileStealing;
        }
    }
}
