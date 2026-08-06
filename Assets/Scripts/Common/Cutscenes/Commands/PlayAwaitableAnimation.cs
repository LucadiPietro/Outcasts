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
        [SerializeField] bool m_ShouldWaitEnd = true;
        [SerializeField, Label("CharacterReference"), HideInInspector]
        CutsceneCharacter m_Character;
        [SerializeField, Label("AnimationReference"), HideInInspector]
        AwaitableAnimation m_Animation;

#if UNITY_EDITOR
        [Dropdown(nameof(GetSceneCharacters)), OnValueChanged(nameof(SceneCharacterChanged)), AllowNesting]
        [SerializeField, Label("Character")]
        int m_CharacterId;
        [Dropdown(nameof(GetCharacterAnimations)), OnValueChanged(nameof(CharacterAnimationChanged)), AllowNesting]
        [SerializeField, Label("Animation")]
        int m_AnimationId;

        Dictionary<int, CutsceneCharacter> m_CharactersById;
        Dictionary<int, AwaitableAnimation> m_AnimationById;
#endif

        public bool ShouldWaitEnd => m_ShouldWaitEnd;

        public void Execute()
        {
            if (m_Animation == null)
            {
                Debug.LogWarning("PlayAwaitableAnimation skipped because its animation reference is missing.");
                return;
            }

            m_Animation.Execute();
        }

        public IEnumerator ExecuteAwaitable()
        {
            if (m_Animation == null)
            {
                Debug.LogWarning("PlayAwaitableAnimation skipped because its animation reference is missing.");
                yield break;
            }

            yield return m_Animation.ExecuteAwaitable();
        }

        public void FastForward()
        {
            // The following command applies the authored final state. Triggering the
            // animation during fast-forward would reintroduce temporal side effects.
        }

#if UNITY_EDITOR
        Dictionary<int, CutsceneCharacter> CharactersById
        {
            get
            {
                if (m_CharactersById == null) m_CharactersById = new Dictionary<int, CutsceneCharacter>();
                m_CharactersById.Clear();
                m_CharactersById[0] = null;
                CutsceneCharacter[] characters = GameObject.FindObjectsOfType<CutsceneCharacter>(true);
                for (int i = 0; i < characters.Length; i++)
                {
                    m_CharactersById[characters[i].GetInstanceID()] = characters[i];
                }
                return m_CharactersById;
            }
        }

        DropdownList<int> GetSceneCharacters()
        {
            var list = new DropdownList<int>();
            foreach (KeyValuePair<int, CutsceneCharacter> pair in CharactersById)
            {
                list.Add(pair.Value != null ? pair.Value.name : "<None>", pair.Key);
            }

            if (!CharactersById.Values.Contains(m_Character))
            {
                m_CharacterId = 0;
                m_Character = null;
            }
            else
            {
                m_CharacterId = CharactersById.First(pair => pair.Value == m_Character).Key;
            }
            return list;
        }

        void SceneCharacterChanged()
        {
            CharactersById.TryGetValue(m_CharacterId, out m_Character);
            m_AnimationId = 0;
            m_Animation = null;
        }

        Dictionary<int, AwaitableAnimation> AnimationById
        {
            get
            {
                if (m_AnimationById == null) m_AnimationById = new Dictionary<int, AwaitableAnimation>();
                m_AnimationById.Clear();
                m_AnimationById[0] = null;

                if (m_Character != null)
                {
                    IReadOnlyList<AwaitableAnimation> animations = m_Character.GetAwaitableAnimations();
                    for (int i = 0; i < animations.Count; i++)
                    {
                        m_AnimationById[animations[i].GetInstanceID()] = animations[i];
                    }
                }

                return m_AnimationById;
            }
        }

        DropdownList<int> GetCharacterAnimations()
        {
            var list = new DropdownList<int>();
            foreach (KeyValuePair<int, AwaitableAnimation> pair in AnimationById)
            {
                list.Add(pair.Value != null ? pair.Value.Trigger : "<None>", pair.Key);
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
            AnimationById.TryGetValue(m_AnimationId, out m_Animation);
        }
#endif
    }
}
