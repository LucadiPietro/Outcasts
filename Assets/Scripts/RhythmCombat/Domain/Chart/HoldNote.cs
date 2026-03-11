using System;

namespace RhythmCombat.Domain.Chart
{
    public sealed class HoldNote : CombatNote
    {
        public HoldNote(
            string Id,
            int LaneIndex,
            double StartTimeSeconds,
            double EndTimeSeconds,
            float Power = 1f)
            : base(Id, LaneIndex, StartTimeSeconds, Power, NoteType.Hold)
        {
            if (EndTimeSeconds <= StartTimeSeconds)
                throw new ArgumentOutOfRangeException(nameof(EndTimeSeconds), "Hold end time must be greater than hold start time.");

            this.EndTimeSeconds = EndTimeSeconds;
        }

        public double EndTimeSeconds { get; }
        public double DurationSeconds => EndTimeSeconds - HitTimeSeconds;
    }
}