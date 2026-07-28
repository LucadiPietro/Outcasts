namespace Outcasts.Battle
{
    using System;
    using UnityEngine;

    public sealed class BeatMapImporter : MonoBehaviour
    {
    }
    [Serializable]
    public struct Note : IEquatable<Note>
    {
        [SerializeField] NoteType m_Type;
        public NoteType Type => m_Type;
        [SerializeField] NoteLane m_Lane;
        public NoteLane Lane => m_Lane;
        [SerializeField] NotePhase m_Phase;
        public NotePhase Phase => m_Phase;
        [SerializeField] double m_Time;
        public double Time => m_Time;

        public NotePlacement Placement => new NotePlacement(Type, Lane);

        public bool IsNull => Phase == NotePhase.None;

        public Note(NoteType type, NoteLane area, NotePhase phase, double time)
        {
            m_Type = type;
            m_Lane = area;
            m_Phase = phase;
            m_Time = time;
        }

        public override string ToString() => $"{Phase} {Type} {Lane} at {Time}";

        public override bool Equals(object obj)
        {
            if (obj is Note other) return Equals(other);
            return false;
        }
        public bool Equals(Note other)
        {
            return true
                && Type == other.Type
                && Lane == other.Lane
                && Phase == other.Phase
                && Time == other.Time
            ;
        }
        public override int GetHashCode() => HashCode.Combine(Type, Lane, Phase, Time);
    }
    public enum NoteType
    {
        Attack,
        Defence,
    }

    public enum NoteLane
    {
        // It's important the first value is 0, because these are converted to list indices
        First = 0,
        Middle,
        Last,
    }
    public enum NotePhase
    {
        /// <summary>
        /// Used to check if a note is "null"
        /// </summary>
        None = 0,
        /// <summary>
        /// A single note that needs to be hit
        /// </summary>
        Instant = 1,
        /// <summary>
        /// The starting part of a held-note
        /// </summary>
        Start = 2,
        /// <summary>
        /// The ending part of a held-note
        /// </summary>
        End = 3,
    }
}
