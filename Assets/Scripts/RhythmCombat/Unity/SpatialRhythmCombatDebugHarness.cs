using RhythmCombat.Domain.Chart;
using RhythmCombat.Domain.Combat;
using RhythmCombat.Domain.Geometry;
using RhythmCombat.Domain.Movement;
using RhythmCombat.Domain.Super;
using RhythmCombat.Domain.Timing;
using RhythmCombat.Runtime;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmCombat.Unity
{
    public sealed class SpatialRhythmCombatDebugHarness : MonoBehaviour
    {
        [SerializeField] bool m_RunOnStart;

        void Start()
        {
            if (m_RunOnStart)
                RunHarness();
        }

        [ContextMenu("Run Spatial Rhythm Combat Harness")]
        public void RunHarness()
        {
            var layout = BattlefieldLayout.CreateStandard(
                laneSpacing: 2d,
                rowOffset: 1d,
                toleranceRadius: 1d);

            var travelSettings = new NoteTravelSettings(
                approachDurationSeconds: 2d,
                despawnAfterHitSeconds: 1d);

            var motionService = new NoteMotionService(layout, travelSettings);
            var spatialJudgment = new SpatialJudgmentService(
                layout,
                motionService,
                new SpatialJudgmentConfig(0.10d, 0.25d, 0.50d));

            var damageResolver = new DamageResolver();
            var engine = new CombatRhythmEngine(
                spatialJudgment,
                new JudgmentService(JudgmentConfig.Default),
                new SuperMeterService(),
                damageResolver,
                new DefenseResolver(),
                new SuperModeService(damageResolver));

            var tapAttack = new CombatNote("tap-attack-lane-0", 0, 2d, 10f);
            var tapDefense = new CombatNote("tap-defense-lane-3", 3, 3d, 7f);
            var hold = new HoldNote("hold-attack-lane-1", 1, 4d, 5d, 12f);

            var chart = new CombatChart(new CombatNote[]
            {
                tapAttack,
                tapDefense,
                hold
            });

            var state = new CombatRhythmState(
                chart,
                CreateParty(),
                CreateEnemies(),
                CreateSupers(),
                new MultiplierService());

            LogPosition(motionService, spatialJudgment, tapAttack, 1d);
            LogPosition(motionService, spatialJudgment, tapAttack, 2d);

            var attackResult = engine.ProcessTapHit(state, 0, 2d);
            LogResult("perfect attack tap", state, attackResult);

            LogPosition(motionService, spatialJudgment, tapDefense, 3.6d);
            foreach (var result in engine.Update(state, 3.6d))
                LogResult("auto miss defense tap", state, result);

            LogPosition(motionService, spatialJudgment, hold, 4d);
            var holdStart = engine.ProcessTapHit(state, 1, 4d);
            LogResult("hold start", state, holdStart);

            var holdRelease = engine.ProcessLaneRelease(state, 1, 5d);
            LogResult("hold release", state, holdRelease);
        }

        private static List<PartyCharacterSlot> CreateParty()
        {
            return new List<PartyCharacterSlot>
            {
                new PartyCharacterSlot("party-0", 0, 100f),
                new PartyCharacterSlot("party-1", 1, 100f),
                new PartyCharacterSlot("party-2", 2, 100f)
            };
        }

        private static List<EnemySlot> CreateEnemies()
        {
            return new List<EnemySlot>
            {
                new EnemySlot("enemy-0", 0, 100f),
                new EnemySlot("enemy-1", 1, 100f),
                new EnemySlot("enemy-2", 2, 100f)
            };
        }

        private static List<CharacterSuperState> CreateSupers()
        {
            return new List<CharacterSuperState>
            {
                new CharacterSuperState(0, new SuperMeter(10f)),
                new CharacterSuperState(1, new SuperMeter(10f)),
                new CharacterSuperState(2, new SuperMeter(10f))
            };
        }

        private static void LogPosition(
            NoteMotionService motionService,
            SpatialJudgmentService spatialJudgment,
            CombatNote note,
            double timeSeconds)
        {
            var position = motionService.GetPosition(note, timeSeconds);
            var distance = spatialJudgment.GetDistanceFromLaneCenter(note, timeSeconds);
            var normalizedDistance = spatialJudgment.GetNormalizedDistanceFromLaneCenter(note, timeSeconds);
            var judgment = spatialJudgment.Evaluate(note, timeSeconds);

            Debug.Log(
                "note=" + note.Id +
                " lane=" + note.LaneIndex +
                " time=" + timeSeconds +
                " position=" + position.Position +
                " progress=" + position.NormalizedProgress +
                " distance=" + distance +
                " normalizedDistance=" + normalizedDistance +
                " judgment=" + judgment.Grade +
                " shouldDespawn=" + position.ShouldDespawn);
        }

        private static void LogResult(string label, CombatRhythmState state, CombatActionResult result)
        {
            var judgment = result.Judgment.HasValue ? result.Judgment.Value.Grade.ToString() : "None";

            Debug.Log(
                label +
                " action=" + result.ActionKind +
                " note=" + result.NoteId +
                " judgment=" + judgment +
                " scoreGained=" + result.ScoreGained +
                " score=" + state.Score +
                " multiplier=" + result.MultiplierAfter +
                " superGained=" + result.SuperMeterGained +
                " damage=" + result.DamageResult.TotalAppliedDamage);
        }
    }
}
