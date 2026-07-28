namespace Common.Cutscenes.Commands
{
    using NaughtyAttributes;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    [Serializable]
    [AddTypeMenu("Character/Look at cardinal")]
    public sealed class LookAtCardinal : ICinematicCommand
    {
        public bool ShouldWaitEnd => true;

        public void Execute() => m_Character.View.LookAtOrientation(m_orientation);
        public IEnumerator ExecuteAwaitable()
        {
            Execute();
            if (ShouldWaitEnd) yield break;
        }
        public void FastForward()
        {
            Execute();
        }

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
#endif
        [SerializeField, Label("CharacterReference"), HideInInspector] CutsceneCharacter m_Character;
        [SerializeField] Orientation m_orientation;
    }
}