namespace RhythmCombat.Domain.Combat
{
    public sealed class PartyCharacterSlot : CombatSlot
    {
        public PartyCharacterSlot(string id, int characterIndex, float maxHealth)
            : base(id, characterIndex, maxHealth)
        {
        }
    }
}