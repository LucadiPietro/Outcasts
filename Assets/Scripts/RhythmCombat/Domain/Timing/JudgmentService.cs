using RhythmCombat.Domain.Chart;
using RhythmCombat.Domain.Timing;
using System;

namespace RhythmCombat.Domain.Timing
{
    public sealed class JudgmentService
    {
        public JudgmentService(JudgmentConfig config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public JudgmentConfig Config { get; }

        public JudgmentResult EvaluateTap(double scheduledTimeSeconds, double inputTimeSeconds)
        {
            var delta = inputTimeSeconds - scheduledTimeSeconds;
            var abs = Math.Abs(delta);

            if (abs <= Config.PerfectWindowSeconds)
                return new JudgmentResult(JudgmentGrade.Perfect, delta);

            if (abs <= Config.GoodWindowSeconds)
                return new JudgmentResult(JudgmentGrade.Good, delta);

            if (abs <= Config.BadWindowSeconds)
                return new JudgmentResult(JudgmentGrade.Bad, delta);

            return new JudgmentResult(JudgmentGrade.Miss, delta);
        }

        public bool IsTapWindowClosed(double scheduledTimeSeconds, double currentTimeSeconds)
            => currentTimeSeconds > scheduledTimeSeconds + Config.BadWindowSeconds;

        public bool IsHoldReleaseValid(HoldNote holdNote, double releaseTimeSeconds)
        {
            if (holdNote is null)
                throw new ArgumentNullException(nameof(holdNote));

            return releaseTimeSeconds >= holdNote.EndTimeSeconds - Config.HoldReleaseWindowSeconds;
        }

        public bool IsHoldReleaseWindowClosed(HoldNote holdNote, double currentTimeSeconds)
        {
            if (holdNote is null)
                throw new ArgumentNullException(nameof(holdNote));

            return currentTimeSeconds > holdNote.EndTimeSeconds + Config.HoldReleaseWindowSeconds;
        }
    }
}