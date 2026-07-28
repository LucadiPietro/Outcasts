using RhythmCombat.Domain.Chart;
using System;

namespace RhythmCombat.Domain.Super
{
    public sealed class CharacterSuperState
    {
        public CharacterSuperState(int characterIndex, SuperMeter meter)
        {
            LaneMapping.ValidateCharacterIndex(characterIndex);
            Meter = meter ?? throw new ArgumentNullException(nameof(meter));
            CharacterIndex = characterIndex;
        }

        public int CharacterIndex { get; }
        public int AttackLaneIndex => LaneMapping.ToAttackLaneIndex(CharacterIndex);
        public int DefenseLaneIndex => LaneMapping.ToDefenseLaneIndex(CharacterIndex);
        public SuperMeter Meter { get; }
    }
}