using RhythmCombat.Domain.Chart;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RhythmCombat.Domain.Combat
{
    public sealed class DamageResolver
    {
        public DamageResolutionResult ResolveAttack(IReadOnlyList<EnemySlot> enemies, int attackLaneIndex, float baseDamage)
        {
            if (enemies is null)
                throw new ArgumentNullException(nameof(enemies));

            LaneMapping.ValidateLaneIndex(attackLaneIndex);

            if (LaneMapping.GetLaneType(attackLaneIndex) != LaneType.Attack)
                throw new ArgumentException("ResolveAttack expects an attack lane.", nameof(attackLaneIndex));

            if (baseDamage < 0f)
                throw new ArgumentOutOfRangeException(nameof(baseDamage));

            if (baseDamage == 0f)
                return DamageResolutionResult.Empty;

            var characterIndex = LaneMapping.ToCharacterIndex(attackLaneIndex);
            var primary = characterIndex < enemies.Count ? enemies[characterIndex] : null;

            if (primary is not null && primary.IsAlive)
                return Apply(primary, baseDamage);

            var fallbackTargets = enemies
                .Where(e => e.CharacterIndex != characterIndex && e.IsAlive)
                .ToList();

            if (fallbackTargets.Count == 0)
                return DamageResolutionResult.Empty;

            return ApplySplit(fallbackTargets, baseDamage * 0.5f);
        }

        private static DamageResolutionResult Apply(EnemySlot target, float damage)
        {
            var applied = target.TakeDamage(damage);
            return new DamageResolutionResult(new[]
            {
                new DamageApplication(target.Id, target.CharacterIndex, applied)
            });
        }

        private static DamageResolutionResult ApplySplit(IReadOnlyList<EnemySlot> targets, float totalDamage)
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