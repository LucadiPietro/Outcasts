namespace Outcasts.Battle
{
    using DG.Tweening;
    using LemonGames;
    using NaughtyAttributes;
    using Newtonsoft.Json.Linq;
    using UnityEngine;
    using UnityEngine.UI;

    namespace View
    {
        public sealed class LaneView : MonoBehaviour
        {
            [SerializeField] ButtonView m_TopButton;
            [SerializeField] ButtonView m_BottomButton;
            [SerializeField] Image m_Background;
            [SerializeField] float m_BackgroundAlpha = 0.25f;

            [SerializeField] RectTransform m_TopSpawnPosition;
            [SerializeField] RectTransform m_BottomSpawnPosition;
            RectTransform TopTargetPosition => m_TopButton.transform as RectTransform;
            RectTransform BottomTargetPosition => m_BottomButton.transform as RectTransform;

            [SerializeField, OnValueChanged(nameof(UpdateIcons))] NoteLane m_Lane;
            public NoteLane Lane
            {
                get { return m_Lane; }
                set 
                { 
                    m_Lane = value;
                    UpdateIcons();
                }
            }

            // TODO: Remove when we decide the layout
            void OnEnable()
            {
                UpdatePositions();
            }

            void UpdateIcons()
            {
                if (BattleViewSystem.Instance == null) return;

                m_TopButton.Button = BattleViewSystem.Instance.GetButtonData(NoteType.Attack, Lane);
                m_BottomButton.Button = BattleViewSystem.Instance.GetButtonData(NoteType.Defence, Lane);
                m_Background.color = m_TopButton.Button.MainColor.With(a: m_BackgroundAlpha);
            }

            [SerializeField, OnValueChanged(nameof(UpdatePositions))] bool m_IsHorizontal;
            public bool IsHorizontal
            {
                get { return m_IsHorizontal; }
                set 
                {
                    m_IsHorizontal = value;
                    UpdatePositions();
                }
            }
            void UpdatePositions()
            {
                var buttonTransform = m_TopButton.transform as RectTransform;
                float halfSize = buttonTransform.sizeDelta.y * 0.5f;
                var enemyBar = m_TopButton.transform.parent as RectTransform;
                var alliesBar = m_BottomButton.transform.parent as RectTransform;
                if (!IsHorizontal)
                {
                    enemyBar.anchorMin = new Vector2(0, 1);
                    enemyBar.anchorMax = new Vector2(1, 1);
                    enemyBar.sizeDelta= new Vector2(0, halfSize * 2);
                    enemyBar.anchoredPosition = new Vector2(0, -halfSize * 3);
                    m_TopSpawnPosition.anchoredPosition = new Vector2(0, halfSize);

                    alliesBar.anchorMin = new Vector2(0, 0);
                    alliesBar.anchorMax = new Vector2(1, 0);
                    alliesBar.anchoredPosition = new Vector2(0, halfSize * 3);
                    alliesBar.sizeDelta = new Vector2(0, halfSize * 2);
                    m_BottomSpawnPosition.anchoredPosition = new Vector2(0, -halfSize);
                }
                else
                {
                    enemyBar.anchorMin = new Vector2(1, 0);
                    enemyBar.anchorMax = new Vector2(1, 1);
                    enemyBar.sizeDelta = new Vector2(halfSize * 2, 0);
                    enemyBar.anchoredPosition = new Vector2(-halfSize * 3, 0);
                    m_TopSpawnPosition.anchoredPosition = new Vector2(halfSize, 0);

                    alliesBar.anchorMin = new Vector2(0, 0);
                    alliesBar.anchorMax = new Vector2(0, 1);
                    alliesBar.sizeDelta = new Vector2(halfSize * 2, 0);
                    alliesBar.anchoredPosition = new Vector2(halfSize * 3, 0);
                    m_BottomSpawnPosition.anchoredPosition = new Vector2(-halfSize, 0);
                }

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif

                UpdateIcons();
            }

            public RectTransform GetSpawnPosition(NoteType type)
            {
                switch (type)
                {
                    case NoteType.Attack:
                        return m_TopSpawnPosition;
                    case NoteType.Defence:
                    default:
                        return m_BottomSpawnPosition;
                }
            }
            public RectTransform GetTargetPosition(NoteType type)
            {
                switch (type)
                {
                    case NoteType.Attack:
                        return TopTargetPosition;
                    case NoteType.Defence:
                    default:
                        return BottomTargetPosition;
                }
            }
        }
    }
}
