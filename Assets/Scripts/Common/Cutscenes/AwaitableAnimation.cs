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
    /// You can use this class to trigger an animation and wait for it to end
    /// </summary>
    public sealed class AwaitableAnimation : AwaitableActionBase
    {
        public string Trigger => m_Trigger;

        public override void Execute()
        {
            m_Animator.SetTrigger(TriggerHash);
        }

        public override IEnumerator ExecuteAwaitable()
        {
            Execute();

            // Start waiting for the target state to activate
            {
                const int kExpectedFramesToWait = 1;
                const int kFramesTimeout = 60;
                int framesWaited = 0;
                while (!m_IsStateActive)
                {
                    if (framesWaited >= kFramesTimeout)
                    {
                        Debug.LogError($"State was not activated during the {kFramesTimeout} frames after the trigger has been set. This should never happen. Please, ensure a transition exists from the 'Any' state to the {m_TriggeredStateName} with a trigger condition on {m_Trigger} and a duration of 0 seconds");
                        yield break;
                    }

                    yield return null;
                    framesWaited++;
                }

                if (framesWaited > kExpectedFramesToWait) Debug.LogWarning($"State {m_TriggeredStateName} was activated in {framesWaited} frames insteaf of the expected {kExpectedFramesToWait}");
            }

            // Target state is active, start waiting it exits (i.e. the animation is completed)
            {
                float elapsed = 0f;
                while (m_IsStateActive)
                {
                    yield return null;
                    elapsed += Time.deltaTime;
                    if (elapsed > m_Timeout)
                    {
                        Debug.LogWarning($"State {m_TriggeredStateName} didn't reach the end before the timeout of {m_Timeout} seconds");
                        break;
                    }
                }
            }

            // The state is inactive
            yield break;
        }

        // The CurrentState of the animator is updated every frame
        void Update() => CurrentState = m_Animator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo m_CurrentState;
        AnimatorStateInfo CurrentState
        {
            get => m_CurrentState;
            set
            {
                var previousState = m_CurrentState;
                var newState = value;

                // Do nothing if the transition is to the same state
                if (previousState.shortNameHash == newState.shortNameHash) return;

                m_CurrentState = value;

                bool wasOurState = previousState.shortNameHash == m_TriggeredStateHash;
                bool isOurState = newState.shortNameHash == m_TriggeredStateHash;

                // Callbacks for when our state has entered or exited
                if (!wasOurState && isOurState) OnStateEntered();
                else if (wasOurState && !isOurState) OnStateExited();
                // else there was a state change but not related to our triggered state
            }
        }

        /// <summary>
        /// True, if the triggered state is currently playing, false otherwise
        /// </summary>
        bool m_IsStateActive;
        void OnStateEntered() => m_IsStateActive = true;
        void OnStateExited() => m_IsStateActive = false;

        [SerializeField] Animator m_Animator;
#if UNITY_EDITOR
        [Dropdown(nameof(GetTriggerParameters)), OnValueChanged(nameof(UpdateStateInfo))]
#endif
        [SerializeField, Tooltip("Play() will call Animator.SetTrigger with this string to start the animation")] string m_Trigger;
        [SerializeField, Tooltip("If something goes wrong, we don't wait more than this amount of seconds")] float m_Timeout = 4;

        /// <summary>
        /// When Animator.SetTrigger is called, the controller will enter a new State: this is the hash identifying that state
        /// </summary>
        [SerializeField, HideInInspector] int m_TriggeredStateHash;
        /// <summary>
        /// When Animator.SetTrigger is called, the controller will enter a new State: this is the human-readable name identifying that state; Used in error messages
        /// </summary>
        [SerializeField, HideInInspector] string m_TriggeredStateName;

#if UNITY_EDITOR
        bool IsError() => !string.IsNullOrEmpty(m_ErrorMessage);
        [InfoBox("Error", EInfoBoxType.Error)]
        [SerializeField, ShowIf(nameof(IsError)), ReadOnly, ResizableTextArea, Label("")] string m_ErrorMessage;

        AnimatorController GetAnimatorController(Animator animator)
        {
            if (animator == null) return null;

            var runtimeController = animator.runtimeAnimatorController;
            if (runtimeController is AnimatorOverrideController overrideController) runtimeController = overrideController.runtimeAnimatorController;

            return runtimeController as AnimatorController;
        }

        // Returns a list of the Trigger parameters for the current animator, used by the Dropdown inspector attribute
        DropdownList<string> GetTriggerParameters()
        {
            var list = new DropdownList<string>();
            list.Add((m_Animator == null) ? "Select Animator first" : "None", "");

            if (m_Animator == null) return list;

            var editorController = GetAnimatorController(m_Animator);

            // Go through all the parameters and only add them to the list if they are triggers
            var triggers = editorController.parameters.Where(p => p.type == AnimatorControllerParameterType.Trigger);
            foreach (var trigger in triggers) list.Add(trigger.name, trigger.name);

            return list;
        }

        void OnValidate() => UpdateStateInfo();

        /// <summary>
        /// Called every time a value is changed in the inspector, it sets TriggeredStateHash, TriggeredStateName, and ErrorMessage if needed
        /// </summary>
        void UpdateStateInfo()
        {
            if (m_Animator == null)
            {
                m_ErrorMessage = $"The Animator is unset";
                m_TriggeredStateHash = 0;
                m_TriggeredStateName = "";
                return;
            }

            var editorController = GetAnimatorController(m_Animator);

            if (string.IsNullOrEmpty(m_Trigger))
            {
                m_ErrorMessage = $"The Trigger is unset";
                m_TriggeredStateHash = 0;
                m_TriggeredStateName = "";
                return;
            }

            bool IsOurTrigger(AnimatorControllerParameter parameter) => parameter.type == AnimatorControllerParameterType.Trigger && parameter.name == m_Trigger;
            if (editorController.parameters.Count(IsOurTrigger) < 1)
            {
                m_ErrorMessage = $"AnimatorController '{editorController.name}' has no trigger parameter named {m_Trigger}; Add it, to fix this error";
                m_TriggeredStateHash = 0;
                m_TriggeredStateName = "";
                return;
            }

            var mainLayer = editorController.layers[0];
            var stateMachine = mainLayer.stateMachine;
            AnimatorState destinationState = null;
            AnimatorStateTransition ourTransition = null;
            foreach (var transition in stateMachine.anyStateTransitions)
            {
                if (transition.conditions.Length != 1) continue;
                var condition = transition.conditions[0];

                string parameterName = condition.parameter;
                if (parameterName != m_Trigger) continue;

                bool isTrigger = condition.mode == AnimatorConditionMode.If;
                if (!isTrigger)
                {
                    m_ErrorMessage = $"AnimatorController '{editorController.name}' has a transition from Any to {transition.destinationState} with a condition on a parameter named {m_Trigger} but it's not a Trigger; To fix this, change the parameter's type to Trigger";
                    m_TriggeredStateHash = 0;
                    m_TriggeredStateName = "";
                    return;
                }
                else if (transition.duration > 0)
                {
                    m_ErrorMessage = $"AnimatorController '{editorController.name}': the transition triggered by {m_Trigger} has a duration of {transition.duration} seconds. {nameof(AwaitableAnimation)} only works with transitions with duration 0";
                    m_TriggeredStateHash = 0;
                    m_TriggeredStateName = "";
                    return;
                }
                else
                {
                    ourTransition = transition;
                    destinationState = transition.destinationState;
                }
            }
            if (ourTransition == null)
            {
                m_ErrorMessage = $"AnimatorController '{editorController.name}' has no transition starting from Any with a condition on a trigger parameter named {m_Trigger}; To fix this create a new state and add a transition from Any to it using {m_Trigger} as a trigger and with a duration = 0";
                m_TriggeredStateHash = 0;
                m_TriggeredStateName = "";
                return;
            }

            m_ErrorMessage = "";
            m_TriggeredStateHash = destinationState.nameHash;
            m_TriggeredStateName = destinationState.name;
        }
#endif

        int m_TriggerHash;
        /// <summary>
        /// The hash of the current trigger
        /// </summary>
        int TriggerHash
        {
            get
            {
                if (m_TriggerHash == 0) m_TriggerHash = Animator.StringToHash(m_Trigger);
                return m_TriggerHash;
            }
        }
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
