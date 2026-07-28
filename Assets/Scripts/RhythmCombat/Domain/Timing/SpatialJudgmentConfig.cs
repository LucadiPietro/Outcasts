using System;

namespace RhythmCombat.Domain.Timing
{
    public sealed class SpatialJudgmentConfig
    {
        public SpatialJudgmentConfig(double perfectPercent, double goodPercent, double badPercent)
        {
            if (perfectPercent < 0d)
                throw new ArgumentOutOfRangeException(nameof(perfectPercent), "Perfect percent cannot be negative.");

            if (goodPercent < perfectPercent)
                throw new ArgumentOutOfRangeException(nameof(goodPercent), "Good percent must be greater than or equal to perfect percent.");

            if (badPercent < goodPercent)
                throw new ArgumentOutOfRangeException(nameof(badPercent), "Bad percent must be greater than or equal to good percent.");

            PerfectPercent = perfectPercent;
            GoodPercent = goodPercent;
            BadPercent = badPercent;
        }

        public double PerfectPercent { get; }
        public double GoodPercent { get; }
        public double BadPercent { get; }
    }
}
