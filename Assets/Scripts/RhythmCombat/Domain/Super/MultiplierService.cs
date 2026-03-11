using System;

namespace RhythmCombat.Domain.Super
{
    public sealed class MultiplierService
    {
        private readonly int _minMultiplier;
        private readonly int _maxMultiplier;
        private readonly int _step;

        public MultiplierService(int minMultiplier = 1, int maxMultiplier = 8, int step = 1)
        {
            if (minMultiplier <= 0)
                throw new ArgumentOutOfRangeException(nameof(minMultiplier));

            if (maxMultiplier < minMultiplier)
                throw new ArgumentOutOfRangeException(nameof(maxMultiplier));

            if (step <= 0)
                throw new ArgumentOutOfRangeException(nameof(step));

            _minMultiplier = minMultiplier;
            _maxMultiplier = maxMultiplier;
            _step = step;
            CurrentMultiplier = _minMultiplier;
        }

        public int CurrentMultiplier { get; private set; }

        public int RegisterHit()
        {
            CurrentMultiplier = Math.Min(_maxMultiplier, CurrentMultiplier + _step);
            return CurrentMultiplier;
        }

        public int RegisterMiss()
        {
            CurrentMultiplier = _minMultiplier;
            return CurrentMultiplier;
        }

        public void Reset() => CurrentMultiplier = _minMultiplier;
    }
}