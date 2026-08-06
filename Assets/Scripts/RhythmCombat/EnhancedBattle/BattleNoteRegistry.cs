using System.Collections.Generic;
using UnityEngine;

public static class BattleNoteRegistry
{
    static readonly List<BattleButton>[] NotesByLane =
    {
        new List<BattleButton>(16),
        new List<BattleButton>(16),
        new List<BattleButton>(16),
        new List<BattleButton>(16),
        new List<BattleButton>(16),
        new List<BattleButton>(16)
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetRegistry()
    {
        for (int i = 0; i < NotesByLane.Length; i++)
        {
            NotesByLane[i].Clear();
        }
    }

    public static void Register(BattleButton button)
    {
        if (button == null)
        {
            return;
        }

        int lane = ResolveLane(button);

        if (lane < 0 || lane >= NotesByLane.Length)
        {
            return;
        }

        List<BattleButton> notes = NotesByLane[lane];

        if (!notes.Contains(button))
        {
            notes.Add(button);
        }
    }

    public static void Unregister(BattleButton button)
    {
        if (button == null)
        {
            return;
        }

        int lane = ResolveLane(button);

        if (lane < 0 || lane >= NotesByLane.Length)
        {
            return;
        }

        NotesByLane[lane].Remove(button);
    }

    public static BattleButton Peek(BMButtonPrefab.Cell cell)
    {
        int lane = (int)cell;

        if (lane < 0 || lane >= NotesByLane.Length)
        {
            return null;
        }

        List<BattleButton> notes = NotesByLane[lane];
        BattleButton best = null;
        double bestTime = double.MaxValue;

        for (int i = notes.Count - 1; i >= 0; i--)
        {
            BattleButton note = notes[i];

            if (note == null)
            {
                notes.RemoveAt(i);
                continue;
            }

            if (note.isResolved || note.HoldStarted)
            {
                continue;
            }

            if (note.hitTimeSeconds < bestTime)
            {
                bestTime = note.hitTimeSeconds;
                best = note;
            }
        }

        return best;
    }

    static int ResolveLane(BattleButton button)
    {
        return button.laneIndex >= 0
            ? button.laneIndex
            : (int)button.cell;
    }
}
