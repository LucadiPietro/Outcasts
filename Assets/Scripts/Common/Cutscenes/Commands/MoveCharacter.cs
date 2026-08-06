namespace Common.Cutscenes.Commands
{
    using NaughtyAttributes;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Moves a cutscene character to a target and optionally applies a final orientation.
    /// </summary>
    [Serializable]
    [AddTypeMenu("Character/Move")]
    public sealed class MoveCharacter : ICinematicCommand
    {
        [SerializeField, Label("CharacterReference"), HideInInspector]
        CutsceneCharacter m_Character;
        [SerializeField] MoveType m_WalkType;
        [SerializeField, ShowIf(nameof(IsCrouched)), AllowNesting]
        bool m_StayCrouched;
        [SerializeField] Transform m_Target;
        [SerializeField] Orientation m_endOrientation;
        [SerializeField] bool m_ShouldWaitEnd = true;

#if UNITY_EDITOR
        [Dropdown(nameof(GetSceneCharacters)), OnValueChanged(nameof(SceneCharacterChanged)), AllowNesting]
        [SerializeField, Label("Character")]
        int m_CharacterId;

        Dictionary<int, CutsceneCharacter> m_CharactersById;
#endif

        public bool ShouldWaitEnd => m_ShouldWaitEnd;
        bool IsCrouched => m_WalkType == MoveType.Crouch;

        public void Execute()
        {
            if (!TryGetMovementData(out Vector2 destination, out Vector2? lookAt))
            {
                return;
            }

            m_Character.Movable.MoveTo(
                destination,
                lookAt,
                m_WalkType,
                m_StayCrouched);
        }

        public IEnumerator ExecuteAwaitable()
        {
            if (!TryGetMovementData(out Vector2 destination, out Vector2? lookAt))
            {
                yield break;
            }

            yield return m_Character.Movable.MoveToAwaitable(
                destination,
                lookAt,
                m_WalkType,
                m_StayCrouched);
        }

        public void FastForward()
        {
            if (!TryGetMovementData(out Vector2 destination, out Vector2? lookAt))
            {
                return;
            }

            m_Character.Movable.TeleportTo(destination, lookAt);
        }

        /// <summary>
        /// Validates references and resolves the optional final look target.
        /// </summary>
        bool TryGetMovementData(out Vector2 destination, out Vector2? lookAt)
        {
            destination = default;
            lookAt = null;

            if (m_Character == null || m_Character.Movable == null || m_Target == null)
            {
                Debug.LogWarning("MoveCharacter skipped because a character, Movable, or target is missing.");
                return false;
            }

            destination = m_Target.position;
            Vector2 orientation = GetLookAtOrientation(m_endOrientation);
            if (orientation.sqrMagnitude > 0f)
            {
                lookAt = destination + orientation.normalized;
            }

            return true;
        }

        static Vector2 GetLookAtOrientation(Orientation orientation)
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
                default: return Vector2.zero;
            }
        }

#if UNITY_EDITOR
        Dictionary<int, CutsceneCharacter> CharactersById
        {
            get
            {
                if (m_CharactersById == null) m_CharactersById = new Dictionary<int, CutsceneCharacter>();
                m_CharactersById.Clear();
                m_CharactersById[0] = null;

                CutsceneCharacter[] characters =
                    GameObject.FindObjectsOfType<CutsceneCharacter>(true);
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
        NW,
    }
}
