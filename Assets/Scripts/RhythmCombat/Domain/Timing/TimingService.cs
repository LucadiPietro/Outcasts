using System;

namespace RhythmCombat.Domain.Timing
{
    public sealed class TimingService
    {
        public double BeatToSeconds(double offsetSeconds, double beat, double bpm)
        {
            if (bpm <= 0d)
                throw new ArgumentOutOfRangeException(nameof(bpm), "BPM must be greater than zero.");

            return offsetSeconds + beat * (60d / bpm);
        }
    }
}