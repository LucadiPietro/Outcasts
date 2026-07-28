using System;
using System.Collections.Generic;
using System.Linq;

namespace RhythmCombat.Domain.Combat
{
    public readonly struct DamageApplication
    {
        public DamageApplication(string targetId, int characterIndex, float damageApplied)
        {
            TargetId = targetId;
            CharacterIndex = characterIndex;
            DamageApplied = damageApplied;
        }

        public string TargetId { get; }
        public int CharacterIndex { get; }
        public float DamageApplied { get; }
    }

    public sealed class DamageResolutionResult
    {
        public static DamageResolutionResult Empty { get; } = new DamageResolutionResult(Array.Empty<DamageApplication>());

        public DamageResolutionResult(IEnumerable<DamageApplication> applications)
        {
            Applications = applications.ToList().AsReadOnly();
        }

        public IReadOnlyList<DamageApplication> Applications { get; }
        public float TotalAppliedDamage => Applications.Sum(a => a.DamageApplied);
    }
}