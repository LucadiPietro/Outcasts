namespace Common.Cutscenes
{
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

        [SerializeField] Movable m_Movable;
        public Movable Movable => m_Movable;
    }
}
