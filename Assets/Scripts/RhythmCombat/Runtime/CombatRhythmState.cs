using RhythmCombat.Domain.Chart;
using RhythmCombat.Domain.Combat;
using RhythmCombat.Domain.Input;
using RhythmCombat.Domain.Super;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RhythmCombat.Runtime
{
    public sealed class CombatRhythmState
    {
        private readonly Dictionary<int, int> _nextNoteIndexByLane = new();
        private readonly HashSet<string> _resolvedNoteIds = new(StringComparer.Ordinal);
        private readonly Dictionary<int, InputLaneState> _laneStates = new();
        private readonly Dictionary<int, CharacterSuperState> _superByCharacter = new();

        public CombatRhythmState(
            CombatChart chart,
            IReadOnlyList<PartyCharacterSlot> party,
            IReadOnlyList<EnemySlot> enemies,
            IReadOnlyList<CharacterSuperState> characterSupers,
            MultiplierService multiplier)
        {
            Chart = chart ?? throw new ArgumentNullException(nameof(chart));
            Party = party ?? throw new ArgumentNullException(nameof(party));
            Enemies = enemies ?? throw new ArgumentNullException(nameof(enemies));
            Multiplier = multiplier ?? throw new ArgumentNullException(nameof(multiplier));
            HoldTracker = new HoldTracker();

            if (characterSupers is null)
                throw new ArgumentNullException(nameof(characterSupers));

            foreach (var characterSuper in characterSupers)
                _superByCharacter[characterSuper.CharacterIndex] = characterSuper;

            for (var lane = 0; lane < chart.NumLanes; lane++)
            {
                _nextNoteIndexByLane[lane] = 0;
                _laneStates[lane] = new InputLaneState(lane);
            }
        }

        public CombatChart Chart { get; }
        public IReadOnlyList<PartyCharacterSlot> Party { get; }
        public IReadOnlyList<EnemySlot> Enemies { get; }
        public MultiplierService Multiplier { get; }
        public HoldTracker HoldTracker { get; }

        public int Score { get; private set; }
        public int Combo { get; private set; }
        public double CurrentTimeSeconds { get; private set; }
        public double TimeScale { get; private set; } = 1d;
        public double? SlowMotionEndsAtSeconds { get; private set; }

        public CharacterSuperState GetCharacterSuper(int characterIndex)
        {
            LaneMapping.ValidateCharacterIndex(characterIndex);
            return _superByCharacter[characterIndex];
        }

        public CharacterSuperState GetCharacterSuperByLane(int laneIndex)
            => GetCharacterSuper(LaneMapping.ToCharacterIndex(laneIndex));

        public InputLaneState GetLaneState(int laneIndex)
        {
            LaneMapping.ValidateLaneIndex(laneIndex);
            return _laneStates[laneIndex];
        }

        public bool IsPartyCharacterAlive(int characterIndex)
        {
            LaneMapping.ValidateCharacterIndex(characterIndex);
            return characterIndex < Party.Count && Party[characterIndex].IsAlive;
        }

        public void AdvanceTime(double currentTimeSeconds)
        {
            if (currentTimeSeconds < CurrentTimeSeconds)
                throw new InvalidOperationException("Time cannot go backwards.");

            CurrentTimeSeconds = currentTimeSeconds;

            if (SlowMotionEndsAtSeconds.HasValue && currentTimeSeconds >= SlowMotionEndsAtSeconds.Value)
            {
                TimeScale = 1d;
                SlowMotionEndsAtSeconds = null;
            }
        }

        public void EnableSlowMotion(double timeScale, double currentTimeSeconds, double durationSeconds)
        {
            if (timeScale <= 0d || timeScale > 1d)
                throw new ArgumentOutOfRangeException(nameof(timeScale));

            if (durationSeconds <= 0d)
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));

            TimeScale = timeScale;
            SlowMotionEndsAtSeconds = currentTimeSeconds + durationSeconds;
        }

        public void AddScore(int score)
        {
            if (score < 0)
                throw new ArgumentOutOfRangeException(nameof(score));

            Score += score;
        }

        public void RegisterSuccessfulHit() => Combo++;

        public void RegisterMiss() => Combo = 0;

        public bool IsResolved(CombatNote note)
        {
            if (note is null)
                throw new ArgumentNullException(nameof(note));

            return _resolvedNoteIds.Contains(note.Id);
        }

        public CombatNote? PeekNextPendingNoteForLane(int laneIndex)
        {
            LaneMapping.ValidateLaneIndex(laneIndex);

            var laneNotes = Chart.GetLaneNotes(laneIndex);
            var nextIndex = _nextNoteIndexByLane[laneIndex];

            while (nextIndex < laneNotes.Count && _resolvedNoteIds.Contains(laneNotes[nextIndex].Id))
                nextIndex++;

            _nextNoteIndexByLane[laneIndex] = nextIndex;
            return nextIndex < laneNotes.Count ? laneNotes[nextIndex] : null;
        }

        public void MarkResolved(CombatNote note)
        {
            if (note is null)
                throw new ArgumentNullException(nameof(note));

            if (!_resolvedNoteIds.Add(note.Id))
                return;

            var laneNotes = Chart.GetLaneNotes(note.LaneIndex);
            var nextIndex = _nextNoteIndexByLane[note.LaneIndex];

            while (nextIndex < laneNotes.Count && _resolvedNoteIds.Contains(laneNotes[nextIndex].Id))
                nextIndex++;

            _nextNoteIndexByLane[note.LaneIndex] = nextIndex;
        }

        public bool RemoveActiveHold(string noteId) => HoldTracker.RemoveByNoteId(noteId);

        public IEnumerable<CombatNote> GetPendingNotes()
            => Chart.Notes.Where(n => !_resolvedNoteIds.Contains(n.Id));

        public int ClearPendingNotesAndActiveHolds(Func<CombatNote, bool> predicate)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            var pending = GetPendingNotes().Where(predicate).ToList();
            foreach (var note in pending)
            {
                MarkResolved(note);
                RemoveActiveHold(note.Id);
            }

            return pending.Count;
        }
    }
}