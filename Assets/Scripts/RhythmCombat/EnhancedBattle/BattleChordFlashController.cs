using System.Collections.Generic;
using UnityEngine;

public class BattleChordFlashController : MonoBehaviour
{
    sealed class ChordGroup
    {
        public ChordGroup(int expectedCount)
        {
            ExpectedCount = expectedCount;
        }

        public int ExpectedCount;
        public readonly List<BattleButton> Notes =
            new List<BattleButton>(6);
    }

    public static BattleChordFlashController Instance
    {
        get;
        private set;
    }

    [SerializeField] RectTransform coordinateSpace;
    [SerializeField] List<ChordFlashLink> linkPool =
        new List<ChordFlashLink>(36);

    [SerializeField] Color linkColor =
        new Color(1f, 0.95f, 0.40f, 1f);

    [SerializeField] float linkThickness = 9f;
    [SerializeField] float spawnPulseDuration = 0.24f;
    [SerializeField, Range(0.05f, 1f)]
    float steadyAlpha = 0.68f;

    readonly Dictionary<int, ChordGroup> groups =
        new Dictionary<int, ChordGroup>();

    int nextPoolIndex;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

#if UNITY_EDITOR
    public void ConfigureEditor(
        RectTransform newCoordinateSpace,
        List<ChordFlashLink> links)
    {
        coordinateSpace = newCoordinateSpace;
        linkPool = links ?? new List<ChordFlashLink>(36);
    }
#endif

    public void RegisterSpawn(
        int groupId,
        int expectedCount,
        BattleButton note)
    {
        if (groupId < 0 ||
            expectedCount < 2 ||
            note == null)
        {
            return;
        }

        if (!groups.TryGetValue(
                groupId,
                out ChordGroup group))
        {
            group = new ChordGroup(expectedCount);
            groups.Add(groupId, group);
        }

        if (!group.Notes.Contains(note))
        {
            group.Notes.Add(note);
        }

        if (group.Notes.Count < group.ExpectedCount)
        {
            return;
        }

        LinkGroup(group);
        groups.Remove(groupId);
    }

    void LinkGroup(ChordGroup group)
    {
        group.Notes.RemoveAll(note => note == null);
        group.Notes.Sort(
            (left, right) =>
                left.laneIndex.CompareTo(right.laneIndex));

        for (int i = 0; i < group.Notes.Count; i++)
        {
            group.Notes[i]?.PlayChordPulse();
        }

        for (int i = 0; i < group.Notes.Count - 1; i++)
        {
            ChordFlashLink link = AcquireLink();

            if (link == null)
            {
                break;
            }

            link.Attach(
                group.Notes[i],
                group.Notes[i + 1],
                coordinateSpace,
                linkColor,
                linkThickness,
                spawnPulseDuration,
                steadyAlpha);
        }
    }

    ChordFlashLink AcquireLink()
    {
        if (linkPool == null || linkPool.Count == 0)
        {
            return null;
        }

        for (int i = 0; i < linkPool.Count; i++)
        {
            int index =
                (nextPoolIndex + i) % linkPool.Count;

            ChordFlashLink candidate = linkPool[index];

            if (candidate != null &&
                !candidate.gameObject.activeSelf)
            {
                nextPoolIndex =
                    (index + 1) % linkPool.Count;

                return candidate;
            }
        }

        ChordFlashLink reused =
            linkPool[nextPoolIndex];

        nextPoolIndex =
            (nextPoolIndex + 1) % linkPool.Count;

        reused?.Detach();
        return reused;
    }
}
