namespace Outcasts.Battle
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine.Pool;

    public sealed class Chart : IReadOnlyChart
    {
        SortedList<double, List<Note>> m_EventsByTiming;
        public Chart()
        {
            m_EventsByTiming = new();
            m_NoteToLinkedNote = new();
        }

        public void AddNote(Note action)
        {
            double timing = action.Time;
            if (!m_EventsByTiming.TryGetValue(timing, out var list))
            {
                list = new List<Note>();
                m_EventsByTiming.Add(timing, list);
            }
            list.Add(action);
        }
        Dictionary<Note, Note> m_NoteToLinkedNote;
        public void AddLinkedNotes(Note startAction, Note endAction)
        {
            AddNote(startAction);
            AddNote(endAction);

            m_NoteToLinkedNote.Add(startAction, endAction);
            m_NoteToLinkedNote.Add(endAction, startAction);
        }
        public Note GetLinkedNote(Note note) => m_NoteToLinkedNote[note];
        public void Remove(Note note) => RemoveFirstNote(NoteFilter.FromNote(note));
        public Note RemoveFirstNote(NoteFilter filter) => GetNotesInternal(filter, limit: 1, remove: true).FirstOrDefault();
        public IEnumerable<Note> RemoveNotes(NoteFilter filter) => GetNotesInternal(filter, remove: true);

        public IEnumerable<Note> GetNotes(NoteFilter filter) => GetNotesInternal(filter);
        public Note GetFirstNote(NoteFilter filter) => GetNotesInternal(filter, limit: 1).FirstOrDefault();

        IEnumerable<Note> GetNotesInternal(NoteFilter filter, int limit = int.MaxValue, bool remove = false)
        {
            if (limit == 0) yield break;

            int startIndex = 0;
            if (filter.StartTime.HasValue)
            {
                (startIndex, _) = m_EventsByTiming.Keys.BinarySearch(filter.StartTime.Value);
            }

            int endIndex = m_EventsByTiming.Count - 1;
            if (filter.EndTime.HasValue)
            {
                bool doesIndexExist;
                (endIndex, doesIndexExist) = m_EventsByTiming.Keys.BinarySearch(filter.EndTime.Value);
                if (!doesIndexExist) endIndex--;
            }

            // Collect notes to return
            var notesToReturn = ListPool<Note>.Get();
            for (int index = startIndex; index <= endIndex; index++)
            {
                var key = m_EventsByTiming.Keys[index];
                var list = m_EventsByTiming[key];

                for (int noteIndexInList = list.Count - 1; noteIndexInList >= 0; noteIndexInList--)
                {
                    var note = list[noteIndexInList];
                    if (filter.IsAccepted(note))
                    {
                        notesToReturn.Add(note);
                        if(remove) list.RemoveAt(noteIndexInList);

                        // I only use "goto" to jump out of nested loops as its the cleanest and most readable way
                        if(notesToReturn.Count >= limit) goto GotoLabel_StopCollectingNotes;
                    }
                }
            }

        GotoLabel_StopCollectingNotes:
            foreach (var note in notesToReturn) yield return note;

            ListPool<Note>.Release(notesToReturn);
        }
    }

    public interface IReadOnlyChart
    {
        IEnumerable<Note> GetNotes(NoteFilter filter);
        Note GetFirstNote(NoteFilter filter);
        Note GetLinkedNote(Note note);
    }
}
