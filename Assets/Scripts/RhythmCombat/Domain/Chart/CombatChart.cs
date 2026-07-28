using System;
using System.Collections.Generic;
using System.Linq;

namespace RhythmCombat.Domain.Chart
{
    public sealed class CombatChart
    {
        private readonly Dictionary<int, IReadOnlyList<CombatNote>> _notesByLane;

        public CombatChart(IEnumerable<CombatNote> notes, int numLanes = LaneMapping.TotalLaneCount)
        {
            if (notes is null)
                throw new ArgumentNullException(nameof(notes));

            if (numLanes != LaneMapping.TotalLaneCount)
                throw new ArgumentOutOfRangeException(nameof(numLanes), "This combat system expects exactly 6 lanes.");

            var orderedNotes = notes
                .OrderBy(n => n.HitTimeSeconds)
                .ThenBy(n => n.LaneIndex)
                .ToList();

            Notes = orderedNotes.AsReadOnly();
            NumLanes = numLanes;

            _notesByLane = Enumerable.Range(0, NumLanes)
                .ToDictionary(
                    lane => lane,
                    lane => (IReadOnlyList<CombatNote>)orderedNotes
                        .Where(n => n.LaneIndex == lane)
                        .ToList()
                        .AsReadOnly());
        }

        public int NumLanes { get; }
        public IReadOnlyList<CombatNote> Notes { get; }

        public IReadOnlyList<CombatNote> GetLaneNotes(int laneIndex)
        {
            LaneMapping.ValidateLaneIndex(laneIndex);
            return _notesByLane[laneIndex];
        }
    }
}