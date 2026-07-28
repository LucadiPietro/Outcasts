namespace Outcasts.Battle
{
    using DG.Tweening;
    using LemonGames;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Pool;
    using UnityEngine.SceneManagement;

    public sealed class BattleManager : MonoBehaviour
    {
        static BattleManager s_Instance = default;
        public static BattleManager Instance
        {
            get
            {
                if (s_Instance == null) s_Instance = FindObjectOfType<BattleManager>();
                return s_Instance;
            }
        }

        [SerializeField] BattleSettings m_Settings;
        public BattleSettings Settings => m_Settings;

        [SerializeField] AudioSource m_Music;

        Chart m_Chart;
        public IReadOnlyChart Chart => m_Chart;

        double m_GameStartTime = 0;
        double m_Elapsed = 0;
        public double Elapsed => m_Elapsed;

        bool m_HasSongStartedPlaying = false;

        void OnEnable()
        {
            s_Instance = this; 
            
            m_Chart = LoadFirstChartInFolder(Application.persistentDataPath + "/StepManiaEditor");
            PlaySong();
        }

        Dictionary<NotePlacement, Note> m_HeldNotesByPlacement = new ();
        public void PerformAction(Note action)
        {
            if(action.Phase == NotePhase.End)
            {
                if(m_HeldNotesByPlacement.TryGetValue(action.Placement, out Note endNote))
                {
                    if (action.Time.IsInRange(endNote.Time - Settings.Timings.Perfect, endNote.Time + Settings.Timings.Perfect))
                    {
                        OnNoteConsumed(endNote, NoteResult.Perfect);
                    }
                    else if (action.Time.IsInRange(endNote.Time - Settings.Timings.Great, endNote.Time + Settings.Timings.Great))
                    {
                        OnNoteConsumed(endNote, NoteResult.Great);
                    }
                    else if (action.Time.IsInRange(endNote.Time - Settings.Timings.Good, endNote.Time + Settings.Timings.Good))
                    {
                        OnNoteConsumed(endNote, NoteResult.Good);
                    }
                    else
                    {
                        OnNoteConsumed(endNote, NoteResult.Deactivated);
                    }

                    // Remove both notes
                    var startNote = m_Chart.GetLinkedNote(endNote);
                    m_Chart.Remove(startNote);

                    m_Chart.Remove(endNote);
                    Debug.Log($"Finished holding {endNote}");
                    m_HeldNotesByPlacement.Remove(endNote.Placement);
                }
                return;
            }

            // We don't specify the phase in the filter because a button press can both match with an instant note and the start of an held note
            var firstGoodNote = m_Chart.GetFirstNote(new NoteFilter(
                startTime: action.Time - Settings.Timings.Good,
                endTime: action.Time + Settings.Timings.Good,
                type: action.Type,
                lane: action.Lane
            ));
                
            // A note is hit
            if (!firstGoodNote.IsNull)
            {
                if (action.Time.IsInRange(firstGoodNote.Time - Settings.Timings.Perfect, firstGoodNote.Time + Settings.Timings.Perfect))
                {
                    OnNoteConsumed(firstGoodNote, NoteResult.Perfect);
                }
                else if (action.Time.IsInRange(firstGoodNote.Time - Settings.Timings.Great, firstGoodNote.Time + Settings.Timings.Great))
                {
                    OnNoteConsumed(firstGoodNote, NoteResult.Great);
                }
                else if (action.Time.IsInRange(firstGoodNote.Time - Settings.Timings.Good, firstGoodNote.Time + Settings.Timings.Good))
                {
                    OnNoteConsumed(firstGoodNote, NoteResult.Good);
                }

                if (firstGoodNote.Phase == NotePhase.Start)
                {
                    var endNote = m_Chart.GetLinkedNote(firstGoodNote);
                    // TODO: Remove links from the charts
                    m_HeldNotesByPlacement.Add(endNote.Placement, endNote);
                    Debug.Log($"Started holding: {endNote}");
                }
                m_Chart.Remove(firstGoodNote);
            }
            // No note is hit, this is a ghost input
            else OnNoteConsumed(action, NoteResult.Ghost);
        }

        void Update()
        {
            // Don't increase time unless the song has started playing
            if (!m_HasSongStartedPlaying) return;

            m_Elapsed = Time.timeAsDouble - m_GameStartTime;

            RemoveMissedNotes();

            // Press R -> Reload scene
            if (Input.GetKeyDown(KeyCode.R)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        double m_LastMissTime;
        void RemoveMissedNotes()
        {
            // We want to find notes that have traveled beyond the last opportunity to hit them
            // This means:      Elapsed > note.Time + Settings.Timings.Good
            // Which becomes:   note.Time < Elapsed - Settings.Timings.Good
            double newMissTime =            Elapsed - Settings.Timings.Good;

            // This will only accept notes that were missed this frame
            var filter = new NoteFilter(
                startTime: m_LastMissTime,
                endTime: newMissTime
            );
            var notesMissedThisFrame = m_Chart.GetNotes(filter);

            foreach (var note in notesMissedThisFrame)
            {
                OnNoteConsumed(note, NoteResult.Miss);

                switch (note.Phase)
                {
                    // If you miss a start-note, also deactivate its end-note
                    case NotePhase.Start:
                    {
                        var endNote = m_Chart.GetLinkedNote(note);
                        OnNoteConsumed(endNote, NoteResult.Deactivated);

                        // TODO: This will be superfluous if removing a start/end note also automatically removes its counterpart
                        m_Chart.Remove(endNote);
                    }
                    break;
                    // If you miss an end-note that you were holding, it's no longer held
                    case NotePhase.End:
                    {
                        var startNote = m_Chart.GetLinkedNote(note);
                        m_Chart.Remove(startNote);
                        Debug.Log($"Cancel holding {note} because of miss");
                        m_HeldNotesByPlacement.Remove(note.Placement);
                    }
                    break;
                }
            }
            m_Chart.RemoveNotes(filter);

            m_LastMissTime = newMissTime + 0.00001d;
        }

        void PlaySong()
        {
            void PlaySongInternal()
            {
                m_GameStartTime = Time.timeAsDouble;
                m_Elapsed = 0;
                m_HasSongStartedPlaying = true;

                if (!m_Music.clip.loadState.Equals(AudioDataLoadState.Loaded)) m_Music.clip.LoadAudioData();
                m_Music.Play();
            }

#if UNITY_EDITOR
            // The Audio System takes a while to fire up and without the delayed call the music wouldn't be in sync with the notes
            // The delay is 0.1s here but it can be anything and it's only needed to warm up the Audio System
            // In the build, this is not needed
            DOVirtual.DelayedCall(0.1f, PlaySongInternal, ignoreTimeScale: false);
#else
            PlaySongInternal();
#endif
        }

        Chart LoadFirstChartInFolder(string folderPath)
        {
            string filePath = Directory.GetFiles(folderPath, "*.sm").FirstOrDefault();
            if (filePath == null)
            {
                Debug.LogError($"Couldn't find any .sm file at {folderPath}");
                return null;
            }
            return LoadChart(filePath);
        }
        Chart LoadChart(string filePath)
        {
            var chart = new Chart();

            string title = null;
            string audioFile = null;
            double offset = 0;
            int beatsPerMinute = 0;

            var lines = File.ReadAllLines(filePath);

            int lineIndex = 0;

            // Collect metadata
            while (lineIndex < lines.Length)
            {
                var line = lines[lineIndex];
                if (line.StartsWith("#TITLE:")) title = line.Substring(7, line.Length - 8);
                else if (line.StartsWith("#MUSIC:")) audioFile = line.Substring(7, line.Length - 8);
                else if (line.StartsWith("#OFFSET:")) offset = double.Parse(line.Substring(8, line.Length - 9), CultureInfo.InvariantCulture);
                else if (line.StartsWith("#BPMS:0.000=")) beatsPerMinute = Mathf.RoundToInt(float.Parse(line.Substring(12, line.Length - 13), CultureInfo.InvariantCulture));
                else if (line.StartsWith("0") || line.StartsWith("1") || line.StartsWith("2")) break;

                lineIndex++;

                if (lineIndex > 30) break;
            }

            int beatsPerMeasure = 4;
            double secondsPerBeat = 60d / beatsPerMinute;
            double secondsPerMeasure = secondsPerBeat * beatsPerMeasure;

            // The number of possible notes is the number of characters in the string
            int notesAmount = lines[lineIndex].Length;
            if (notesAmount % 2 != 0) throw new InvalidDataException($"Expected an even number of notes, but there are {notesAmount}");
            int attackNotesAmount = notesAmount / 2;

            var idToNote = DictionaryPool<NotePlacement, Note>.Get();

            int measureIndex = 0;
            // Convert notes to game data
            while (lineIndex < lines.Length)
            {
                int linesInThisMeasure = 0;
                // Peek forward in the file to determine how many lines this measure has (tipically 4, 8, or 16)
                for (int futureLineIndex = lineIndex; futureLineIndex < lines.Length; futureLineIndex++)
                {
                    string futureLine = lines[futureLineIndex];
                    // The loop stops if we reach the end of the measure (',') or the end of the chart (';')
                    if (futureLine == "," || futureLine == ";") break;
                    // Otherwise, we add this line to the current measure
                    else linesInThisMeasure++;
                }

                double startTimeForThisMeasure = secondsPerMeasure * measureIndex - offset;
                double secondsPerLine = secondsPerMeasure / linesInThisMeasure;

                // A line is a just a line in the text-file with numbers
                // We loop over all the lines in this measure
                for (int i = 0; i < linesInThisMeasure; i++)
                {
                    string line = lines[lineIndex + i];
                    // Time corresponding to this row
                    double time = startTimeForThisMeasure + secondsPerLine * i;

                    // Create all notes in this row
                    for (int noteIndex = 0; noteIndex < notesAmount; noteIndex++)
                    {
                        char noteChar = line[noteIndex];

                        NotePhase phase;
                        if (noteChar is '0') continue;
                        else if (noteChar is '1' or 'M') phase = NotePhase.Instant;
                        else if (noteChar is '2') phase = NotePhase.Start;
                        else if (noteChar is '3') phase = NotePhase.End;
                        else throw new ArgumentOutOfRangeException(nameof(noteChar));

                        NoteType noteType = (noteIndex < attackNotesAmount) ? NoteType.Attack : NoteType.Defence;
                        NoteLane lane = (NoteLane)(noteIndex % attackNotesAmount);

                        Note note = new Note(noteType, lane, phase, time);
                        switch (note.Phase)
                        {
                            case NotePhase.Instant:
                                chart.AddNote(note);
                                break;
                            case NotePhase.Start:
                                idToNote.Add(note.Placement, note);
                                break;
                            case NotePhase.End:
                                var startNote = idToNote[note.Placement];
                                idToNote.Remove(note.Placement);
                                chart.AddLinkedNotes(startNote, note);
                                break;
                        }
                    }
                }

                measureIndex++;
                lineIndex += linesInThisMeasure;

                // If there's a comma, move to the next line and keep going
                if (lines[lineIndex] == ",") lineIndex++;
                // If we're at the end of the file (";"), we stop
                else break;
            }

            DictionaryPool<NotePlacement, Note>.Release(idToNote);

            return chart;
        }

        Action<Note, NoteResult> m_NoteConsumed;
        /// <summary>
        /// Triggered both when notes are hit, missed, or with ghost input
        /// </summary>
        public event Action<Note, NoteResult> NoteConsumed
        {
            add { m_NoteConsumed += value; }
            remove { m_NoteConsumed -= value; }
        }
        void OnNoteConsumed(Note note, NoteResult result) { m_NoteConsumed?.Invoke(note, result); }
    }
    public struct NotePlacement : IEquatable<NotePlacement>
    {
        [SerializeField] NoteType m_Type;
        public NoteType Type => m_Type;
        [SerializeField] NoteLane m_Lane;
        public NoteLane Lane => m_Lane;

        public NotePlacement(NoteType type, NoteLane lane)
        {
            m_Type = type;
            m_Lane = lane;
        }

        public override bool Equals(object obj)
        {
            if (obj is NotePlacement other) return Equals(other);
            return false;
        }
        public bool Equals(NotePlacement other) => Type == other.Type && Lane == other.Lane;
        public override int GetHashCode() => HashCode.Combine(Type, Lane);
    }
    public struct NoteFilter
    {
        double? m_StartTime;
        public double? StartTime => m_StartTime;
        double? m_EndTime;
        public double? EndTime => m_EndTime;
        NoteType? m_Type;
        public NoteType? Type => m_Type;
        NoteLane? m_Lane;
        public NoteLane? Lane => m_Lane;
        NotePhase? m_Phase;
        public NotePhase? Phase => m_Phase;

        public NoteFilter(double? startTime = null, double? endTime = null, NoteType? type = null, NoteLane? lane = null, NotePhase? phase = null)
        {
            m_StartTime = startTime;
            m_EndTime = endTime;
            m_Type = type;
            m_Lane = lane;
            m_Phase = phase;
        }

        public bool IsAccepted(Note note)
        {
            if (StartTime.HasValue && note.Time < StartTime.Value) return false;
            if (EndTime.HasValue && note.Time > EndTime.Value) return false;
            if (Type.HasValue && note.Type != Type.Value) return false;
            if (Lane.HasValue && note.Lane != Lane.Value) return false;
            if (Phase.HasValue && note.Phase != Phase.Value) return false;
            return true;
        }

        public static NoteFilter FromNote(Note note)
        {
            return new NoteFilter(
                startTime: note.Time,
                endTime: note.Time,
                type: note.Type,
                lane: note.Lane,
                phase: note.Phase
            );
        }
    }

    public enum NoteResult
    {
        /// <summary>
        /// Only the end-note of an held-note can be deactivated. This happens when you start the press at the right time but release before the end-note time. A note can't be hit anymore once deactivated
        /// </summary>
        Deactivated = -2,
        /// <summary>
        /// A Ghost result is when you press a button not corresponding to any note
        /// </summary>
        Ghost = -1,
        /// <summary>
        /// A Miss is when there is a note but you don't press the button in time or at all
        /// </summary>
        Miss,
        Good,
        Great,
        Perfect,
    }

    /// <summary>
    /// Represents a fraction identifying the position a Line has inside of a Measure.
    /// For example, if a Measure has 8 lines then <see cref="LinesPerMeasure"/> = 8.
    /// The 1st line in that Measure will have <see cref="LineIndex"/> = 0 while the last will have <see cref="LineIndex"/> = 7
    /// </summary>
    [Serializable]
    public struct LinePosition
    {
        /// <summary>
        /// Index of the line in this position
        /// </summary>
        [SerializeField] int m_LineIndex;
        public int LineIndex => m_LineIndex;

        /// <summary>
        /// Number of lines this measure contains
        /// </summary>
        [SerializeField] int m_LinesPerMeasure;
        public int LinesPerMeasure => m_LinesPerMeasure;

        public LinePosition(int lineIndex, int linesPerMeasure)
        {
            m_LineIndex = lineIndex;
            m_LinesPerMeasure = linesPerMeasure;
        }

        /// <summary>
        /// Percent of the measure where this line is positioned
        /// </summary>
        public double Percent => (double)LineIndex / (LinesPerMeasure - 1);
    }
}
