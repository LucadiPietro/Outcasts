using System;
using System.Collections.Generic;
using RhythmCombat.Domain.Timing;
using UnityEngine;

public class BattleVisualFeedbackController : MonoBehaviour
{
    public static BattleVisualFeedbackController Instance
    {
        get;
        private set;
    }

    [SerializeField] List<BattleLaneFeedbackSlot> laneSlots =
        new List<BattleLaneFeedbackSlot>(6);

    [SerializeField] float feedbackDuration = 0.55f;

    readonly Dictionary<BMButtonPrefab.Cell, BattleLaneFeedbackSlot>
        slotByCell =
            new Dictionary<BMButtonPrefab.Cell, BattleLaneFeedbackSlot>();

    void Awake()
    {
        Instance = this;
        RebuildLookup();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

#if UNITY_EDITOR
    public void ConfigureEditor(
        List<BattleLaneFeedbackSlot> slots)
    {
        laneSlots = slots ??
            new List<BattleLaneFeedbackSlot>(6);

        RebuildLookup();
    }
#endif

    public void ShowJudgment(
        BMButtonPrefab.Cell cell,
        JudgmentResult result,
        Color laneColor,
        bool isHoldRelease)
    {
        string prefix = isHoldRelease
            ? "HOLD "
            : string.Empty;

        string text =
            prefix +
            result.Grade.ToString().ToUpperInvariant() +
            "\n" +
            FormatDelta(result.DeltaSeconds);

        Show(
            cell,
            text,
            GetGradeColor(result.Grade, laneColor));
    }

    public void ShowHoldRelease(
        BMButtonPrefab.Cell cell,
        JudgmentGrade grade,
        double deltaSeconds,
        Color laneColor)
    {
        ShowJudgment(
            cell,
            new JudgmentResult(grade, deltaSeconds),
            laneColor,
            true);
    }

    public void ShowMiss(
        BMButtonPrefab.Cell cell,
        string reason,
        Color laneColor)
    {
        Show(
            cell,
            string.IsNullOrWhiteSpace(reason)
                ? "MISS"
                : reason,
            GetGradeColor(
                JudgmentGrade.Miss,
                laneColor));
    }

    void Show(
        BMButtonPrefab.Cell cell,
        string text,
        Color color)
    {
        if (!slotByCell.TryGetValue(
                cell,
                out BattleLaneFeedbackSlot slot) ||
            slot == null)
        {
            return;
        }

        slot.Show(text, color, feedbackDuration);
    }

    void RebuildLookup()
    {
        slotByCell.Clear();

        for (int i = 0; i < laneSlots.Count; i++)
        {
            BattleLaneFeedbackSlot slot = laneSlots[i];

            if (slot != null)
            {
                slotByCell[slot.Cell] = slot;
            }
        }
    }

    static string FormatDelta(double deltaSeconds)
    {
        int milliseconds =
            Mathf.RoundToInt(
                (float)(deltaSeconds * 1000d));

        if (milliseconds > 0)
        {
            return "+" + milliseconds + " ms";
        }

        return milliseconds + " ms";
    }

    static Color GetGradeColor(
        JudgmentGrade grade,
        Color laneColor)
    {
        switch (grade)
        {
            case JudgmentGrade.Perfect:
                return Color.Lerp(
                    laneColor,
                    new Color(1f, 0.92f, 0.25f, 1f),
                    0.72f);

            case JudgmentGrade.Good:
                return Color.Lerp(
                    laneColor,
                    new Color(0.25f, 1f, 0.55f, 1f),
                    0.62f);

            case JudgmentGrade.Bad:
                return new Color(1f, 0.55f, 0.15f, 1f);

            default:
                return new Color(1f, 0.16f, 0.16f, 1f);
        }
    }
}
