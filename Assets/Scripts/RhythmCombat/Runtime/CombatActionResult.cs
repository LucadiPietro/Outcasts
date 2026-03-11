using RhythmCombat.Domain.Combat;
using RhythmCombat.Domain.Timing;

namespace RhythmCombat.Runtime
{
    public sealed class CombatActionResult
    {
        public CombatActionResult()
        {
            CharacterIndex = -1;
            ActionKind = string.Empty;
            Message = string.Empty;
            DamageResult = DamageResolutionResult.Empty;
        }

        public int LaneIndex { get; set; }
        public int CharacterIndex { get; set; }
        public double InputTimeSeconds { get; set; }
        public string ActionKind { get; set; }
        public string Message { get; set; }
        public string NoteId { get; set; }
        public JudgmentResult? Judgment { get; set; }
        public int ScoreGained { get; set; }
        public float SuperMeterGained { get; set; }
        public int MultiplierAfter { get; set; }
        public DamageResolutionResult DamageResult { get; set; }
        public bool HoldStarted { get; set; }
        public bool HoldCompleted { get; set; }
    }
}