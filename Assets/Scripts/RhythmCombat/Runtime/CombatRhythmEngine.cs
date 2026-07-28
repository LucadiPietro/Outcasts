using RhythmCombat.Domain.Chart;
using RhythmCombat.Domain.Combat;
using RhythmCombat.Domain.Input;
using RhythmCombat.Domain.Super;
using RhythmCombat.Domain.Timing;
using System;
using System.Collections.Generic;

namespace RhythmCombat.Runtime
{
    public sealed class CombatRhythmEngine
    {
        private readonly INoteJudgmentService _noteJudgmentService;
        private readonly JudgmentService _judgmentService;
        private readonly SuperMeterService _superMeterService;
        private readonly DamageResolver _damageResolver;
        private readonly DefenseResolver _defenseResolver;
        private readonly SuperModeService _superModeService;

        public CombatRhythmEngine(
            JudgmentService judgmentService,
            SuperMeterService superMeterService,
            DamageResolver damageResolver,
            DefenseResolver defenseResolver,
            SuperModeService superModeService)
            : this(
                new TemporalNoteJudgmentService(judgmentService),
                judgmentService,
                superMeterService,
                damageResolver,
                defenseResolver,
                superModeService)
        {
        }

        public CombatRhythmEngine(
            INoteJudgmentService noteJudgmentService,
            JudgmentService judgmentService,
            SuperMeterService superMeterService,
            DamageResolver damageResolver,
            DefenseResolver defenseResolver,
            SuperModeService superModeService)
        {
            _noteJudgmentService = noteJudgmentService ?? throw new ArgumentNullException(nameof(noteJudgmentService));
            _judgmentService = judgmentService ?? throw new ArgumentNullException(nameof(judgmentService));
            _superMeterService = superMeterService ?? throw new ArgumentNullException(nameof(superMeterService));
            _damageResolver = damageResolver ?? throw new ArgumentNullException(nameof(damageResolver));
            _defenseResolver = defenseResolver ?? throw new ArgumentNullException(nameof(defenseResolver));
            _superModeService = superModeService ?? throw new ArgumentNullException(nameof(superModeService));
        }

        public IReadOnlyList<CombatActionResult> Update(CombatRhythmState state, double currentTimeSeconds)
        {
            if (state is null)
                throw new ArgumentNullException(nameof(state));

            state.AdvanceTime(currentTimeSeconds);

            var results = new List<CombatActionResult>();
            results.AddRange(ProcessExpiredTapWindows(state, currentTimeSeconds));
            results.AddRange(ProcessExpiredHoldReleaseWindows(state, currentTimeSeconds));
            return results;
        }

        public CombatActionResult ProcessTapHit(CombatRhythmState state, int laneIndex, double inputTimeSeconds)
        {
            if (state is null)
                throw new ArgumentNullException(nameof(state));

            LaneMapping.ValidateLaneIndex(laneIndex);
            Update(state, inputTimeSeconds);

            var laneState = state.GetLaneState(laneIndex);
            laneState.RegisterPress(inputTimeSeconds);

            if (state.HoldTracker.HasActiveHold(laneIndex))
            {
                return Ignored(laneIndex, inputTimeSeconds, null, "Lane already has an active hold.", state.Multiplier.CurrentMultiplier);
            }

            var nextNote = state.PeekNextPendingNoteForLane(laneIndex);
            if (nextNote is null)
            {
                return Ignored(laneIndex, inputTimeSeconds, null, "No pending note on this lane.", state.Multiplier.CurrentMultiplier);
            }

            if (nextNote.LaneType == LaneType.Defense && !state.IsPartyCharacterAlive(nextNote.CharacterIndex))
            {
                return Ignored(laneIndex, inputTimeSeconds, nextNote.Id, "Defense lane owner is dead, input is ignored.", state.Multiplier.CurrentMultiplier);
            }

            var judgment = _noteJudgmentService.Evaluate(nextNote, inputTimeSeconds);
            if (!judgment.IsHit)
            {
                return Ignored(laneIndex, inputTimeSeconds, nextNote.Id, "Input is outside the hit window.", state.Multiplier.CurrentMultiplier);
            }

            if (nextNote is HoldNote holdNote)
            {
                state.HoldTracker.Start(holdNote, judgment, inputTimeSeconds);

                return new CombatActionResult
                {
                    LaneIndex = laneIndex,
                    CharacterIndex = nextNote.CharacterIndex,
                    InputTimeSeconds = inputTimeSeconds,
                    ActionKind = "HoldStart",
                    Message = "Hold started successfully.",
                    NoteId = nextNote.Id,
                    Judgment = judgment,
                    MultiplierAfter = state.Multiplier.CurrentMultiplier,
                    HoldStarted = true
                };
            }

            state.MarkResolved(nextNote);
            return ApplySuccessfulResolvedNote(state, nextNote, judgment, inputTimeSeconds, "TapHit", false, false);
        }

        public CombatActionResult ProcessLaneRelease(CombatRhythmState state, int laneIndex, double inputTimeSeconds)
        {
            if (state is null)
                throw new ArgumentNullException(nameof(state));

            LaneMapping.ValidateLaneIndex(laneIndex);
            Update(state, inputTimeSeconds);

            var laneState = state.GetLaneState(laneIndex);
            laneState.RegisterRelease(inputTimeSeconds);

            var activeHold = state.HoldTracker.Release(laneIndex);
            if (activeHold is null)
            {
                return Ignored(laneIndex, inputTimeSeconds, null, "No active hold on this lane.", state.Multiplier.CurrentMultiplier);
            }

            if (!_judgmentService.IsHoldReleaseValid(activeHold.Value.Note, inputTimeSeconds))
            {
                state.MarkResolved(activeHold.Value.Note);
                return ApplyMiss(state, activeHold.Value.Note, inputTimeSeconds, "Hold released too early.", "HoldReleaseMiss");
            }

            state.MarkResolved(activeHold.Value.Note);

            return ApplySuccessfulResolvedNote(
                state,
                activeHold.Value.Note,
                activeHold.Value.StartJudgment,
                inputTimeSeconds,
                "HoldComplete",
                holdStarted: true,
                holdCompleted: true);
        }

        public SuperActivationResult TryActivateSuper(
            CombatRhythmState state,
            int defenseLaneIndex,
            SuperModeEffect effect,
            double inputTimeSeconds)
        {
            if (state is null)
                throw new ArgumentNullException(nameof(state));

            Update(state, inputTimeSeconds);
            return _superModeService.TryActivate(state, defenseLaneIndex, effect, inputTimeSeconds);
        }

        #region Private Methods

        private IReadOnlyList<CombatActionResult> ProcessExpiredTapWindows(CombatRhythmState state, double currentTimeSeconds)
        {
            var results = new List<CombatActionResult>();

            for (var lane = 0; lane < state.Chart.NumLanes; lane++)
            {
                if (state.HoldTracker.HasActiveHold(lane))
                    continue;

                while (true)
                {
                    var note = state.PeekNextPendingNoteForLane(lane);
                    if (note is null)
                        break;

                    if (!_noteJudgmentService.IsMissed(note, currentTimeSeconds))
                        break;

                    state.MarkResolved(note);
                    results.Add(ApplyMiss(state, note, currentTimeSeconds, "Tap window expired.", "AutoMiss"));
                }
            }

            return results;
        }

        private IReadOnlyList<CombatActionResult> ProcessExpiredHoldReleaseWindows(CombatRhythmState state, double currentTimeSeconds)
        {
            var results = new List<CombatActionResult>();
            var expiredHolds = new List<ActiveHold>();

            foreach (var activeHold in state.HoldTracker.ActiveHolds)
            {
                if (_judgmentService.IsHoldReleaseWindowClosed(activeHold.Note, currentTimeSeconds))
                    expiredHolds.Add(activeHold);
            }

            foreach (var activeHold in expiredHolds)
            {
                state.HoldTracker.Release(activeHold.Note.LaneIndex);
                state.MarkResolved(activeHold.Note);
                results.Add(ApplyMiss(state, activeHold.Note, currentTimeSeconds, "Hold release window expired.", "HoldAutoMiss"));
            }

            return results;
        }

        private CombatActionResult ApplySuccessfulResolvedNote(
            CombatRhythmState state,
            CombatNote note,
            JudgmentResult judgment,
            double inputTimeSeconds,
            string actionKind,
            bool holdStarted,
            bool holdCompleted)
        {
            state.RegisterSuccessfulHit();

            var multiplierAfter = state.Multiplier.RegisterHit();
            var scoreGained = _judgmentService.Config.GetScore(judgment.Grade) * multiplierAfter;
            state.AddScore(scoreGained);

            var characterSuper = state.GetCharacterSuper(note.CharacterIndex);
            var meterGained = _superMeterService.RegisterHit(characterSuper, judgment, multiplierAfter);

            var damage = note.LaneType == LaneType.Attack
                ? _damageResolver.ResolveAttack(state.Enemies, note.LaneIndex, note.Power)
                : DamageResolutionResult.Empty;

            return new CombatActionResult
            {
                LaneIndex = note.LaneIndex,
                CharacterIndex = note.CharacterIndex,
                InputTimeSeconds = inputTimeSeconds,
                ActionKind = actionKind,
                Message = "Successful hit.",
                NoteId = note.Id,
                Judgment = judgment,
                ScoreGained = scoreGained,
                SuperMeterGained = meterGained,
                MultiplierAfter = multiplierAfter,
                DamageResult = damage,
                HoldStarted = holdStarted,
                HoldCompleted = holdCompleted
            };
        }

        private CombatActionResult ApplyMiss(
            CombatRhythmState state,
            CombatNote note,
            double inputTimeSeconds,
            string message,
            string actionKind)
        {
            state.RegisterMiss();
            var multiplierAfter = state.Multiplier.RegisterMiss();

            var damage = note.LaneType == LaneType.Defense
                ? _defenseResolver.ResolveDefenseMiss(state.Party, note.LaneIndex, note.Power)
                : DamageResolutionResult.Empty;

            return new CombatActionResult
            {
                LaneIndex = note.LaneIndex,
                CharacterIndex = note.CharacterIndex,
                InputTimeSeconds = inputTimeSeconds,
                ActionKind = actionKind,
                Message = message,
                NoteId = note.Id,
                Judgment = new JudgmentResult(JudgmentGrade.Miss, inputTimeSeconds - note.HitTimeSeconds),
                MultiplierAfter = multiplierAfter,
                DamageResult = damage
            };
        }

        private static CombatActionResult Ignored(
            int laneIndex,
            double inputTimeSeconds,
            string? noteId,
            string message,
            int currentMultiplier)
        {
            return new CombatActionResult
            {
                LaneIndex = laneIndex,
                CharacterIndex = LaneMapping.ToCharacterIndex(laneIndex),
                InputTimeSeconds = inputTimeSeconds,
                ActionKind = "IgnoredInput",
                Message = message,
                NoteId = noteId,
                MultiplierAfter = currentMultiplier
            };
        }

        #endregion
    }
}
