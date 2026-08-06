namespace Common.Cutscenes.Commands
{
    using NaughtyAttributes;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    [Serializable]
    [AddTypeMenu("Character/StartFollowing")]
    public sealed class StartFollowing : ICinematicCommand
    {
        [SerializeField, Label("CharacterReference"), HideInInspector]
        CutsceneCharacter m_Character;
        [SerializeField] Transform m_Target;

#if UNITY_EDITOR
        [Dropdown(nameof(GetSceneCharacters)), OnValueChanged(nameof(SceneCharacterChanged)), AllowNesting]
        [SerializeField, Label("Character")]
        int m_CharacterId;
        Dictionary<int, CutsceneCharacter> m_CharactersById;
#endif

        public bool ShouldWaitEnd => false;

        public void Execute()
        {
            if (m_Character == null || m_Character.Movable == null)
            {
                Debug.LogWarning("StartFollowing skipped because its character or Movable is missing.");
                return;
            }

            m_Character.Movable.StartFollowing(m_Target);
        }

        public IEnumerator ExecuteAwaitable()
        {
            Execute();
            yield break;
        }

        public void FastForward()
        {
            Execute();
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
        }
#endif
    }
}
