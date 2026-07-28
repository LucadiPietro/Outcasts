namespace Outcasts.Battle
{
    using NaughtyAttributes;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.DualShock;
    using UnityEngine.InputSystem.XInput;
    using UnityEngine.Pool;
    using UnityEngine.UI;
    using static InputMapping;

    namespace View
    {
        public sealed class BattleViewSystem : MonoBehaviour, IBattleActions, IBattleHorizontalActions
        {
            static BattleViewSystem s_Instance = default;
            public static BattleViewSystem Instance
            {
                get
                {
                    if (s_Instance == null) s_Instance = FindObjectOfType<BattleViewSystem>();
                    return s_Instance;
                }
            }

            List<LaneView> m_Lanes;
            List<LaneView> Lanes
            {
                get
                {
                    if (m_Lanes == null) m_Lanes = new List<LaneView>();
                    if(m_Lanes.Count == 0) GetComponentsInChildren(m_Lanes);

                    return m_Lanes;
                }
            }
            LaneView GetLaneView(NoteLane lane) => Lanes[(int)lane];

            [SerializeField] BattleManager m_Manager;
            IReadOnlyChart Chart => m_Manager.Chart;

            [SerializeField] ControllerViewSettings m_PsControllerSettings;
            [SerializeField] ControllerViewSettings m_XBoxControllerSettings;
            [SerializeField] ControllerViewSettings m_KeyboardControllerSettings;

            public ControllerViewSettings ControllerSettings
            {
                get
                {
                    if (Gamepad.current is XInputController) return m_XBoxControllerSettings;
                    else if (Gamepad.current is DualShockGamepad) return m_PsControllerSettings;
                    else return m_KeyboardControllerSettings;
                }
            }
            [SerializeField, OnValueChanged(nameof(UpdateView))] bool m_Horizontal;
            public bool Horizontal => m_Horizontal;

            void UpdateView()
            {

                var layoutGroup = m_LanesContainer.GetComponent<HorizontalOrVerticalLayoutGroup>();
                if (m_Horizontal && layoutGroup is VerticalLayoutGroup) return;
                if (!m_Horizontal && layoutGroup is HorizontalLayoutGroup) return;

                DestroyImmediate(layoutGroup);

                if (m_Horizontal) layoutGroup = m_LanesContainer.gameObject.AddComponent<VerticalLayoutGroup>();
                else layoutGroup = m_LanesContainer.gameObject.AddComponent<HorizontalLayoutGroup>();

                layoutGroup.childAlignment = TextAnchor.MiddleCenter;
                layoutGroup.childControlHeight = true;
                layoutGroup.childControlWidth = true;
                layoutGroup.childForceExpandHeight = true;
                layoutGroup.childForceExpandWidth = true;

                foreach (var lane in Lanes)
                {
                    lane.IsHorizontal = m_Horizontal;
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(lane.gameObject);
#endif
                }
            }

            [SerializeField] RectTransform m_LanesContainer;
            public InputButton GetButtonData(NoteType eventType, NoteLane noteLane)
            {
                if (m_Horizontal)
                {
                    switch ((eventType, noteLane))
                    {
                        case (NoteType.Attack, NoteLane.First):
                            return ControllerSettings.North;
                        case (NoteType.Defence, NoteLane.First):
                            return ControllerSettings.DpadUp;
                        case (NoteType.Attack, NoteLane.Middle):
                            return ControllerSettings.East;
                        case (NoteType.Defence, NoteLane.Middle):
                            return ControllerSettings.DpadLeft;
                        case (NoteType.Attack, NoteLane.Last):
                            return ControllerSettings.South;
                        case (NoteType.Defence, NoteLane.Last):
                            return ControllerSettings.DpadDown;
                        default:
                            throw new ArgumentOutOfRangeException($"Unknown combinaion ({eventType}, {noteLane})");
                    }
                }
                else
                {
                    switch ((eventType, noteLane))
                    {
                        case (NoteType.Attack, NoteLane.First):
                            return ControllerSettings.West;
                        case (NoteType.Defence, NoteLane.First):
                            return ControllerSettings.DpadLeft;
                        case (NoteType.Attack, NoteLane.Middle):
                            return ControllerSettings.North;
                        case (NoteType.Defence, NoteLane.Middle):
                            return ControllerSettings.DpadDown;
                        case (NoteType.Attack, NoteLane.Last):
                            return ControllerSettings.East;
                        case (NoteType.Defence, NoteLane.Last):
                            return ControllerSettings.DpadRight;
                        default:
                            throw new ArgumentOutOfRangeException($"Unknown combinaion ({eventType}, {noteLane})");
                    }
                }

            }

            InputMapping m_Input;
            void Awake()
            {
                m_Input = new InputMapping();
                if(m_Horizontal) m_Input.BattleHorizontal.SetCallbacks(this);
                else m_Input.Battle.SetCallbacks(this);
            }

            [SerializeField] NoteView m_NotePrefab;
            [SerializeField] NoteLinkView m_NoteLinkPrefab;
            void OnEnable()
            {
                UpdateView();

                if(m_Horizontal) m_Input.BattleHorizontal.Enable();
                else m_Input.Battle.Enable();
                m_Manager.NoteConsumed += DestroyView;
            }

            void DestroyView(Note note, NoteResult result)
            {
                if (m_NoteToView.TryGetValue(note, out var view))
                {
                    m_NoteToView.Remove(note);
                    view.Destroy(result);

                    if(note.Phase == NotePhase.End)
                    {
                        Debug.Log($"Destroying end note {note}");
                        var linkView = m_NoteToLinkView[note];
                        var startNote = Chart.GetLinkedNote(note);
                        linkView.Destroy();
                        m_NoteToLinkView.Remove(startNote);
                        m_NoteToLinkView.Remove(note);
                    }
                }
            }

            Dictionary<Note, NoteView> m_NoteToView = new();
            Dictionary<Note, NoteLinkView> m_NoteToLinkView = new();

            double m_LastSpawn = -1d;
            void Update()
            {
                double now = m_Manager.Elapsed;
                double endTime = now + m_Manager.Settings.NoteTravelTime;
                var notesToSpawn = Chart.GetNotes(new NoteFilter(startTime: m_LastSpawn, endTime: endTime));

                foreach (var note in notesToSpawn)
                {
                    if (note.Phase == NotePhase.End) continue;
                    var view = SpawnView(note);

                    // If this note is the start of a held note, also spawn the end and link them
                    if(note.Phase == NotePhase.Start)
                    {
                        var endNote = Chart.GetLinkedNote(note);
                        var endView = SpawnView(endNote);

                        var link = Instantiate(m_NoteLinkPrefab, GetLaneView(note.Lane).transform);
                        link.SetViews(view, endView);
                        m_NoteToLinkView.Add(note, link);
                        m_NoteToLinkView.Add(endNote, link);
                    }
                }

                m_LastSpawn = endTime + 0.00001d;
            }

            NoteView SpawnView(Note note)
            {
                var laneView = GetLaneView(note.Lane);
                var spawnPosition = laneView.GetSpawnPosition(note.Type);
                var targetPosition = laneView.GetTargetPosition(note.Type);
                var view = Instantiate(m_NotePrefab, spawnPosition.position, Quaternion.identity, laneView.transform);
                view.Note = note;
                view.SetDestination(targetPosition.position, m_Manager.Settings.NoteTravelTime);

                m_NoteToView.Add(note, view);
                return view;
            }

            void OnDisable()
            {
                m_Input.Battle.Disable();
                m_Input.BattleHorizontal.Disable();
            }

            void PerformAction(Note fightAction)
            {
                m_Manager.PerformAction(fightAction);
            }

            #region IBattleActions implementation
            void IBattleActions.OnAttackFirst(InputAction.CallbackContext context) => OnAttackFirst(context);
            void IBattleHorizontalActions.OnAttackFirst(InputAction.CallbackContext context) => OnAttackFirst(context);
            void OnAttackFirst(InputAction.CallbackContext context)
            {
                switch (context.phase)
                {
                    case InputActionPhase.Started:
                        PerformAction(new Note(NoteType.Attack, NoteLane.First, NotePhase.Start, BattleManager.Instance.Elapsed));
                        break;
                    case InputActionPhase.Canceled:
                        PerformAction(new Note(NoteType.Attack, NoteLane.First, NotePhase.End, BattleManager.Instance.Elapsed));
                        break;
                }
            }
            void IBattleActions.OnAttackMiddle(InputAction.CallbackContext context) => OnAttackMiddle(context);
            void IBattleHorizontalActions.OnAttackMiddle(InputAction.CallbackContext context) => OnAttackMiddle(context);
            void OnAttackMiddle(InputAction.CallbackContext context)
            {
                switch (context.phase)
                {
                    case InputActionPhase.Started:
                        PerformAction(new Note(NoteType.Attack, NoteLane.Middle, NotePhase.Start, BattleManager.Instance.Elapsed));
                        break;
                    case InputActionPhase.Canceled:
                        PerformAction(new Note(NoteType.Attack, NoteLane.Middle, NotePhase.End, BattleManager.Instance.Elapsed));
                        break;
                }
            }
            void IBattleActions.OnAttackLast(InputAction.CallbackContext context) => OnAttackLast(context);
            void IBattleHorizontalActions.OnAttackLast(InputAction.CallbackContext context) => OnAttackLast(context);
            void OnAttackLast(InputAction.CallbackContext context)
            {
                switch (context.phase)
                {
                    case InputActionPhase.Started:
                        PerformAction(new Note(NoteType.Attack, NoteLane.Last, NotePhase.Start, BattleManager.Instance.Elapsed));
                        break;
                    case InputActionPhase.Canceled:
                        PerformAction(new Note(NoteType.Attack, NoteLane.Last, NotePhase.End, BattleManager.Instance.Elapsed));
                        break;
                }
            }
            void IBattleActions.OnDefenceFirst(InputAction.CallbackContext context) => OnDefenceFirst(context);
            void IBattleHorizontalActions.OnDefenceFirst(InputAction.CallbackContext context) => OnDefenceFirst(context);
            void OnDefenceFirst(InputAction.CallbackContext context)
            {
                switch (context.phase)
                {
                    case InputActionPhase.Started:
                        PerformAction(new Note(NoteType.Defence, NoteLane.First, NotePhase.Start, BattleManager.Instance.Elapsed));
                        break;
                    case InputActionPhase.Canceled:
                        PerformAction(new Note(NoteType.Defence, NoteLane.First, NotePhase.End, BattleManager.Instance.Elapsed));
                        break;
                }
            }
            void IBattleActions.OnDefenceMiddle(InputAction.CallbackContext context) => OnDefenceMiddle(context);
            void IBattleHorizontalActions.OnDefenceMiddle(InputAction.CallbackContext context) => OnDefenceMiddle(context);
            void OnDefenceMiddle(InputAction.CallbackContext context)
            {
                switch (context.phase)
                {
                    case InputActionPhase.Started:
                        PerformAction(new Note(NoteType.Defence, NoteLane.Middle, NotePhase.Start, BattleManager.Instance.Elapsed));
                        break;
                    case InputActionPhase.Canceled:
                        PerformAction(new Note(NoteType.Defence, NoteLane.Middle, NotePhase.End, BattleManager.Instance.Elapsed));
                        break;
                }
            }
            void IBattleActions.OnDefenceLast(InputAction.CallbackContext context) => OnDefenceLast(context);
            void IBattleHorizontalActions.OnDefenceLast(InputAction.CallbackContext context) => OnDefenceLast(context);
            void OnDefenceLast(InputAction.CallbackContext context)
            {
                switch (context.phase)
                {
                    case InputActionPhase.Started:
                        PerformAction(new Note(NoteType.Defence, NoteLane.Last, NotePhase.Start, BattleManager.Instance.Elapsed));
                        break;
                    case InputActionPhase.Canceled:
                        PerformAction(new Note(NoteType.Defence, NoteLane.Last, NotePhase.End, BattleManager.Instance.Elapsed));
                        break;
                }
            }
            #endregion
        }
    }
}
