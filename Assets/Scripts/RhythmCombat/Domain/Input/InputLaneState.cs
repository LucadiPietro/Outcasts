using System;

namespace RhythmCombat.Domain.Input
{
    public sealed class InputLaneState
    {
        public InputLaneState(int laneIndex)
        {
            if (laneIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(laneIndex));

            LaneIndex = laneIndex;
        }

        public int LaneIndex { get; }
        public bool IsPressed { get; private set; }
        public double? LastPressTimeSeconds { get; private set; }
        public double? LastReleaseTimeSeconds { get; private set; }

        public void RegisterPress(double timeSeconds)
        {
            IsPressed = true;
            LastPressTimeSeconds = timeSeconds;
        }

        public void RegisterRelease(double timeSeconds)
        {
            IsPressed = false;
            LastReleaseTimeSeconds = timeSeconds;
        }
    }
}