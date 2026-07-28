namespace Common.Cutscenes.Commands
{
    using NaughtyAttributes;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    [Serializable]
    [AddTypeMenu("Character/Animation")]
    public sealed class PlayAwaitableAnimation : ICinematicCommand
    {
        public bool ShouldWaitEnd => m_ShouldWaitEnd;

        public void Execute()
        {
            if (m_Animation != null) m_Animation.Execute();
            else Debug.LogError($"The {nameof(PlayAwaitableAnimation)} that is currently playing has a null reference");
        }
        public IEnumerator ExecuteAwaitable()
        {
            if (m_Animation != null) yield return m_Animation.ExecuteAwaitable();
            else Debug.LogError($"The {nameof(PlayAwaitableAnimation)} that is currently playing has a null reference");
        }
        public void FastForward() { }

        [SerializeField] bool m_ShouldWaitEnd = true;

        [SerializeField, Label("CharacterReference"), HideInInspector] CutsceneCharacter m_Character;
        [SerializeField, Label("AnimationReference"), HideInInspector] AwaitableAnimation m_Animation;

#if UNITY_EDITOR
        #region Character
        [Dropdown(nameof(GetSceneCharacters)), OnValueChanged(nameof(SceneCharacterChanged)), AllowNesting]
        [SerializeField, Label("Character")] int m_CharacterId;

        Dictionary<int, CutsceneCharacter> m_CharactersById;
        Dictionary<int, CutsceneCharacter> CharactersById
        {
            get
            {
                if (m_CharactersById == null) m_CharactersById = new Dictionary<int, CutsceneCharacter>();
                else m_CharactersById.Clear();

                m_CharactersById.Add(0, null);
                var allCharacters = GameObject.FindObjectsOfType<CutsceneCharacter>(true);
                foreach (var character in allCharacters) m_CharactersById.Add(character.GetInstanceID(), character);

                return m_CharactersById;
            }
        }
        DropdownList<int> GetSceneCharacters()
        {
            var list = new DropdownList<int>();
            foreach (var pair in CharactersById)
            {
                int id = pair.Key;
                var character = pair.Value;

                string displayValue = character != null ? character.name : "<None>";
                list.Add(displayValue, id);
            }

            if (!CharactersById.Values.Contains(m_Character))
            {
                m_CharacterId = 0;
                m_Character = null;
            }
            else m_CharacterId = m_CharactersById.First(pair => pair.Value == m_Character).Key;

            return list;
        }
        void SceneCharacterChanged()
        {
            m_Character = CharactersById[m_CharacterId];
        }
        #endregion

        #region Animation
        [Dropdown(nameof(GetCharacterAnimations)), OnValueChanged(nameof(CharacterAnimationChanged)), AllowNesting]
        [SerializeField, Label("Animation")] int m_AnimationId;

        Dictionary<int, AwaitableAnimation> m_AnimationById;
        Dictionary<int, AwaitableAnimation> AnimationById
        {
            get
            {
                if (m_AnimationById == null) m_AnimationById = new Dictionary<int, AwaitableAnimation>();
                else m_AnimationById.Clear();

                m_AnimationById.Add(0, null);
                if (m_Character != null)
                {
                    var allAnimations = m_Character.GetAwaitableAnimations();
                    foreach (var animation in allAnimations) m_AnimationById.Add(animation.GetInstanceID(), animation);
                }

                return m_AnimationById;
            }
        }
        DropdownList<int> GetCharacterAnimations()
        {
            var list = new DropdownList<int>();
            foreach (var pair in AnimationById)
            {
                int id = pair.Key;
                var animation = pair.Value;

                string displayValue = animation != null ? animation.Trigger : "<None>";
                list.Add(displayValue, id);
            }

            if (!AnimationById.Values.Contains(m_Animation))
            {
                m_AnimationId = 0;
                m_Animation = null;
            }
            return list;
        }
        void CharacterAnimationChanged()
        {
            m_Animation = AnimationById[m_AnimationId];
        }
        #endregion
#endif
    }
}