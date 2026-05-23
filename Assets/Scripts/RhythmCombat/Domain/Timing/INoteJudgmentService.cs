using RhythmCombat.Domain.Chart;

namespace RhythmCombat.Domain.Timing
{
    public interface INoteJudgmentService
    {
        JudgmentResult Evaluate(CombatNote note, double currentTimeSeconds);
        bool IsMissed(CombatNote note, double currentTimeSeconds);
    }
}
