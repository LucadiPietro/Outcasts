using RhythmCombat.Domain.Chart;
using RhythmCombat.Domain.Combat;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RhythmCombat.Domain.Super
{
    public enum SuperModeEffect
    {
        SlowMotion = 0,
        ClearAttackNotes = 1,
        ClearDefenseNotes = 2,
        ResolveAttackDamageImmediately = 3,
        FullHealParty = 4
    }

    public sealed record SuperActivationResult
    {
        public bool Activated { get; init; }
        public string Reason { get; init; } = string.Empty;
        public SuperModeEffect? Effect { get; init; }
        public int CharacterIndex { get; init; } = -1;
        public int ClearedNotes { get; init; }
        public DamageResolutionResult DamageResult { get; init; } = DamageResolutionResult.Empty;
    }

    public sealed class SuperModeService
    {
        private readonly DamageResolver _damageResolver;
        private readonly double _slowMotionTimeScale;
        private readonly double _slowMotionDurationSeconds;

        public SuperModeService(
            DamageResolver damageResolver,
            double slowMotionTimeScale = 0.5d,
            double slowMotionDurationSeconds = 3.0d)
        {
            _damageResolver = damageResolver ?? throw new ArgumentNullException(nameof(damageResolver));

            if (slowMotionTimeScale <= 0d || slowMotionTimeScale > 1d)
                throw new ArgumentOutOfRangeException(nameof(slowMotionTimeScale));

            if (slowMotionDurationSeconds <= 0d)
                throw new ArgumentOutOfRangeException(nameof(slowMotionDurationSeconds));

            _slowMotionTimeScale = slowMotionTimeScale;
            _slowMotionDurationSeconds = slowMotionDurationSeconds;
        }

        public bool CanActivate(
            CombatRhythmState state, 
            int characterIndex)
        {
            if (state is null)
                throw new ArgumentNullException(nameof(state));

            var superState = state.GetCharacterSuper(characterIndex);
            return superState.Meter.IsFull && state.IsPartyCharacterAlive(characterIndex);
        }

        public SuperActivationResult TryActivate(
            CombatRhythmState state,
            int defenseLaneIndex,
            SuperModeEffect effect,
            double currentTimeSeconds)
        {
            if (state is null)
                throw new ArgumentNullException(nameof(state));

            LaneMapping.ValidateLaneIndex(defenseLaneIndex);
            if (LaneMapping.GetLaneType(defenseLaneIndex) != LaneType.Defense)
            {
                return new SuperActivationResult
                {
                    Activated = false,
                    Reason = "Super activation requires a defense lane.",
                    CharacterIndex = -1
                };
            }

            var characterIndex = LaneMapping.ToCharacterIndex(defenseLaneIndex);
            if (!CanActivate(state, characterIndex))
            {
                return new SuperActivationResult
                {
                    Activated = false,
                    Reason = "The selected character cannot activate super right now.",
                    CharacterIndex = characterIndex
                };
            }

            var superState = state.GetCharacterSuper(characterIndex);
            if (!superState.Meter.ConsumeFull())
            {
                return new SuperActivationResult
                {
                    Activated = false,
                    Reason = "Super meter is not full.",
                    CharacterIndex = characterIndex
                };
            }

            return effect switch
            {
                SuperModeEffect.SlowMotion => ActivateSlowMotion(state, characterIndex, currentTimeSeconds),
                SuperModeEffect.ClearAttackNotes => ClearPendingNotes(state, characterIndex, LaneType.Attack, effect),
                SuperModeEffect.ClearDefenseNotes => ClearPendingNotes(state, characterIndex, LaneType.Defense, effect),
                SuperModeEffect.ResolveAttackDamageImmediately => ResolvePendingAttackDamage(state, characterIndex),
                SuperModeEffect.FullHealParty => FullHealParty(state, characterIndex),
                _ => new SuperActivationResult
                {
                    Activated = false,
                    Reason = "Unsupported super effect.",
                    CharacterIndex = characterIndex
                }
            };
        }

        private SuperActivationResult ActivateSlowMotion(
            CombatRhythmState state, 
            int characterIndex, 
            double currentTimeSeconds)
        {
            state.EnableSlowMotion(_slowMotionTimeScale, currentTimeSeconds, _slowMotionDurationSeconds);

            return new SuperActivationResult
            {
                Activated = true,
                CharacterIndex = characterIndex,
                Effect = SuperModeEffect.SlowMotion,
                Reason = "Slow motion activated."
            };
        }

        private static SuperActivationResult ClearPendingNotes(
            CombatRhythmState state,
            int characterIndex,
            LaneType laneType,
            SuperModeEffect effect)
        {
            var cleared = state.ClearPendingNotesAndActiveHolds(note => note.LaneType == laneType);

            return new SuperActivationResult
            {
                Activated = true,
                CharacterIndex = characterIndex,
                Effect = effect,
                ClearedNotes = cleared,
                Reason = $"Cleared {cleared} {laneType} notes."
            };
        }

        private SuperActivationResult ResolvePendingAttackDamage(CombatRhythmState state, int characterIndex)
        {
            var applications = new List<DamageApplication>();
            var pendingAttackNotes = state.GetPendingNotes()
                .Where(n => n.LaneType == LaneType.Attack)
                .ToList();

            foreach (var note in pendingAttackNotes)
            {
                var damage = _damageResolver.ResolveAttack(state.Enemies, note.LaneIndex, note.Power);
                applications.AddRange(damage.Applications);
                state.MarkResolved(note);
                state.RemoveActiveHold(note.Id);
            }

            return new SuperActivationResult
            {
                Activated = true,
                CharacterIndex = characterIndex,
                Effect = SuperModeEffect.ResolveAttackDamageImmediately,
                DamageResult = new DamageResolutionResult(applications),
                Reason = "Resolved attack damage for all pending attack notes."
            };
        }

        private static SuperActivationResult FullHealParty(CombatRhythmState state, int characterIndex)
        {
            foreach (var member in state.Party)
                member.HealFull();

            return new SuperActivationResult
            {
                Activated = true,
                CharacterIndex = characterIndex,
                Effect = SuperModeEffect.FullHealParty,
                Reason = "Party fully healed."
            };
        }
    }


}