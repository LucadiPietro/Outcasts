namespace Minigames.GameGo
{
    using DG.Tweening;
    using LemonGames;
    using System;
    using System.Collections;
    using UnityEngine;

    public sealed class GuardBrain : MonoBehaviour
    {
        [SerializeField] CharacterController m_Controller;

        [SerializeField] Transform m_Stealer;
        [SerializeField] Transform m_Diversion;

        [SerializeField] ViewCone m_Cone;
        [SerializeField] Color m_AlertedColor = Color.red;
        [SerializeField] Color m_DistractedColor = Color.green;

        GuardPattern m_Pattern;
        /// <summary>
        /// Makes the guard follow the given instructions
        /// </summary>
        public void StartPattern(GuardPattern pattern)
        {
            StopPattern();
            m_Pattern = pattern;
            StartCoroutine(ExecutePatternAwaitable(pattern));
        }
        /// <summary>
        /// Makes the guard stop following instructions
        /// </summary>
        public void StopPattern()
        {
            m_Pattern = null;
            StopAllCoroutines();
        }
        IEnumerator ExecutePatternAwaitable(GuardPattern pattern)
        {
            if (pattern.States.Count == 0) yield break;

            int i = 0;
            // The guard can go on forever...
            while (true)
            {
                // Loop through the states
                var state = pattern.States[i % pattern.States.Count];

                // Look at new target
                {
                    bool isEven = i % 2 == 0;
                    var newTarget = isEven ? m_Stealer : m_Diversion;
                    LookAtTarget(newTarget, state.MoveTime);
                    yield return new WaitForSeconds(state.MoveTime);
                }

                // Wait delay
                {
                    float pauseTime = state.PauseTime;
                    // randomVariation could be negative and bigger (in absolute value) thean pauseTime
                    float randomVariation = UnityEngine.Random.Range(-pattern.MaxVariation, pattern.MaxVariation);
                    // We need to ensure pauseTime is non-negative
                    pauseTime = Mathf.Max(pauseTime + randomVariation, 0);
                    yield return new WaitForSeconds(state.PauseTime);
                }

                i++;
            }
        }
        /// <summary>
        /// True if the guard is looking at the stealer
        /// </summary>
        public bool IsFocusedOnStealer => CurrentTarget == m_Stealer;

        // The guard always starts looking at the stealer
        void OnEnable() => LookAtTarget(m_Stealer);

        // Makes the character look at the given target, either immediately or gradually
        void LookAtTarget(Transform target, float duration = 0)
        {
            bool isImmediate = duration == 0;

            Angle angleToTarget = Angle.FromPosition(target.position - transform.position);
            Angle currentAngle = m_Controller.ForwardAngle;

            Angle angleDelta = Angle.SmallestSignedDifference(currentAngle, angleToTarget);
            Angle newAngle = currentAngle + angleDelta;

            DOTween.Kill(this);
            if (isImmediate)
            {
                CurrentTarget = target;
                SetCurrentAngle(newAngle);
            }
            else
            {
                var tween = DOTween.To(GetCurrentAngle, SetCurrentAngle, newAngle, duration).SetTarget(this);
                // We give some leeway to the stealer:
                // - The guard gains sight of the stealer only after he's fully rotated towards him
                // - The guard loses sight of the stealer as soon as he starts looking away
                if (target == m_Stealer) tween.OnComplete(() => CurrentTarget = m_Stealer);
                else CurrentTarget = target;
            }
        }

        Transform m_CurrentTarget;
        Transform CurrentTarget
        {
            get => m_CurrentTarget;
            set
            {
                if (m_CurrentTarget == value) return;
                m_CurrentTarget = value;

                var coneColor = IsFocusedOnStealer ? m_AlertedColor : m_DistractedColor;
                m_Cone.ApplySettings(m_Cone.ViewAngle, m_Cone.ViewRadius, coneColor);

                OnFocusChanged();
            }
        }

        float GetCurrentAngle() => m_Controller.ForwardAngle;
        void SetCurrentAngle(float angle)
        {
            m_Controller.LookAtAngle(angle);
            m_Cone.Forward = m_Controller.Forward;
        }

        Action m_FocusChanged;
        public event Action FocusChanged
        {
            add { m_FocusChanged += value; }
            remove { m_FocusChanged -= value; }
        }
        void OnFocusChanged()
        {
            if (m_FocusChanged != null) m_FocusChanged();
        }
    }
}
