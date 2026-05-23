using RhythmCombat.Domain.Chart;
using RhythmCombat.Domain.Geometry;
using RhythmCombat.Domain.Movement;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RhythmCombat.Unity
{
    public sealed class SpatialChartVisualTestRunner : MonoBehaviour
    {
        const string kDefaultChartPath = "Assets/RhythmCombat/Generated/Charts/Tutorial Battaglia   Epic Metal Feels_pump-halfdouble_Beginner.asset";

        [Header("Chart")]
        [SerializeField] ChartDataAsset m_Chart;
        [SerializeField] int m_MaxNotesToSpawn = 48;

        [Header("Layout")]
        [SerializeField] double m_LaneSpacing = 2d;
        [SerializeField] double m_RowOffset = 1d;
        [SerializeField] double m_ToleranceRadius = 0.5d;

        [Header("Timing")]
        [SerializeField] double m_ApproachDurationSeconds = 2d;
        [SerializeField] double m_DespawnAfterHitSeconds = 1d;
        [SerializeField] bool m_PlayOnStart = true;
        [SerializeField] float m_TimeScale = 1f;

        [Header("Visuals")]
        [SerializeField] float m_NoteSize = 0.22f;
        [SerializeField] Color m_AttackColor = new Color(0.95f, 0.25f, 0.2f);
        [SerializeField] Color m_DefenseColor = new Color(0.2f, 0.55f, 1f);
        [SerializeField] Color m_HoldColor = new Color(0.9f, 0.75f, 0.2f);
        [SerializeField] Color m_LaneColor = new Color(1f, 1f, 1f, 0.35f);

        readonly List<VisualNote> m_Notes = new List<VisualNote>();

        NoteMotionService m_MotionService;
        BattlefieldLayout m_Layout;
        double m_ElapsedSeconds;
        bool m_IsPlaying;

        void Awake()
        {
            TryLoadDefaultChart();
            SetupCamera();
            BuildServices();
            BuildSceneVisuals();
            BuildNotesFromChart();
        }

        void Start()
        {
            if (m_PlayOnStart)
                Play();
        }

        void Update()
        {
            if (!m_IsPlaying)
                return;

            m_ElapsedSeconds += Time.deltaTime * m_TimeScale;

            for (var i = 0; i < m_Notes.Count; i++)
            {
                var visualNote = m_Notes[i];
                var position = m_MotionService.GetPosition(visualNote.Note, m_ElapsedSeconds);

                visualNote.View.transform.position = ToVector3(position.Position);
                visualNote.View.SetActive(!position.ShouldDespawn && position.NormalizedProgress >= 0d);
            }
        }

        [ContextMenu("Play Visual Test")]
        public void Play()
        {
            m_ElapsedSeconds = 0d;
            m_IsPlaying = true;
        }

        [ContextMenu("Restart Visual Test")]
        public void Restart()
        {
            Play();
        }

        void BuildServices()
        {
            m_Layout = BattlefieldLayout.CreateStandard(m_LaneSpacing, m_RowOffset, m_ToleranceRadius);
            var travelSettings = new NoteTravelSettings(m_ApproachDurationSeconds, m_DespawnAfterHitSeconds);
            m_MotionService = new NoteMotionService(m_Layout, travelSettings);
        }

        void BuildSceneVisuals()
        {
            CreateMarker("Spawn Center", m_Layout.SpawnCenter, Color.white, 0.16f);

            for (var laneIndex = 0; laneIndex < LaneMapping.TotalLaneCount; laneIndex++)
            {
                var lane = m_Layout.GetLane(laneIndex);
                CreateMarker("Lane " + laneIndex, lane.Center, m_LaneColor, 0.36f);
                CreateLabel(laneIndex.ToString(), lane.Center);
            }
        }

        void BuildNotesFromChart()
        {
            if (m_Chart == null)
            {
                Debug.LogError("SpatialChartVisualTestRunner has no ChartDataAsset assigned.", this);
                return;
            }

            var spawned = 0;

            for (var rowIndex = 0; rowIndex < m_Chart.rows.Count && spawned < m_MaxNotesToSpawn; rowIndex++)
            {
                var row = m_Chart.rows[rowIndex];
                if (row == null || row.lanes == null)
                    continue;

                for (var laneIndex = 0; laneIndex < row.lanes.Length && laneIndex < LaneMapping.TotalLaneCount; laneIndex++)
                {
                    var laneValue = row.lanes[laneIndex];
                    if (laneValue == 0)
                        continue;

                    var note = CreateNote(rowIndex, laneIndex, laneValue, row.time);
                    var view = CreateNoteView(note, laneValue);
                    view.SetActive(false);
                    m_Notes.Add(new VisualNote(note, view));

                    spawned++;
                    if (spawned >= m_MaxNotesToSpawn)
                        break;
                }
            }

            Debug.Log("Spatial visual test loaded " + m_Notes.Count + " notes from chart '" + m_Chart.name + "'.", this);
        }

        CombatNote CreateNote(int rowIndex, int laneIndex, byte laneValue, double time)
        {
            var id = "visual-row-" + rowIndex + "-lane-" + laneIndex + "-value-" + laneValue;
            var power = 1f;

            if (laneValue == 2)
                return new HoldNote(id, laneIndex, time, time + 0.5d, power);

            return new CombatNote(id, laneIndex, time, power);
        }

        GameObject CreateNoteView(CombatNote note, byte laneValue)
        {
            var view = GameObject.CreatePrimitive(PrimitiveType.Quad);
            view.name = "Note Lane " + note.LaneIndex + " @ " + note.HitTimeSeconds.ToString("0.000");
            view.transform.SetParent(transform);
            view.transform.localScale = Vector3.one * m_NoteSize;

            var renderer = view.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = CreateMaterial(GetNoteColor(note, laneValue));

            return view;
        }

        Color GetNoteColor(CombatNote note, byte laneValue)
        {
            if (laneValue == 2 || laneValue == 3)
                return m_HoldColor;

            return note.LaneType == LaneType.Attack ? m_AttackColor : m_DefenseColor;
        }

        void CreateMarker(string markerName, Double2 position, Color color, float size)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            marker.name = markerName;
            marker.transform.SetParent(transform);
            marker.transform.position = ToVector3(position);
            marker.transform.localScale = Vector3.one * size;

            var renderer = marker.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = CreateMaterial(color);
        }

        void CreateLabel(string text, Double2 position)
        {
            var labelObject = new GameObject("Lane Label " + text);
            labelObject.transform.SetParent(transform);
            labelObject.transform.position = ToVector3(new Double2(position.X, position.Y + 0.38d));

            var label = labelObject.AddComponent<TextMesh>();
            label.text = text;
            label.fontSize = 48;
            label.characterSize = 0.08f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = Color.white;
        }

        Material CreateMaterial(Color color)
        {
            var material = new Material(Shader.Find("Sprites/Default"));
            material.color = color;
            return material;
        }

        Vector3 ToVector3(Double2 position)
        {
            return new Vector3((float)position.X, (float)position.Y, 0f);
        }

        void SetupCamera()
        {
            if (Camera.main == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                cameraObject.AddComponent<Camera>();
            }

            var camera = Camera.main;
            camera.orthographic = true;
            camera.orthographicSize = 3.2f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.05f, 0.06f, 0.08f);
        }

        void TryLoadDefaultChart()
        {
            if (m_Chart != null)
                return;

#if UNITY_EDITOR
            m_Chart = AssetDatabase.LoadAssetAtPath<ChartDataAsset>(kDefaultChartPath);
#endif
        }

        readonly struct VisualNote
        {
            public VisualNote(CombatNote note, GameObject view)
            {
                Note = note;
                View = view;
            }

            public CombatNote Note { get; }
            public GameObject View { get; }
        }
    }
}
