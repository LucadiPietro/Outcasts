using System;

namespace RhythmCombat.Domain.Movement
{
    public sealed class NoteTravelSettings
    {
        public NoteTravelSettings(double approachDurationSeconds, double despawnAfterHitSeconds)
        {
            if (approachDurationSeconds <= 0d)
                throw new ArgumentOutOfRangeException(nameof(approachDurationSeconds), "Approach duration must be greater than zero.");

            if (despawnAfterHitSeconds < 0d)
                throw new ArgumentOutOfRangeException(nameof(despawnAfterHitSeconds), "Despawn delay cannot be negative.");

            ApproachDurationSeconds = approachDurationSeconds;
            DespawnAfterHitSeconds = despawnAfterHitSeconds;
        }

        public double ApproachDurationSeconds { get; }
        public double DespawnAfterHitSeconds { get; }
    }
}
