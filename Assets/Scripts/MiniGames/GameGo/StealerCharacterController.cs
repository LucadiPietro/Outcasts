namespace Minigames.GameGo
{
    using UnityEngine;

    public sealed class StealerCharacterController : CharacterController
    {
        readonly int kSteal = Animator.StringToHash("Steal");

        public void Steal() => Animator.SetTrigger(kSteal);
    }
}
