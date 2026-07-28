namespace RhythmCombat.Domain.Combat
{
    public sealed class EnemySlot : CombatSlot
    {
        public EnemySlot(string id, int characterIndex, float maxHealth)
            : base(id, characterIndex, maxHealth)
        {
        }
    }
}
