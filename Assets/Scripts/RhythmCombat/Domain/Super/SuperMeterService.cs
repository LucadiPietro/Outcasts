using RhythmCombat.Domain.Timing;
using System;

namespace RhythmCombat.Domain.Super
{
    public sealed class SuperMeterService
    {
        public float RegisterHit(CharacterSuperState characterSuper, JudgmentResult judgment, int multiplierAfterHit)
        {
            if (characterSuper is null)
                throw new ArgumentNullException(nameof(characterSuper));

            if (!judgment.IsHit)
                return 0f;

            if (multiplierAfterHit <= 0)
                throw new ArgumentOutOfRangeException(nameof(multiplierAfterHit));

            return characterSuper.Meter.Add(multiplierAfterHit);
        }
    }
}