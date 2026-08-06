namespace Common.Cutscenes
{
    using NaughtyAttributes;
    using System.Collections;
    using System.Linq;
#if UNITY_EDITOR
    using UnityEditor.Animations;
#endif
    using UnityEngine;

    /// <summary>
    /// Triggers an Animator state and waits for it without polling every frame when idle.
    /// </summary>
    public sealed class AwaitableAnimation : AwaitableActionBase
    {
        [SerializeField] Animator m_Animator;
#if UNITY_EDITOR
        [Dropdown(nameof(GetTriggerParameters)), OnValueChanged(nameof(UpdateStateInfo))]
#endif
        [SerializeField, Tooltip("Trigger parameter used to start the animation.")]
        string m_Trigger;

        [SerializeField, Tooltip("Maximum time spent waiting for the state to finish.")]
        float m_Timeout = 4f;

        [SerializeField, HideInInspector] int m_TriggeredStateHash;
        [SerializeField, HideInInspector] string m_TriggeredStateName;

#if UNITY_EDITOR
        [InfoBox("Error", EInfoBoxType.Error)]
        [SerializeField, ShowIf(nameof(IsError)), ReadOnly, ResizableTextArea, Label("")]
        string m_ErrorMessage;
#endif

        int m_TriggerHash;

        public string Trigger => m_Trigger;

        /// <summary>
        /// Triggers the configured animation when references are valid.
        /// </summary>
        public override void Execute()
        {
            if (!ValidateRuntimeConfiguration())
            {
                return;
            }

            m_Animator.ResetTrigger(TriggerHash);
            m_Animator.SetTrigger(TriggerHash);
        }

        /// <summary>
        /// Waits for state entry and exit with independent safety timeouts.
        /// </summary>
        public override IEnumerator ExecuteAwaitable()
        {
            if (!ValidateRuntimeConfiguration())
            {
                yield break;
            }

            Execute();

            const int activationFrameTimeout = 60;
            int framesWaited = 0;

            while (!IsTriggeredStateActive() && framesWaited < activationFrameTimeout)
            {
                framesWaited++;
                yield return null;
            }

            if (!IsTriggeredStateActive())
            {
                Debug.LogError(
                    $"Animator state '{m_TriggeredStateName}' was not activated by trigger " +
                    $"'{m_Trigger}' within {activationFrameTimeout} frames.",
                    this);
                yield break;
            }

            float elapsed = 0f;
            float timeout = Mathf.Max(0.1f, m_Timeout);

            while (IsTriggeredStateActive() && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (elapsed >= timeout && IsTriggeredStateActive())
            {
                Debug.LogWarning(
                    $"Animator state '{m_TriggeredStateName}' exceeded its {timeout:0.##}s timeout.",
                    this);
            }
        }

        /// <summary>
        /// Checks current and next Animator states to remain correct during transitions.
        /// </summary>
        bool IsTriggeredStateActive()
        {
            if (m_Animator == null || m_TriggeredStateHash == 0)
            {
                return false;
            }

            AnimatorStateInfo current = m_Animator.GetCurrentAnimatorStateInfo(0);
            if (current.shortNameHash == m_TriggeredStateHash)
            {
                return true;
            }

            if (m_Animator.IsInTransition(0))
            {
                AnimatorStateInfo next = m_Animator.GetNextAnimatorStateInfo(0);
                return next.shortNameHash == m_TriggeredStateHash;
            }

            return false;
        }

        /// <summary>
        /// Validates runtime references and authored state information.
        /// </summary>
        bool ValidateRuntimeConfiguration()
        {
            if (m_Animator == null)
            {
                Debug.LogError("AwaitableAnimation has no Animator reference.", this);
                return false;
            }

            if (string.IsNullOrWhiteSpace(m_Trigger))
            {
                Debug.LogError("AwaitableAnimation has no trigger configured.", this);
                return false;
            }

            if (m_TriggeredStateHash == 0)
            {
                Debug.LogError(
                    $"AwaitableAnimation trigger '{m_Trigger}' has no resolved destination state.",
                    this);
                return false;
            }

            return true;
        }

        int TriggerHash
        {
            get
            {
                if (m_TriggerHash == 0)
                {
                    m_TriggerHash = Animator.StringToHash(m_Trigger);
                }

                return m_TriggerHash;
            }
        }

#if UNITY_EDITOR
        bool IsError() => !string.IsNullOrEmpty(m_ErrorMessage);

        /// <summary>
        /// Resolves the editable AnimatorController behind possible override controllers.
        /// </summary>
        AnimatorController GetAnimatorController(Animator animator)
        {
            if (animator == null)
            {
                return null;
            }

            RuntimeAnimatorController runtimeController = animator.runtimeAnimatorController;
            if (runtimeController is AnimatorOverrideController overrideController)
            {
                runtimeController = overrideController.runtimeAnimatorController;
            }

            return runtimeController as AnimatorController;
        }

        /// <summary>
        /// Builds the trigger dropdown used by the custom inspector.
        /// </summary>
        DropdownList<string> GetTriggerParameters()
        {
            var list = new DropdownList<string>();
            list.Add(m_Animator == null ? "Select Animator first" : "None", string.Empty);

            AnimatorController controller = GetAnimatorController(m_Animator);
            if (controller == null)
            {
                return list;
            }

            foreach (AnimatorControllerParameter parameter in controller.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    list.Add(parameter.name, parameter.name);
                }
            }

            return list;
        }

        /// <summary>
        /// Keeps cached hashes and validation messages synchronized in the editor.
        /// </summary>
        void OnValidate()
        {
            m_TriggerHash = 0;
            UpdateStateInfo();
        }

        /// <summary>
        /// Resolves the Any State transition driven by the selected trigger.
        /// </summary>
        void UpdateStateInfo()
        {
            m_TriggeredStateHash = 0;
            m_TriggeredStateName = string.Empty;

            if (m_Animator == null)
            {
                m_ErrorMessage = "The Animator is unset.";
                return;
            }

            AnimatorController controller = GetAnimatorController(m_Animator);
            if (controller == null)
            {
                m_ErrorMessage = "The Animator does not use an editable AnimatorController.";
                return;
            }

            if (string.IsNullOrWhiteSpace(m_Trigger))
            {
                m_ErrorMessage = "The trigger is unset.";
                return;
            }

            bool triggerExists = controller.parameters.Any(
                parameter => parameter.type == AnimatorControllerParameterType.Trigger &&
                             parameter.name == m_Trigger);
            if (!triggerExists)
            {
                m_ErrorMessage = $"AnimatorController '{controller.name}' has no trigger named '{m_Trigger}'.";
                return;
            }

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            foreach (AnimatorStateTransition transition in stateMachine.anyStateTransitions)
            {
                if (transition.conditions.Length != 1)
                {
                    continue;
                }

                AnimatorCondition condition = transition.conditions[0];
                if (condition.parameter != m_Trigger ||
                    condition.mode != AnimatorConditionMode.If ||
                    transition.destinationState == null)
                {
                    continue;
                }

                if (transition.duration > 0f)
                {
                    m_ErrorMessage =
                        $"Transition for '{m_Trigger}' must have zero duration for deterministic waiting.";
                    return;
                }

                m_TriggeredStateHash = transition.destinationState.nameHash;
                m_TriggeredStateName = transition.destinationState.name;
                m_ErrorMessage = string.Empty;
                return;
            }

            m_ErrorMessage =
                $"No Any State transition driven only by trigger '{m_Trigger}' was found.";
        }
#endif
    }

    public interface IAwaitableAction
    {
        void Execute();
        IEnumerator ExecuteAwaitable();
    }

    public abstract class AwaitableActionBase : MonoBehaviour, IAwaitableAction
    {
        public abstract void Execute();
        public abstract IEnumerator ExecuteAwaitable();
    }
}
