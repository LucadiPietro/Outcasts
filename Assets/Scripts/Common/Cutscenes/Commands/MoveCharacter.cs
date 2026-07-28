namespace Common.Cutscenes.Commands
{
    using NaughtyAttributes;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Makes a Movable character Walk/Run/Crouch until it reaches a target Transform and then look into the Transform.up direction
    /// </summary>
    [Serializable]
    [AddTypeMenu("Character/Move")]
    public sealed class MoveCharacter : ICinematicCommand
    {
        public bool ShouldWaitEnd => m_ShouldWaitEnd;

        public void Execute()
        {
            m_Character.Movable.MoveTo(m_Target.position, lookAt: m_Target.position + GetLookAtOrientation(m_endOrientation), m_WalkType, m_StayCrouched);
        }
        public IEnumerator ExecuteAwaitable()
        {
            yield return m_Character.Movable.MoveToAwaitable(m_Target.position, lookAt: m_Target.position + GetLookAtOrientation(m_endOrientation), m_WalkType, m_StayCrouched);
        }
        public void FastForward()
        {
            m_Character.transform.position = m_Target.position;
            m_Character.View.LookAtDirection(m_Target.up);
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
        [SerializeField] MoveType m_WalkType;
        bool IsCrouched => m_WalkType == MoveType.Crouch;
        [SerializeField, ShowIf(nameof(IsCrouched)), AllowNesting] bool m_StayCrouched;
        [SerializeField] Transform m_Target;
        [SerializeField] Orientation m_endOrientation;

        [SerializeField] bool m_ShouldWaitEnd = true;
    
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


    public enum DestinationType
    {
        Transform,
        Vector,
    }

    public enum Orientation
    {
        None,
        N,
        NE,
        E,
        SE,
        S,
        SW,
        W,
        NW
    }
}
