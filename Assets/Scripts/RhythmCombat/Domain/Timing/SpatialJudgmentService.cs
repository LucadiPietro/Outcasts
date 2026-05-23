using RhythmCombat.Domain.Chart;
using RhythmCombat.Domain.Geometry;
using RhythmCombat.Domain.Movement;
using System;

namespace RhythmCombat.Domain.Timing
{
    public sealed class SpatialJudgmentService : INoteJudgmentService
    {
        private readonly BattlefieldLayout _layout;
        private readonly NoteMotionService _motionService;
        private readonly SpatialJudgmentConfig _config;

        public SpatialJudgmentService(
            BattlefieldLayout layout,
            NoteMotionService motionService,
            SpatialJudgmentConfig config)
        {
            _layout = layout ?? throw new ArgumentNullException(nameof(layout));
            _motionService = motionService ?? throw new ArgumentNullException(nameof(motionService));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public JudgmentResult Evaluate(CombatNote note, double currentTimeSeconds)
        {
            if (note is null)
                throw new ArgumentNullException(nameof(note));

            var normalizedDistance = GetNormalizedDistanceFromLaneCenter(note, currentTimeSeconds);
            var grade = GetGrade(normalizedDistance);

            return new JudgmentResult(grade, currentTimeSeconds - note.HitTimeSeconds);
        }

        public bool IsMissed(CombatNote note, double currentTimeSeconds)
        {
            if (note is null)
                throw new ArgumentNullException(nameof(note));

            var position = _motionService.GetPosition(note, currentTimeSeconds);
            if (position.ShouldDespawn)
                return true;

            if (position.NormalizedProgress < 1d)
                return false;

            return GetNormalizedDistanceFromLaneCenter(note, currentTimeSeconds) > _config.BadPercent;
        }

        public double GetDistanceFromLaneCenter(CombatNote note, double currentTimeSeconds)
        {
            if (note is null)
                throw new ArgumentNullException(nameof(note));

            var position = _motionService.GetPosition(note, currentTimeSeconds);
            var lane = _layout.GetLane(note.LaneIndex);

            return Double2.Distance(position.Position, lane.Center);
        }

        public double GetNormalizedDistanceFromLaneCenter(CombatNote note, double currentTimeSeconds)
        {
            var lane = _layout.GetLane(note.LaneIndex);
            return GetDistanceFromLaneCenter(note, currentTimeSeconds) / lane.ToleranceRadius;
        }

        private JudgmentGrade GetGrade(double normalizedDistance)
        {
            if (normalizedDistance <= _config.PerfectPercent)
                return JudgmentGrade.Perfect;

            if (normalizedDistance <= _config.GoodPercent)
                return JudgmentGrade.Good;

            if (normalizedDistance <= _config.BadPercent)
                return JudgmentGrade.Bad;

            return JudgmentGrade.Miss;
        }
    }
}
