using System;

namespace RhythmCombat.Domain.Timing
{
    public enum JudgmentGrade
    {
        Miss = 0,
        Bad = 1,
        Good = 2,
        Perfect = 3
    }

    public readonly struct JudgmentResult
    {
        public JudgmentResult(JudgmentGrade grade, double deltaSeconds)
        {
            Grade = grade;
            DeltaSeconds = deltaSeconds;
        }

        public JudgmentGrade Grade { get; }
        public double DeltaSeconds { get; }
        
        public bool IsHit => Grade != JudgmentGrade.Miss;
        public double AbsoluteDeltaSeconds => Math.Abs(DeltaSeconds);
    }
}