using RhythmCombat.Domain.Chart;
using RhythmCombat.Domain.Timing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RhythmCombat.Domain.Input
{
    public readonly struct ActiveHold
    {
        public ActiveHold(HoldNote note, JudgmentResult startJudgment, double startedAtSeconds)
        {
            Note = note ?? throw new ArgumentNullException(nameof(note));
            StartJudgment = startJudgment;
            StartedAtSeconds = startedAtSeconds;
        }

        public HoldNote Note { get; }
        public JudgmentResult StartJudgment { get; }
        public double StartedAtSeconds { get; }
    }

    public sealed class HoldTracker
    {
        private readonly Dictionary<int, ActiveHold> _activeByLane = new();

        public IReadOnlyCollection<ActiveHold> ActiveHolds => _activeByLane.Values.ToList().AsReadOnly();

        public bool HasActiveHold(int laneIndex) => _activeByLane.ContainsKey(laneIndex);

        public ActiveHold? GetActiveHold(int laneIndex)
            => _activeByLane.TryGetValue(laneIndex, out var active) ? active : null;

        public void Start(HoldNote note, JudgmentResult startJudgment, double startedAtSeconds)
        {
            if (note is null)
                throw new ArgumentNullException(nameof(note));

            _activeByLane[note.LaneIndex] = new ActiveHold(note, startJudgment, startedAtSeconds);
        }

        public ActiveHold? Release(int laneIndex)
        {
            if (!_activeByLane.TryGetValue(laneIndex, out var active))
                return null;

            _activeByLane.Remove(laneIndex);
            return active;
        }

        public bool RemoveByNoteId(string noteId)
        {
            if (string.IsNullOrWhiteSpace(noteId))
                return false;

            var match = _activeByLane.FirstOrDefault(kvp => kvp.Value.Note.Id == noteId);
            if (string.IsNullOrEmpty(match.Value.Note.Id))
                return false;

            return _activeByLane.Remove(match.Key);
        }

        public IReadOnlyList<ActiveHold> RemoveWhere(Func<ActiveHold, bool> predicate)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            var removed = _activeByLane.Values.Where(predicate).ToList();
            foreach (var active in removed)
                _activeByLane.Remove(active.Note.LaneIndex);

            return removed.AsReadOnly();
        }
    }
}