using RhythmCombat.Domain.Chart;
using System;

namespace RhythmCombat.Domain.Geometry
{
    public sealed class LaneGeometry
    {
        public LaneGeometry(int laneIndex, Double2 center, double toleranceRadius)
        {
            LaneMapping.ValidateLaneIndex(laneIndex);

            if (toleranceRadius <= 0d)
                throw new ArgumentOutOfRangeException(nameof(toleranceRadius), "Tolerance radius must be greater than zero.");

            LaneIndex = laneIndex;
            Center = center;
            ToleranceRadius = toleranceRadius;
        }

        public int LaneIndex { get; }
        public Double2 Center { get; }
        public double ToleranceRadius { get; }
    }
}
