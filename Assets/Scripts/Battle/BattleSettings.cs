namespace Outcasts.Battle
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "BattleSettings", menuName = "Battle/Settings")]
    public sealed class BattleSettings : ScriptableObject
    {
        [SerializeField] double m_NoteTravelTime = 1.25f;
        public double NoteTravelTime => m_NoteTravelTime;
        [SerializeField] TimingSettings m_Timings;
        public TimingSettings Timings => m_Timings;
    }

    [System.Serializable]
    public class TimingSettings
    {
        [SerializeField] double m_Good = 0.1f;
        public double Good => m_Good;
        [SerializeField] double m_Great = 0.06f;
        public double Great => m_Great;
        [SerializeField] double m_Perfect = 0.03f;
        public double Perfect => m_Perfect;
    }
}
