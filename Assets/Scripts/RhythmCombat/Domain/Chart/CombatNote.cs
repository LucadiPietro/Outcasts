using System;

namespace RhythmCombat.Domain.Chart
{
    public class CombatNote
    {
        public CombatNote(
            string id,
            int laneIndex,
            double hitTimeSeconds,
            float power = 1f)
            : this(id, laneIndex, hitTimeSeconds, power, NoteType.Tap)
        {
        }

        protected CombatNote(
            string id,
            int laneIndex,
            double hitTimeSeconds,
            float power,
            NoteType noteType)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Note id cannot be null or empty.", nameof(id));

            LaneMapping.ValidateLaneIndex(laneIndex);

            if (hitTimeSeconds < 0d)
                throw new ArgumentOutOfRangeException(nameof(hitTimeSeconds), "Hit time cannot be negative.");

            if (power < 0f)
                throw new ArgumentOutOfRangeException(nameof(power), "Power cannot be negative.");

            Id = id;
            LaneIndex = laneIndex;
            CharacterIndex = LaneMapping.ToCharacterIndex(laneIndex);
            LaneType = LaneMapping.GetLaneType(laneIndex);
            NoteType = noteType;
            HitTimeSeconds = hitTimeSeconds;
            Power = power;
        }

        public string Id { get; }
        public int LaneIndex { get; }
        public int CharacterIndex { get; }
        public LaneType LaneType { get; }
        public NoteType NoteType { get; }
        public double HitTimeSeconds { get; }
        public float Power { get; }
    }
}