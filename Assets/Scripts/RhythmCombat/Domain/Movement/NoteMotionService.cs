using RhythmCombat.Domain.Chart;
using RhythmCombat.Domain.Geometry;
using System;

namespace RhythmCombat.Domain.Movement
{
    public sealed class NoteMotionService
    {
        public NoteMotionService(BattlefieldLayout layout, NoteTravelSettings settings)
        {
            Layout = layout ?? throw new ArgumentNullException(nameof(layout));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public BattlefieldLayout Layout { get; }
        public NoteTravelSettings Settings { get; }

        public NotePositionResult GetPosition(CombatNote note, double currentTimeSeconds)
        {
            if (note is null)
                throw new ArgumentNullException(nameof(note));

            var lane = Layout.GetLane(note.LaneIndex);
            var spawnCenter = Layout.GetSpawnCenter(note.LaneIndex);
            var spawnTime = note.HitTimeSeconds - Settings.ApproachDurationSeconds;
            var progress = (currentTimeSeconds - spawnTime) / Settings.ApproachDurationSeconds;
            var position = Double2.Lerp(spawnCenter, lane.Center, progress);
            var shouldDespawn = currentTimeSeconds > note.HitTimeSeconds + Settings.DespawnAfterHitSeconds;

            return new NotePositionResult(position, progress, shouldDespawn);
        }
    }
}
