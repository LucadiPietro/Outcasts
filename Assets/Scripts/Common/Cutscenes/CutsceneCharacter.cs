namespace Common.Cutscenes
{
    using Minigames.GameGo;
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class CutsceneCharacter : MonoBehaviour
    {
        List<AwaitableAnimation> m_AwaitableAnimations = new List<AwaitableAnimation>();
        public IReadOnlyList<AwaitableAnimation> GetAwaitableAnimations()
        {
            GetComponentsInChildren(m_AwaitableAnimations);
            return m_AwaitableAnimations;
        }

        [SerializeField] CharacterView m_View;
        public CharacterView View => m_View;
    }
}
