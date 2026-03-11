using System;

namespace RhythmCombat.Domain.Combat
{
    public abstract class CombatSlot
    {
        protected CombatSlot(string id, int characterIndex, float maxHealth)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Slot id cannot be null or empty.", nameof(id));

            if (characterIndex < 0 || characterIndex > 2)
                throw new ArgumentOutOfRangeException(nameof(characterIndex), "Character index must be between 0 and 2.");

            if (maxHealth <= 0f)
                throw new ArgumentOutOfRangeException(nameof(maxHealth), "Max health must be greater than zero.");

            Id = id;
            CharacterIndex = characterIndex;
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
        }

        public string Id { get; }
        public int CharacterIndex { get; }
        public float MaxHealth { get; }
        public float CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0f;

        public float TakeDamage(float amount)
        {
            if (amount < 0f)
                throw new ArgumentOutOfRangeException(nameof(amount));

            var applied = Math.Min(CurrentHealth, amount);
            CurrentHealth -= applied;
            return applied;
        }

        public float Heal(float amount)
        {
            if (amount < 0f)
                throw new ArgumentOutOfRangeException(nameof(amount));

            var missing = MaxHealth - CurrentHealth;
            var restored = Math.Min(missing, amount);
            CurrentHealth += restored;
            return restored;
        }

        public void HealFull() => CurrentHealth = MaxHealth;
    }
}