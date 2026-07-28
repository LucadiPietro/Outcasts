using RhythmCombat.Domain.Geometry;

namespace RhythmCombat.Domain.Movement
{
    public struct NotePositionResult
    {
        public NotePositionResult(Double2 position, double normalizedProgress, bool shouldDespawn)
        {
            Position = position;
            NormalizedProgress = normalizedProgress;
            ShouldDespawn = shouldDespawn;
        }

        public Double2 Position { get; }
        public double NormalizedProgress { get; }
        public bool ShouldDespawn { get; }
    }
}
