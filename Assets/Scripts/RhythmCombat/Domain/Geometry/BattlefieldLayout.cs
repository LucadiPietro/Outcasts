using RhythmCombat.Domain.Chart;
using System;
using System.Collections.Generic;

namespace RhythmCombat.Domain.Geometry
{
    public sealed class BattlefieldLayout
    {
        private readonly Dictionary<int, LaneGeometry> _lanesByIndex;

        public BattlefieldLayout(Double2 spawnCenter, IEnumerable<LaneGeometry> lanes)
        {
            if (lanes is null)
                throw new ArgumentNullException(nameof(lanes));

            SpawnCenter = spawnCenter;
            _lanesByIndex = new Dictionary<int, LaneGeometry>();

            foreach (var lane in lanes)
            {
                if (lane is null)
                    throw new ArgumentException("Lane collection cannot contain null entries.", nameof(lanes));

                if (_lanesByIndex.ContainsKey(lane.LaneIndex))
                    throw new ArgumentException("Duplicate lane index: " + lane.LaneIndex, nameof(lanes));

                _lanesByIndex.Add(lane.LaneIndex, lane);
            }
        }

        public Double2 SpawnCenter { get; }

        public LaneGeometry GetLane(int laneIndex)
        {
            LaneMapping.ValidateLaneIndex(laneIndex);

            if (!_lanesByIndex.TryGetValue(laneIndex, out var lane))
                throw new KeyNotFoundException("No geometry configured for lane " + laneIndex + ".");

            return lane;
        }

        public static BattlefieldLayout CreateStandard(double laneSpacing = 2d, double rowOffset = 1d, double toleranceRadius = 1d)
        {
            var lanes = new[]
            {
                new LaneGeometry(0, new Double2(-laneSpacing, rowOffset), toleranceRadius),
                new LaneGeometry(1, new Double2(0d, rowOffset), toleranceRadius),
                new LaneGeometry(2, new Double2(laneSpacing, rowOffset), toleranceRadius),
                new LaneGeometry(3, new Double2(-laneSpacing, -rowOffset), toleranceRadius),
                new LaneGeometry(4, new Double2(0d, -rowOffset), toleranceRadius),
                new LaneGeometry(5, new Double2(laneSpacing, -rowOffset), toleranceRadius)
            };

            return new BattlefieldLayout(new Double2(0d, 0d), lanes);
        }
    }
}
