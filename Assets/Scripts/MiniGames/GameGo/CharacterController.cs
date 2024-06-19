namespace Minigames.GameGo
{
    using LemonGames;
    using UnityEngine;

    /// <summary>
    /// Base class for controlling the visual appearance of characters
    /// </summary>
    public class CharacterController : MonoBehaviour
    {
        [SerializeField] Animator m_Animator;
        protected Animator Animator => m_Animator;

        readonly int kDirectionX = Animator.StringToHash("idle_x_input");
        readonly int kDirectionY = Animator.StringToHash("idle_y_input");

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
        /// <summary>
        /// Rotates the character towards the given world-space direction
        /// </summary>
        public void LookAtDirection(Vector2 direction)
        {
            if (Mathf.Approximately(direction.sqrMagnitude, 0f)) return;
            direction.Normalize();

            m_Forward = direction;

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
        }
    }
}
