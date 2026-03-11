using System;

namespace RhythmCombat.Domain.Super
{
    public sealed class SuperMeter
    {
        public SuperMeter(float maxValue)
        {
            if (maxValue <= 0f)
                throw new ArgumentOutOfRangeException(nameof(maxValue));

            MaxValue = maxValue;
        }

        public float CurrentValue { get; private set; }
        public float MaxValue { get; }
        public bool IsFull => CurrentValue >= MaxValue;

        public float Add(float amount)
        {
            if (amount < 0f)
                throw new ArgumentOutOfRangeException(nameof(amount));

            var before = CurrentValue;
            CurrentValue = Math.Min(MaxValue, CurrentValue + amount);
            return CurrentValue - before;
        }

        public bool ConsumeFull()
        {
            if (!IsFull)
                return false;

            CurrentValue = 0f;
            return true;
        }
    }
}