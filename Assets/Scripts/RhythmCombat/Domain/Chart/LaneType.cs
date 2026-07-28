using System;

namespace RhythmCombat.Domain.Chart
{
    public enum LaneType
    {
        Attack = 0,
        Defense = 1
    }

    public static class LaneMapping
    {
        public const int AttackLaneCount = 3;
        public const int DefenseLaneCount = 3;
        public const int TotalLaneCount = 6;
        public const int CharacterCount = 3;

        public static LaneType GetLaneType(int laneIndex) => laneIndex switch
        {
            >= 0 and <= 2 => LaneType.Attack,
            >= 3 and <= 5 => LaneType.Defense,
            _ => throw new ArgumentOutOfRangeException(nameof(laneIndex), "Lane index must be between 0 and 5.")
        };

        public static int ToCharacterIndex(int laneIndex) => laneIndex switch
        {
            >= 0 and <= 2 => laneIndex,
            >= 3 and <= 5 => laneIndex - 3,
            _ => throw new ArgumentOutOfRangeException(nameof(laneIndex), "Lane index must be between 0 and 5.")
        };

        public static int ToAttackLaneIndex(int characterIndex)
        {
            ValidateCharacterIndex(characterIndex);
            return characterIndex;
        }

        public static int ToDefenseLaneIndex(int characterIndex)
        {
            ValidateCharacterIndex(characterIndex);
            return characterIndex + 3;
        }

        public static void ValidateLaneIndex(int laneIndex)
        {
            if (laneIndex < 0 || laneIndex >= TotalLaneCount)
                throw new ArgumentOutOfRangeException(nameof(laneIndex), "Lane index must be between 0 and 5.");
        }

        public static void ValidateCharacterIndex(int characterIndex)
        {
            if (characterIndex < 0 || characterIndex >= CharacterCount)
                throw new ArgumentOutOfRangeException(nameof(characterIndex), "Character index must be between 0 and 2.");
        }
    }

}



