using RhythmCombat.Domain.Chart;
using System;

namespace RhythmCombat.Domain.Timing
{
    public sealed class TemporalNoteJudgmentService : INoteJudgmentService
    {
        private readonly JudgmentService _judgmentService;

        public TemporalNoteJudgmentService(JudgmentService judgmentService)
        {
            _judgmentService = judgmentService ?? throw new ArgumentNullException(nameof(judgmentService));
        }

        public JudgmentResult Evaluate(CombatNote note, double currentTimeSeconds)
        {
            if (note is null)
                throw new ArgumentNullException(nameof(note));

            return _judgmentService.EvaluateTap(note.HitTimeSeconds, currentTimeSeconds);
        }

        public bool IsMissed(CombatNote note, double currentTimeSeconds)
        {
            if (note is null)
                throw new ArgumentNullException(nameof(note));

            return _judgmentService.IsTapWindowClosed(note.HitTimeSeconds, currentTimeSeconds);
        }
    }
}
