using System;

namespace RhythmCombat.Domain.Chart
{
    public sealed class HoldNote : CombatNote
    {
        public HoldNote(
            string id,
            int laneIndex,
            double startTimeSeconds,
            double endTimeSeconds,
            float power = 1f)
            : base(id, laneIndex, startTimeSeconds, power, NoteType.Hold)
        {
            if (endTimeSeconds <= startTimeSeconds)
                throw new ArgumentOutOfRangeException(nameof(endTimeSeconds), "Hold end time must be greater than hold start time.");

            EndTimeSeconds = endTimeSeconds;
        }

        public double EndTimeSeconds { get; }
        public double DurationSeconds => EndTimeSeconds - HitTimeSeconds;
    }
}