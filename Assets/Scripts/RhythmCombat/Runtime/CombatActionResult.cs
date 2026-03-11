using RhythmCombat.Domain.Combat;
using RhythmCombat.Domain.Timing;

namespace RhythmCombat.Runtime
{
    public sealed record CombatActionResult
    {
        public int LaneIndex { get; init; }
        public int CharacterIndex { get; init; } = -1;
        public double InputTimeSeconds { get; init; }
        public string ActionKind { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string? NoteId { get; init; }
        public JudgmentResult? Judgment { get; init; }
        public int ScoreGained { get; init; }
        public float SuperMeterGained { get; init; }
        public int MultiplierAfter { get; init; }
        public DamageResolutionResult DamageResult { get; init; } = DamageResolutionResult.Empty;
        public bool HoldStarted { get; init; }
        public bool HoldCompleted { get; init; }
    }
}