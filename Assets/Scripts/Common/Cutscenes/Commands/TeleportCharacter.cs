namespace Common.Cutscenes.Commands
{
    using NaughtyAttributes;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    [Serializable]
    [AddTypeMenu("Character/Teleport")]
    public class TeleportCharacter : ICinematicCommand
    {
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
            else m_CharacterId = CharactersById.First(pair => pair.Value == m_Character).Key;

            return list;
        }
        void SceneCharacterChanged()
        {
            m_Character = CharactersById[m_CharacterId];
        }
        #endregion
#endif



        [SerializeField, Label("CharacterReference"), HideInInspector] CutsceneCharacter m_Character;
        [SerializeField] Transform m_Target;
        [SerializeField] Orientation m_endOrientation;

        
        public bool ShouldWaitEnd => false;



        public void Execute()
        {
            m_Character.transform.position = m_Target.position;
            m_Character.View.LookAtDirection(GetLookAtOrientation(m_endOrientation));
        }

        public IEnumerator ExecuteAwaitable() 
        {
            Execute();
            if (ShouldWaitEnd) yield break;
        }

        public void FastForward()
        {

        }

        Vector3 GetLookAtOrientation(Orientation orientation)
        {
            switch (orientation)
            {
                case Orientation.N: return Vector2.up;
                case Orientation.NE: return Vector2.up + Vector2.right;
                case Orientation.E: return Vector2.right;
                case Orientation.SE: return Vector2.down + Vector2.right;
                case Orientation.S: return Vector2.down;
                case Orientation.SW: return Vector2.down + Vector2.left;
                case Orientation.W: return Vector2.left;
                case Orientation.NW: return Vector2.up + Vector2.left;
                case Orientation.None: return Vector2.zero;
            }

            return Vector2.zero;
        }
    }
}