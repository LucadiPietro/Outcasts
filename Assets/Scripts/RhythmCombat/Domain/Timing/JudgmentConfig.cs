using System;

namespace RhythmCombat.Domain.Timing
{
    public sealed class JudgmentConfig
    {
        public static JudgmentConfig Default { get; } = new JudgmentConfig(
            perfectWindowSeconds: 0.050d,
            goodWindowSeconds: 0.100d,
            badWindowSeconds: 0.150d,
            holdReleaseWindowSeconds: 0.075d,
            perfectScore: 1000,
            goodScore: 500,
            badScore: 100);

        public JudgmentConfig(
            double perfectWindowSeconds,
            double goodWindowSeconds,
            double badWindowSeconds,
            double holdReleaseWindowSeconds,
            int perfectScore,
            int goodScore,
            int badScore)
        {
            if (perfectWindowSeconds <= 0d)
                throw new ArgumentOutOfRangeException(nameof(perfectWindowSeconds));

            if (goodWindowSeconds < perfectWindowSeconds)
                throw new ArgumentOutOfRangeException(nameof(goodWindowSeconds));

            if (badWindowSeconds < goodWindowSeconds)
                throw new ArgumentOutOfRangeException(nameof(badWindowSeconds));

            if (holdReleaseWindowSeconds < 0d)
                throw new ArgumentOutOfRangeException(nameof(holdReleaseWindowSeconds));

            if (perfectScore < 0 || goodScore < 0 || badScore < 0)
                throw new ArgumentOutOfRangeException(nameof(perfectScore));

            PerfectWindowSeconds = perfectWindowSeconds;
            GoodWindowSeconds = goodWindowSeconds;
            BadWindowSeconds = badWindowSeconds;
            HoldReleaseWindowSeconds = holdReleaseWindowSeconds;

            PerfectScore = perfectScore;
            GoodScore = goodScore;
            BadScore = badScore;
        }

        public double PerfectWindowSeconds { get; }
        public double GoodWindowSeconds { get; }
        public double BadWindowSeconds { get; }
        public double HoldReleaseWindowSeconds { get; }

        public int PerfectScore { get; }
        public int GoodScore { get; }
        public int BadScore { get; }

        public int GetScore(JudgmentGrade grade) => grade switch
        {
            JudgmentGrade.Perfect => PerfectScore,
            JudgmentGrade.Good => GoodScore,
            JudgmentGrade.Bad => BadScore,
            _ => 0
        };
    }
}