namespace Minigames.GameGo
{
    using Common.Cutscenes;
    using DG.Tweening;
    using LemonGames;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using static InputMapping;
    using static UnityEngine.InputSystem.InputAction;

    /// <summary>
    /// The view is responsible for showing the timer and all actions to perform, animate characters, and give visual and auditory feedback for player's actions.
    /// It also enables and disables input system according to the game state.
    /// 
    /// The suffix "System" instead of "Manager" means it's self managed: it doesn't require external inputs and has no public methods
    /// </summary>
    public sealed class GameViewSystem : MonoBehaviour, IGameGoActions
    {
        [Header("References")]
        [SerializeField] GameManager m_Manager;
        [SerializeField] ScrollRect m_ScrollView;
        [SerializeField] TextMeshProUGUI m_TimerLabel;
        [SerializeField] CanvasGroup m_GameStateCanvas;
        [SerializeField] CanvasGroup m_AlertCanvas;
        [SerializeField] AwaitableAnimation m_StealAction;

        [Header("Characters")]
        [SerializeField] CharacterView m_Stealer;
        [SerializeField] CharacterView m_Cover1;
        [SerializeField] CharacterView m_Cover2;
        [SerializeField] CharacterView m_Diversion;
        [SerializeField] GuardBrain m_Guard;

        [Space(5)]
        [Header("Customization")]
        [SerializeField] SDictionary<StealAction, ActionView> m_ActionToPrefab;
        [SerializeField] float m_ButtonTransitionDuration = 0.1f;
        [Header("SFX")]
        [SerializeField] AudioSource m_SfxGameWon;
        [SerializeField] AudioSource m_SfxGameLost;
        [SerializeField] AudioSource m_SfxStealSuccess;
        [SerializeField] AudioSource m_SfxStealMistake;

        RectTransform ActionContainer => m_ScrollView.content;
        InputMapping m_Input;
        void Awake()
        {
            m_Input = new InputMapping();
            m_Input.GameGo.SetCallbacks(this);
        }

        void OnEnable()
        {
            m_Manager.GameStarted += Show;
            m_Manager.GameLost += HideIfTimeUp;
            m_Guard.FocusChanged += UpdateAlertCanvas;
            UpdateAlertCanvas();
        }
        void OnDisable()
        {
            if (m_Manager != null)
            {
                m_Manager.GameStarted -= Show;
                m_Manager.GameLost -= HideIfTimeUp;
            }
            if (m_Guard != null) m_Guard.FocusChanged -= UpdateAlertCanvas;
        }

        List<ActionView> m_SpawnedViews = new List<ActionView>();
        bool m_IsGameRunning = false;

        // Called when the game starts
        void Show()
        {
            // Spawn views for each action to perform
            for (int i = 0; i < m_Manager.GameDefinition.ActionsToPerform.Count; i++)
            {
                var stealAction = m_Manager.GameDefinition.ActionsToPerform[i];
                var prefab = m_ActionToPrefab[stealAction];
                var view = Instantiate(prefab, ActionContainer);
                view.Action = stealAction;
                view.SetSize(GetActionSize(i));
                m_SpawnedViews.Add(view);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(ActionContainer);
            m_ScrollView.verticalNormalizedPosition = 0;

            // Prepare characters
            m_Stealer.LookAtDirection(Vector2.up);
            m_Cover1.LookAtDirection(Vector2.up);
            m_Cover2.LookAtDirection(Vector2.up);
            m_Diversion.LookAtDirection(Vector2.right);

            m_Input.GameGo.Enable();
            m_IsGameRunning = true;

            m_GameStateCanvas.DOFade(1f, 0.2f);
        }
        // Called when the game is over (might be delayed by some animation playing)
        void Hide()
        {
            if (!m_IsGameRunning) return;

            m_IsGameRunning = false;
            m_Input.GameGo.Disable();

            // Destory all action views
            for (int i = 0; i < ActionContainer.childCount; i++)
            {
                var view = ActionContainer.GetChild(i)?.gameObject;
                if (view != null) Destroy(view);
            }
            m_SpawnedViews.Clear();

            m_GameStateCanvas.DOFade(0f, 0.2f);
        }
        void HideIfTimeUp(FailReason failReason)
        {
            // Only hide if the time ran out while not performing any action, because it's already handled by PerformAction otherwise
            if (!m_IsPerformingActions && failReason == FailReason.OutOfTime) Hide();
        }

        bool m_IsPerformingActions = false;
        // Called by the input system. Forwards the action to the Manager and shows the correct feedback based on the result
        void PerformAction(StealAction actionPerformed)
        {
            IEnumerator PerformActionAwaitable(StealAction actionPerformed)
            {
                m_IsPerformingActions = true;
                m_Input.GameGo.Disable();
                var result = m_Manager.PerformAction(actionPerformed);
                m_StealAction.Execute();
                //m_Stealer.Animator.SetTrigger(kSteal);
                yield return ShowFeedback(result);
                if (m_Manager.GameState != GameState.Running) Hide();
                else m_Input.GameGo.Enable();
                m_IsPerformingActions = false;
            }
            StartCoroutine(PerformActionAwaitable(actionPerformed));
        }
        IEnumerator ShowFeedback(ActionSummary actionSummary)
        {
            // View of the current action to perform
            var currentView = m_SpawnedViews[0];

            // You've been caught
            if (actionSummary.WasGuardAlerted)
            {
                // TODO: Show FX for when you get caught
                currentView.ShowError();
                m_SfxGameLost.Play();
                if (m_Manager.GameState == GameState.Lost) Hide();
            }
            // You've pressed the wrong button
            else if (actionSummary.ActionPerformed != actionSummary.ActionToPerform)
            {
                // TODO: Show FX for when you press the wrong button
                currentView.ShowError();
                ShowTimePenalty();
                if (m_Manager.GameState == GameState.Lost)
                {
                    m_SfxGameLost.Play();
                    Hide();
                }
                else m_SfxStealMistake.Play();
            }
            // You've pressed the right button
            else
            {
                // TODO: Show FX for when you press the right button
                currentView.ShowSuccess();
                m_SfxStealSuccess.Play();

                // Removes the first view from the list
                m_SpawnedViews.RemoveAt(0);

                // If there's at least one more view, activate it and scroll to focus on it
                if (m_SpawnedViews.Count > 0)
                {
                    // This starts an animation that spans the next m_ButtonTransitionDuration seconds
                    for (int i = 0; i < Mathf.Min(4, m_SpawnedViews.Count); i++)
                    {
                        var viewToResize = m_SpawnedViews[i];
                        viewToResize.SetSize(GetActionSize(i), m_ButtonTransitionDuration);
                    }

                    var nextView = m_SpawnedViews[0];
                    // Since the animation might change the size of the view, we need to recalculate the desired scroll position every frame
                    float elapsed = 0f;
                    float startScrollPosition = m_ScrollView.verticalNormalizedPosition;
                    // This loop updates the scroll position every frame
                    while (elapsed < m_ButtonTransitionDuration)
                    {
                        // Calculate the ideal scroll position for this frame (based on the current size of nextView)
                        var targetScrollPosition = m_ScrollView.CalculateNormalizedScrollPosition(nextView.transform as RectTransform, Vector2.one * ActionView.FullSize * 0.5f).y;
                        // Lerp so that the scroll position smoothly goes from its starting value to the ideal one
                        m_ScrollView.verticalNormalizedPosition = Mathf.Lerp(startScrollPosition, targetScrollPosition, elapsed / m_ButtonTransitionDuration);

                        yield return new WaitForEndOfFrame();
                        elapsed += Time.deltaTime;
                    }
                    // Set the scroll position to the finalized ideal place after the animation is over
                    m_ScrollView.verticalNormalizedPosition = m_ScrollView.CalculateNormalizedScrollPosition(nextView.transform as RectTransform, Vector2.one * ActionView.FullSize * 0.5f).y;
                }

                // If the game was won with the last action performed
                if (m_Manager.GameState == GameState.Won)
                {
                    m_SfxGameWon.Play();
                    Hide();
                }
            }
        }
        float GetActionSize(int index) => 1f - (Mathf.Clamp(index, 0, 4) / 3f);

        // Tints the timer red
        void ShowTimePenalty()
        {
            m_TimerLabel.DOKill(true);
            m_TimerLabel.DOColor(Color.red, 0.25f).SetLoops(2, LoopType.Yoyo);
            m_TimerLabel.transform.DOScale(1.2f, 0.25f).SetLoops(2, LoopType.Yoyo).SetTarget(m_TimerLabel);
            m_TimerLabel.transform.DOPunchRotation(Vector3.forward * 10, 0.25f).SetTarget(m_TimerLabel);
        }

        // Updates the timer
        void Update() => m_TimerLabel.text = $"{Mathf.CeilToInt(m_Manager.RemainingTime)}";

        void UpdateAlertCanvas()
        {
            float alpha = m_Guard.IsFocusedOnStealer ? 1f : 0f;
            m_AlertCanvas.DOFade(alpha, 0.1f);
        }

        #region IGameGoAction implementation
        void IGameGoActions.OnButtonUp(CallbackContext context)
        {
            if (context.performed) PerformAction(StealAction.Up);
        }
        void IGameGoActions.OnButtonLeft(CallbackContext context)
        {
            if (context.performed) PerformAction(StealAction.Left);
        }
        void IGameGoActions.OnButtonDown(CallbackContext context)
        {
            if (context.performed) PerformAction(StealAction.Down);
        }
        void IGameGoActions.OnButtonRight(CallbackContext context)
        {
            if (context.performed) PerformAction(StealAction.Right);
        }
        #endregion
    }
}
