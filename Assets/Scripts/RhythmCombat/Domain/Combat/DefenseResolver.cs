using RhythmCombat.Domain.Chart;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RhythmCombat.Domain.Combat
{
    public sealed class DefenseResolver
    {
        public DamageResolutionResult ResolveDefenseMiss(IReadOnlyList<PartyCharacterSlot> party, int defenseLaneIndex, float baseDamage)
        {
            if (party is null)
                throw new ArgumentNullException(nameof(party));

            LaneMapping.ValidateLaneIndex(defenseLaneIndex);

            if (LaneMapping.GetLaneType(defenseLaneIndex) != LaneType.Defense)
                throw new ArgumentException("ResolveDefenseMiss expects a defense lane.", nameof(defenseLaneIndex));

            if (baseDamage < 0f)
                throw new ArgumentOutOfRangeException(nameof(baseDamage));

            if (baseDamage == 0f)
                return DamageResolutionResult.Empty;

            var characterIndex = LaneMapping.ToCharacterIndex(defenseLaneIndex);
            var primary = characterIndex < party.Count ? party[characterIndex] : null;

            if (primary is not null && primary.IsAlive)
                return Apply(primary, baseDamage);

            var fallbackTargets = party
                .Where(p => p.CharacterIndex != characterIndex && p.IsAlive)
                .ToList();

            if (fallbackTargets.Count == 0)
                return DamageResolutionResult.Empty;

            return ApplySplit(fallbackTargets, baseDamage * 0.5f);
        }

        private static DamageResolutionResult Apply(PartyCharacterSlot target, float damage)
        {
            var applied = target.TakeDamage(damage);
            return new DamageResolutionResult(new[]
            {
                new DamageApplication(target.Id, target.CharacterIndex, applied)
            });
        }

        private static DamageResolutionResult ApplySplit(IReadOnlyList<PartyCharacterSlot> targets, float totalDamage)
        {
            var perTarget = totalDamage / targets.Count;
            var applications = new List<DamageApplication>(targets.Count);

            foreach (var target in targets)
            {
                var applied = target.TakeDamage(perTarget);
                applications.Add(new DamageApplication(target.Id, target.CharacterIndex, applied));
            }

            return new DamageResolutionResult(applications);
        }
    }
}